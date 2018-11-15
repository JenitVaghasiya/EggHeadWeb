using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[MetadataType(typeof(FrontendAttr))]
	[Validator(typeof(FrontendFormValidator))]
	public class Frontend
	{
		public virtual ICollection<AdminFrontend> AdminFrontends
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

		public string PageContent
		{
			get;
			set;
		}

		public Frontend()
		{
			this.AdminFrontends = new HashSet<AdminFrontend>();
		}
	}
}