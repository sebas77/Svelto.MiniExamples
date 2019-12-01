using System.Collections.Generic;
using Svelto.ServiceLayer.Experimental;
using Svelto.ServiceLayer.Experimental.Unity;
using Svelto.Tasks;

namespace User.Services.Authentication
{
    public class UnifiedAuthVerifyDisplayNameService : IUnifiedAuthVerifyDisplayNameService
    {
        string _displayName;

        public IServiceRequest Inject(string registerData)
        {
            _displayName = registerData;
            
            return this;
        }

        public IEnumerator<TaskContract> Execute()
        {
            response = new VerifyDisplayNameResponse() { valid = _displayName.Contains("sex") == false };

            yield break;
        }

        public WebRequestResult result => WebRequestResult.Success;
        public VerifyDisplayNameResponse response { get; private set; }
    }
    
    public struct VerifyDisplayNameResponse
    {
        public bool valid;
    }

    public interface IUnifiedAuthVerifyDisplayNameService: IServiceRequest<string>
    {
        VerifyDisplayNameResponse response { get; }
        WebRequestResult result { get; }
    }
}