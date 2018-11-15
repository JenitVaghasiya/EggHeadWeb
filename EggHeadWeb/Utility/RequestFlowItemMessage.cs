using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Utility
{
	public class RequestFlowItemMessage
	{
		public string Message
		{
			get;
			set;
		}

		public RequestFlowItemMessageType Type
		{
			get;
			set;
		}

		public RequestFlowItemMessage()
		{
		}
	}
}