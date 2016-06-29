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
        public async void CheckReadyStatusReturnsTrue()
        {
            var res = await _client.GetStringAsync("/checkreadystatus");
            Assert.Equal("true", res);
        }
    }
}
