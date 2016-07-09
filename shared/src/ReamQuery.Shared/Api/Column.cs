
namespace ReamQuery.Shared
{
    using System.Reflection;
    using Newtonsoft.Json;

    public struct Column
    {
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int? Parent { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        [JsonIgnore]
        public PropertyInfo Prop { get; set; }
    }
}
