using Bogus;
using EyeExamApi.Core;
using EyeExamApi.Core.DTOs;
using EyeExamApi.Diagnostics;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Serilog;
using Xunit;

namespace Tests.Unit.Api
{
    public class GetRawSchedulesDiagnosticPipelineTests
    {
        readonly IDiagnosticContext _context;
        readonly GetRawSchedulesDiagnosticPipeline _pipeline;

        public GetRawSchedulesDiagnosticPipelineTests()
        {
            _context = Substitute.For<IDiagnosticContext>();
            _pipeline = new GetRawSchedulesDiagnosticPipeline(_context);
        }

        [Fact]
        public async Task Handle_AddsScheduleMetadataToDiagnosticContext()
        {
            var data = new Faker<RawScheduleNoticeOfLease>().GenerateBetween(2, 2);

            RequestHandlerDelegate<IReadOnlyCollection<RawScheduleNoticeOfLease>> handler =
                () => Task.FromResult<IReadOnlyCollection<RawScheduleNoticeOfLease>>(data);

            _context.WhenForAnyArgs(c => c.Set("Schedules", Arg.Any<Dictionary<string, object>>()))
                .Do(c => AssertMetadata(c.Arg<Dictionary<string, object>>(), data));

            await _pipeline.Handle(new GetRawSchedulesRequest(), default, handler);
        }

        bool AssertMetadata(Dictionary<string, object> metadata, List<RawScheduleNoticeOfLease> schedules)
        {
            metadata.Should().Contain("Count", schedules.Count);

            return true;
        }
    }
}
