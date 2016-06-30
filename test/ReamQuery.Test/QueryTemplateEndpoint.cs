namespace ReamQuery.Test
{
    using Xunit;
    using ReamQuery.Models;
    using System.Net.Http;
    using Newtonsoft.Json;

    public class QueryTemplateEndpoint : E2EBase
    {
        [Theory, MemberData("Connections")]
        [Trait("Category", "Integration")]
        public async void Querytemplate_Returns_Expected_Template_For_Database(string connectionString, DatabaseProviderType dbType)
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
    }
}
