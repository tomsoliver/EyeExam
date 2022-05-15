using EyeExamApi.Domain;
using EyeExamApi.DTOs;
using EyeExamApi.Implementations;
using FluentAssertions;
using Xunit;

namespace Tests.Unit;

public class GetSchedulesRequestHandlerTests
{
    [Fact]
    public async Task Handle_Test()
    {
        var destination = new ParsedScheduleDataService().GetParsedScheduleNoticeOfLeases();

        var result = await new GetSchedulesRequestHandler(new RawScheduleDataService())
            .Handle(new GetSchedulesRequest(), default);

        result.Should().BeEquivalentTo(destination, o => o
            .WithoutStrictOrdering()
            .ComparingByMembers<ParsedScheduleNoticeOfLease>());
    }
}