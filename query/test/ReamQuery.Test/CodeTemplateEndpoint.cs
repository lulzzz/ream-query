namespace ReamQuery.Test
{
    using System;
    using Xunit;
    using System.Linq;
    using ReamQuery.Api;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            
            Assert.Equal(StatusCode.Ok, output.Code);
            Assert.NotNull(output.Namespace);
            var nodes = CSharpSyntaxTree.ParseText(output.Template).GetRoot().DescendantNodes();
            Assert.NotEmpty(nodes.OfType<NamespaceDeclarationSyntax>().Where(x => x.Name.ToString() == output.Namespace));
        }
    }
}
