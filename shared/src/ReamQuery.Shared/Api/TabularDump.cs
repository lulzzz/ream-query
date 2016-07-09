namespace ReamQuery.Shared
{
    using System.Collections.Generic;

    public class TabularDump : DumpResult
    {
        public IEnumerable<Row> Rows { get; set; }

        public IEnumerable<Column> Headers { get; set; }
    }
}
