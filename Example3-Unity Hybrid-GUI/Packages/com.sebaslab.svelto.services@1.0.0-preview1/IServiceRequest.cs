using System.Threading.Tasks;

namespace Svelto.ServiceLayer
{
	public interface IServiceRequest
	{
		Task Execute();
	}

	public interface IServiceRequest<in TDependency>: IServiceRequest
	{
		IServiceRequest Inject(TDependency registerData);
	}
}