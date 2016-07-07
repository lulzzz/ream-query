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
            var ns = "ns";
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = ns,
                Text = ""
            };
            var json = JsonConvert.SerializeObject(request);

            var res = await _client.PostAsync(EndpointAddress, new StringContent(json));
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<TemplateResponse>(jsonRes);
            var nodes = CSharpSyntaxTree.ParseText(output.Template).GetRoot().DescendantNodes();
            var tbls = nodes.OfType<AttributeSyntax>().Where(x => x.Name.ToString() == "Table");
            
            Assert.Single(tbls.Where(tbl => tbl.DescendantNodes().OfType<AttributeArgumentSyntax>().Single().ToString() == "\"city\""));
            Assert.Single(tbls.Where(tbl => tbl.DescendantNodes().OfType<AttributeArgumentSyntax>().Single().ToString() == "\"country\""));
            Assert.Single(tbls.Where(tbl => tbl.DescendantNodes().OfType<AttributeArgumentSyntax>().Single().ToString() == "\"countrylanguage\""));
        }

        [Theory, MemberData("WorldDatabaseWithInvalidNamespaceIdentifiers")]
        public async void Returns_Expected_Error_For_Invalid_Namespace(string connectionString, DatabaseProviderType dbType, string nsName)
        {
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = nsName,
                Text = ""
            };
            var json = JsonConvert.SerializeObject(request);

            var res = await _client.PostAsync(EndpointAddress, new StringContent(json));

            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<TemplateResponse>(jsonRes);
            Assert.Equal(Api.StatusCode.NamespaceIdentifier, output.Code);
        }
    }
}
