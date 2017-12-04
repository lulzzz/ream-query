namespace ReamQuery.Server.Helpers
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    
    public static class SyntaxHelpers
    {
        public static bool IsValidIdentifier(this string str)
        {
            var ok = SyntaxFacts.IsValidIdentifier(str);
            return ok;
        }
    }
}