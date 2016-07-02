namespace ReamQuery.Shared.Test
{
    using System;
    using Xunit;
    using System.Collections.Generic;
    using System.Linq;
    using ReamQuery.Shared;

    public class Dumper
    {
        class TestClassForDump
        {
            public int Id { get; set; }
            public string Foo { get; set; }
        }

        [Theory, MemberData("Simple_Value_Expressions")]
        public void Dumps_Simple_Value_Expressions(object dumpExpression, DumpResult expected)
        {
            var queryId = Guid.NewGuid();
            dumpExpression.Dump(queryId);
            var drain = DrainContainer.CloseDrain(queryId);
            var result = drain.GetData().Single();

            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.Columns, result.Columns);
            Assert.Equal(expected.Values, result.Values);
        }

        public static IEnumerable<object[]> Simple_Value_Expressions
        {
            get
            {
                return new object[][]
                {
                    new object[]
                    {
                        42,
                        new DumpResult
                        {
                            Name = "int (1)",
                            Columns = new ResultColumn[]
                            {
                                new ResultColumn
                                {
                                    SetId = 0,
                                    Name = ReamQuery.Shared.Dumper.RawValueColumnName,
                                    Type = "int"
                                }
                            },
                            Values = new object[] { 42 }
                        }
                    },
                    new object[]
                    {
                        "hello world",
                        new DumpResult
                        {
                            Name = "string (1)",
                            Columns = new ResultColumn[]
                            {
                                new ResultColumn { Name = ReamQuery.Shared.Dumper.RawValueColumnName, Type = "string" }
                            },
                            Values = new object[] { "hello world" }
                        }
                    },
                    new object[] 
                    {
                        new List<TestClassForDump>()
                        {
                            new TestClassForDump { Id = 1, Foo = "hello" },
                            new TestClassForDump { Id = 2, Foo = "world" },
                        },
                        new DumpResult
                        {
                            Name = "TestClassForDump (2)",
                            Columns = new ResultColumn[]
                            {
                                new ResultColumn { Name = "Id", Type = "int" },
                                new ResultColumn { Name = "Foo", Type = "string" }
                            },
                            Values = new object[] {
                                new object[] { 1, "hello" },
                                new object[] { 2, "world" },
                            }
                        }
                    },
                    new object[] 
                    {
                        new []
                        {
                            new { AnotherId = 1, Bar = "baz" },
                            new { AnotherId = 2, Bar = "qux" },
                        },
                        new DumpResult
                        {
                            Name = "AnonymousType (2)",
                            Columns = new ResultColumn[]
                            {
                                new ResultColumn { Name = "AnotherId", Type = "int" },
                                new ResultColumn { Name = "Bar", Type = "string" }
                            },
                            Values = new object[] {
                                new object[] { 1, "baz" },
                                new object[] { 2, "qux" },
                            }
                        }
                    }
                };
            }
        }
    }
}
