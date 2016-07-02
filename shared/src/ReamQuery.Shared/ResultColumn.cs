using System.Collections.Generic;

namespace ReamQuery.Shared
{
    public class ResultColumn
    {
        public int SetId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class ResultColumnComparer : IEqualityComparer<ResultColumn>
    {
        public bool Equals(ResultColumn x, ResultColumn y)
        {
            return x.Name + x.SetId + x.Type == y.Name + y.SetId + y.Type;
        }

        public int GetHashCode(ResultColumn x)
        {
            return (x.Name + x.SetId + x.Type).GetHashCode();
        }
    }
}
