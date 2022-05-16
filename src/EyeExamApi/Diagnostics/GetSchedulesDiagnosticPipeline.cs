using EyeExamApi.Core.DTOs;
using EyeExamApi.Domain;
using MediatR;
using Serilog;

namespace EyeExamApi.Diagnostics
{
    public class GetSchedulesDiagnosticPipeline : 
        IPipelineBehavior<GetSchedulesRequest, IReadOnlyCollection<ParsedScheduleNoticeOfLease>>
    {
        private readonly IDiagnosticContext _context;

        public GetSchedulesDiagnosticPipeline(IDiagnosticContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<ParsedScheduleNoticeOfLease>> Handle(
            GetSchedulesRequest request, 
            CancellationToken cancellationToken, 
            RequestHandlerDelegate<IReadOnlyCollection<ParsedScheduleNoticeOfLease>> next)
        {
            var result = await next();

            _context.Set("Schedules", new Dictionary<string, object>
            {
                ["Count"] = result.Count,
                ["LesseesTitles"] = result.Select(s => s.LesseesTitle)
            });

            return result;
        }
    }
}
