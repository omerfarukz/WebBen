using System.Net;
using WebBen.CLI.CredentialProviders;

namespace WebBen.CLI.Common;

internal class WebBenHttpClientAccessor : IDisposable
{
    private readonly HttpClientHandler _handler;
    public HttpClient Client { get; }
    public CookieContainer CookieContainer { get; }

    public WebBenHttpClientAccessor(TestCase testCase, ICredentials? credentials)
    {
        if (testCase == null)
            throw new ArgumentNullException(nameof(testCase));

        _handler = new HttpClientHandler
        {
            AllowAutoRedirect = testCase.Configuration.AllowAutoRedirect,
            MaxConnectionsPerServer = int.MaxValue
        };
        _handler.UseDefaultCredentials = false;

        if (credentials != null)
            _handler.Credentials = credentials;

        // Enable cookie handling
        if (_handler.UseCookies || testCase.Configuration.Cookies != null)
        {
            CookieContainer = new CookieContainer();

            _handler.UseCookies = true;
            _handler.CookieContainer = CookieContainer;
        }

        Client = new HttpClient(_handler, true) {BaseAddress = testCase.Configuration.Uri};
        Client.MaxResponseContentBufferSize = testCase.Configuration.MaxResponseContentBufferSize;
        Client.Timeout = TimeSpan.FromMilliseconds(testCase.Configuration.TimeoutInMs);
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}