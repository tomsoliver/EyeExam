using EyeExamApi.Core;
using EyeExamApi.Core.DTOs;
using MediatR;
using MinimalApi.Endpoint;

namespace EyeExamApi.Endpoints;

public class ParsedScheduleEndpoint : IEndpoint<IResult>
{
    private readonly IMediator _mediator;

    public ParsedScheduleEndpoint(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<IResult> HandleAsync()
    { 
        var result = await _mediator.Send(new GetParsedSchedulesRequest());

        return Results.Ok(result);
    }

    public void AddRoute(IEndpointRouteBuilder app) =>
        app.MapGet("/results", HandleAsync)
            .Produces<IEnumerable<ParsedScheduleNoticeOfLease>>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization();
}
