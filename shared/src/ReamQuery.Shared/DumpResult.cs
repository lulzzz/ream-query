namespace ReamQuery.Shared
{
    using System;
    using System.Collections.Generic;

    public class DumpResult
    {
        public string Name { get; set; }

        public IEnumerable<Tuple<string, int>> Sets { get; set; }

        public IEnumerable<object> Values { get; set; }

        public IEnumerable<ResultColumn> Columns { get; set; }
    }
}