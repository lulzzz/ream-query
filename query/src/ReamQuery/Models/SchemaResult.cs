
namespace ReamQuery.Models 
{
    using ReamQuery.Api;

    public class SchemaResult : BaseResponse
    {
        public string Schema { get; set; }
        public string DefaultTable { get; set; }
    }
}