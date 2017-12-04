namespace ReamQuery.Server.Api
{
    using ReamQuery.Server.Models;
    
    public class QueryRequest : BaseRequest
    {
        public DatabaseProviderType ServerType { get; set; }
        public string ConnectionString { get; set; }
        public string Text { get; set; }
    }
}
