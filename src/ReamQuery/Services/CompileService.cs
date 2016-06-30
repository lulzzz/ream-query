namespace ReamQuery.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Loader;
    using Microsoft.DotNet.ProjectModel.Workspaces;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.CSharp;
    using ReamQuery.Models;

    public class CompileService
    {
        SchemaService _schemaService;
        string _projectjsonPath;
        IEnumerable<MetadataReference> _references;

        public CompileService(SchemaService schemaService) 
        {
            _schemaService = schemaService;
            _projectjsonPath = GetProjectJsonFolder();
        }

        public IEnumerable<MetadataReference> GetReferences() 
        {
            if (_references == null)
            {
                _references = new ProjectJsonWorkspace(_projectjsonPath)
                    .CurrentSolution.Projects.First().MetadataReferences;
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

            var compilerOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary);
            var trees = new SyntaxTree[] {
                CSharpSyntaxTree.ParseText(source),
            };

            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(compilerOptions)
                .WithReferences(references)
                .AddSyntaxTrees(trees);

            var stream = new MemoryStream();
            var compilationResult = compilation.Emit(stream, options: new EmitOptions());
            stream.Position = 0;
            if (!compilationResult.Success) 
            {
                foreach(var r in compilationResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error)) 
                {
                    Console.WriteLine("Error: {0}", r);
                }
            }
            var asm = LibraryLoader.LoadFromStream(stream); 
            var programType = asm.GetTypes().Single(t => t.Name == "Main");
            stream.Position = 0;

            return new CompileResult 
            {
                Type = programType,
                Assembly = asm,
                Reference = MetadataReference.CreateFromStream(stream)
            };
        }

        private static string GetProjectJsonFolder() 
        {
            var dir = System.AppContext.BaseDirectory;
            var devDir = Path.Combine(dir, "query");
            var json = Path.Combine(dir, "project.json");
            var devJson = Path.Combine(devDir, "project.json");
            if (File.Exists(json)) 
            {
                return dir;
            }
            return devDir;
        }
    }
}