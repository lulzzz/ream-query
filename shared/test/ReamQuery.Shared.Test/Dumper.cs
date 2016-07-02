namespace ReamQuery.Shared.Test
{
    using System;
    using Xunit;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Microsoft.Extensions.PlatformAbstractions;
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
                            Columns = new Tuple<string, string>[] { Tuple.Create(ReamQuery.Shared.Dumper.RawValueColumnName, "int") },
                            Values = new object[] { 42 }
                        }
                    },
                    new object[]
                    {
                        "hello world",
                        new DumpResult
                        {
                            Name = "string (1)",
                            Columns = new Tuple<string, string>[] { Tuple.Create(ReamQuery.Shared.Dumper.RawValueColumnName, "string") },
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
                            Columns = new Tuple<string, string>[] { Tuple.Create("Id", "int"), Tuple.Create("Foo", "string") },
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
                            Columns = new Tuple<string, string>[] { Tuple.Create("AnotherId", "int"), Tuple.Create("Bar", "string") },
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
