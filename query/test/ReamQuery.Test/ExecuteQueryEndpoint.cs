namespace ReamQuery.Test
{
    using System;
    using Xunit;
    using System.Linq;
    using ReamQuery.Models;
    using ReamQuery.Api;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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
        public async void Handles_Multiple_Expressions(string connectionString, DatabaseProviderType dbType)
        {
            var msgs = GetMessagesAsync();
            
            var request = new QueryRequest 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = "ns",
                Text = @"
                    City.Take(10)
                    Countrylanguage.Take(10)"
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);
            await msgs;
            
            Assert.Equal(StatusCode.Ok, output.Code);
            Assert.Equal(2, msgs.Result.Count(x => x.Type == ItemType.Header));
            Assert.Equal(20, msgs.Result.Count(x => x.Type == ItemType.Row));
            var cols = msgs.Result.Where(x => x.Type == ItemType.Header).SelectMany(x => x.Values).Cast<JObject>();
            var cityColumns = cols.Where(x => x["Parent"].ToString() == "1").Select(x => x["Name"].ToString());
            var langColumns = cols.Where(x => x["Parent"].ToString() == "2").Select(x => x["Name"].ToString());
            Assert.Equal(new []
            {
                "Id", "Name", "CountryCode", "District", "Population",
            }, cityColumns);
            Assert.Equal(new []
            {
                "CountryCode", "Language", "IsOfficial", "Percentage",
            }, langColumns);
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
            Assert.NotEmpty(rows);
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
