using Svelto.ServiceLayer;
using Svelto.ServiceLayer.Experimental;

namespace User.Services.Authentication
{
    public class MockUserServicesFactory : ServiceRequestsFactory
    {
        public MockUserServicesFactory()
        {
            AddRelation<INameValidationService, MockNameValidationService>();
            //AddRelation<IAnotherService1, AnotherService1>();
            //AddRelation<IAnotherService2, AnotherService2>();
        }
    }
}
