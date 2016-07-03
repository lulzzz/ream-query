namespace ReamQuery.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ReamQuery.Shared;

    public class QueryResult 
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } 
        public IEnumerable<DumpResult> Results { get; set; }
    }
}
