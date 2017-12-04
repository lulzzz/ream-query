namespace ReamQuery.Server.Test
{
    using System;
    using Xunit;
    using System.Linq;
    using ReamQuery.Server.Api;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis;
    using ReamQuery.Server.Helpers;
    using System.IO;
    using Microsoft.CodeAnalysis.Emit;
    using Services;

    public class CodeTemplateEndpoint : E2EBase
    {
        protected override string EndpointAddress { get { return  "/codetemplate"; } }

        [Fact]
        public async void Returns_Expected_Template_For_Code_Sample()
        {
            var request = new CodeRequest { Id = Guid.NewGuid(), Text = "" };
            var json = JsonConvert.SerializeObject(request);

            var res = await _client.PostAsync(EndpointAddress, new StringContent(json));
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<TemplateResponse>(jsonRes);

            // insert some legal code at the offsets returned
            var userCode = "var x = 10;";
            var modifiedTemplate = output.Template.InsertTextAt(userCode, output.LineOffset, output.ColumnOffset);

            // setup emitting the source text to check for syntax and other errors
            var syntaxTree = CSharpSyntaxTree.ParseText(modifiedTemplate);
            var references = new CompileService(null).GetReferences();

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToIdentifierWithPrefix("test"))
                .WithOptions(new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary))
                .WithReferences(references)
                .AddSyntaxTrees(new SyntaxTree[] { syntaxTree });
            
            // emit
            var stream = new MemoryStream();
            var compilationResult = compilation.Emit(stream, options: new EmitOptions());
            var errors = compilationResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error);
            foreach(var err in errors)
            {
                Console.WriteLine("err: {0}", err.GetMessage());
            }
            // check we had zero errors from all this
            Assert.Empty(errors);

            // check the template looks like expected
            Assert.Equal(StatusCode.Ok, output.Code);
            Assert.NotNull(output.Namespace);
            var nodes = syntaxTree.GetRoot().DescendantNodes();
            Assert.NotEmpty(nodes.OfType<NamespaceDeclarationSyntax>().Where(x => x.Name.ToString() == output.Namespace));

            // check that the contents of the usercode method contains the snippet inserted at the returned offsets
            var mb = nodes.OfType<MethodDeclarationSyntax>()
                .Single(x => x.Identifier.ToString().StartsWith("UserCodeImpl"));

            Assert.Contains(userCode, mb.Body.ToString());
        }

        [Fact]
        public async void Template_Contains_User_Text()
        {
            var request = new CodeRequest { Id = Guid.NewGuid(), Text = "hej mor" };
            var json = JsonConvert.SerializeObject(request);

            var res = await _client.PostAsync(EndpointAddress, new StringContent(json));
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<TemplateResponse>(jsonRes);

            Assert.Contains(request.Text, output.Template);
        }
    }
}
