
namespace ReamQuery.Shared
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Message
    {
        public int Session { get; set; }
        
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public object Id { get; set; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public object Parent { get; set; }
        
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
