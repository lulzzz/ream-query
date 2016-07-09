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

    public static class Dumper
    {
        static ConcurrentDictionary<Guid, DumpEmitter> emitters  = new ConcurrentDictionary<Guid, DumpEmitter>();
        
        public const string RawValueColumnName = "<RawValue>";

        /// <summary>ReamQuery.Inlined.Dumper</summary>
        public static T Dump<T>(this T o, Guid queryId)
        {
            var emitter = GetEmitter(queryId);
            if (o == null)
            {
                emitter.Null(typeof(T).GetDisplayName());
            }
            else
            {
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
                        emitter.SingleAtomic(oType.GetDisplayName(), o);
                    }
                }
            }
            return o;
        }

        public static DumpEmitter GetEmitter(Guid id)
        {
            DumpEmitter emitter;
            if (!emitters.ContainsKey(id))
            {
                emitter = new DumpEmitter();
                emitters.TryAdd(id, emitter);
            }
            emitters.TryGetValue(id, out emitter);
            if (emitter == null)
            {
                throw new Exception("emitter was null");
            }
            return emitter;
        }

        public static void CloseDrain(Guid id)
        {
            DumpEmitter drain;
            emitters.TryRemove(id, out drain);
            drain.Dispose();
        }
    }
}
