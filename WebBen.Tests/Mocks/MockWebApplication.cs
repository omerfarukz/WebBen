using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebBen.Tests.Mocks;

public class MockWebApplication
{
    public static WebApplication CreateServer()
    {
        var builder = WebApplication.CreateBuilder(new string[0]);
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();
        app.Map("/", () => "hello");
        app.Start();
        
        return app;
    }
}