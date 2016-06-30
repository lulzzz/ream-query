namespace QueryEngine.Test
{
    using Xunit;
    using System.IO;
    using QueryEngine.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System;
    using Newtonsoft.Json.Linq;

    public class SchemaServiceTests
    {
        TestServer _server;
        HttpClient _client;
        
        public SchemaServiceTests()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.test.json")
                .Build()
                ;
            
            QueryEngine.Startup.Configuration = config;

            _server = new TestServer(
                new WebHostBuilder()
                    .UseConfiguration(QueryEngine.Startup.Configuration)
                    .UseStartup<QueryEngine.Startup>()
            );
            _client = _server.CreateClient();
        }

        [Theory, MemberData("Connections")]
        [Trait("Category", "Integration")]
        public async void Can_Query_SqlServer_And_Return_Expected_Template(string connectionString, DatabaseProviderType dbType)
        {
            var request = new QueryInput 
            {
                ServerType = dbType,
                ConnectionString = connectionString,
                Namespace = "ns",
                Text = ""
            };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync("/querytemplate", new StringContent(json))
                ;
            
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<TemplateResult>(jsonRes);
            Assert.Contains("public partial class Foo", output.Template);
        }

        public static IEnumerable<object[]> Connections() 
        {
            var path = Path.Combine(AppContext.BaseDirectory, "connections.json");
            var json = System.IO.File.ReadAllText(path);
            dynamic data = JObject.Parse(json);
            return new object[][]
            {
                new object[] { data.local.sqlserver[0].ToString(), DatabaseProviderType.SqlServer },
                new object[] { data.local.npgsql[0].ToString(), DatabaseProviderType.NpgSql }
            };
        }
    }
}
