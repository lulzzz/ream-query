namespace ReamQuery.Shared.Test
{
    using System;
    using Xunit;
    using System.Collections.Generic;
    using System.Linq;
    using ReamQuery.Shared;
    using Newtonsoft.Json;

    public class Dumper
    {
        class TestClassForDump
        {
            public int Id { get; set; }
            public string Foo { get; set; }
        }

        List<Message> RecordedMessages = new List<Message>();

        [Fact]
        public void Dumps_Typed_Null_Value()
        {
            var queryId = Guid.NewGuid();
            var emitter = ReamQuery.Shared.Dumper.GetEmitter(queryId);
            emitter.Messages.Subscribe(msg => RecordedMessages.Add(msg));
            IEnumerable<string> o = null;

            o.Dump(queryId);

            var emptyMsg = RecordedMessages.Single(m => m.Type == ItemType.Empty);
            Assert.Equal("string[]", emptyMsg.Values.First());
        }

        [Theory, MemberData("Simple_Value_Expressions")]
        public void Dumps_Simple_Value_Expressions(object dumpExpression, IEnumerable<Message> expectedMsgs)
        {
            var queryId = Guid.NewGuid();
            var emitter = ReamQuery.Shared.Dumper.GetEmitter(queryId);
            emitter.Messages.Subscribe(msg => RecordedMessages.Add(msg));

            dumpExpression.Dump(queryId);

            Assert.All(expectedMsgs, (expect) => {
                Assert.Single(RecordedMessages, (msg) => msg.CompareWith(expect));
            });
        }

        public static IEnumerable<object[]> Simple_Value_Expressions
        {
            get
            {
                return new object[][]
                {
                    new object[]
                    {
                        null,
                        new List<Message>
                        {
                            new Message { Type = ItemType.Empty, Values = new object[] { "object" } }
                        }
                    },
                    new object[]
                    {
                        42,
                        new List<Message>
                        {
                            new Message { Type = ItemType.SingleAtomic, Values = new object[] { "int", 42 } }
                        }
                    },
                    new object[]
                    {
                        "hello world",
                        new List<Message>
                        {
                            new Message { Type = ItemType.SingleAtomic, Values = new object[] { "string", "hello world" } }
                        }
                    },
                    new object[]
                    {
                        new List<TestClassForDump>()
                        {
                            new TestClassForDump { Id = 1, Foo = "hello" },
                            new TestClassForDump { Id = 2, Foo = "world" },
                        },
                        new List<Message>
                        {
                            new Message { Id = 1, Type = ItemType.Table, Values = new object[] { "TestClassForDump[]" } },
                            new Message
                            {
                                Id = 1,
                                Parent = 1,
                                Type = ItemType.Header,
                                Values = new object[]
                                {
                                    new Column { Parent = 1, Name = "Id", Type = "System.Int32" },
                                    new Column { Parent = 1, Name = "Foo", Type = "System.String" },
                                } 
                            }
                        }
                    },
                };
            }
        }
    }
}
