using System.Net;
using WebBen.CLI.CredentialProviders;

namespace WebBen.CLI.Common;

internal class WebBenHttpClientAccessor : IDisposable
{
    private readonly HttpClientHandler _handler;
    
    public WebBenHttpClientAccessor(TestCase testCase, ICredentials? credentials)
    {
        if (testCase == null)
            throw new ArgumentNullException(nameof(testCase));
        
        _handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            MaxConnectionsPerServer = int.MaxValue
        };
        _handler.UseDefaultCredentials = false;
        _handler.UseCookies = testCase.Configuration.UseCookieContainer;

        if (credentials != null)
            _handler.Credentials = credentials;

        if (_handler.UseCookies)
            _handler.CookieContainer = new CookieContainer();

        Client = new HttpClient(_handler, false);
    }

    public HttpClient Client { get; }

    public void Dispose()
    {
        _handler.Dispose();
    }
}