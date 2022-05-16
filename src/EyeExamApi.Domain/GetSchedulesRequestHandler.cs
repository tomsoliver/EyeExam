using EyeExamApi.Core.DTOs;
using EyeExamApi.Core.Interfaces;
using EyeExamApi.Core.Parsing;
using MediatR;

namespace EyeExamApi.Core;

public record GetParsedSchedulesRequest() : IRequest<IReadOnlyCollection<ParsedScheduleNoticeOfLease>>;

public class GetParsedSchedulesRequestHandler
    : IRequestHandler<GetParsedSchedulesRequest, IReadOnlyCollection<ParsedScheduleNoticeOfLease>>
{
    readonly IRawScheduleDataService _service;

    public GetParsedSchedulesRequestHandler(IRawScheduleDataService service)
    {
        _service = service;
    }

    public Task<IReadOnlyCollection<ParsedScheduleNoticeOfLease>> Handle(
        GetParsedSchedulesRequest request, CancellationToken token)
    {
        var rawSchedules = _service.GetRawScheduleNoticeOfLeases();

        var parsedSchedules = rawSchedules.Select(r => new RawScheduleParser(r).Parse())
            .ToList();

        return Task.FromResult<IReadOnlyCollection<ParsedScheduleNoticeOfLease>>(parsedSchedules);
    }
}
