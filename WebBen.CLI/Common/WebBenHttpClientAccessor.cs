using System.Net;

namespace WebBen.CLI.Common;

internal class WebBenHttpClientAccessor : IDisposable
{
    public HttpClient Client { get; }
    public CookieContainer? CookieContainer { get; }
    
    public WebBenHttpClientAccessor(TestCase testCase, ICredentials? credentials)
    {
        if (testCase == null)
            throw new ArgumentNullException(nameof(testCase));

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = testCase.Configuration.AllowRedirect,
            MaxConnectionsPerServer = int.MaxValue
        };
 
        // Credential
        handler.UseDefaultCredentials = testCase.Configuration.UseDefaultCredentials;
        if (credentials != null)
            handler.Credentials = credentials;

        // Cookie
        if (testCase.Configuration.UseCookieContainer || testCase.Configuration.Cookies != null)
        {
            CookieContainer = new CookieContainer();
            
            handler.UseCookies = true;
            handler.CookieContainer = CookieContainer;
        }

        Client = new HttpClient(handler, true) {BaseAddress = testCase.Configuration.Uri};
        Client.MaxResponseContentBufferSize = testCase.Configuration.MaxBufferSize;
        Client.Timeout = TimeSpan.FromMilliseconds(testCase.Configuration.TimeoutInMs);
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}