using EggheadWeb.Models.Common;
using EggHeadWeb.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

namespace EggheadWeb.DAL
{
	public class UnitOfWork
	{
		private EggheadContext context = new EggheadContext();

		private bool disposed;

		private GenericRepository<Parent> parentRepository;

		private GenericRepository<Student> studentRepository;

		private GenericRepository<Area> areaRepository;

		private GenericRepository<Booking> bookingRepository;

		private GenericRepository<BookingShortInfoTemp> bookingShortInfoTempRepository;

		private GenericRepository<Admin> adminRepository;

		private GenericRepository<Coupon> couponRepository;

		private GenericRepository<Payment> paymentRepository;

		private EggheadWeb.DAL.ClassRepository classRepository;

		private EggheadWeb.DAL.CampRepository campRepository;

		private EggheadWeb.DAL.AdminFrontendRepository adminFrontendRepository;

		public EggheadWeb.DAL.AdminFrontendRepository AdminFrontendRepository
		{
			get
			{
				if (this.adminFrontendRepository == null)
				{
					this.adminFrontendRepository = new EggheadWeb.DAL.AdminFrontendRepository(this.context);
				}
				return this.adminFrontendRepository;
			}
		}

		public GenericRepository<Admin> AdminRepository
		{
			get
			{
				if (this.adminRepository == null)
				{
					this.adminRepository = new GenericRepository<Admin>(this.context);
				}
				return this.adminRepository;
			}
		}

		public GenericRepository<Area> AreaRepository
		{
			get
			{
				if (this.areaRepository == null)
				{
					this.areaRepository = new GenericRepository<Area>(this.context);
				}
				return this.areaRepository;
			}
		}

		public GenericRepository<Booking> BookingRepository
		{
			get
			{
				if (this.bookingRepository == null)
				{
					this.bookingRepository = new GenericRepository<Booking>(this.context);
				}
				return this.bookingRepository;
			}
		}

		public GenericRepository<BookingShortInfoTemp> BookingTempRepository
		{
			get
			{
				if (this.bookingShortInfoTempRepository == null)
				{
					this.bookingShortInfoTempRepository = new GenericRepository<BookingShortInfoTemp>(this.context);
				}
				return this.bookingShortInfoTempRepository;
			}
		}

		public EggheadWeb.DAL.CampRepository CampRepository
		{
			get
			{
				if (this.campRepository == null)
				{
					this.campRepository = new EggheadWeb.DAL.CampRepository(this.context);
				}
				return this.campRepository;
			}
		}

		public EggheadWeb.DAL.ClassRepository ClassRepository
		{
			get
			{
				if (this.classRepository == null)
				{
					this.classRepository = new EggheadWeb.DAL.ClassRepository(this.context);
				}
				return this.classRepository;
			}
		}

		public GenericRepository<Coupon> CouponRepository
		{
			get
			{
				if (this.couponRepository == null)
				{
					this.couponRepository = new GenericRepository<Coupon>(this.context);
				}
				return this.couponRepository;
			}
		}

		public GenericRepository<Parent> ParentRepository
		{
			get
			{
				if (this.parentRepository == null)
				{
					this.parentRepository = new GenericRepository<Parent>(this.context);
				}
				return this.parentRepository;
			}
		}

		public GenericRepository<Payment> PaymentRepository
		{
			get
			{
				if (this.paymentRepository == null)
				{
					this.paymentRepository = new GenericRepository<Payment>(this.context);
				}
				return this.paymentRepository;
			}
		}

		public GenericRepository<Student> StudentRepository
		{
			get
			{
				if (this.studentRepository == null)
				{
					this.studentRepository = new GenericRepository<Student>(this.context);
				}
				return this.studentRepository;
			}
		}

		public UnitOfWork()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed && disposing)
			{
				this.context.Dispose();
			}
			this.disposed = true;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Save()
		{
			try
			{
				this.context.SaveChanges();
			}
			catch (DbEntityValidationException dbEntityValidationException)
			{
				DbEntityValidationException dbEx = dbEntityValidationException;
				Exception raise = dbEx;
				foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
				{
					foreach (DbValidationError validationError in validationErrors.ValidationErrors)
					{
						string message = string.Format("{0}:{1}", validationErrors.Entry.Entity.ToString(), validationError.ErrorMessage);
						raise = new InvalidOperationException(message, raise);
					}
				}
				throw raise;
			}
		}
	}
}