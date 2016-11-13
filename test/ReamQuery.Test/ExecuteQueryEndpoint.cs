namespace ReamQuery.Test
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using ReamQuery.Api;
    using ReamQuery.Models;
    using ReamQuery.Core.Api;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Microsoft.CodeAnalysis;
    using Xunit;

    public class ExecuteQueryEndpoint : E2EBase
    {
        protected override string EndpointAddress { get { return  "/executequery"; } }

        [Theory, MemberData("WorldDatabase")]
        public async void Returns_Expected_Data_For_Database(string connectionString, DatabaseProviderType dbType)
        {
            var id = Guid.NewGuid();
            var request = new QueryRequest 
            {
                Id = id,
                ServerType = dbType,
                ConnectionString = connectionString,
                Text = "city.Take(10)"
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
                
            var msgs = GetMessages();
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);
            Assert.Equal(StatusCode.Ok, output.Code);
            Assert.Equal(10, msgs.Count(x => x.Type == ItemType.Row));
        }

        [Theory, MemberData("WorldDatabase")]
        public async void Handles_Multiple_Expressions(string connectionString, DatabaseProviderType dbType)
        {
            var id = Guid.NewGuid();
            var request = new QueryRequest 
            {
                Id = id,
                ServerType = dbType,
                ConnectionString = connectionString,
                Text = @"
                    city.Take(10)
                    countrylanguage.Take(10)"
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);
            var msgs = GetMessages();
            
            Assert.Equal(StatusCode.Ok, output.Code);
            Assert.Equal(2, msgs.Count(x => x.Type == ItemType.Header));
            Assert.Equal(20, msgs.Count(x => x.Type == ItemType.Row));
            var cols = msgs.Where(x => x.Type == ItemType.Header);
            // the types might vary, so dont check those
            var cityColumns = new object[]{ 
                new Column { Name = "Id" },
                new Column { Name = "Name" },
                new Column { Name = "CountryCode" },
                new Column { Name = "District" },
                new Column { Name = "Population" }
            };
            cols.Single(x => {
                return CompareValueLists(cityColumns, x.Values);
            });
            var langColumns = new object[]{ 
                new Column { Name = "CountryCode" },
                new Column { Name = "Language" },
                new Column { Name = "IsOfficial" },
                new Column { Name = "Percentage" }
            };            
            cols.Single(x => {
                return CompareValueLists(langColumns, x.Values);
            });
        }

        [Theory, MemberData("WorldDatabase")]
        public async void Handles_Malformed_Source_Code_In_Request(string connectionString, DatabaseProviderType dbType)
        {
            var id = Guid.NewGuid();
            var queryStr = @"

    NOT_DEFINED";
            var request = new QueryRequest 
            {
                Id = id,
                ServerType = dbType,
                ConnectionString = connectionString,
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
            var id = Guid.NewGuid();
            var request = new QueryRequest 
            {
                Id = id,
                ServerType = dbType,
                ConnectionString = connectionString,
                Text = @"
from c in city 
where c.Name.StartsWith(""Ca"") 
select c
"
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json));
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);
            Assert.Equal(StatusCode.Ok, output.Code);
            var msgs = GetMessages();
            var rows = msgs.Where(msg => msg.Type == ItemType.Row);
            Assert.NotEmpty(rows);
            Assert.All(rows, (val) => val.Values[1].ToString().StartsWith("Ca"));
        }

        [Theory, MemberData("InvalidConnectionStrings")]
        public async void Returns_Expected_StatusCode_For_Invalid_ConnectionString(string connectionString, DatabaseProviderType dbType, Api.StatusCode expectedCode)
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
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);

            Assert.Equal(expectedCode, output.Code);
        }
    }
}