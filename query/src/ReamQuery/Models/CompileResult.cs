namespace ReamQuery.Models
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.CodeAnalysis;

    public class CompileResult
    {
        public Type Type { get; set; }
        public Assembly Assembly { get; set; }
        public MetadataReference Reference { get; set; }
        public IEnumerable<CompileDiagnostics> Diagnostics { get; set; }
        public bool Success { get; set; }
    }
}
