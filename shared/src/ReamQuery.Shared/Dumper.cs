namespace ReamQuery.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using DumpType = System.Tuple<System.Collections.Generic.IEnumerable<System.Tuple<string, string>>, object>;
    using Newtonsoft.Json;

    public static class Dumper
    {
        public const string RawValueColumnName = "<RawValue>";

        /// <summary>ReamQuery.Inlined.Dumper</summary>
        public static T Dump<T>(this T o, Guid queryId)
        {
            var drain = DrainContainer.GetDrain(queryId);
            // prefer runtime type (to detect anonymous types)
            var asEnum = o as IEnumerable<object>;
            var instanceList = asEnum != null && asEnum.Count() > 0;
            var genericList = asEnum != null && asEnum.GetType().GenericTypeArguments.Count() > 0;

            var targetType = instanceList ? asEnum.First().GetType() :
                genericList ? asEnum.GetType().GenericTypeArguments.First() :
                o != null ? o.GetType() : typeof(T);
            PropertyInfo[] propertyInfos;
            // will fail if the list contains mixed types
            DumpTypeInfo(drain, targetType, out propertyInfos);
            if (asEnum != null)
            {
                var memberList = new List<PropertyInfo>();
                foreach(var val in asEnum)
                {
                    var list = new List<object>();
                    foreach(var prop in propertyInfos)
                    {
                        list.Add(prop.GetValue(val));
                    }
                    drain.EmitValue(list.ToArray());
                }
            }
            else
            {
                drain.EmitValue(o);
            }
            return o;
        }

        static void DumpTypeInfo(Drain drain, Type type, out PropertyInfo[] propertyInfos)
        {
            var props = new List<PropertyInfo>();
            drain.Start(TypeDisplayName(type));
            if (!IsSingular(type))
            {
                foreach(var m in type.GetMembers())
                {
                    var propInfo = m as System.Reflection.PropertyInfo;
                    if (propInfo != null)
                    {
                        props.Add(propInfo);
                        drain.EmitColumn(Tuple.Create(m.Name, TypeDisplayName(propInfo.GetGetMethod().ReturnType)));
                    }
                }
            }
            else
            {
                drain.EmitColumn(Tuple.Create(RawValueColumnName, TypeDisplayName(type)));
            }
            propertyInfos = props.ToArray();
        }


        

        static bool IsSingular(Type type)
        {
            return type == typeof(int) || 
                type == typeof(string);
        }

        static string TypeDisplayName(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(int))
            {
                return "int";
            }
            else if (type.Name.Contains("AnonymousType"))
            {
                return "AnonymousType";
            }
            else
            {
                return type.GetTypeInfo().Name;
            }
        }
    }
}
