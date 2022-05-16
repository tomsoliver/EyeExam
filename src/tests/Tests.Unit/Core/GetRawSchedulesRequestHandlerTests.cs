using EyeExamApi.Core;
using EyeExamApi.Core.DTOs;
using EyeExamApi.Core.Implementations;
using FluentAssertions;
using Xunit;

namespace Tests.Unit.Core;

public class GetRawSchedulesRequestHandlerTests
{
    [Fact]
    public async Task Handle_ParsesSchedulesCorrectly()
    {
        var service = new RawScheduleDataService();

        var expected = service.GetRawScheduleNoticeOfLeases();

        var actual = await new GetRawSchedulesRequestHandler(service)
            .Handle(new GetRawSchedulesRequest(), default);

        actual.OrderBy(s => s.EntryNumber)
            .Should()
            .BeEquivalentTo(expected, o => o
                .WithoutStrictOrdering()
                .ComparingByMembers<RawScheduleNoticeOfLease>());
    }
}