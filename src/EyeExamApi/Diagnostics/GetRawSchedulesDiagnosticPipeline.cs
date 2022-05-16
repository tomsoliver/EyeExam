using EyeExamApi.Core;
using EyeExamApi.Core.DTOs;
using MediatR;
using Serilog;

namespace EyeExamApi.Diagnostics
{
    public class GetRawSchedulesDiagnosticPipeline : 
        IPipelineBehavior<GetRawSchedulesRequest, IReadOnlyCollection<RawScheduleNoticeOfLease>>
    {
        private readonly IDiagnosticContext _context;

        public GetRawSchedulesDiagnosticPipeline(IDiagnosticContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<RawScheduleNoticeOfLease>> Handle(
            GetRawSchedulesRequest request, 
            CancellationToken cancellationToken, 
            RequestHandlerDelegate<IReadOnlyCollection<RawScheduleNoticeOfLease>> next)
        {
            var result = await next();

            _context.Set("Schedules", new Dictionary<string, object>
            {
                ["Count"] = result.Count
            });

            return result;
        }
    }
}
