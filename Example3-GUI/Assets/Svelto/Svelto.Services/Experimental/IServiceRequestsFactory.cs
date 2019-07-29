namespace Svelto.ServiceLayer.Experimental
{
	public interface IServiceRequestsFactory
	{
		RequestInterface Create<RequestInterface>() where RequestInterface:class, IServiceRequest;
	}
}

