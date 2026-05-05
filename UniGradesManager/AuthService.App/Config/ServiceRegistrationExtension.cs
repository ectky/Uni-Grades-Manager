using System.Security.Principal;
using AuthService.App.Clients;
using AuthService.App.Clients.Contracts;
using AuthService.App.Services;
using AuthService.App.Services.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Nestle.PromoCatalog.WebApi.Config
{
    public static class ServiceRegistrationExtension
	{
		/// <summary>
		/// Register required Application services here...
		/// </summary>
		/// <param name="services">IServiceCollection object</param>
		/// <param name="configuration">IConfiguration object</param>
		/// <returns>A reference to this instance after the operation has completed.</returns>
		public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton(configuration);
			services.AddScoped<IUserService, UserService>();			
			services.AddScoped<IUserServiceClient, UserServiceClient>();

            return services;
		}
	}
}