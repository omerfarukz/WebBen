using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using WebBen.Core;
using WebBen.Core.Configuration;
using WebBen.Core.Extensions;
using WebBen.Tests.Mocks;

namespace WebBen.Tests.Common;

public class LoggerTests
{
    [Test]
    public void ShouldLogWithAllLevels()
    {
        var logger = new MockLogger();
        logger.Debug(string.Empty);
        logger.Error(string.Empty);
        logger.Info(string.Empty);
    }

}