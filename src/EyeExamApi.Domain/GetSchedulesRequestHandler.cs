using EyeExamApi.Core.DTOs;
using EyeExamApi.Interfaces;
using MediatR;

namespace EyeExamApi.Domain;

public record GetSchedulesRequest() : IRequest<IReadOnlyCollection<ParsedScheduleNoticeOfLease>>;

public class GetSchedulesRequestHandler : IRequestHandler<GetSchedulesRequest, IReadOnlyCollection<ParsedScheduleNoticeOfLease>>
{
    readonly IRawScheduleDataService _service;

    public GetSchedulesRequestHandler(IRawScheduleDataService service)
    {
        _service = service;
    }

    public Task<IReadOnlyCollection<ParsedScheduleNoticeOfLease>> Handle(GetSchedulesRequest request, CancellationToken token)
    {
        var rawSchedules = _service.GetRawScheduleNoticeOfLeases();

        var parsedSchedules = rawSchedules.Select(r => new RawScheduleParser(r).Parse())
            .ToList();

        return Task.FromResult<IReadOnlyCollection<ParsedScheduleNoticeOfLease>>(parsedSchedules);
    }
}
