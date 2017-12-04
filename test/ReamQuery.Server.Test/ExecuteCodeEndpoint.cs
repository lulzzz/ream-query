namespace ReamQuery.Server.Test
{
    using System;
    using System.Net.Http;
    using ReamQuery.Server.Api;
    using ReamQuery.Core.Api;
    using Newtonsoft.Json;
    using Xunit;
    using System.Linq;
    using System.Collections.Generic;

    public class ExecuteCodeEndpoint : E2EBase
    {
        protected override string EndpointAddress { get { return  "/executecode"; } }
        
        [Fact]
        public async void Handles_Endless_Queries()
        {
            var id = Guid.NewGuid();
            var code = @"

    var x = 42;
    while (true) {
        System.Threading.Thread.Sleep(1000);
        x.Dump();
    }
";
            var request = new CodeRequest { Text = code, Id = id };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json))
                ;

            // default timeout is 5 secs
            var msgs = GetMessages();
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<CodeResponse>(jsonRes);
            Assert.Equal(StatusCode.Ok, output.Code);

            Assert.True(1 < msgs.Count(), "More then 1 msg expected");
            Assert.True(msgs.Count() < 10, "Less than 10 msgs expected");
        }

        [Theory, MemberData(nameof(Execute_Code_Samples))]
        public async void Returns_Expected_Data_For_Code_Sample(Guid id, string code, Message[] expectedMsgs)
        {
            var request = new CodeRequest { Text = code, Id = id };
            var json = JsonConvert.SerializeObject(request);
            var res = await _client
                .PostAsync(EndpointAddress, new StringContent(json))
                ;
                
            var msgs = GetMessages();
            var jsonRes = await res.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<CodeResponse>(jsonRes);
            Assert.Equal(StatusCode.Ok, output.Code);
            foreach(var expected in expectedMsgs)
            {
                msgs.Single(x => {
                    return x.Id == expected.Id && x.Type == expected.Type &&
                        CompareValueLists(expected.Values, x.Values);
                });
            }
        }

        public static IEnumerable<object[]> Execute_Code_Samples()
        {
            var id1 = Guid.NewGuid();
            return new object[][]
            {
                new object[]
                {
                    id1,
                    @"
                    var x = 10;
                    x + 1
                    ",
                    new Message[]
                    {
                        new Message
                        {
                            Session = id1,
                            Type = ItemType.Single,
                            Values = new object[]
                            {
                                11
                            }
                        }
                    }    
                }
            };
        }
    }
}
