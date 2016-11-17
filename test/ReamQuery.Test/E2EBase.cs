namespace ReamQuery.Test
{
    using System.IO;
    using ReamQuery.Models;
    using ReamQuery.Core.Api;
    using ReamQuery.Helpers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;
    using System.Collections.Generic;
    using System;
    using Newtonsoft.Json;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Threading.Tasks;

    public abstract class E2EBase
    {
        protected abstract string EndpointAddress { get; }
        protected TestServer _server;
        protected HttpClient _client;
        protected WebSocketClient _wsClient;

        public E2EBase()
        {
            var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/ReamQuery"));
            Environment.SetEnvironmentVariable("REAMQUERY_BASEDIR", baseDir);
            Environment.SetEnvironmentVariable("REAMQUERY_DISTDIR", baseDir);
            
            ReamQuery.Startup.Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
            var builder = new WebHostBuilder()
                .UseConfiguration(ReamQuery.Startup.Configuration)
                .UseStartup<ReamQuery.Startup>();

            _server = new TestServer(builder);
            _client = _server.CreateClient();
            _wsClient = _server.CreateWebSocketClient();
            _wsTask = StartSocketTask();
        }

        protected IEnumerable<Message> GetMessages(int timeoutSeconds = 5)
        {
            var timeout = Task.Delay(timeoutSeconds * 1000);
            var done = Task.WaitAny(_wsTask, timeout);
            return _msgs;
        }


        List<Message> _msgs = new List<Message>();

        Task _wsTask;

        async Task StartSocketTask()
        {
            bool _closeFlag = false;
            long _expectedCount = -1;
            long _receivedCount = 0;
            var ws = await _wsClient.ConnectAsync(new System.Uri("ws://localhost/ws"), System.Threading.CancellationToken.None);
            byte[] buffer = new byte[1024 * 4];
            while(ws.State == WebSocketState.Open)
            {
                var json = await ws.ReadString();
                var msgs = JsonConvert.DeserializeObject<IEnumerable<Message>>(json);
                
                _msgs.AddRange(msgs);
                foreach(var msg in msgs)
                {
                    _receivedCount++;
                    if(msg.Type == ItemType.Close && !_closeFlag)
                    {
                        _closeFlag = true;
                        _expectedCount = (long)msg.Values[0]; 
                    }
                }
                if (_expectedCount > -1 && _closeFlag && _expectedCount == _receivedCount)
                {
                    break;
                }
            }
            return;
        }

        protected static IEnumerable<object> WorldDatabase()
        {
            var sqlServer = Environment.GetEnvironmentVariable("REAMQUERY_WORLDDB_SQLSERVER");
            if (sqlServer.StartsWith("\"")) {
                sqlServer = sqlServer.Substring(1);
            }
            if (sqlServer.EndsWith("\"")) {
                sqlServer = sqlServer.Substring(0, sqlServer.Length - 1);
            }
            var npgsql = Environment.GetEnvironmentVariable("REAMQUERY_WORLDDB_NPGSQL");
            var sqlite = SqliteConnectionString();
            var conns = new object[][]
            {
                new object[] { sqlite, DatabaseProviderType.Sqlite }
            };
            if (!string.IsNullOrWhiteSpace(sqlServer))
            {
                conns = conns.Concat(new object[][] { new object[] { sqlServer, DatabaseProviderType.SqlServer }}).ToArray();
            }
            if (!string.IsNullOrWhiteSpace(npgsql))
            {
                conns = conns.Concat(new object[][] { new object[] { npgsql, DatabaseProviderType.NpgSql }}).ToArray();
            }
            return conns;
        }

        static string SqliteConnectionString()
        {
            var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var dir = Path.Combine(Path.Combine(baseDir, "sql"), "world.sqlite");
            return string.Format("Data Source={0}", Path.GetFullPath(dir));
        }

        protected static IEnumerable<object> SqlServer_TypeTestDatabase()
        {
            var sqlServer = Environment.GetEnvironmentVariable("REAMQUERY_TYPETEST_SQLSERVER");
            if (string.IsNullOrWhiteSpace(sqlServer))
            {
                return new object[][] { }; // skips if not available
            }
            var conns = new object[][] { new object[] { sqlServer }};
            return conns;
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

        protected bool CompareValueLists(object[] expected, object[] actual)
        {
            for(var i = 0; i < expected.Length; i++)
            {
                try {
                    var x = expected[i];
                    var y = actual[i] as Newtonsoft.Json.Linq.JObject;
                    if (x is Column && y != null)
                    {
                        var c = (Column) x;
                        Xunit.Assert.Equal(c.Name, y["Name"]);
                        if (!string.IsNullOrWhiteSpace(c.Type)) {
                            Xunit.Assert.Equal(c.Type, y["Type"]);
                        }
                        continue;
                    }

                    // otherwise,
                    var val = Convert.ChangeType(actual[i], actual[i].GetType()); 
                    if (x is int) 
                    {
                        Xunit.Assert.Equal(Convert.ChangeType(x, typeof(Int64)), val);
                    }
                    else
                    {
                        Xunit.Assert.Equal(x, val);
                    }
                } 
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }
}
