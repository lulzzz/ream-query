namespace ReamQuery.Shared.Test
{
    using System;
    using Xunit;
    using System.Collections.Generic;
    using System.Linq;
    using ReamQuery.Shared;
    using System.Threading;

    public class Dumper
    {
        class TestClassForDump
        {
            public int Id { get; set; }
            public string Foo { get; set; }
        }

        static int SessionId;

        List<Message> RecordedMessages = new List<Message>();

        [Fact]
        public void Dumps_Typed_Null_Value()
        {
            var sessionId = Interlocked.Increment(ref SessionId);
            var emitter = ReamQuery.Shared.Dumper.InitializeEmitter(sessionId, 1);
            emitter.Messages.Subscribe(msg => RecordedMessages.Add(msg));
            IEnumerable<string> o = null;

            o.Dump(sessionId);

            var emptyMsg = RecordedMessages.Single(m => m.Type == ItemType.Empty);
            var col = (Column)emptyMsg.Values.First();
            Assert.Equal("string[]", col.Name);
        }

        [Theory, MemberData("Simple_Value_Expressions")]
        public void Dumps_Simple_Value_Expressions(int sessionId, object dumpExpression, IEnumerable<Message> expectedMsgs)
        {
            var emitter = ReamQuery.Shared.Dumper.InitializeEmitter(sessionId, 1);
            emitter.Messages.Subscribe(msg => RecordedMessages.Add(msg));

            dumpExpression.Dump(sessionId);

            Assert.All(expectedMsgs, (expect) => {
                Assert.Single(RecordedMessages, (msg) => msg.CompareWith(expect));
            });
        }

        public static IEnumerable<object[]> Simple_Value_Expressions
        {
            get
            {
                var id1 = Interlocked.Increment(ref SessionId);
                var id2 = Interlocked.Increment(ref SessionId);
                var id3 = Interlocked.Increment(ref SessionId);
                var id4 = Interlocked.Increment(ref SessionId);
                var id5 = Interlocked.Increment(ref SessionId);
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
                                Type = ItemType.Empty,
                                Values = new object[]
                                {
                                    new Column { Name = "object", Type = "System.Object" },
                                }
                            },
                            new Message { Session = id1, Type = ItemType.Close, Values = new object[] { 2 } }
                        }
                    },
                    new object[]
                    {
                        id2,
                        42,
                        new List<Message>
                        {
                            new Message {
                                Session = id2,
                                Type = ItemType.SingleAtomic,
                                Values = new object[]
                                {
                                    new Column { Name = "int", Type = "System.Int32" },
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
                                Type = ItemType.SingleAtomic,
                                Values = new object[]
                                {
                                    new Column { Name = "string", Type = "System.String" },
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
                            new Message { Session = id4, Id = 1, Type = ItemType.Table, Values = new object[] { "TestClassForDump[]" } },
                            new Message
                            {
                                Session = id4,
                                Id = 1,
                                Parent = 1,
                                Type = ItemType.Header,
                                Values = new object[]
                                {
                                    new Column { Parent = 1, Name = "Id", Type = "System.Int32" },
                                    new Column { Parent = 1, Name = "Foo", Type = "System.String" },
                                } 
                            },
                            new Message
                            {
                                Session = id4,
                                Parent = 1,
                                Type = ItemType.Row,
                                Values = new object[] { 1, "hello" } 
                            },
                            new Message
                            {
                                Session = id4,
                                Parent = 1,
                                Type = ItemType.Row,
                                Values = new object[] { 2, "world" } 
                            },
                            new Message { Session = id4, Type = ItemType.Close, Values = new object[] { 5 } }
                        }
                    },
                };
            }
        }
    }
}
