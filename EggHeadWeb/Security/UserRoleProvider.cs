using EggheadWeb.Models.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web.Security;

namespace EggheadWeb.Security
{
	public class UserRoleProvider : RoleProvider
	{
		public const string SUPER_ADMIN = "superadmin";

		public const string ADMIN = "admin";

		public const string PARENT = "parent";

		private EggheadEntities db;

		public override string ApplicationName
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public UserRoleProvider() : this(new EggheadEntities())
		{
		}

		public UserRoleProvider(EggheadEntities db)
		{
			this.db = db;
		}

		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			throw new NotImplementedException();
		}

		public override void CreateRole(string roleName)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			throw new NotImplementedException();
		}

		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			throw new NotImplementedException();
		}

		public override string[] GetAllRoles()
		{
			return new string[] { "superadmin", "admin", "parent" };
		}

		public override string[] GetRolesForUser(string username)
		{
			Admin admin = this.db.Admins.FirstOrDefault<Admin>((Admin a) => a.Username == username);
			if (admin != null)
			{
				if (!admin.IsSuperAdmin)
				{
					return new string[] { "admin" };
				}
				return new string[] { "superadmin", "admin" };
			}
			if (this.db.Parents.FirstOrDefault<Parent>((Parent p) => p.Email == username) == null)
			{
				return null;
			}
			return new string[] { "parent" };
		}

		public override string[] GetUsersInRole(string roleName)
		{
			throw new NotImplementedException();
		}

		public override bool IsUserInRole(string username, string roleName)
		{
			if (roleName == "superadmin")
			{
				if (this.db.Admins.Any<Admin>((Admin a) => (a.Username == username) && a.IsSuperAdmin))
				{
					return true;
				}
			}
			if (roleName == "admin")
			{
				if (this.db.Admins.Any<Admin>((Admin a) => a.Username == username))
				{
					return true;
				}
			}
			if (roleName == "parent")
			{
				if (this.db.Parents.Any<Parent>((Parent p) => p.Email == username))
				{
					return true;
				}
			}
			return false;
		}

		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			throw new NotImplementedException();
		}

		public override bool RoleExists(string roleName)
		{
			if (roleName == "superadmin" || roleName == "admin")
			{
				return true;
			}
			return roleName == "parent";
		}
	}
}