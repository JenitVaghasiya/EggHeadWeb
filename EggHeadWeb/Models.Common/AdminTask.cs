using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(AdminTaskValidator))]
	public class AdminTask
	{
		public virtual EggheadWeb.Models.Common.Admin Admin
		{
			get;
			set;
		}

		public long AdminId
		{
			get;
			set;
		}

		public DateTime CreateDate
		{
			get;
			set;
		}

		public DateTime? DueDate
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string Notes
		{
			get;
			set;
		}

		public byte Priority
		{
			get;
			set;
		}

		public byte Status
		{
			get;
			set;
		}

		public AdminTask()
		{
		}
	}
}