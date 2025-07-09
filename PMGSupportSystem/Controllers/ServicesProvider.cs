using Microsoft.Extensions.DependencyInjection;

namespace PMGSupportSystem.Services
{
    public interface IServicesProvider
    {
        IUserService UserService { get; }
    }
    public class ServicesProvider : IServicesProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ServicesProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IUserService UserService => _serviceProvider.GetRequiredService<IUserService>();
    }
}
