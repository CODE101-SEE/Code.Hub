using AutoMapper;
using Code.Hub.Core.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Code.Hub.App.Startups.Extensions
{
    public static class RegisterMapperExtensions
    {
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CustomObjectMapper>();
            });

            services.AddSingleton<IMapper>(new Mapper(configuration));

            return services;
        }
    }
}
