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
        protected override string EndpointAddress { get { return  "/querytemplate"; } }

        [Theory, MemberData("WorldDatabase")]
        public async void Returns_Expected_Template_For_Database(string connectionString, DatabaseProviderType dbType)
        {
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Text = ""
            };
            var json = JsonConvert.SerializeObject(request);

            var res = await _client.PostAsync(EndpointAddress, new StringContent(json));
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<TemplateResponse>(jsonRes);
            var nodes = CSharpSyntaxTree.ParseText(output.Template).GetRoot().DescendantNodes();
            var tbls = nodes.OfType<ClassDeclarationSyntax>();
            Assert.Equal(StatusCode.Ok, output.Code);
            Assert.Single(tbls.Where(tbl => tbl.Identifier.ToString() == "city"));
            Assert.Single(tbls.Where(tbl => tbl.Identifier.ToString() == "country"));
            Assert.Single(tbls.Where(tbl => tbl.Identifier.ToString() == "countrylanguage"));
        }
    }
}
