using EggheadWeb.Models.Common;
using System;
using System.Runtime.CompilerServices;
using System.Security.Principal;

namespace EggheadWeb.Security
{
	public class LoginUser : IPrincipal
	{
		public EggheadWeb.Models.Common.Admin Admin
		{
			get;
			set;
		}

		public IIdentity Identity
		{
			get
			{
				return JustDecompileGenerated_get_Identity();
			}
			set
			{
				JustDecompileGenerated_set_Identity(value);
			}
		}

		private IIdentity JustDecompileGenerated_Identity_k__BackingField;

		public IIdentity JustDecompileGenerated_get_Identity()
		{
			return this.JustDecompileGenerated_Identity_k__BackingField;
		}

		public void JustDecompileGenerated_set_Identity(IIdentity value)
		{
			this.JustDecompileGenerated_Identity_k__BackingField = value;
		}

		public EggheadWeb.Models.Common.Parent Parent
		{
			get;
			set;
		}

		public LoginUser()
		{
		}

		public bool IsInRole(string role)
		{
			if (role == "superadmin" && this.Admin != null && this.Admin.IsSuperAdmin)
			{
				return true;
			}
			if (role == "admin" && this.Admin != null)
			{
				return true;
			}
			if (role == "parent" && this.Parent != null)
			{
				return true;
			}
			return false;
		}
	}
}