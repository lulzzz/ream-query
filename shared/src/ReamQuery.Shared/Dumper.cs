namespace ReamQuery.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class Dumper
    {
        public const string RawValueColumnName = "<RawValue>";

        /// <summary>ReamQuery.Inlined.Dumper</summary>
        public static T Dump<T>(this T o, Guid queryId)
        {
            var drain = DrainContainer.GetDrain(queryId);
            // prefer runtime type (to detect anonymous types)
            var asEnum = o as IEnumerable<object>;
            var targetType = GetTargetType<T>(o);
            PropertyInfo[] propertyInfos;
            // will fail if the list contains mixed types
            DumpTypeInfo(drain, targetType, out propertyInfos);
            if (asEnum != null)
            {
                var memberList = new List<PropertyInfo>();
                foreach(var val in asEnum)
                {
                    var type = val.GetType();
                    var list = new List<object>();
                    foreach(var prop in propertyInfos)
                    {
                        var objVal = prop.GetValue(val) as object;
                        list.Add(objVal);
                    }
                    drain.EmitValue(list.ToArray());
                }
            }
            else if (o != null)
            {
                drain.EmitValue(o);
            }

            return o;
        }

        static Type[] GenericListTypes = new []
        {
            typeof(IEnumerable<>), typeof(IList<>), typeof(List<>),
            // todo dont depend on internals
            typeof(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable<>)
        };

        static Type GetTargetType<T>(T o)
        {
            Type targetType = null;
            var startType = o == null ? typeof(T) : o.GetType();
            Type genericTypeDef = null;
            try 
            {
                genericTypeDef = startType.GetGenericTypeDefinition();
            }
            catch(Exception) {}

            if (startType.IsArray)
            {
                targetType = startType.GetElementType();
            }
            else if (genericTypeDef != null && GenericListTypes.Any(t => t.IsAssignableFrom(genericTypeDef)) && 
                startType.GetGenericArguments().Count() == 1)
            {
                targetType = startType.GetGenericArguments().Single();
            }
            else
            {
                targetType = startType;
            }
            return targetType;
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
                        drain.EmitColumn(new ResultColumn
                        {
                            SetId = 0,
                            Name = m.Name,
                            Type = TypeDisplayName(propInfo.GetGetMethod().ReturnType)
                        });
                    }
                }
            }
            else
            {
                drain.EmitColumn(new ResultColumn
                {
                    SetId = 0,
                    Name = RawValueColumnName,
                    Type = TypeDisplayName(type)
                });
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
            else if (type.FullName == "System.Object")
            {
                return "object";
            }
            else
            {
                return type.Name;
            }
        }
    }
}
