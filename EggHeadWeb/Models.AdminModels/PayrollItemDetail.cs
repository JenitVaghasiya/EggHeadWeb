using EggheadWeb.Models.Common;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class PayrollItemDetail
	{
		public long Count
		{
			get;
			set;
		}

		public string Dates
		{
			get;
			set;
		}

		public long InstructorId
		{
			get;
			set;
		}

		public long ServiceId
		{
			get;
			set;
		}

		public string ServiceName
		{
			get;
			set;
		}

		public ServiceType Type
		{
			get;
			set;
		}

		public PayrollItemDetail()
		{
		}
	}
}