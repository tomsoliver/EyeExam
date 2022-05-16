using Bogus;
using EyeExamApi.Core.DTOs;
using EyeExamApi.Diagnostics;
using EyeExamApi.Domain;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Serilog;
using Xunit;

namespace Tests.Unit.Api
{
    public class GetSchedulesDiagnosticPipelineTests
    {
        readonly IDiagnosticContext _context;
        readonly GetSchedulesDiagnosticPipeline _pipeline;

        public GetSchedulesDiagnosticPipelineTests()
        {
            _context = Substitute.For<IDiagnosticContext>();
            _pipeline = new GetSchedulesDiagnosticPipeline(_context);
        }

        [Fact]
        public async Task Handle_AddsScheduleMetadataToDiagnosticContext()
        {
            var data = new Faker<ParsedScheduleNoticeOfLease>().GenerateBetween(2, 2);

            RequestHandlerDelegate<IReadOnlyCollection<ParsedScheduleNoticeOfLease>> handler =
                () => Task.FromResult<IReadOnlyCollection<ParsedScheduleNoticeOfLease>>(data);

            _context.WhenForAnyArgs(c => c.Set("Schedules", Arg.Any<Dictionary<string, object>>()))
                .Do(c => AssertMetadata(c.Arg<Dictionary<string, object>>(), data));

            await _pipeline.Handle(new GetSchedulesRequest(), default, handler);
        }

        bool AssertMetadata(Dictionary<string, object> metadata, List<ParsedScheduleNoticeOfLease> schedules)
        {
            metadata.Should().Contain("Count", schedules.Count);

            metadata.Should().ContainKey("LesseesTitles")
                .WhoseValue
                .Should().BeEquivalentTo(schedules.Select(s => s.LesseesTitle));

            return true;
        }
    }
}
