namespace ReamQuery.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using ReamQuery.Shared.Helpers;
    using NLog;

    public static class Dumper
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        
        static ConcurrentDictionary<int, Emitter> emitters  = new ConcurrentDictionary<int, Emitter>();
        
        public const string RawValueColumnName = "<RawValue>";

        /// <summary>ReamQuery.Inlined.Dumper</summary>
        public static T Dump<T>(this T o, int sessionId)
        {
            var emitter = GetEmitter(sessionId);
            if (o == null)
            {
                Logger.Info("Dumping null in session {0}", sessionId);
                var nullColumn = new Column
                {
                    Name = typeof(T).GetDisplayName(),
                    Type = typeof(T).FullName
                };
                emitter.Null(nullColumn);
            }
            else
            {
                Logger.Info("Dumping object in session {0}", sessionId);
                var oType = o.GetType();
                var seenTypes = new Dictionary<Type, Tuple<int, Column[]>>();
                Type listType;
                if (oType.TryGetListType(out listType))
                {
                    var tblId = emitter.Table(oType.GetDisplayName());
                    // if we detect a list, we always emit a table of values
                    var asEnum = o as IEnumerable<object>;
                    Debug.Assert(asEnum != null, "expected list");
                    foreach (var val in asEnum)
                    {
                        var valType = val.GetType();
                        if (!seenTypes.ContainsKey(valType))
                        {
                            Column[] valColumns; 
                            if (valType.TryGetTypeColumns(tblId, out valColumns))
                            {
                                //tabular
                                var colId = emitter.Header(valColumns, tblId);
                                seenTypes.Add(valType, Tuple.Create(colId, valColumns));
                            }
                            else
                            {
                                var atomicColumn = new Column[] 
                                {
                                    new Column
                                    {
                                        Parent = tblId,
                                        Name = valType.GetDisplayName(),
                                        Type = valType.FullName
                                    }
                                };
                                var colId = emitter.Header(atomicColumn, tblId);
                            }
                        }
                        var typeInfo = seenTypes[valType];
                        var columns = typeInfo.Item2;
                        var headerId = typeInfo.Item1;
                        var rowVals = new object[columns.Length];
                        
                        if (columns.Length == 1 && columns[0].Prop == null)
                        {
                            // if theres no PropertyInfo, its an atomic value, dump entire item into slot
                            rowVals[0] = val;
                        }
                        else
                        {
                            // otherwise, is some tabular thing
                            for(var i = 0; i < columns.Length; i++)
                            {
                                var prop = columns[i].Prop;
                                rowVals[i] = prop.GetValue(val);
                            }
                        }
                        emitter.Row(rowVals, headerId);
                    }
                }
                else
                {
                    // a singular value, either atomic, or tabular (with one row)
                    Column[] columns;
                    if (oType.TryGetTypeColumns(null, out columns))
                    {
                        var rowVals = new object[columns.Length];
                        for(var i = 0; i < columns.Length; i++)
                        {
                            var prop = columns[i].Prop;
                            rowVals[i] = prop.GetValue(o);
                        }
                        emitter.SingleTabular(oType.GetDisplayName(), columns, rowVals);
                    }
                    else
                    {
                        var singleColumn = new Column
                        {
                            Name = oType.GetDisplayName(),
                            Type = oType.FullName
                        };
                        emitter.SingleAtomic(singleColumn, o);
                    }
                }
            }
            var dispose = emitter.Complete();
            if (dispose)
            {
                emitters.TryRemove(sessionId, out emitter);
                emitter.Dispose();
            }
            return o;
        }

        public static Emitter InitializeEmitter(int session, int dumpCount)
        {
            Emitter emitter;
            if (!emitters.ContainsKey(session))
            {
                emitter = new Emitter(session, dumpCount);
                emitters.TryAdd(session, emitter);
            }
            else
            {
                throw new ArgumentException("session already has emitter");
            }
            return emitter;
        }

        static Emitter GetEmitter(int session)
        {
            Emitter emitter;
            emitters.TryGetValue(session, out emitter);
            if (emitter == null)
            {
                throw new Exception("emitter was null");
            }
            return emitter;
        }
    }
}
