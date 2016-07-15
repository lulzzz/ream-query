namespace ReamQuery.Test
{
    using System;
    using Xunit;
    using System.Linq;
    using ReamQuery.Api;
    using ReamQuery.Models;
    using ReamQuery.Shared;
    using System.Net.Http;
    using Newtonsoft.Json;

    public class ExecuteQueryEndpointForSqlServer : E2EBase
    {
        protected override string EndpointAddress { get { return  "/executequery"; } }

        [Theory, MemberData("SqlServer_TypeTestDatabase")]
        public async void Handles_All_Types(string connectionString)
        {

            var ns = "ns";
            var request = new QueryRequest 
            {
                ServerType = DatabaseProviderType.SqlServer,
                ConnectionString = connectionString,
                Namespace = ns,
                Text = "TypeTestTable.Take(10)"
            };
            var json = JsonConvert.SerializeObject(request);

            var res = await _client.PostAsync(EndpointAddress, new StringContent(json));
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<QueryResponse>(jsonRes);
            var msgs = await GetMessagesAsync();

            var header = msgs.Single(x => x.Type == ItemType.Header);
            var row = msgs.Single(x => x.Type == ItemType.Row);
             
            Console.WriteLine(JsonConvert.SerializeObject(header));
        }
    }
}
