namespace ReamQuery.Test
{
    using System;
    using Xunit;
    using System.Linq;
    using ReamQuery.Models;
    using ReamQuery.Api;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class QueryTemplateEndpoint : E2EBase
    {
        [Theory, MemberData("WorldDatabase")]
        public async void Querytemplate_Returns_Expected_Template_For_Database(string connectionString, DatabaseProviderType dbType)
        {
            var ns = "ns";
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = ns,
                Text = ""
            };
            var json = JsonConvert.SerializeObject(request);

            var res = await _client.PostAsync("/querytemplate", new StringContent(json));
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<TemplateResponse>(jsonRes);
            var nodes = CSharpSyntaxTree.ParseText(output.Template).GetRoot().DescendantNodes();
            var tbls = nodes.OfType<AttributeSyntax>().Where(x => x.Name.ToString() == "Table");
            
            Assert.Single(tbls.Where(tbl => tbl.DescendantNodes().OfType<AttributeArgumentSyntax>().Single().ToString() == "\"city\""));
            Assert.Single(tbls.Where(tbl => tbl.DescendantNodes().OfType<AttributeArgumentSyntax>().Single().ToString() == "\"country\""));
            Assert.Single(tbls.Where(tbl => tbl.DescendantNodes().OfType<AttributeArgumentSyntax>().Single().ToString() == "\"countrylanguage\""));
        }
    }
}
