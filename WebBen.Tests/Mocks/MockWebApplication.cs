using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebBen.Tests.Mocks;

public static class MockWebApplication
{
    public static WebApplication CreateServer()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());

        var app = builder.Build();
        app.Map("/", () => "hello");
        app.Map("/slow", () =>
        {
            SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(1500));
            return "oh no";
        });
        app.Start();

        return app;
    }
}