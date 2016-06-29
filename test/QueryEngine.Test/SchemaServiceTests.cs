namespace QueryEngine.Test
{
    using Xunit;
    using QueryEngine.Services;
    using QueryEngine.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;
    using System;
    using Newtonsoft.Json;
    using System.Threading.Tasks;

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

        [Fact]
        public async void Can_Query_SqlServer_And_Return_Expected_Template()
        {
            var request = new QueryInput 
            {
                ServerType = DatabaseProviderType.SqlServer,
                ConnectionString = @"Data Source=.\sqlexpress; Integrated Security=True; Initial Catalog=testdb",
                Namespace = "foo",
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
    }
}
