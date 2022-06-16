using System.Net;

namespace WebBen.CLI.Common;

internal class WebBenHttpClientAccessor : IDisposable
{
    public WebBenHttpClientAccessor(TestCase testCase, ICredentials? credentials)
    {
        if (testCase == null)
            throw new ArgumentNullException(nameof(testCase));

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = testCase.Configuration.AllowRedirect,
            MaxConnectionsPerServer = int.MaxValue
        };
        handler.UseDefaultCredentials = false;

        if (credentials != null)
            handler.Credentials = credentials;

        // Enable cookie handling
        if (handler.UseCookies || testCase.Configuration.Cookies != null)
        {
            CookieContainer = new CookieContainer();

            handler.UseCookies = true;
            handler.CookieContainer = CookieContainer;
        }

        Client = new HttpClient(handler, true) {BaseAddress = testCase.Configuration.Uri};
        Client.MaxResponseContentBufferSize = testCase.Configuration.MaxBufferSize;
        Client.Timeout = TimeSpan.FromMilliseconds(testCase.Configuration.TimeoutInMs);
    }

    public HttpClient Client { get; }
    public CookieContainer? CookieContainer { get; }

    public void Dispose()
    {
        Client.Dispose();
    }
}