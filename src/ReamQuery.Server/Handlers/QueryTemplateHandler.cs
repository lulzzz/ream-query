namespace ReamQuery.Server.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Server.Services;
    using ReamQuery.Server.Api;
    using System.Threading.Tasks;
    using System;
    using NLog;

    public class QueryTemplateHandler : BaseHandler<TemplateResponse, QueryRequest>
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Info("{0}:{1}", input.Id, "/querytemplate");
            return await _queryService.GetTemplate(input);
        }
    }
}