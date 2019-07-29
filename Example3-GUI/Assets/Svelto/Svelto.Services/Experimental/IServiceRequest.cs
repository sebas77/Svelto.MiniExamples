using System;
using System.Collections.Generic;
using Svelto.Tasks;

namespace Svelto.ServiceLayer.Experimental
{
	public interface IServiceRequest
	{
		IEnumerator<TaskContract> Execute();
	}
	
	public interface IServiceRequest<Result>:IServiceRequest
	{
		Result result { get; }
	}

	public interface IServiceRequest<Result, in TDependency>: IServiceRequest<Result> where Result : Enum
	{
		IServiceRequest<Result> Inject(TDependency registerData);
	}
}
