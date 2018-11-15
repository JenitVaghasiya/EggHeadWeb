using FluentValidation.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[MetadataType(typeof(AdminFrontendAttr))]
	[Validator(typeof(AdminFrontendFormValidator))]
	public class AdminFrontend
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

		public virtual EggheadWeb.Models.Common.Frontend Frontend
		{
			get;
			set;
		}

		public int? FrontendId
		{
			get;
			set;
		}

		public int Id
		{
			get;
			set;
		}

		public bool IsActive
		{
			get;
			set;
		}

		public string MenuName
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string OverridePageContent
		{
			get;
			set;
		}

		public AdminFrontend()
		{
		}
	}
}