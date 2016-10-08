namespace ReamQuery.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using Microsoft.DotNet.ProjectModel.Workspaces;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.CSharp;
    using ReamQuery.Models;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using NLog;

    public class CompileService
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        SchemaService _schemaService;
        string _projectjsonPath;
        IEnumerable<MetadataReference> _references;

        public CompileService(SchemaService schemaService) 
        {
            _schemaService = schemaService;
            _projectjsonPath = Startup.Configuration["REAMQUERY_BASEDIR"];
        }

        public IEnumerable<MetadataReference> GetReferences() 
        {
            if (_references == null)
            {
                var projs = new ProjectJsonWorkspace(_projectjsonPath)
                    .CurrentSolution.Projects;
                _references = projs.SelectMany(p => p.MetadataReferences);
            }
            return _references;
        }
        
        public CompileResult LoadType(string source, string assemblyName, MetadataReference context = null)
        {
            var references = GetReferences();
            if (context != null)
            {
                references = references.Concat(new MetadataReference[] { context });
            }
            foreach(var dllRef in references) {
                Logger.Debug("MetadataReference {0}/{1}", dllRef.Display, dllRef.Properties.Kind);
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
                Logger.Info("Diagnostic: {0}", diag.ToString());
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
