using Bogus;
using EyeExamApi.Diagnostics;
using EyeExamApi.Domain;
using EyeExamApi.DTOs;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Serilog;
using Xunit;

namespace Tests.Unit.Api
{
    public class GetSchedulesDiagnosticPipelineTests
    {
        [Fact]
        public async Task Handle_AddsScheduleMetadataToDiagnosticContext()
        {
            var context = Substitute.For<IDiagnosticContext>();
            var pipeline = new GetSchedulesDiagnosticPipeline(context);

            var data = new Faker<ParsedScheduleNoticeOfLease>().GenerateBetween(2, 2);

            RequestHandlerDelegate<IReadOnlyCollection<ParsedScheduleNoticeOfLease>> handler =
                () => Task.FromResult<IReadOnlyCollection<ParsedScheduleNoticeOfLease>>(data);

            await pipeline.Handle(new GetSchedulesRequest(), default, handler);

            context.Received(1).Set("Schedules", Arg.Is<Dictionary<string, object>>(c => AssertMetadata(c, data)));
        }

        bool AssertMetadata(Dictionary<string, object> metadata, List<ParsedScheduleNoticeOfLease> schedules)
        {
            metadata.Should().Contain("Count", schedules.Count);

            return true;
        }
    }
}
