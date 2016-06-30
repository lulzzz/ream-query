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

    public static class Dumper
    {
        public static IDictionary<string, DumpType> _results;
        public static IDictionary<string, int> _counts;
        public static int _anonynousCount;

        /// <summary>ReamQuery.Inlined.Dumper</summary>
        public static T Dump<T>(this T o)
        {
            if (o != null)
            {
                var objType = o.GetType();
                var name = objType.Name;
                if (o is IEnumerable<object>) {
                    name = objType.GetTypeInfo().GenericTypeArguments[0].Name; 
                }
                _results.Add(FormatNameKey(name), Tuple.Create(TypeColumns((object)o), (object)o));
            }
            return o;
        }
        
        static IEnumerable<Tuple<string, string>> TypeColumns(object o) 
        {
            var t = o.GetType();
            if (o is IEnumerable<object>) 
            {
                t = t.GetTypeInfo().GenericTypeArguments[0];
            }
            var list = new List<Tuple<string, string>>();
            foreach(var m in t.GetMembers())
            {
                var propInfo = m as System.Reflection.PropertyInfo;
                if (propInfo != null)
                {
                    list.Add(Tuple.Create(m.Name, propInfo.GetGetMethod().ReturnType.Name));
                }
            }
            return list;
        }
        
        static string FormatNameKey(string name) 
        {
            var isAnon = name.Contains("AnonymousType");
            var count = -1;
            if (isAnon)
            {
                count = ++_anonynousCount;
            }
            else 
            {
                if (!_counts.ContainsKey(name))
                {
                    _counts.Add(name, 0);
                }
                count = _counts[name] + 1;
                _counts[name] = count;
            }
            var typeName = isAnon ? "AnonymousType" : name;
            var result =  string.Format("{0} {1}", typeName, count);
            return result;
        }
    }
}