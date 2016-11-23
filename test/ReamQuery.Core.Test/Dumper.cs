namespace ReamQuery.Core.Test
{
    using System;
    using Xunit;
    using System.Collections.Generic;
    using System.Linq;
    using ReamQuery.Core;
    using ReamQuery.Core.Api;
    using Newtonsoft.Json;
    using System.Threading.Tasks;

    public class Dumper
    {
        class TestClassForDump
        {
            public int Id { get; set; }
            public string Foo { get; set; }
        }

        List<Message> RecordedMessages = new List<Message>();

        [Fact]
        public async Task Dumps_With_Optional_Title()
        {
            var sessionId = Guid.NewGuid();
            var emitter = new Emitter(sessionId);
            emitter.Messages.Subscribe(msg => RecordedMessages.Add(msg));
            var foo = 42;

            foo.Dump(emitter, title: "custom");
            emitter.Complete();
            await Task.Delay(500);

            var listMsg = RecordedMessages.Single(m => m.Type == ItemType.Single);
            Assert.Equal("custom", listMsg.Title);
        }

        [Theory, MemberData("Simple_Value_Expressions")]
        public async Task Dumps_Simple_Value_Expressions(Guid sessionId, object dumpExpression, IEnumerable<Message> expectedMsgs)
        {
            var emitter = new Emitter(sessionId);
            emitter.Messages.Subscribe(msg => RecordedMessages.Add(msg));

            dumpExpression.Dump(emitter);
            emitter.Complete();
            await Task.Delay(500);

            Assert.All(expectedMsgs, (expect) => {
                Assert.Single(RecordedMessages, (msg) => msg.CompareWith(expect));
            });
        }

        public static IEnumerable<object[]> Simple_Value_Expressions
        {
            get
            {
                var id1 = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var id3 = Guid.NewGuid();
                var id4 = Guid.NewGuid();
                var id5 = Guid.NewGuid();
                return new object[][]
                {
                    new object[]
                    {
                        id1,
                        null,
                        new List<Message>
                        {
                            new Message
                            {
                                Session = id1,
                                Type = ItemType.Single,
                                Values = new object[]
                                {
                                    null
                                }
                            }
                        }
                    }
                    ,
                    new object[]
                    {
                        id2,
                        42,
                        new List<Message>
                        {
                            new Message {
                                Session = id2,
                                Type = ItemType.Single,
                                Values = new object[]
                                {
                                    42
                                }
                            },
                        }
                    },
                    new object[]
                    {
                        id3,
                        "hello world",
                        new List<Message>
                        {
                            new Message {
                                Session = id3,
                                Type = ItemType.Single,
                                Values = new object[]
                                {
                                    "hello world" 
                                } 
                            }
                        }
                    },
                    new object[]
                    {
                        id4,
                        new List<TestClassForDump>()
                        {
                            new TestClassForDump { Id = 1, Foo = "hello" },
                            new TestClassForDump { Id = 2, Foo = "world" },
                        },
                        new List<Message>
                        {
                            new Message
                            {
                                Session = id4,
                                Id = 1,
                                Type = ItemType.List,
                                // includes first row item, for possible type info dumping
                                Values = new object[] { new TestClassForDump { Id = 1, Foo = "hello" } } 
                            },
                            new Message
                            {
                                Session = id4,
                                Id = 1,
                                Type = ItemType.ListValues,
                                Values = new object[]
                                {
                                    new TestClassForDump { Id = 1, Foo = "hello" },
                                    new TestClassForDump { Id = 2, Foo = "world" }
                                } 
                            },
                            new Message { Session = id4, Id = 1, Type = ItemType.ListClose }
                        }
                    },
                };
            }
        }
    }
}
