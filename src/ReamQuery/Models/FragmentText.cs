
namespace ReamQuery.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.Text;

    public class FragmentText
    {
        public string Text;

        public IEnumerable<int> ExpressionLocations;
    }
}