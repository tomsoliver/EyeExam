using EyeExamApi.Core.Properties;
using EyeExamApi.Implementations;
using EyeExamApi.Interfaces;
using MediatR;
using MinimalApi.Endpoint.Extensions;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMediatR(typeof(AssemblyMarker))
    .AddSingleton<IRawScheduleDataService, RawScheduleDataService>();

builder.Services
    .AddEndpoints()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .Scan(s => s
        .FromAssemblyOf<Program>()
        .AddClasses(c => c.AssignableTo(typeof(IPipelineBehavior<,>)))
        .AsImplementedInterfaces());

builder.Host.UseSerilog((ctx, config) => 
    config.ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(new RenderedCompactJsonFormatter()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwagger();
else
    app.UseHsts();

app.UseSerilogRequestLogging();
app.UseRouting();
app.MapGet("/", () => "Hello World!");
app.MapEndpoints();

app.Run();
