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
    using Newtonsoft.Json.Linq;

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
            var msgs = GetMessages();

            var columns = msgs.Single(x => x.Type == ItemType.Header).Values.Cast<JObject>();
            var row = msgs.Single(x => x.Type == ItemType.Row);

            var expectedTypeMap = new Tuple<string, Type>[]
            {
                Tuple.Create("bigintcol", typeof(System.Int64?))
            };

            foreach(var expected in expectedTypeMap)
            {
                Assert.Single(columns, (v) => {
                    var name = v["Name"].ToString(); 
                    var type = Type.GetType(v["Type"].ToString());
                    return name == expected.Item1 && expected.Item2 == typeof(System.Int64?);
                });
            }
        }
    }
}
