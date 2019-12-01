using System;

namespace Svelto.ServiceLayer.Experimental
{
	public class ServiceRequestFactoryArgumentException: ArgumentException
	{
		public ServiceRequestFactoryArgumentException(string message):base(message)
		{}
	}
}