namespace ReamQuery.Api
{
    using System;
    using System.Collections.Generic;
    using ReamQuery.Models;
    using ReamQuery.Shared;

    public class QueryResponse : BaseResponse
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } 
        public IEnumerable<DumpResult> Results { get; set; }
        public IEnumerable<CompileDiagnostics> Diagnostics { get; set; }
    }
}
