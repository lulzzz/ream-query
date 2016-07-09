
namespace ReamQuery.Models 
{
    using ReamQuery.Api;

    public class SchemaResult : ResponseBase
    {
        public string Schema { get; set; }
        public string DefaultTable { get; set; }
    }
}