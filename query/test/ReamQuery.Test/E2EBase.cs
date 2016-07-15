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
            var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../query/src/ReamQuery"));
            Environment.SetEnvironmentVariable("REAMQUERY_BASEDIR", baseDir);
            
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

        protected async Task<IEnumerable<Message>> GetMessagesAsync()
        {
            var timeout = Task.Delay(5000);
            var done = Task.WaitAny(_wsTask, timeout);
            if (done == 0)
            {
                return _wsTask.Result;
            }
            return new Message[] {};
        }

        Task<IEnumerable<Message>> _wsTask;

        async Task<IEnumerable<Message>> StartSocketTask()
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
            var sqlServer = Environment.GetEnvironmentVariable("REAMQUERY_WORLDDB_SQLSERVER");
            var npgsql = Environment.GetEnvironmentVariable("REAMQUERY_WORLDDB_NPGSQL");
            var conns = new object[][] { };
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

        protected static IEnumerable<object> SqlServer_TypeTestDatabase()
        {
            var sqlServer = Environment.GetEnvironmentVariable("REAMQUERY_TYPETEST_SQLSERVER");
            if (string.IsNullOrWhiteSpace(sqlServer))
            {
                throw new InvalidOperationException("REAMQUERY_TYPETEST_SQLSERVER was not found");
            }
            var conns = new object[][] { new object[] { sqlServer }};
            return conns;
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
    }
}
