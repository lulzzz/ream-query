namespace ReamQuery.Server.Api
{
    using System;
    using System.Collections.Generic;
    using ReamQuery.Server.Models;

    public class QueryResponse : ResponseBase
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } 
        public IEnumerable<CompileDiagnostics> Diagnostics { get; set; }
    }
}
