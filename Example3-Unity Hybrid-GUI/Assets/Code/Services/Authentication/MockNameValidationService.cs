using System.Threading.Tasks;
using Svelto.ServiceLayer;

namespace User.Services.Authentication
{
    public class MockNameValidationService : INameValidationService
    {
        string _displayName;

        public IServiceRequest Inject(string registerData)
        {
            _displayName = registerData;

            return this;
        }

        public Task Execute()
        {
            response = new VerifyDisplayNameResponse
            {
                status = string.IsNullOrWhiteSpace(_displayName)
                    ? NameValidationStatus.NameRequired
                    : _displayName.Contains("sex")
                        ? NameValidationStatus.Forbidden
                        : NameValidationStatus.Valid
            };

            return Task.CompletedTask;
        }

        public WebRequestResult result => WebRequestResult.Success;
        public VerifyDisplayNameResponse response { get; private set; }
    }

    public enum WebRequestResult
    {
        Success
    }

    public struct VerifyDisplayNameResponse
    {
        public NameValidationStatus status;

        public bool valid => status == NameValidationStatus.Valid;
    }

    public enum NameValidationStatus
    {
        Valid,
        NameRequired,
        Forbidden
    }

    public interface INameValidationService: IServiceRequest<string>
    {
        VerifyDisplayNameResponse response { get; }
        WebRequestResult result { get; }
    }
}
