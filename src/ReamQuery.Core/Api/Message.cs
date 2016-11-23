namespace ReamQuery.Core.Api
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Message
    {
        public Guid Session { get; set; }

        public string Title { get; set; }

        public int Id { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemType Type { get; set; }
        public IEnumerable<object> Values { get; set; }

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
