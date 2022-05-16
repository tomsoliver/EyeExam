using EyeExamApi.Core.DTOs;
using EyeExamApi.Domain;
using EyeExamApi.Implementations;
using FluentAssertions;
using Xunit;

namespace Tests.Unit;

public class GetSchedulesRequestHandlerTests
{
    [Fact]
    public async Task Handle_ParsesSchedulesCorrectly()
    {
        var destination = new ParsedScheduleDataService().GetParsedScheduleNoticeOfLeases();

        var result = await new GetSchedulesRequestHandler(new RawScheduleDataService())
            .Handle(new GetSchedulesRequest(), default);

        result.OrderBy(s => s.EntryNumber)
            .Should()
            .BeEquivalentTo(destination, o => o
                .WithStrictOrdering()
                .ComparingByMembers<ParsedScheduleNoticeOfLease>());
    }
}