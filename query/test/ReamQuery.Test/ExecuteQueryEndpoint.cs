namespace ReamQuery.Test
{
    using System;
    using Xunit;
    using System.Linq;
    using ReamQuery.Models;
    using ReamQuery.Api;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Diagnostics;

    public class ExecuteQueryEndpoint : E2EBase
    {
        [Theory, MemberData("WorldDatabase")]
        public async void Returns_Expected_Data_For_Database(string connectionString, DatabaseProviderType dbType)
        {
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = "ns",
                Text = "City.Take(10)"
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync("/executequery", new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);
            Assert.Equal(10, output.Results.Single().Values.Count());
        }

        [Theory, MemberData("WorldDatabase")]
        public async void Handles_Malformed_Source_Code_In_Request(string connectionString, DatabaseProviderType dbType)
        {
            var queryStr = @"

    NOT_DEFINED";
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = "ns",
                Text = queryStr
            };
            var json = JsonConvert.SerializeObject(request);
            
            var res = await _client
                .PostAsync("/executequery", new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);

            Assert.Equal(2, output.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).First().Line);
            Assert.Equal(4, output.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).First().Column);
            Assert.Null(output.Results);
        }

        [Theory, MemberData("WorldDatabase")]
        public async void Executes_Linq_Style_Statements(string connectionString, DatabaseProviderType dbType)
        {
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = "ns",
                Text = @"
from c in City 
where c.Name.StartsWith(""Ca"") 
select c
"
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync("/executequery", new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);
            Assert.All(output.Results.Single().Values, (val) => val.ToString().StartsWith("Ca"));
        }
    }
}
