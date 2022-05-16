using EyeExamApi.Core;
using EyeExamApi.Core.DTOs;
using MediatR;
using Serilog;

namespace EyeExamApi.Diagnostics
{
    public class GetParsedSchedulesDiagnosticPipeline : 
        IPipelineBehavior<GetParsedSchedulesRequest, IReadOnlyCollection<ParsedScheduleNoticeOfLease>>
    {
        private readonly IDiagnosticContext _context;

        public GetParsedSchedulesDiagnosticPipeline(IDiagnosticContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<ParsedScheduleNoticeOfLease>> Handle(
            GetParsedSchedulesRequest request, 
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
