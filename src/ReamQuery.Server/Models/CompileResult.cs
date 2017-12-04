namespace ReamQuery.Server.Models
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ReamQuery.Server.Api;
    using Microsoft.CodeAnalysis;

    public class CompileResult : ResponseBase
    {
        public Type Type { get; set; }
        public Assembly Assembly { get; set; }
        public MetadataReference Reference { get; set; }
        public IEnumerable<CompileDiagnostics> Diagnostics { get; set; }
    }
}
