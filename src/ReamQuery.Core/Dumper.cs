namespace ReamQuery.Core
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using ReamQuery.Core.Helpers;
    using ReamQuery.Core.Api;
    using NLog;

    public static class Dumper
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        
        public const string RawValueColumnName = "<RawValue>";

        /// <summary>ReamQuery.Core.Dumper.Dump</summary>
        public static T Dump<T>(this T o, Emitter emitter, int batchSize = 1000000, string title = null)
        {
            try 
            {
                var wasList = false;
                if (o != null)
                {
                    var oType = o.GetType();
                    Type listType;
                    if (oType.TryGetListType(out listType))
                    {
                        int listId = -1;
                        var initialEmit = true;
                        var asEnum = o as IEnumerable<object>;
                        Debug.Assert(asEnum != null, "expected list");
                        while (true) {
                            var rows = asEnum.Take(batchSize).ToList();
                            asEnum = asEnum.Skip(batchSize);
                            if (rows.Count == 0)
                            {
                                break;
                            }
                            else
                            {
                                if (initialEmit && rows.Count > 0)
                                {
                                    var first = rows.First();
                                    listId = emitter.List(first, title);
                                    emitter.ListValues(rows, listId);
                                    initialEmit = false;
                                }
                                else if (listId > -1)
                                {
                                    emitter.ListValues(rows, listId);
                                }
                            }
                        }
                        emitter.ListClose(listId);
                        wasList = true;
                    }
                }
                
                if (!wasList)
                {
                    emitter.Single(o, title);
                }
            }
            catch (Exception exn)
            {
                Logger.Error("Exception {0} while dumping, message {1}", exn.HResult, exn.Message);
            }
            return o;
        }
    }
}
