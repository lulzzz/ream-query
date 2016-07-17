namespace ReamQuery.Test
{
    using System;
    using System.Net.Http;
    using ReamQuery.Api;
    using ReamQuery.Shared;
    using Newtonsoft.Json;
    using Xunit;
    using System.Collections.Generic;

    public class ExecuteCodeEndpoint : E2EBase
    {
        protected override string EndpointAddress { get { return  "/executecode"; } }

        [Theory, MemberData("Execute_Code_Samples")]
        public async void Returns_Expected_Data_For_Code_Sample(Guid id, string code, string[] expectedMsgs)
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
            // todo assert msgs
            foreach(var expected in expectedMsgs)
            {
                Assert.Single(msgs, expected);
            }
        }

        protected static IEnumerable<object> Execute_Code_Samples()
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
                    new string[]
                    {
                        JsonConvert.SerializeObject(new Message
                        {
                            Session = id1,
                            Type = ItemType.SingleAtomic,
                            Values = new object[]
                            {
                                new Column { Name = "int", Type = "System.Int32" },
                                11
                            }
                        })
                    }    
                }
            };
        }
    }
}
