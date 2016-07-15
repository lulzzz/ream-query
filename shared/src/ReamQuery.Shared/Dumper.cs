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
        
        public const string RawValueColumnName = "<RawValue>";

        public static T Dump<T>(this T o, Emitter emitter)
        {
            try 
            {
                if (o == null)
                {
                    Logger.Debug("Dumping null using emitter {0}");
                    var nullColumn = new Column
                    {
                        Name = typeof(T).GetDisplayName(),
                        Type = typeof(T).FullName
                    };
                    emitter.Null(nullColumn);
                }
                else
                {
                    Logger.Debug("Dumping object #{1}", o.GetHashCode());
                    var oType = o.GetType();
                    var seenTypes = new Dictionary<Type, Tuple<int, Column[]>>();
                    Type listType;
                    if (oType.TryGetListType(out listType))
                    {
                        Logger.Debug("Inner type {0}", oType.FullName);
                        var tblId = emitter.Table(oType.GetDisplayName());
                        // if we detect a list, we always emit a table of values
                        var asEnum = o as IEnumerable<object>;
                        Debug.Assert(asEnum != null, "expected list");
                        foreach (var val in asEnum.ToList())
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
                            Logger.Debug("Single table");
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
                    Logger.Debug("Disposing emitter");
                    emitter.Dispose();
                }
            }
            catch (Exception exn)
            {
                Logger.Error("Exception {1} while dumping, message {0}", exn.Message, exn.HResult);
            }
            return o;
        }
    }
}
