using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Code.Hub.Core.Services.Base
{
    public abstract class CodeHubBaseService
    {
        public IMapper ObjectMapper { get; set; }
        public ILogger Logger { get; set; }

        protected CodeHubBaseService(IServiceProvider serviceProvider)
        {
            Logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger(GetType());
            ObjectMapper = serviceProvider.GetService<IMapper>();
        }
    }
}
