using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.AdminModels
{
	public class PayrollItem
	{
		public int AssistCount
		{
			get;
			set;
		}

		public List<PayrollItemDetail> AssistDetails
		{
			get;
			set;
		}

		public int BirthdayCount
		{
			get;
			set;
		}

		public List<PayrollItemDetail> BirthdayDetails
		{
			get;
			set;
		}

		public long InstructorId
		{
			get;
			set;
		}

		public string InstructorName
		{
			get;
			set;
		}

		public int TeachCount
		{
			get;
			set;
		}

		public List<PayrollItemDetail> TeachDetails
		{
			get;
			set;
		}

		public int WorkshopCount
		{
			get;
			set;
		}

		public List<PayrollItemDetail> WorkshopDetails
		{
			get;
			set;
		}

		public PayrollItem()
		{
		}
	}
}