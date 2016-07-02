namespace ReamQuery.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DumpType = System.Tuple<System.Collections.Generic.IEnumerable<System.Tuple<string, string>>, object>;

    public class DumpResult
    {
        public string Name { get; set; }

        public IEnumerable<string> Sets { get; set; }

        public IEnumerable<object> Values { get; set; }

        public IEnumerable<Tuple<string, string>> Columns { get; set; }
    }
}