
namespace ReamQuery.Server.Models
{
    using Microsoft.CodeAnalysis;
    
    public class CompileDiagnostics
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; }
        public DiagnosticSeverity Severity { get; set; }
    }
}
