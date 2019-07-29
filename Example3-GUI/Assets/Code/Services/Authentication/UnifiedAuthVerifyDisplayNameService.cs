using System.Collections.Generic;
using Svelto.ServiceLayer.Experimental;
using Svelto.ServiceLayer.Experimental.Unity;
using Svelto.Tasks;

namespace User.Services.Authentication.Steam
{
    public class UnifiedAuthVerifyDisplayNameService : IUnifiedAuthVerifyDisplayNameService
    {
        public WebRequestResult result => _webRequest.result;
        public VerifyDisplayNameResponse response { get; private set; }

        public UnifiedAuthVerifyDisplayNameService()
        {
            _webRequest = new StandardWebRequest();

            _webRequest.responseHandler = new JsonResponseHandler<VerifyDisplayNameResponse>();
            _webRequest.URL = "http://172.30.10.10/account/displayname/available";
        }

        public IServiceRequest<WebRequestResult> Inject(string registerData) { _displayName = registerData; return this; }

        public IEnumerator<TaskContract> Execute()
        {
            DBC.Check.Require(_displayName != null, "You forgot to inject the name");

            var displayNameData = new DisplayNameData
            {
                DisplayName = _displayName,
            };

            yield return _webRequest.Execute(displayNameData).Continue();

            response = ((JsonResponseHandler<VerifyDisplayNameResponse>) _webRequest.responseHandler).response;
        }

        readonly StandardWebRequest _webRequest;
        string                      _displayName;
    }
}

namespace User.Services.Authentication
{
    public struct VerifyDisplayNameResponse
    {
        public bool Available;
    }

    public struct DisplayNameData
    {
        public string DisplayName;
    }

    public interface IUnifiedAuthVerifyDisplayNameService: IServiceRequest<WebRequestResult, string>
    {
        VerifyDisplayNameResponse response { get; }
    }
}