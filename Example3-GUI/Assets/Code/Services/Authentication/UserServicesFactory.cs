using Svelto.ServiceLayer.Experimental;

namespace User.Services.Authentication.Steam
{
    public class UserServicesFactory:ServiceRequestsFactory, IUserServicesFactory
    {
        public UserServicesFactory()
        {
            AddRelation<IUnifiedAuthVerifyDisplayNameService, UnifiedAuthVerifyDisplayNameService>();
        }
    }
}

namespace User.Services.Authentication
{
    public interface IUserServicesFactory:IServiceRequestsFactory
    {
    }
}