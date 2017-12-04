namespace ReamQuery.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.CSharp;
    using ReamQuery.Models;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using NLog;
    using Core.Helpers;
    using System.Reflection;

    public class CompileService
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        SchemaService _schemaService;
        IEnumerable<MetadataReference> _references;

        public CompileService(SchemaService schemaService) 
        {
            _schemaService = schemaService;
        }

        public IEnumerable<MetadataReference> GetReferences() 
        {
            if (_references == null)
            {
            
            }
            return null;
        }

        public static IEnumerable<MetadataReference> GetReamQueryReferences()
        {
            var prefix = string.Format("file://{0}", PlatformHelper.IsWindows ? "/" : "");
            var coreAsm = typeof(Core.Dumper).GetTypeInfo().Assembly; 
            var thisAsm = typeof(Program).GetTypeInfo().Assembly;
            var corePath = Path.GetFullPath(coreAsm.CodeBase.Substring(prefix.Length));
            var thisPath = Path.GetFullPath(thisAsm.CodeBase.Substring(prefix.Length));
            return new []
            {
                MetadataReference.CreateFromFile(corePath),
                MetadataReference.CreateFromFile(thisPath)
            };
        }
        
        public CompileResult LoadType(string source, string assemblyName, MetadataReference context = null)
        {
            var references = GetReferences();
            if (context != null)
            {
                references = references.Concat(new MetadataReference[] { context });
            }
            var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary);
            var trees = new SyntaxTree[] {
                CSharpSyntaxTree.ParseText(source),
            };

            LinePosition textOffset = LinePosition.Zero;
            if (context != null)
            {
                textOffset = trees.Single().GetRoot()
                    .DescendantNodes()
                    .OfType<NamespaceDeclarationSyntax>()
                    .Where(x => x.Name.ToString() == assemblyName)
                    .Last()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Last(x => x.Identifier.ToString() == "Main")
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Single(x => x.Identifier.ToString().StartsWith("UserCodeImpl"))
                    .Body
                    .OpenBraceToken
                    .GetLocation()
                    .GetLineSpan()
                    .EndLinePosition
                    ;
            }
            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(compilerOptions)
                .WithReferences(references)
                .AddSyntaxTrees(trees);

            var compileResult = new CompileResult();
            var stream = new MemoryStream();
            var compilationResult = compilation.Emit(stream, options: new EmitOptions());
            stream.Position = 0;
            compileResult.Code = compilationResult.Success ? Api.StatusCode.Ok : Api.StatusCode.CompilationError;
            compileResult.Diagnostics = GetDiagnostics(compilationResult, textOffset.Line);

            if (compilationResult.Success) 
            {
                var asm = LibraryLoader.LoadFromStream(stream); 
                var programType = asm.GetTypes().Single(t => t.Name == "Main");
                stream.Position = 0;
                compileResult.Type = programType;
                compileResult.Assembly = asm;
                compileResult.Reference = MetadataReference.CreateFromStream(stream);
            }
            foreach(var diag in compilationResult.Diagnostics)
            {
                if (diag.Severity == DiagnosticSeverity.Error)
                {
                    Logger.Info("Diagnostic: {0}", diag.ToString());
                }
            }
            return compileResult;
        }

        IEnumerable<CompileDiagnostics> GetDiagnostics(EmitResult result, int lineOffset)
        {
            return result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => {
                var start = x.Location.GetMappedLineSpan().StartLinePosition;
                return new CompileDiagnostics
                {
                    Line = start.Line - lineOffset,
                    Column = start.Character,
                    Message = x.GetMessage(),
                    Severity = x.Severity
                };
            });
        }
    }
}
