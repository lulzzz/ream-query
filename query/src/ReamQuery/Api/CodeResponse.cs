namespace ReamQuery.Api
{
    using System;
    using System.Collections.Generic;
    using ReamQuery.Models;

    public class CodeResponse : ResponseBase
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } 
        public IEnumerable<CompileDiagnostics> Diagnostics { get; set; }
    }
}
