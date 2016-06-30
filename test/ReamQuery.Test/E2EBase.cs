namespace ReamQuery.Test
{
    using System.IO;
    using ReamQuery.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;
    using System.Collections.Generic;
    using System;
    using Newtonsoft.Json.Linq;

    public abstract class E2EBase
    {
        protected TestServer _server;
        protected HttpClient _client;

        protected dynamic SqlData;
        
        public E2EBase()
        {
            var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../src/ReamQuery"));
            Environment.SetEnvironmentVariable("REAMQUERY_BASEDIR", baseDir);
            
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.test.json")
                .Build()
                ;
            
            ReamQuery.Startup.Configuration = config;

            _server = new TestServer(
                new WebHostBuilder()
                    .UseConfiguration(ReamQuery.Startup.Configuration)
                    .UseStartup<ReamQuery.Startup>()
            );
            _client = _server.CreateClient();
            
            var path = Path.Combine(AppContext.BaseDirectory, "db.sql.json");
            var json = File.ReadAllText(path);
            SqlData = JArray.Parse(json);
        }

        protected static IEnumerable<object[]> Connections() 
        {
            var path = Path.Combine(AppContext.BaseDirectory, "connections.json");
            var json = File.ReadAllText(path);
            dynamic data = JObject.Parse(json);
            return new object[][]
            {
                new object[] { data.local.sqlserver2[0].ToString(), DatabaseProviderType.SqlServer },
                new object[] { data.local.npgsql[0].ToString(), DatabaseProviderType.NpgSql }
            };
        }
    }
}
