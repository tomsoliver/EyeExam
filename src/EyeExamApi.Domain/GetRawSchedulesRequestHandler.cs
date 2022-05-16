using EyeExamApi.Core.DTOs;
using EyeExamApi.Core.Interfaces;
using MediatR;

namespace EyeExamApi.Core;

public record GetRawSchedulesRequest() : IRequest<IReadOnlyCollection<RawScheduleNoticeOfLease>>;

public class GetRawSchedulesRequestHandler
    : IRequestHandler<GetRawSchedulesRequest, IReadOnlyCollection<RawScheduleNoticeOfLease>>
{
    readonly IRawScheduleDataService _service;

    public GetRawSchedulesRequestHandler(IRawScheduleDataService service)
    {
        _service = service;
    }

    public Task<IReadOnlyCollection<RawScheduleNoticeOfLease>> Handle(
        GetRawSchedulesRequest request, CancellationToken token)
    {
        var rawSchedules = _service.GetRawScheduleNoticeOfLeases()
            .ToList();

        return Task.FromResult<IReadOnlyCollection<RawScheduleNoticeOfLease>>(rawSchedules);
    }
}
