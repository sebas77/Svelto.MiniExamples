using Svelto.ServiceLayer;
using Svelto.ServiceLayer.Experimental;

namespace User.Services.Authentication
{
    public class UserServicesFactoryMockup:ServiceRequestsFactory
    {
        public UserServicesFactoryMockup()
        {
            AddRelation<IUnifiedAuthVerifyDisplayNameService, UnifiedAuthVerifyDisplayNameService>();
            //AddRelation<IAnotherService1, AnotherService1>();
            //AddRelation<IAnotherService2, AnotherService2>();
        }
    }
}
