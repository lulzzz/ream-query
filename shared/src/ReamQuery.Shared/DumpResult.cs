namespace ReamQuery.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class DumpResult
    {
        public string Name { get; set; }

        public IEnumerable<Tuple<string, int>> Sets { get; set; }

        public IEnumerable<object> Values { get; set; }

        public IEnumerable<ResultColumn> Columns { get; set; }
    }
}