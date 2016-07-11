namespace ReamQuery.Test
{
    using Xunit;
    using System.Linq;
    using ReamQuery.Models;
    using ReamQuery.Api;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.CodeAnalysis;
    using Shared;

    public class ExecuteQueryEndpoint : E2EBase
    {
        protected override string EndpointAddress { get { return  "/executequery"; } }

        [Theory, MemberData("WorldDatabase")]
        public async void Returns_Expected_Data_For_Database(string connectionString, DatabaseProviderType dbType)
        {
            var msgs = GetMessagesAsync();
            
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = "ns",
                Text = "City.Take(10)"
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);
            Assert.Equal(StatusCode.Ok, output.Code);
            await msgs;
            Assert.Equal(10, msgs.Result.Count(x => x.Type == ItemType.Row));
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
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);

            Assert.Equal(StatusCode.CompilationError, output.Code);
            Assert.Equal(2, output.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).First().Line);
            Assert.Equal(4, output.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).First().Column);
        }

        [Theory, MemberData("WorldDatabase")]
        public async void Executes_Linq_Style_Statements(string connectionString, DatabaseProviderType dbType)
        {
            var msgs = GetMessagesAsync();

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
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);
            Assert.Equal(StatusCode.Ok, output.Code);
            await msgs;
            var rows = msgs.Result.Where(msg => msg.Type == ItemType.Row);
            Assert.All(rows, (val) => val.Values[1].ToString().StartsWith("Ca"));
        }

        [Theory, MemberData("InvalidConnectionStrings")]
        public async void Returns_Expected_StatusCode_For_Invalid_ConnectionString(string connectionString, DatabaseProviderType dbType, Api.StatusCode expectedCode)
        {
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = "ns",
                Text = ""
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);

            Assert.Equal(expectedCode, output.Code);
        }
    }
}
