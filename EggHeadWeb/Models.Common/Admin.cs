using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(AdminValidator))]
	public class Admin
	{
		public string Address
		{
			get;
			set;
		}

		public virtual ICollection<AdminEmailTemplate> AdminEmailTemplates
		{
			get;
			set;
		}

		public virtual ICollection<AdminFrontend> AdminFrontends
		{
			get;
			set;
		}

		public virtual ICollection<AdminPaymentInfo> AdminPaymentInfoes
		{
			get;
			set;
		}

		public virtual ICollection<AdminTask> AdminTasks
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Area Area
		{
			get;
			set;
		}

		public long? AreaId
		{
			get;
			set;
		}

		public string City
		{
			get;
			set;
		}

		public virtual ICollection<Coupon> Coupons
		{
			get;
			set;
		}

		public string Email
		{
			get;
			set;
		}

		public string EmailPassword
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public bool IsSuperAdmin
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}

		public string PhoneNumber
		{
			get;
			set;
		}

		public virtual ICollection<PrivateMessage> PrivateMessages
		{
			get;
			set;
		}

		public virtual ICollection<PrivateMessage> PrivateMessages1
		{
			get;
			set;
		}

		public string State
		{
			get;
			set;
		}

		public string Username
		{
			get;
			set;
		}

		public string Zip
		{
			get;
			set;
		}

		public Admin()
		{
			this.AdminEmailTemplates = new HashSet<AdminEmailTemplate>();
			this.AdminPaymentInfoes = new HashSet<AdminPaymentInfo>();
			this.AdminTasks = new HashSet<AdminTask>();
			this.AdminFrontends = new HashSet<AdminFrontend>();
			this.Coupons = new HashSet<Coupon>();
			this.PrivateMessages = new HashSet<PrivateMessage>();
			this.PrivateMessages1 = new HashSet<PrivateMessage>();
		}
	}
}