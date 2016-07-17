namespace ReamQuery.Core.Helpers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using ReamQuery.Core.Api;

    public static class TypeHelpers
    {
        public static bool TryGetListType(this Type type, out Type nestedType)
        {
            Type genericTypeDef = null;
            Type listType = null;
            try 
            {
                genericTypeDef = type.GetGenericTypeDefinition();
            }
            catch(Exception) {}
            
            if (type.IsArray)
            {
                listType = type;
            }
            else if (genericTypeDef != null && GenericListTypes.Any(t => t.IsAssignableFrom(genericTypeDef)) &&
                type.GetGenericArguments().Count() == 1)
            {
                listType = type.GetGenericArguments().Single();
            }
            else
            {
                listType = null;
            }
            nestedType = listType;
            return listType != null;
        }

        public static bool TryGetTypeColumns(this Type type, int? parent, out Column[] typeColumns)
        {
            var columns = new List<Column>();
            var isList = !SingularTypes.Contains(type);
            if (isList)
            {
                foreach(var m in type.GetMembers())
                {
                    var propInfo = m as System.Reflection.PropertyInfo;
                    if (propInfo != null)
                    {
                        columns.Add(new Column
                        {
                            Name = m.Name,
                            Parent = parent,
                            Type = propInfo.GetGetMethod().ReturnType.FullName,
                            Prop = propInfo
                        });
                    }
                }
            }
            typeColumns = columns.ToArray();
            return isList;
        }

        public static string GetDisplayName(this Type type)
        {
            string displayName;
            Type mainType;
            var isList = false;
            if (type.TryGetListType(out mainType))
            {
                isList = true;
            }
            else
            {
                mainType = type;
            }

            if (mainType == typeof(string))
            {
                displayName = "string";
            }
            else if (mainType == typeof(int))
            {
                displayName = "int";
            }
            else if (mainType.Name.Contains("AnonymousType"))
            {
                displayName = "AnonymousType";
            }
            else if (mainType.FullName == "System.Object")
            {
                displayName = "object";
            }
            else
            {
                displayName = mainType.Name;
            }

            return displayName + (isList ? "[]" : "");
        }

        static Type[] GenericListTypes = new []
        {
            typeof(IEnumerable<>), typeof(IList<>), typeof(List<>),
            // todo dont depend on internals
            typeof(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable<>)
        };

        static Type[] SingularTypes = new []
        {
            typeof(Boolean),
            typeof(Char),
            typeof(SByte),
            typeof(Byte),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            
            typeof(Int16),
            typeof(Int32),
            typeof(Int64),
            typeof(UInt16),
            typeof(UInt32),
            typeof(UInt64), 
            
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(String),
            typeof(Char),
            typeof(UIntPtr),
            typeof(Guid),
        };
    }
}