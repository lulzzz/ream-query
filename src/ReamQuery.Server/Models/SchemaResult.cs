
namespace ReamQuery.Server.Models 
{
    using ReamQuery.Server.Api;

    public class SchemaResult : ResponseBase
    {
        public string Schema { get; set; }
        public string DefaultTable { get; set; }
    }
}