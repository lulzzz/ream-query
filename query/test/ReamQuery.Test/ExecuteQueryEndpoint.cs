namespace ReamQuery.Test
{
    using Xunit;
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using ReamQuery.Models;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.Extensions.PlatformAbstractions;

    public class ExecuteQueryEndpoint : E2EBase
    {
        [Theory, MemberData("Connections")]
        [Trait("Category", "Integration")]
        public async void Returns_Expected_Data_For_Database(string connectionString, DatabaseProviderType dbType)
        {
            var request = new QueryInput 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = "ns",
                Text = "Foo.Take(10)"
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync("/executequery", new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResult>(jsonRes);
            Assert.Equal(10, output.Results.Single().Values.Count());
        }
    }
}
