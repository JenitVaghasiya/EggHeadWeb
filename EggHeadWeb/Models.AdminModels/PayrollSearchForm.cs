using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class PayrollSearchForm
	{
		public DateTime? DateFrom
		{
			get;
			set;
		}

		public DateTime? DateTo
		{
			get;
			set;
		}

		public long? InstructorId
		{
			get;
			set;
		}

		public long? LocationId
		{
			get;
			set;
		}

		public PayrollSearchForm()
		{
		}
	}
}