
namespace ReamQuery.Shared
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Message
    {
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int? Parent { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemType Type { get; set; }
        public object[] Values { get; set; }


        public bool CompareWith(Message other)
        {
            if (other == null)
            {
                return false;
            }
            return this.ToJson() == other.ToJson();
        }
        
        string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
