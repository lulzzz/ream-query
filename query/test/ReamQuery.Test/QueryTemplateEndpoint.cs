namespace ReamQuery.Test
{
    using System;
    using Xunit;
    using System.Linq;
    using ReamQuery.Api;
    using ReamQuery.Helpers;
    using ReamQuery.Models;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.DotNet.ProjectModel.Workspaces;
    using Microsoft.CodeAnalysis;
    using System.IO;
    using Microsoft.CodeAnalysis.Emit;

    public class QueryTemplateEndpoint : E2EBase
    {
        protected override string EndpointAddress { get { return  "/querytemplate"; } }

        [Theory, MemberData("WorldDatabase")]
        public async void Returns_Expected_Template_For_Database(string connectionString, DatabaseProviderType dbType)
        {
            var id = Guid.NewGuid();
            var request = new QueryRequest 
            {
                Id = id,
                ServerType = dbType,
                ConnectionString = connectionString,
                Text = ""
            };
            var json = JsonConvert.SerializeObject(request);

            var res = await _client.PostAsync(EndpointAddress, new StringContent(json));
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<TemplateResponse>(jsonRes);

            // insert some legal code at the offsets returned
            var userCode = "city.Take(10);";
            var modifiedTemplate = output.Template.InsertTextAt(userCode, output.LineOffset, output.ColumnOffset);

            // setup emitting the source text to check for syntax and other errors
            var syntaxTree = CSharpSyntaxTree.ParseText(modifiedTemplate);
            var projectjsonPath = ReamQuery.Startup.Configuration["REAMQUERY_BASEDIR"];
            var references = new ProjectJsonWorkspace(projectjsonPath)
                    .CurrentSolution
                    .Projects
                    .SelectMany(x => x.MetadataReferences);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix("test"))
                .WithOptions(new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary))
                .WithReferences(references)
                .AddSyntaxTrees(new SyntaxTree[] { syntaxTree });
            
            // emit
            var stream = new MemoryStream();
            var compilationResult = compilation.Emit(stream, options: new EmitOptions());
            var errors = compilationResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error);

            // check we had zero errors from all this
            Assert.Equal(0, errors.Count());

            // check the template looks like expected
            var nodes = syntaxTree.GetRoot().DescendantNodes();
            var tbls = nodes.OfType<ClassDeclarationSyntax>();
            Assert.Equal(StatusCode.Ok, output.Code);
            Assert.NotEmpty(nodes.OfType<NamespaceDeclarationSyntax>().Where(x => x.Name.ToString() == output.Namespace));
            Assert.Single(tbls.Where(tbl => tbl.Identifier.ToString() == "city"));
            Assert.Single(tbls.Where(tbl => tbl.Identifier.ToString() == "country"));
            Assert.Single(tbls.Where(tbl => tbl.Identifier.ToString() == "countrylanguage"));

            // check that the contents of the usercode method contains the snippet inserted at the returned offsets
            var mb = nodes.OfType<MethodDeclarationSyntax>()
                .Single(x => x.Identifier.ToString().StartsWith("UserCodeImpl"));

            Assert.Contains(userCode, mb.Body.ToString());
        }
    }
}
