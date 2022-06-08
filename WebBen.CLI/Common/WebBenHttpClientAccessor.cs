using System.Net;

namespace WebBen.CLI.Common;

internal class WebBenHttpClientAccessor : IDisposable
{
    private readonly HttpClientHandler _handler;

    public WebBenHttpClientAccessor(bool useDefaultCredentials, bool useCookieContainer, ICredentials? credentials)
    {
        _handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            MaxConnectionsPerServer = int.MaxValue
            // TODO: optional cookie container
            // TODO: credentials
        };
        _handler.UseDefaultCredentials = useDefaultCredentials;
        _handler.UseCookies = useCookieContainer;
        _handler.Credentials = credentials;

        if (useCookieContainer) _handler.CookieContainer = new CookieContainer();

        Client = new HttpClient(_handler, false);
    }

    public HttpClient Client { get; }

    public void Dispose()
    {
        _handler.Dispose();
    }
}