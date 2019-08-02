using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class MockHostingEnvironment : Microsoft.AspNetCore.Hosting.IHostingEnvironment
    {
        public string EnvironmentName
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string ApplicationName
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public string WebRootPath
        {
            get
            {

                var startupFolder = Environment.CurrentDirectory.Replace("XUnitTestProject1\\bin\\Debug\\netcoreapp2.2", "");
                return startupFolder + "Doge\\wwwroot";
            }
            set
            {

            }

        }


        public IFileProvider WebRootFileProvider
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string ContentRootPath { get; set; } = Environment.CurrentDirectory;

        public IFileProvider ContentRootFileProvider
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        IFileProvider Microsoft.AspNetCore.Hosting.IHostingEnvironment.WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IFileProvider Microsoft.AspNetCore.Hosting.IHostingEnvironment.ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
