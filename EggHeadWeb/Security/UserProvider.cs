using EggheadWeb.Models.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Web.Security;

namespace EggheadWeb.Security
{
	public class UserProvider : MembershipProvider
	{
		internal EggheadEntities db;

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

		public static LoginUser Current
		{
			get
			{
				return ((UserProvider)Membership.Provider).User;
			}
		}

		public override bool EnablePasswordReset
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool EnablePasswordRetrieval
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override int MaxInvalidPasswordAttempts
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override int MinRequiredPasswordLength
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override int PasswordAttemptWindow
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override string PasswordStrengthRegularExpression
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool RequiresQuestionAndAnswer
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool RequiresUniqueEmail
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public LoginUser User
		{
			get;
			set;
		}

		public UserProvider() : this(new EggheadEntities())
		{
		}

		public UserProvider(EggheadEntities db)
		{
			this.db = db;
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			status = (MembershipCreateStatus)0;
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			totalRecords = 0;
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			totalRecords = 0;
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			totalRecords = 0;
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public LoginUser GetUser(EggheadEntities db, IIdentity identity, string role)
		{
			string str = role;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "superadmin" || str1 == "admin")
				{
					Admin admin = db.Admins.SingleOrDefault<Admin>((Admin a) => a.Username == identity.Name);
					if (admin == null)
					{
						return null;
					}
					return new LoginUser()
					{
						Admin = admin,
						Identity = identity
					};
				}
				if (str1 == "parent")
				{
					Parent parent = db.Parents.SingleOrDefault<Parent>((Parent a) => a.Email == identity.Name);
					if (parent == null)
					{
						return null;
					}
					return new LoginUser()
					{
						Parent = parent,
						Identity = identity
					};
				}
			}
			return null;
		}

		public override string GetUserNameByEmail(string email)
		{
			throw new NotImplementedException();
		}

		public override string ResetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool UnlockUser(string userName)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}

		public bool ValidateAdmin(EggheadEntities db, string username, string password)
		{
			db = new EggheadEntities();
			Admin admin = db.Admins.FirstOrDefault<Admin>((Admin a) => a.Username == username);
			if (admin == null)
			{
				return false;
			}
			return BCrypt.Net.BCrypt.Verify(password, admin.Password);
		}

		public bool ValidateParent(EggheadEntities db, string username, string password)
		{
			Parent parent = db.Parents.FirstOrDefault<Parent>((Parent p) => p.Email == username);
			if (parent == null)
			{
				return false;
			}
			return BCrypt.Net.BCrypt.Verify(password, parent.Password);
		}

		public override bool ValidateUser(string username, string password)
		{
			throw new NotImplementedException();
		}
	}
}