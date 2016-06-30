namespace ReamQuery.Test
{
    using System;
    using Xunit;
    using System.Linq;
    using ReamQuery.Models;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    public class QueryTemplateEndpoint : E2EBase
    {
        [Theory, MemberData("Connections")]
        [Trait("Category", "Integration")]
        public async void Querytemplate_Returns_Expected_Template_For_Database(string connectionString, DatabaseProviderType dbType)
        {
            var ns = "ns";
            var request = new QueryInput 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = ns,
                Text = ""
            };
            var json = JsonConvert.SerializeObject(request);

            var res = await _client.PostAsync("/querytemplate", new StringContent(json));
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<TemplateResult>(jsonRes);
            var nodes = CSharpSyntaxTree.ParseText(output.Template).GetRoot().DescendantNodes();
            
            Assert.Single(nodes.OfType<ClassDeclarationSyntax>(), cls => {
                return cls.Identifier.ToString() == "Foo";
            });
        }
    }
}
