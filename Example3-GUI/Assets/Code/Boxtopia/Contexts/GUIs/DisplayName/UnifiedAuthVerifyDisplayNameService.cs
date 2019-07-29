using System.Collections.Generic;
using Svelto.ServiceLayer.Experimental;
using Svelto.ServiceLayer.Experimental.Unity;
using Svelto.Tasks;
using User.Services.Authentication;

namespace Boxtopia.GUIs.DisplayName
{
    public class UnifiedAuthVerifyDisplayNameService : IUnifiedAuthVerifyDisplayNameService
    {
        string _displayName;

        public IServiceRequest<WebRequestResult> Inject(string registerData)
        {
            _displayName = registerData;
            
            return this;
        }

        public IEnumerator<TaskContract> Execute() { yield break; }

        public WebRequestResult result => WebRequestResult.Success;
        public VerifyDisplayNameResponse response => new VerifyDisplayNameResponse { Available = true };
    }
}