using System.Collections.Generic;
using Svelto.Tasks;

namespace Svelto.ServiceLayer.Experimental
{
	public interface IServiceRequest
	{
		IEnumerator<TaskContract> Execute();
	}
	
	public interface IServiceRequest<in TDependency>: IServiceRequest
	{
		IServiceRequest Inject(TDependency registerData);
	}
}
