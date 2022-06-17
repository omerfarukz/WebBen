using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Newtonsoft.Json;
using WebBen.Core.Configuration;
using WebBen.Core.Configuration.Source;

namespace WebBen.Tests.Mocks;

public class MockConfigurationSource : IConfigurationSource
{
    private readonly string _content;

    public MockConfigurationSource(TestConfiguration testConfiguration)
    {
        _content = JsonConvert.SerializeObject(testConfiguration);
    }
    
    public Task<string> GetContent()
    {
        return Task.FromResult(_content);
    }
}