using Svelto.ServiceLayer.Experimental;
using User.Services.Authentication;

namespace Boxtopia.GUIs.DisplayName
{
    public class UserServicesFactoryMockup : ServiceRequestsFactory, IUserServicesFactory
    {
        public UserServicesFactoryMockup()
        {
            AddRelation<IUnifiedAuthVerifyDisplayNameService, UnifiedAuthVerifyDisplayNameService>();
        }
    }
}