using EyeExamApi.Core;
using EyeExamApi.Core.DTOs;
using EyeExamApi.Core.Implementations;
using FluentAssertions;
using Xunit;

namespace Tests.Unit.Core;

public class GetParsedSchedulesRequestHandlerTests
{
    [Fact]
    public async Task Handle_ParsesSchedulesCorrectly()
    {
        var expected = new ParsedScheduleDataService().GetParsedScheduleNoticeOfLeases();

        var actual = await new GetParsedSchedulesRequestHandler(new RawScheduleDataService())
            .Handle(new GetParsedSchedulesRequest(), default);

        actual.Should()
            .BeEquivalentTo(expected, o => o
                .WithoutStrictOrdering()
                .ComparingByMembers<ParsedScheduleNoticeOfLease>());
    }
}