namespace ReamQuery.Server.Test
{
    using System;
    using Xunit;
    using ReamQuery.Helpers;
    using Microsoft.CodeAnalysis.Text;

    public class Helpers
    {
        [Fact]
        public void String_InsertTextAt()
        {
            var inp = Environment.NewLine + "  text" + Environment.NewLine;
            var exp = Environment.NewLine + " new text" + Environment.NewLine;
            var output = inp.InsertTextAt("new", 1, 1);

            Assert.Equal(exp, output);
        }

        [Fact]
        public void String_NormalizeNewlines_And_InsertTextAt()
        {
            var inp = "\r\n  text\n";
            var exp = Environment.NewLine + " new text" + Environment.NewLine;
            var output = inp.NormalizeNewlines().InsertTextAt("new", 1, 1);

            Assert.Equal(exp, output);
        }

        [Fact]
        public void String_NormalizeNewlines()
        {
            var inp = "\r\n  text\n";
            var exp = Environment.NewLine + "  text" + Environment.NewLine;
            var output = inp.NormalizeNewlines();

            Assert.Equal(exp, output);
        }

        [Fact]
        public void String_ReplaceToken()
        {
            var inp = Environment.NewLine + Environment.NewLine + " token " + Environment.NewLine;
            var exp = Environment.NewLine + Environment.NewLine + "  " + Environment.NewLine;
            LinePosition pos;
            var output = inp.ReplaceToken("token", string.Empty, out pos);

            Assert.Equal(new LinePosition(2, 1), pos);
            Assert.Equal(exp, output);
        }
    }
}
