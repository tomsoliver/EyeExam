using EyeExamApi.Authentication;
using EyeExamApi.Core.Properties;
using EyeExamApi.Implementations;
using EyeExamApi.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
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

builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);

builder.Services.AddAuthorization();

builder.Host.UseSerilog((ctx, config) => 
    config.ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(new RenderedCompactJsonFormatter()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();

// Would usually only have these in debug
app.MapSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();

app.Run();
