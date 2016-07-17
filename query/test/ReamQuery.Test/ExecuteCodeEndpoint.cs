namespace ReamQuery.Test
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using ReamQuery.Api;
    using ReamQuery.Models;
    using ReamQuery.Shared;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Microsoft.CodeAnalysis;
    using Xunit;

    public class ExecuteCodeEndpoint : E2EBase
    {
        protected override string EndpointAddress { get { return  "/executecode"; } }

        [Theory, MemberData("Execute_Code_Samples")]
        public async void Returns_Expected_Data_For_Code_Sample(string code, Message[] expectedMsgs)
        {
            var request = new CodeRequest { Text = code, Id = Guid.NewGuid() };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
                
            var msgs = GetMessages();
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<CodeResponse>(jsonRes);
            Assert.Equal(StatusCode.Ok, output.Code);
            // todo assert msgs
        }
    }
}
