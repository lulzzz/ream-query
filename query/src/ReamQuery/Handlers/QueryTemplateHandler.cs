namespace ReamQuery.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Services;
    using ReamQuery.Api;
    using System.Threading.Tasks;
    using System;

    public class QueryTemplateHandler : BaseHandler<TemplateResponse, QueryRequest>
    {
        QueryService _queryService;

        public QueryTemplateHandler(RequestDelegate next, QueryService queryService) : base(next) 
        {
            _queryService = queryService;
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/querytemplate");
        }

        protected override async Task<TemplateResponse> Execute(QueryRequest input)
        {
            if (input.Id == Guid.Empty)
            {
                throw new ArgumentException("Id");
            }
            return await _queryService.GetTemplate(input);
        }
    }
}