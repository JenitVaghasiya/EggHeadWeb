using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class EggheadEntities : DbContext
	{
		public DbSet<AdminEmailTemplate> AdminEmailTemplates
		{
			get;
			set;
		}

		public DbSet<AdminFrontend> AdminFrontends
		{
			get;
			set;
		}

		public DbSet<AdminPaymentInfo> AdminPaymentInfoes
		{
			get;
			set;
		}

		public DbSet<Admin> Admins
		{
			get;
			set;
		}

		public DbSet<AdminTask> AdminTasks
		{
			get;
			set;
		}

		public DbSet<APVariable> APVariables
		{
			get;
			set;
		}

		public DbSet<Area> Areas
		{
			get;
			set;
		}

		public DbSet<Assign> Assigns
		{
			get;
			set;
		}

		public DbSet<Birthday> Birthdays
		{
			get;
			set;
		}

		public DbSet<Booking> Bookings
		{
			get;
			set;
		}

		public DbSet<BookingShortInfoTemp> BookingShortInfoTemps
		{
			get;
			set;
		}

		public DbSet<Camp> Camps
		{
			get;
			set;
		}

		public DbSet<Class> Classes
		{
			get;
			set;
		}

		public DbSet<Coupon> Coupons
		{
			get;
			set;
		}

		public DbSet<EmailTemplate> EmailTemplates
		{
			get;
			set;
		}

		public DbSet<Frontend> Frontends
		{
			get;
			set;
		}

		public DbSet<GradeGroup> GradeGroups
		{
			get;
			set;
		}

		public DbSet<Grade> Grades
		{
			get;
			set;
		}

		public DbSet<Instructor> Instructors
		{
			get;
			set;
		}

		public DbSet<Location> Locations
		{
			get;
			set;
		}

		public DbSet<Parent> Parents
		{
			get;
			set;
		}

		public DbSet<Payment> Payments
		{
			get;
			set;
		}

		public DbSet<PrivateMessage> PrivateMessages
		{
			get;
			set;
		}

		public DbSet<Student> Students
		{
			get;
			set;
		}

		public DbSet<Workshop> Workshops
		{
			get;
			set;
		}

		public EggheadEntities() : base("name=EggheadEntities")
		{
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			throw new UnintentionalCodeFirstException();
		}
	}
}