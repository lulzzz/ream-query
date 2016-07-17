namespace ReamQuery.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Services;
    using ReamQuery.Api;
    using System.Threading.Tasks;
    using System;

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
            if (input.Id == Guid.Empty)
            {
                throw new ArgumentException("Id");
            }
            return await _service.ExecuteQuery(input);
        }
    }
}
