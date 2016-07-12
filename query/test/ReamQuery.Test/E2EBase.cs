namespace ReamQuery.Test
{
    using System.IO;
    using ReamQuery.Models;
    using ReamQuery.Shared;
    using ReamQuery.Helpers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;
    using System.Collections.Generic;
    using System;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Threading.Tasks;

    public abstract class E2EBase
    {
        protected TestServer _server;
        protected HttpClient _client;
        protected WebSocketClient _wsClient;

        protected dynamic SqlData;
        
        public E2EBase()
        {
            var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../query/src/ReamQuery"));
            Environment.SetEnvironmentVariable("REAMQUERY_BASEDIR", baseDir);
            
            var config = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build()
                ;
            
            ReamQuery.Startup.Configuration = config;

            _server = new TestServer(
                new WebHostBuilder()
                    .UseConfiguration(ReamQuery.Startup.Configuration)
                    .UseStartup<ReamQuery.Startup>()
            );
            _client = _server.CreateClient();
            _wsClient = _server.CreateWebSocketClient();
            
            var path = Path.Combine(AppContext.BaseDirectory, "db.sql.json");
            var json = File.ReadAllText(path);
            SqlData = JArray.Parse(json);
        }

        protected async Task<IEnumerable<Message>> GetMessagesAsync()
        {
            bool _closeFlag = false;
            long _expectedCount = -1;
            long _receivedCount = 0;
            var list = new List<Message>();
            var ws = await _wsClient.ConnectAsync(new System.Uri("ws://localhost/ws"), System.Threading.CancellationToken.None);
            byte[] buffer = new byte[1024 * 4];
            while(ws.State == WebSocketState.Open)
            {
                var json = await ws.ReadString();
                var msg = JsonConvert.DeserializeObject<Message>(json);
                list.Add(msg);
                _receivedCount++;
                if(msg.Type == ItemType.Close && !_closeFlag)
                {
                    _closeFlag = true;
                    _expectedCount = (long)msg.Values[0]; 
                }
                if (_expectedCount > -1 && _closeFlag && _expectedCount == _receivedCount)
                {
                    break;
                }
            }
            return list;
        }

        protected static IEnumerable<object> WorldDatabase()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "connections.json");
            var json = File.ReadAllText(path);
            dynamic data = JObject.Parse(json);
            if (IsTravisCi())
            {
                return new object[][]
                {
                    new object[] { data.travis.npgsql[0].ToString(), DatabaseProviderType.NpgSql },
                };
            }
            else if (IsAppveyorCi())
            {
                return new object[][]
                {
                    new object[] { data.appveyor.sqlserver[0].ToString(), DatabaseProviderType.SqlServer },
                };
            }
            else
            {
                return new object[][]
                {
                    new object[] { data.local.sqlserver2[0].ToString(), DatabaseProviderType.SqlServer },
                    new object[] { data.local.npgsql[0].ToString(), DatabaseProviderType.NpgSql },
                };
            }
        }

        protected static IEnumerable<object> WorldDatabaseWithInvalidNamespaceIdentifiers()
        {
            var dbs = WorldDatabase();
            var invalidNamespaces = new object[]
            {
                "", "foo bar", "?@#¤"
            }.SelectMany(x => 
                dbs.Select(db => ((IEnumerable<object>)db)
                    .Concat(new object[] { x }).ToArray()).ToArray()).ToArray(); // mo arrays
            return invalidNamespaces;
        }

        protected static IEnumerable<object> InvalidConnectionStrings()
        {
            var sqlServerConn1 = @"Data Source=noservershouldbenamedthis; User Id=sa; Password=p; Initial Catalog=db";
            var npgsqlConn1 = @"Server=noservershouldbenamedthis; User Id=sa; Password=p; Database=db";
            var randomStuff = "54¤#e54&¤/7wu peiur0*-ø92?´3=ur932trxy|3";
            return new object[][]
            {
                new object[] { sqlServerConn1, DatabaseProviderType.SqlServer, Api.StatusCode.ServerUnreachable },
                new object[] { sqlServerConn1, DatabaseProviderType.NpgSql, Api.StatusCode.ConnectionStringSyntax },
                new object[] { npgsqlConn1, DatabaseProviderType.NpgSql, Api.StatusCode.ServerUnreachable },
                new object[] { randomStuff, DatabaseProviderType.SqlServer, Api.StatusCode.ConnectionStringSyntax },
                new object[] { randomStuff, DatabaseProviderType.NpgSql, Api.StatusCode.ConnectionStringSyntax },
            };
        }

        static bool IsTravisCi()
        {
            return Environment.GetEnvironmentVariable("TRAVIS") == "true";
        }

        static bool IsAppveyorCi()
        {
            return Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        }

        protected abstract string EndpointAddress { get; }
    }
}
