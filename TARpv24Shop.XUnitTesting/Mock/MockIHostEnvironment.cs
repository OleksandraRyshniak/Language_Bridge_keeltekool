using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace TARpv24Keeltekooli.XUnitTesting.Mock
{
    public class MockIHostEnvironment : IHostEnvironment
    {
        string IHostEnvironment.EnvironmentName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IHostEnvironment.ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IHostEnvironment.ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IFileProvider IHostEnvironment.ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
