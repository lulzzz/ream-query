namespace ReamQuery.Handlers
{
    using Microsoft.AspNetCore.Http;
    using ReamQuery.Services;
    using ReamQuery.Api;
    using System.Threading.Tasks;
    using System;
    using NLog;

    public class ExecuteQueryHandler : BaseHandler<QueryResponse, QueryRequest>
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Info("{0}:{1}", input.Id, "/executequery");
            return await _service.ExecuteQuery(input);
        }
    }
}
