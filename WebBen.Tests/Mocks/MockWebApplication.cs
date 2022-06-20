using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebBen.Tests.Mocks;

public class MockWebApplication
{
    public static WebApplication CreateServer()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();
        app.Map("/", () => "hello");
        app.Start();

        return app;
    }
}