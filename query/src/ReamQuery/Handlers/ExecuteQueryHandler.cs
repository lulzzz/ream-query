namespace ReamQuery.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Services;
    using ReamQuery.Api;
    using System.Threading.Tasks;

    public class ExecuteQueryHandler : BaseHandler<QueryResponse, QueryRequest>
    {
        QueryService _service;

        public ExecuteQueryHandler(RequestDelegate next, QueryService service) : base(next) 
        {
            _service = service; 
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/executequery");
        }

        protected override async Task<QueryResponse> Execute(QueryRequest input)
        {
            return await _service.ExecuteQuery(input);
        }
    }
}
