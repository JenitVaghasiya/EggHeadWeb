using AuthorizeNet;
using EggheadWeb.Common;
using EggheadWeb.DAL;
using EggheadWeb.Mailers;
using EggheadWeb.Models.Common;
using EggheadWeb.Models.UserModels;
using EggheadWeb.Security;
using EggheadWeb.Utility;
using EggHeadWeb.DatabaseContext;
using log4net;
using Microsoft.CSharp.RuntimeBinder;
using Mvc.Mailer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace EggheadWeb.Controllers
{
	public class UsersController : WithAuthenController
	{
		protected const int MIN_PASSWORDL_ENGTH = 6;

		protected const int WORK_FACTOR = 10;

		private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private UnitOfWork unitOfWork = new UnitOfWork();

		private IUserMailer _userMailer;

		private List<ChildBookingItem> CampBookingInfo
		{
			get
			{
				return System.Web.HttpContext.Current.Session["Camp.EnrolledStudent"] as List<ChildBookingItem>;
			}
			set
			{
				System.Web.HttpContext.Current.Session["Camp.EnrolledStudent"] = value;
			}
		}

		private List<ChildBookingItem> ClassBookingInfo
		{
			get
			{
				return System.Web.HttpContext.Current.Session["Class.EnrolledStudent"] as List<ChildBookingItem>;
			}
			set
			{
				System.Web.HttpContext.Current.Session["Class.EnrolledStudent"] = value;
			}
		}

		private Parent CurrentParent
		{
			get
			{
				return base.User.Parent;
			}
		}

		public IUserMailer UserMailer
		{
			get
			{
				return this._userMailer;
			}
			set
			{
				this._userMailer = value;
			}
		}

		public UsersController() : base("parent")
		{
			this._userMailer = new EggheadWeb.Mailers.UserMailer(this.db);
		}

		[ActionName("account-info")]
		[Authorize]
		[HttpGet]
		public ActionResult AccountInfo(string location)
		{
			if (!this.CheckLocation())
			{
				return base.RedirectToRoute("Users", new { action = "area-not-found" });
			}
			((dynamic)ViewBag).SelectedMenuItem = "Account";
			((dynamic)ViewBag).HelpQuestions = this.unitOfWork.AdminFrontendRepository.GetAdminFrontendItems(this.GetCurrentAreaId());
			base.SetSession("CurrentAdmin", this.GetCurrentAdmin());
			System.Web.HttpContext.Current.Session["Register.Parent"] = null;
			System.Web.HttpContext.Current.Session["Register.Students"] = null;
			if (this.unitOfWork.BookingRepository.Get((Booking t) => t.Student.ParentId == this.CurrentParent.Id, null, "").Count<Booking>() == 0)
			{
				this.SetErrorMessages(Messages.NotEnrollWarningMain, Messages.NotEnrollWarningSub);
			}
			((dynamic)ViewBag).Classes = this.unitOfWork.ClassRepository.GetClassEnrolledByParent(this.CurrentParent.Id);
			((dynamic)ViewBag).Camps = this.unitOfWork.CampRepository.GetCampEnrolledByParent(this.CurrentParent.Id);
			return base.View(base.User.Parent);
		}

		[ActionName("student-add-init")]
		[Authorize]
		[HttpPost]
		public ActionResult AddStudent()
		{
			return base.PartialView("student-add");
		}

		[ActionName("student-add")]
		[Authorize]
		[HttpPost]
		public ActionResult AddStudent(StudentItem model)
		{
			if (!ModelState.IsValid)
			{
				System.Web.HttpContext.Current.Response.StatusCode = 600;
				System.Web.HttpContext.Current.Response.TrySkipIisCustomErrors = true;
				return base.View(model);
			}
			GenericRepository<Student> studentRepository = this.unitOfWork.StudentRepository;
			Student student = new Student()
			{
				ParentId = this.CurrentParent.Id,
				FirstName = model.FirstName,
				LastName = model.LastName,
				GradeId = model.GradeId,
				Gender = model.Gender,
				BirthDate = model.BirthDate.Value,
				Notes = model.Notes
			};
			studentRepository.Insert(student);
			this.unitOfWork.Save();
			ModelState.Clear();
			this.SetSuccessMessages(Messages.AddStudentInfoSuccessMain, Messages.AddStudentInfoSuccessSub);
			return this.PartialView("student-add", new StudentItem());
		}

		[ActionName("all-activity")]
		[HttpPost]
		public ActionResult AllActivity()
		{
			this.GetCurrentAreaId();
			((dynamic)ViewBag).Classes = this.unitOfWork.ClassRepository.GetEnrollableClassesOfParent(this.CurrentParent);
			((dynamic)ViewBag).Camps = this.unitOfWork.CampRepository.GetEnrollableCampsOfParent(this.CurrentParent);
			return base.PartialView();
		}

		[ActionName("area-not-found")]
		[AllowAnonymous]
		[HttpGet]
		public ActionResult AreaNotFound(string area)
		{
			((dynamic)ViewBag).Area = area;
			return base.View("Area-not-found");
		}

		private decimal CalculateAdjusmentAmount(Coupon coupon, decimal totalFees)
		{
			if (coupon.Type != 1)
			{
				decimal? discountAmount = coupon.DiscountAmount;
				return Math.Round((totalFees * discountAmount.Value) / new decimal(100), 2);
			}
			if (coupon.DiscountAmount.Value > totalFees)
			{
				return totalFees;
			}
			return coupon.DiscountAmount.Value;
		}

		[Authorize]
		[HttpGet]
		public ActionResult Camps()
		{
			if (!this.CheckLocation())
			{
				return base.RedirectToRoute("Users", new { action = "area-not-found" });
			}
			long currentAreaId = this.GetCurrentAreaId();
			((dynamic)ViewBag).SelectedMenuItem = "Camps";
			((dynamic)ViewBag).HelpQuestions = this.unitOfWork.AdminFrontendRepository.GetAdminFrontendItems(currentAreaId);
			dynamic viewBag = ViewBag;
			ICollection<Student> students = this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id).Students;
			viewBag.Students = (
				from t in students
				select new { StudentId = t.Id, Name = t.FirstName }).ToList();
			if (this.CampBookingInfo == null)
			{
				this.CampBookingInfo = new List<ChildBookingItem>();
			}
			((dynamic)ViewBag).EnrollInfo = this.CampBookingInfo;
			return base.View(this.unitOfWork.CampRepository.GetEnrollableCampsOfParent(this.CurrentParent));
		}

		[ActionName("init-change-password")]
		[HttpPost]
		public ActionResult ChangePassword()
		{
			return base.PartialView("password-change");
		}

		[ActionName("password-change")]
		[HttpPost]
		public ActionResult ChangePassword(Models.Common.ChangePasswordForm model)
		{
			if (ModelState.IsValid)
			{
				Parent updateParent = this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id);
				if ((updateParent == null ? true : !global::BCrypt.Net.BCrypt.Verify(model.OldPassword, updateParent.Password)))
				{
					ModelState.AddModelError("_FORM", "The old password provided is incorrect.");
				}
				else
				{
					try
					{
						updateParent.Password = global::BCrypt.Net.BCrypt.HashPassword(model.NewPassword, 10);
						this.unitOfWork.Save();
						this.SetSuccessMessages(Messages.ChangePasswordSuccess, "");
					}
					catch
					{
						this.SetErrorMessages(Messages.ChangePasswordFail, "");
					}
				}
			}
			return base.PartialView("password-change");
		}

		private bool CheckLocation()
		{
			string str = Convert.ToString(RouteData.Values["location"]);
			Area location = this.unitOfWork.AreaRepository.Get((Area t) => t.Name == str, null, "").FirstOrDefault<Area>();
			if (location != null && location.Admins.Count != 0)
			{
				return true;
			}
			return false;
		}

		[ActionName("check-schedule")]
		[Authorize]
		[HttpPost]
		public ActionResult CheckSchedule()
		{
			((dynamic)ViewBag).Classes = this.unitOfWork.ClassRepository.GetClassEnrolledByParent(this.CurrentParent.Id);
			((dynamic)ViewBag).Camps = this.unitOfWork.CampRepository.GetCampEnrolledByParent(this.CurrentParent.Id);
			return base.PartialView();
		}

		[Authorize]
		public ActionResult Classes()
		{
			if (!this.CheckLocation())
			{
				return base.RedirectToRoute("Users", new { action = "area-not-found" });
			}
			long currentAreaId = this.GetCurrentAreaId();
			((dynamic)ViewBag).SelectedMenuItem = "Classes";
			((dynamic)ViewBag).HelpQuestions = this.unitOfWork.AdminFrontendRepository.GetAdminFrontendItems(currentAreaId);
			dynamic viewBag = ViewBag;
			ICollection<Student> students = this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id).Students;
			viewBag.Students = (
				from t in students
				select new { StudentId = t.Id, Name = t.FirstName }).ToList();
			if (this.ClassBookingInfo == null)
			{
				this.ClassBookingInfo = new List<ChildBookingItem>();
			}
			((dynamic)ViewBag).EnrollInfo = this.ClassBookingInfo;
			return base.View(this.unitOfWork.ClassRepository.GetEnrollableClassesOfParent(this.CurrentParent));
		}

		private MembershipCreateStatus CreateUser(UserItem user)
		{
			MembershipCreateStatus membershipCreateStatu;
			try
			{
				Parent parent1 = new Parent()
				{
					LocationId = user.Parent.LocationId,
					Email = user.Parent.Email,
					Password = global::BCrypt.Net.BCrypt.HashPassword(user.Parent.Password, 10),
					FirstName = user.Parent.FirstName,
					LastName = user.Parent.LastName,
					PhoneNumer = user.Parent.PhoneNumer,
					Address = user.Parent.Address,
					City = user.Parent.City,
					State = user.Parent.State,
					Zip = user.Parent.Zip,
					LastLoginDateTime = new DateTime?(DateTime.Now)
				};
				Parent parent = parent1;
				parent.Password = global::BCrypt.Net.BCrypt.HashPassword(user.Parent.Password, 10);
				parent.LastLoginDateTime = new DateTime?(DateTime.Now);
				foreach (StudentItem studentItem in user.Students)
				{
					ICollection<Student> students = parent.Students;
					Student student = new Student()
					{
						FirstName = studentItem.FirstName,
						LastName = studentItem.LastName,
						GradeId = studentItem.GradeId,
						Gender = studentItem.Gender,
						BirthDate = studentItem.BirthDate.Value,
						Notes = studentItem.Notes
					};
					students.Add(student);
				}
				this.unitOfWork.ParentRepository.Insert(parent);
				this.unitOfWork.Save();
				return MembershipCreateStatus.Success;
			}
			catch (Exception exception)
			{
				membershipCreateStatu = MembershipCreateStatus.ProviderError;
			}
			return membershipCreateStatu;
		}

		private void DeleteAllTempBooking()
		{
			long id = this.CurrentParent.Id;
			List<Student> listStudent = this.unitOfWork.StudentRepository.Get((Student r) => r.ParentId == id, null, "").ToList<Student>();
			if (listStudent != null)
			{
				foreach (Student student in listStudent)
				{
					List<BookingShortInfoTemp> listBookingTempItem = this.unitOfWork.BookingTempRepository.Get((BookingShortInfoTemp r) => (r.ServiceType == Constants.BOOK_SERVICE_TYPE_CAMP) && r.StudentId == (long?)student.Id, null, "").ToList<BookingShortInfoTemp>();
					if (listBookingTempItem == null)
					{
						continue;
					}
					foreach (BookingShortInfoTemp bookItem in listBookingTempItem)
					{
						this.unitOfWork.BookingTempRepository.Delete(bookItem);
					}
				}
			}
			this.unitOfWork.Save();
		}

		[ActionName("enroll-camp-delete")]
		[Authorize]
		[HttpPost]
		public ActionResult DeleteCampEnroll(long classId, long studentId)
		{
			ChildBookingItem enrolledStudent = this.CampBookingInfo.FirstOrDefault<ChildBookingItem>((ChildBookingItem t) => t.Child.Id == studentId);
			BookingShortInfo enrolledCamp = enrolledStudent.Booking.First<BookingShortInfo>((BookingShortInfo t) => t.Id == classId);
			enrolledStudent.Booking.Remove(enrolledCamp);
			((dynamic)ViewBag).BookingType = 1;
			((dynamic)ViewBag).EnrollInfo = this.CampBookingInfo;
			if (enrolledStudent.Booking.Count == 0)
			{
				this.CampBookingInfo.Remove(enrolledStudent);
				List<BookingShortInfoTemp> listDeleted = this.unitOfWork.BookingTempRepository.Get((BookingShortInfoTemp r) => r.StudentId == (long?)studentId && r.classId == (long?)classId && (r.ServiceType == Constants.BOOK_SERVICE_TYPE_CAMP), null, "").ToList<BookingShortInfoTemp>();
				if (listDeleted != null)
				{
					foreach (BookingShortInfoTemp deletedItem in listDeleted)
					{
						this.unitOfWork.BookingTempRepository.Delete(deletedItem);
					}
					this.unitOfWork.Save();
				}
			}
			return base.PartialView("booking-list");
		}

		[ActionName("enroll-class-delete")]
		[Authorize]
		[HttpPost]
		public ActionResult DeleteClassEnroll(long classId, long studentId)
		{
			ChildBookingItem enrolledStudent = this.ClassBookingInfo.FirstOrDefault<ChildBookingItem>((ChildBookingItem t) => t.Child.Id == studentId);
			BookingShortInfo enrolledClass = enrolledStudent.Booking.First<BookingShortInfo>((BookingShortInfo t) => t.Id == classId);
			enrolledStudent.Booking.Remove(enrolledClass);
			((dynamic)ViewBag).BookingType = 0;
			if (enrolledStudent.Booking.Count == 0)
			{
				this.ClassBookingInfo.Remove(enrolledStudent);
				List<BookingShortInfoTemp> listDeleted = this.unitOfWork.BookingTempRepository.Get((BookingShortInfoTemp r) => r.StudentId == (long?)studentId && r.classId == (long?)classId && (r.ServiceType == Constants.BOOK_SERVICE_TYPE_CLASS), null, "").ToList<BookingShortInfoTemp>();
				if (listDeleted != null)
				{
					foreach (BookingShortInfoTemp deletedItem in listDeleted)
					{
						this.unitOfWork.BookingTempRepository.Delete(deletedItem);
					}
					this.unitOfWork.Save();
				}
			}
			((dynamic)ViewBag).EnrollInfo = this.ClassBookingInfo;
			return base.PartialView("booking-list");
		}

		[ActionName("student-delete")]
		[Authorize]
		[HttpPost]
		public ActionResult DeleteStudent(long id)
		{
			ActionResult actionResult;
			try
			{
				Student student = this.unitOfWork.StudentRepository.GetByID(id);
				if (student == null)
				{
					this.SetErrorMessages(Messages.DeleteStudentInfoFail, "");
					actionResult = this.PartialView("student-info", this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id).Students);
				}
				else
				{
					this.unitOfWork.StudentRepository.Delete(student);
					this.unitOfWork.Save();
					this.SetSuccessMessages(Messages.DeleteStudentInfoSuccess, "");
					actionResult = this.PartialView("student-info", this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id).Students);
				}
			}
			catch (Exception exception)
			{
				this.SetErrorMessages(Messages.DeleteStudentInfoFail, "");
				actionResult = this.PartialView("student-info", this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id).Students);
			}
			return actionResult;
		}

		protected override void Dispose(bool disposing)
		{
			this.unitOfWork.Dispose();
			base.Dispose(disposing);
		}

		[ActionName("account-edit-init")]
		[Authorize]
		[HttpPost]
		public ActionResult EditAccountInfo()
		{
			((dynamic)ViewBag).Locations = SelectLists.LocationsForFrontEnd(this.db, this.GetCurrentAreaId());
			Parent parent = this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id);
			ParentItem parentItem = new ParentItem()
			{
				Id = parent.Id,
				LocationId = parent.LocationId,
				LocationName = parent.Location.Name,
				Email = parent.Email,
				Password = parent.Password,
				FirstName = parent.FirstName,
				LastName = parent.LastName,
				PhoneNumer = parent.PhoneNumer,
				Address = parent.Address,
				City = parent.City,
				State = parent.State,
				Zip = parent.Zip
			};
			return this.PartialView("account-edit", parentItem);
		}

		[ActionName("account-edit")]
		[Authorize]
		[HttpPost]
		public ActionResult EditAccountInfo(ParentItem model)
		{
			ModelState.Remove("Password");
			if (ModelState.IsValid)
			{
				try
				{
					Parent updateParent = this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id);
					updateParent.LocationId = model.LocationId;
					updateParent.Email = model.Email;
					updateParent.FirstName = model.FirstName;
					updateParent.LastName = model.LastName;
					updateParent.PhoneNumer = model.PhoneNumer;
					updateParent.Address = model.Address;
					updateParent.City = model.City;
					updateParent.State = model.State;
					updateParent.Zip = model.Zip;
					this.unitOfWork.Save();
					this.SetSuccessMessages(Messages.SaveAccountInfoSuccess, "");
				}
				catch
				{
					this.SetErrorMessages(Messages.SaveAccountInfoFail, "");
				}
			}
			else
			{
				System.Web.HttpContext.Current.Response.StatusCode = 600;
				System.Web.HttpContext.Current.Response.TrySkipIisCustomErrors = true;
			}
			((dynamic)ViewBag).Locations = SelectLists.LocationsForFrontEnd(this.db, this.GetCurrentAreaId());
			return this.PartialView("Account-Edit", model);
		}

		[ActionName("student-edit-init")]
		[HttpPost]
		public ActionResult EditStudentInfo(long id)
		{
			Student student = this.unitOfWork.StudentRepository.GetByID(id);
			if (student == null)
			{
				return base.PartialView("student-edit");
			}
			if (student.ParentId != this.CurrentParent.Id)
			{
			    return base.PartialView("Cant not edit not owner student");
			}
			TempData.Clear();
			StudentItem studentItem = new StudentItem()
			{
				Id = student.Id,
				ParentId = student.ParentId,
				FirstName = student.FirstName,
				LastName = student.LastName,
				GradeId = student.GradeId,
				Gender = student.Gender,
				BirthDate = new DateTime?(student.BirthDate),
				Notes = student.Notes
			};
			return this.PartialView("student-edit", studentItem);
		}

		[ActionName("student-edit")]
		[Authorize]
		[HttpPost]
		public ActionResult EditStudentInfo(StudentItem model)
		{
			ActionResult actionResult;
			try
			{
				Student updateStudent = this.unitOfWork.StudentRepository.GetByID(model.Id);
				updateStudent.FirstName = model.FirstName;
				updateStudent.LastName = model.LastName;
				updateStudent.GradeId = model.GradeId;
				updateStudent.Gender = model.Gender;
				updateStudent.BirthDate = model.BirthDate.Value;
				updateStudent.Notes = model.Notes;
				this.unitOfWork.Save();
				this.SetSuccessMessages(Messages.EditStudentInfoSuccess, "");
				actionResult = this.PartialView("student-info", this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id).Students);
			}
			catch
			{
				this.SetErrorMessages(Messages.EditStudentInfoFail, "");
				actionResult = this.PartialView("student-edit", model);
			}
			return actionResult;
		}

		[ActionName("enroll-camp")]
		[Authorize]
		[HttpPost]
		public ActionResult EnrollCamp(long classId, long studentId)
		{
			Camp byID = this.unitOfWork.CampRepository.GetByID(classId);
			Student student = this.unitOfWork.StudentRepository.GetByID(studentId);
			int isBookingCount = this.CampBookingInfo.Sum<ChildBookingItem>((ChildBookingItem t) => t.Booking.Count<BookingShortInfo>((BookingShortInfo x) => x.Id == byID.Id));
			int booked = (byID.Enrolled.HasValue ? byID.Enrolled.Value : 0);
			if (booked >= byID.MaxEnroll)
			{
				return this.ReturnResponseErrorMsg(string.Format(Messages.ClassCampFull, "Camp"));
			}
			if (booked + isBookingCount >= byID.MaxEnroll)
			{
				return this.ReturnResponseErrorMsg(string.Format(Messages.ClassCampFull, "Camp"));
			}
			if (this.CampBookingInfo != null && this.CampBookingInfo.Any<ChildBookingItem>((ChildBookingItem t) => {
				if (t.Child.Id != studentId)
				{
					return false;
				}
				return t.Booking.Any<BookingShortInfo>((BookingShortInfo k) => k.Id == classId);
			}))
			{
				return this.ReturnResponseErrorMsg(string.Format(Messages.ChildAlreadyEnroll, "camp"));
			}
			if (this.CampBookingInfo == null)
			{
				this.CampBookingInfo = new List<ChildBookingItem>();
			}
			if (this.ClassBookingInfo == null)
			{
				this.ClassBookingInfo = new List<ChildBookingItem>();
			}
			ChildBookingItem enrolledStudent = this.CampBookingInfo.FirstOrDefault<ChildBookingItem>((ChildBookingItem t) => t.Child.Id == studentId);
			if (enrolledStudent != null)
			{
				List<BookingShortInfo> booking = enrolledStudent.Booking;
				BookingShortInfo bookingShortInfo = new BookingShortInfo()
				{
					Type = ServiceType.Class,
					Id = byID.Id,
					Name = byID.OnlineName,
					Dates = StringUtil.GetShortDateList((
						from t in byID.Assigns
						orderby t.Date
						select t into k
						select k.Date).ToList<DateTime>()),
					Cost = byID.Cost
				};
				booking.Add(bookingShortInfo);
			}
			else
			{
				List<ChildBookingItem> campBookingInfo = this.CampBookingInfo;
				ChildBookingItem childBookingItem = new ChildBookingItem()
				{
					Child = student
				};
				ChildBookingItem childBookingItem1 = childBookingItem;
				List<BookingShortInfo> bookingShortInfos = new List<BookingShortInfo>();
				List<BookingShortInfo> bookingShortInfos1 = bookingShortInfos;
				BookingShortInfo bookingShortInfo1 = new BookingShortInfo()
				{
					Type = ServiceType.Camp,
					Id = byID.Id,
					Name = byID.OnlineName,
					Dates = StringUtil.GetShortDateList((
						from t in byID.Assigns
						orderby t.Date
						select t into k
						select k.Date).ToList<DateTime>()),
					Cost = byID.Cost
				};
				bookingShortInfos1.Add(bookingShortInfo1);
				childBookingItem1.Booking = bookingShortInfos;
				campBookingInfo.Add(childBookingItem);
			}
			GenericRepository<BookingShortInfoTemp> bookingTempRepository = this.unitOfWork.BookingTempRepository;
			BookingShortInfoTemp bookingShortInfoTemp = new BookingShortInfoTemp()
			{
				StudentId = new long?(student.Id),
				ServiceType = Constants.BOOK_SERVICE_TYPE_CAMP,
				classId = new long?(byID.Id),
				Name = byID.OnlineName,
				Dates = StringUtil.GetShortDateList((
					from t in byID.Assigns
					orderby t.Date
					select t into k
					select k.Date).ToList<DateTime>()),
				Cost = new decimal?(byID.Cost),
				CouponId = (long?)base.GetSession("AppliedCoupondId")
			};
			bookingTempRepository.Insert(bookingShortInfoTemp);
			this.unitOfWork.Save();
			((dynamic)ViewBag).EnrollInfo = this.CampBookingInfo;
			return base.PartialView("booking-list");
		}

		[ActionName("enroll-class")]
		[Authorize]
		[HttpPost]
		public ActionResult EnrollClass(long classId, long studentId)
		{
			Class byID = this.unitOfWork.ClassRepository.GetByID(classId);
			Student student = this.unitOfWork.StudentRepository.GetByID(studentId);
			int isBookingCount = this.ClassBookingInfo.Sum<ChildBookingItem>((ChildBookingItem t) => t.Booking.Count<BookingShortInfo>((BookingShortInfo x) => x.Id == byID.Id));
			int booked = (byID.Enrolled.HasValue ? byID.Enrolled.Value : 0);
			if (booked >= byID.MaxEnroll)
			{
				return this.ReturnResponseErrorMsg(string.Format(Messages.ClassCampFull, "Class"));
			}
			if (booked + isBookingCount >= byID.MaxEnroll)
			{
				return this.ReturnResponseErrorMsg(string.Format(Messages.ClassCampFull, "Class"));
			}
			if (this.ClassBookingInfo != null && this.ClassBookingInfo.Any<ChildBookingItem>((ChildBookingItem t) => {
				if (t.Child.Id != studentId)
				{
					return false;
				}
				return t.Booking.Any<BookingShortInfo>((BookingShortInfo k) => k.Id == classId);
			}))
			{
				return this.ReturnResponseErrorMsg(string.Format(Messages.ChildAlreadyEnroll, "class"));
			}
			if (this.ClassBookingInfo == null)
			{
				this.ClassBookingInfo = new List<ChildBookingItem>();
			}
			ChildBookingItem enrolledStudent = this.ClassBookingInfo.FirstOrDefault<ChildBookingItem>((ChildBookingItem t) => t.Child.Id == studentId);
			if (enrolledStudent != null)
			{
				List<BookingShortInfo> booking = enrolledStudent.Booking;
				BookingShortInfo bookingShortInfo = new BookingShortInfo()
				{
					Type = ServiceType.Class,
					Id = byID.Id,
					Name = byID.OnlineName,
					Dates = StringUtil.GetShortDateList((
						from t in byID.Assigns
						orderby t.Date
						select t into k
						select k.Date).ToList<DateTime>()),
					Cost = byID.Cost
				};
				booking.Add(bookingShortInfo);
			}
			else
			{
				List<ChildBookingItem> classBookingInfo = this.ClassBookingInfo;
				ChildBookingItem childBookingItem = new ChildBookingItem()
				{
					Child = student
				};
				ChildBookingItem childBookingItem1 = childBookingItem;
				List<BookingShortInfo> bookingShortInfos = new List<BookingShortInfo>();
				List<BookingShortInfo> bookingShortInfos1 = bookingShortInfos;
				BookingShortInfo bookingShortInfo1 = new BookingShortInfo()
				{
					Type = ServiceType.Class,
					Id = byID.Id,
					Name = byID.OnlineName,
					Dates = StringUtil.GetShortDateList((
						from t in byID.Assigns
						orderby t.Date
						select t into k
						select k.Date).ToList<DateTime>()),
					Cost = byID.Cost
				};
				bookingShortInfos1.Add(bookingShortInfo1);
				childBookingItem1.Booking = bookingShortInfos;
				classBookingInfo.Add(childBookingItem);
			}
			GenericRepository<BookingShortInfoTemp> bookingTempRepository = this.unitOfWork.BookingTempRepository;
			BookingShortInfoTemp bookingShortInfoTemp = new BookingShortInfoTemp()
			{
				StudentId = new long?(student.Id),
				ServiceType = Constants.BOOK_SERVICE_TYPE_CLASS,
				classId = new long?(byID.Id),
				Name = byID.OnlineName,
				Dates = StringUtil.GetShortDateList((
					from t in byID.Assigns
					orderby t.Date
					select t into k
					select k.Date).ToList<DateTime>()),
				Cost = new decimal?(byID.Cost),
				CouponId = (long?)base.GetSession("AppliedCoupondId")
			};
			bookingTempRepository.Insert(bookingShortInfoTemp);
			this.unitOfWork.Save();
			((dynamic)ViewBag).EnrollInfo = this.ClassBookingInfo;
			return base.PartialView("booking-list");
		}

		[ActionName("init-enroll-confirm")]
		[HttpPost]
		public ActionResult EnrollConfirm()
		{
			int totalEnroll = 0;
			if (this.ClassBookingInfo != null)
			{
				totalEnroll = totalEnroll + this.ClassBookingInfo.Sum<ChildBookingItem>((ChildBookingItem t) => t.Booking.Count);
			}
			if (this.CampBookingInfo != null)
			{
				totalEnroll = totalEnroll + this.CampBookingInfo.Sum<ChildBookingItem>((ChildBookingItem t) => t.Booking.Count);
			}
			if (totalEnroll == 0)
			{
				return this.ReturnResponseErrorMsg(Messages.NotEnrolledBeforePayment);
			}
			decimal originalTotalFees = new decimal(0);
			if (this.ClassBookingInfo != null)
			{
				originalTotalFees = originalTotalFees + this.ClassBookingInfo.Sum<ChildBookingItem>((ChildBookingItem t) => t.Booking.Sum<BookingShortInfo>((BookingShortInfo x) => x.Cost));
			}
			if (this.CampBookingInfo != null)
			{
				originalTotalFees = originalTotalFees + this.CampBookingInfo.Sum<ChildBookingItem>((ChildBookingItem t) => t.Booking.Sum<BookingShortInfo>((BookingShortInfo x) => x.Cost));
			}
			base.SetSession("PaymentDescription", this.GetPaymentDescription());
			base.SetSession("TotalFees", originalTotalFees);
			((dynamic)ViewBag).ClassEnrollInfo = this.ClassBookingInfo;
			((dynamic)ViewBag).CampEnrollInfo = this.CampBookingInfo;
			return base.PartialView("enroll-confirm");
		}

		protected static string ErrorCodeToString(MembershipCreateStatus createStatus)
		{
			if (createStatus == MembershipCreateStatus.DuplicateEmail)
			{
				return "A username for that e-mail address already exists. Please enter a different e-mail address.";
			}
			return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
		}

		[ActionName("get-coupon")]
		[HttpPost]
		public ActionResult GetCoupon(string couponCode, decimal totalFees)
		{
			Coupon coupon = this.unitOfWork.CouponRepository.Get((Coupon t) => t.Code == couponCode, null, "").FirstOrDefault<Coupon>();
			if (coupon == null)
			{
				return base.Json(new { ReturnStatus = "false", ErrorMessage = "Invalid coupon.", AdjustmentFees = 0, RemainFees = totalFees });
			}
			if (coupon.ExpDate < DateTime.Today)
			{
				return base.Json(new { ReturnStatus = "false", ErrorMessage = "This coupon has expired.", AdjustmentFees = 0, RemainFees = totalFees });
			}
			var usedCount = (
				from t in coupon.Bookings
				group t by new { ParentId = t.Student.ParentId, BookDate = t.BookDate } into g
				select new { ParentId = g.Key.ParentId, BookDate = g.Key.BookDate, BookingCount = g.Count<Booking>() }).ToList();
			if (usedCount.Count() >= coupon.MaxAvailable)
			{
				return base.Json(new { ReturnStatus = "false", ErrorMessage = "This coupon reach max use time limit.", AdjustmentFees = 0, RemainFees = totalFees });
			}
			if (usedCount.Count((x) => x.ParentId == base.User.Parent.Id) >= coupon.MaxUsesPerCustomer)
			{
				return base.Json(new { ReturnStatus = "false", ErrorMessage = "Your use times for this coupon reach limit.", AdjustmentFees = 0, RemainFees = totalFees });
			}
			decimal adjusmentAmount = this.CalculateAdjusmentAmount(coupon, totalFees);
			base.SetSession("TotalFees", totalFees - adjusmentAmount);
			base.SetSession("AppliedCoupondId", coupon.Id);
			return base.Json(new { ReturnStatus = "true", AdjustmentFees = adjusmentAmount, RemainFees = totalFees - adjusmentAmount });
		}

		private Admin GetCurrentAdmin()
		{
			string str = Convert.ToString(RouteData.Values["location"]);
			return this.unitOfWork.AreaRepository.Get(t => t.Name.ToLower() == str.ToLower(), null, "").FirstOrDefault<Area>()?.Admins.First<Admin>();
		}

		private Area GetCurrentArea()
		{
			string str = RouteData.Values["location"].ToString();
			return this.unitOfWork.AreaRepository.Get((Area t) => t.Name.ToLower() == str.ToLower(), null, "").FirstOrDefault<Area>();
		}

		private long GetCurrentAreaId()
		{
			string str = Convert.ToString(RouteData.Values["location"]);
			return unitOfWork.AreaRepository.Get(t => t.Name.ToLower() == str.ToLower(), null, "").FirstOrDefault().Id;
		}

		private string GetPaymentDescription()
		{
			StringBuilder paymentDescription = new StringBuilder();
			List<long> classes = new List<long>();
			List<long> camps = new List<long>();
			if (this.ClassBookingInfo != null)
			{
				foreach (ChildBookingItem chilClass in this.ClassBookingInfo)
				{
					foreach (BookingShortInfo childBook in chilClass.Booking)
					{
						if (classes.Contains(childBook.Id))
						{
							continue;
						}
						classes.Add(childBook.Id);
					}
				}
			}
			if (this.CampBookingInfo != null)
			{
				foreach (ChildBookingItem chilCamp in this.CampBookingInfo)
				{
					foreach (BookingShortInfo childBook in chilCamp.Booking)
					{
						if (camps.Contains(childBook.Id))
						{
							continue;
						}
						camps.Add(childBook.Id);
					}
				}
			}
			foreach (long num in classes)
			{
				Class dbClass = this.unitOfWork.ClassRepository.GetByID(num);
				foreach (ChildBookingItem childBook in this.ClassBookingInfo)
				{
					if (!childBook.Booking.Any<BookingShortInfo>((BookingShortInfo t) => t.Id == num))
					{
						continue;
					}
					object[] displayName = new object[] { dbClass.Location.DisplayName, dbClass.Name, childBook.Child.FirstName, childBook.Child.LastName };
					paymentDescription.AppendFormat("{0} - {1} - {2} {3}", displayName);
					paymentDescription.AppendLine();
				}
			}
			foreach (long num1 in camps)
			{
				Camp dbCamp = this.unitOfWork.CampRepository.GetByID(num1);
				foreach (ChildBookingItem childBook in this.CampBookingInfo)
				{
					if (!childBook.Booking.Any<BookingShortInfo>((BookingShortInfo t) => t.Id == num1))
					{
						continue;
					}
					object[] objArray = new object[] { dbCamp.Location.DisplayName, dbCamp.Name, childBook.Child.FirstName, childBook.Child.LastName };
					paymentDescription.AppendFormat("{0} - {1} - {2} {3}", objArray);
					paymentDescription.AppendLine();
				}
			}
			this.log.Debug(string.Concat("Payment Description: ", paymentDescription.ToString()));
			return paymentDescription.ToString();
		}

		[ActionName("help-question")]
		[HttpPost]
		public ActionResult HelpQuestion(int id)
		{
			return base.PartialView(this.unitOfWork.AdminFrontendRepository.GetAdminFrontendItem(id, this.GetCurrentAreaId()));
		}

		[AllowAnonymous]
		[HttpGet]
		public ActionResult Home(string location)
		{
			((dynamic)ViewBag).SelectedMenuItem = "Home";
			((dynamic)ViewBag).Admin = this.GetCurrentAdmin();
			if (!System.Web.HttpContext.Current.Request.IsAuthenticated)
			{
				return base.RedirectToAction("Login");
			}
			return base.RedirectToAction("Account-info");
		}

		[HttpGet]
		public ActionResult Index()
		{
			((dynamic)ViewBag).SelectedMenuItem = "Home";
			((dynamic)ViewBag).Admin = this.GetCurrentAdmin();
			if (!base.User.Identity.IsAuthenticated)
			{
				return base.RedirectToAction("login");
			}
			return base.RedirectToAction("account-info");
		}

		[AllowAnonymous]
		[HttpGet]
		public ActionResult LogIn()
		{
			if (!this.CheckLocation())
			{
				return base.RedirectToAction("area-not-found");
			}
			((dynamic)ViewBag).LocationName = this.GetCurrentArea().DisplayName;
			((dynamic)ViewBag).SelectedMenuItem = "Home";
			((dynamic)ViewBag).PhoneNumber = this.GetCurrentAdmin().PhoneNumber;
			base.SetSession("CurrentAdmin", this.GetCurrentAdmin());
			return base.View();
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult LogIn(LoginForm model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				Parent parent = this.unitOfWork.ParentRepository.Get((Parent t) => t.Email == model.Username, null, "").FirstOrDefault<Parent>();
				if (parent != null)
				{
					global::BCrypt.Net.BCrypt.Verify(model.Password, parent.Password);
				}
				if (base.ValidateUser(model.Username, model.Password))
				{
					base.SignIn(model.Username, model.SaveLogin);
					if (this.Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
					{
						return this.Redirect(returnUrl);
					}
					return base.RedirectToAction("account-info");
				}
				ModelState.AddModelError("", "The user name or password provided is incorrect.");
			}
			((dynamic)ViewBag).LocationName = this.GetCurrentArea().DisplayName;
			return base.View(model);
		}

		[HttpGet]
		public ActionResult LoginMessages()
		{
			TempData.Add("ErrorMain", "Thank You For Registering!");
			TempData.Add("ErrorSub", "Hello ! Welcome to your account page! From here you have the ability to review/edit your information, register for classes and see your current class schedule.");
			return base.PartialView();
		}

		[HttpGet]
		public ActionResult Logout()
		{
			this.ClassBookingInfo = null;
			base.SignOut();
			return base.RedirectToAction("Home");
		}

		private IGateway OpenGateway()
		{
			long currentAreaId = this.GetCurrentAreaId();
			Admin admin = this.unitOfWork.AdminRepository.Get((Admin t) => t.AreaId.Value == currentAreaId, null, "").FirstOrDefault<Admin>();
			AdminPaymentInfo paymentInfo = admin.AdminPaymentInfoes.FirstOrDefault<AdminPaymentInfo>();
			return new Gateway(paymentInfo.APILoginID, paymentInfo.TransactionKey, false);
		}

		[HttpPost]
		public ActionResult Payment(Payment payment)
		{
			this.log.Debug(string.Concat("Start call method payment.", payment.Bill_Email));
			IGateway gate = this.OpenGateway();
			IGatewayRequest apiRequest = CheckoutFormReaders.BuildAuthAndCaptureFromPost();
			string paymentDesscription = Convert.ToString(base.GetSession("PaymentDescription"));
			AuthorizationRequest gatewayRequest = new AuthorizationRequest(apiRequest.CardNum, apiRequest.ExpDate, Convert.ToDecimal(apiRequest.Amount), paymentDesscription)
			{
				FirstName = payment.Bill_FirstName,
				LastName = payment.Bill_LastName,
				Address = payment.Bill_Address,
				Email = payment.Bill_Email,
				City = payment.Bill_City,
				Zip = payment.Bill_Zip,
				State = payment.Bill_State
			};
			IGatewayResponse response = gate.Send(gatewayRequest);
			Convert.ToDecimal(base.GetSession("TotalFees"));
			if (!response.Approved)
			{
				payment.PaymentMessage = response.Message;
				((dynamic)ViewBag).TotalFees = Convert.ToDecimal(base.GetSession("TotalFees"));
				return this.PartialView("payment-error", payment);
			}
			payment.AuthCode = response.AuthorizationCode;
			payment.TransactionID = response.TransactionID;
			payment.PaymentMessage = Messages.PaymentSuccess;
			this.unitOfWork.PaymentRepository.Insert(payment);
			long? appliedCouponId = (long?)base.GetSession("AppliedCoupondId");
			if (this.ClassBookingInfo != null)
			{
				foreach (ChildBookingItem chilClass in this.ClassBookingInfo)
				{
					foreach (BookingShortInfo childBook in chilClass.Booking)
					{
						GenericRepository<Booking> bookingRepository = this.unitOfWork.BookingRepository;
						Booking booking = new Booking()
						{
							StudentId = chilClass.Child.Id,
							ClassId = new long?(childBook.Id),
							BookDate = DateTime.Today,
							CouponId = appliedCouponId
						};
						bookingRepository.Insert(booking);
					}
				}
			}
			if (this.CampBookingInfo != null)
			{
				foreach (ChildBookingItem chilCamp in this.CampBookingInfo)
				{
					foreach (BookingShortInfo childBook in chilCamp.Booking)
					{
						GenericRepository<Booking> genericRepository = this.unitOfWork.BookingRepository;
						Booking booking1 = new Booking()
						{
							StudentId = chilCamp.Child.Id,
							CampId = new long?(childBook.Id),
							BookDate = DateTime.Today,
							CouponId = appliedCouponId
						};
						genericRepository.Insert(booking1);
					}
				}
			}
			this.unitOfWork.Save();
			this.ClassBookingInfo = null;
			this.CampBookingInfo = null;
			System.Web.HttpContext.Current.Session["TotalFees"] = null;
			this.DeleteAllTempBooking();
			return this.PartialView("payment-detail", payment);
		}

		[ActionName("payment-fail")]
		[HttpGet]
		public ActionResult PaymentFail()
		{
			Payment payment = new Payment()
			{
				PaymentMessage = "Your payment request have been process fail."
			};
			long currentAreaId = this.GetCurrentAreaId();
			((dynamic)ViewBag).HelpQuestions = this.unitOfWork.AdminFrontendRepository.GetAdminFrontendItems(currentAreaId);
			return base.View("payment-error", payment);
		}

		[ActionName("payment-init")]
		[HttpPost]
		public ActionResult PaymentInit()
		{
			decimal totalAmount = Convert.ToDecimal(base.GetSession("TotalFees"));
			((dynamic)ViewBag).TotalFees = totalAmount;
			Payment payment1 = new Payment()
			{
				AdminId = new long?(this.GetCurrentAdmin().Id),
				ParentId = this.CurrentParent.Id,
				Amount = totalAmount,
				Bill_FirstName = this.CurrentParent.FirstName,
				Bill_LastName = this.CurrentParent.LastName,
				Bill_Address = this.CurrentParent.Address,
				Bill_City = this.CurrentParent.City,
				Bill_State = this.CurrentParent.State,
				Bill_Zip = this.CurrentParent.Zip,
				Bill_Email = this.CurrentParent.Email
			};
			Payment payment = payment1;
			Parent parent = (
				from r in this.db.Parents
				where r.Id == this.CurrentParent.Id
				select r).FirstOrDefault<Parent>();
			string empty = string.Empty;
			string parentName = string.Empty;
			if (parent != null)
			{
				string email = parent.Email;
				parentName = string.Format("{0} {1}", parent.FirstName, parent.LastName);
			}
			string description = this.GetPaymentDescription();
			if (string.IsNullOrEmpty(description))
			{
				description = string.Format("Payment of parent {0}", parentName);
			}
			long currentAreaId = this.GetCurrentAreaId();
			Admin admin = this.unitOfWork.AdminRepository.Get((Admin t) => t.AreaId.Value == currentAreaId, null, "").FirstOrDefault<Admin>();
			AdminPaymentInfo paymentInfo = admin.AdminPaymentInfoes.FirstOrDefault<AdminPaymentInfo>();
			if (paymentInfo == null)
			{
				payment.PaymentMessage = "Payment informtion is empty. Please config your payment account.";
				return this.PartialView("payment-error", payment);
			}
			string returnURL = string.Concat(PayPalSettings.BaseReturnUrl, this.Url.Action("/payment-success"));
			string cancelURL = string.Concat(PayPalSettings.BaseReturnUrl, this.Url.Action("/payment-fail"));
			string message = string.Empty;
			PayPalOrder payPalOrder = new PayPalOrder()
			{
				Amount = totalAmount,
				Name = string.Format("Payment of parent {0}", parentName),
				Description = description,
				ReturnUrl = returnURL,
				CancelUrl = cancelURL,
				UserName = paymentInfo.APILoginID,
				Password = paymentInfo.TransactionKey,
				Signature = paymentInfo.MD5HashPhrase,
				OverrideAddress = true,
				FirstName = this.CurrentParent.FirstName,
				LastName = this.CurrentParent.LastName,
				Address = this.CurrentParent.Address,
				City = this.CurrentParent.City,
				State = this.CurrentParent.State,
				Zip = this.CurrentParent.Zip,
				Email = this.CurrentParent.Email
			};
			PayPalRedirect redirect = Utility.PayPal.ExpressCheckout(payPalOrder, out message);
			if (!string.IsNullOrEmpty(message))
			{
				payment.PaymentMessage = message;
				return base.Content(string.Concat("{\"error\": \"1\", \"message\": \"", message, "\"}"));
			}
			System.Web.HttpContext.Current.Session["token"] = redirect.Token;
			return base.Content(string.Concat("{\"error\": \"0\", \"message\": \"", redirect.Url, "\"}"));
		}

		[ActionName("payment-success")]
		[HttpGet]
		public ActionResult PaymentSuccess(string token, string PayerID)
		{
			object sessionToken = System.Web.HttpContext.Current.Session["token"];
			if (sessionToken == null || sessionToken.ToString() != token)
			{
				Payment paymentView = new Payment()
				{
					PaymentMessage = "Your payment request have been process fail."
				};
				return this.PartialView("payment-error", paymentView);
			}
			decimal fees = Convert.ToDecimal(base.GetSession("TotalFees"));
			Payment payment1 = new Payment()
			{
				AdminId = new long?(this.GetCurrentAdmin().Id),
				ParentId = this.CurrentParent.Id,
				Amount = fees,
				Bill_FirstName = this.CurrentParent.FirstName,
				Bill_LastName = this.CurrentParent.LastName,
				Bill_Address = this.CurrentParent.Address,
				Bill_City = this.CurrentParent.City,
				Bill_State = this.CurrentParent.State,
				Bill_Zip = this.CurrentParent.Zip,
				Bill_Email = this.CurrentParent.Email
			};
			Payment payment = payment1;
			long num = this.GetCurrentAreaId();
			Admin admin = this.unitOfWork.AdminRepository.Get((Admin t) => t.AreaId.Value == num, null, "").FirstOrDefault<Admin>();
			AdminPaymentInfo paymentInfo = admin.AdminPaymentInfoes.FirstOrDefault<AdminPaymentInfo>();
			if (paymentInfo == null)
			{
				payment.PaymentMessage = "Payment informtion is empty. Please config your payment account.";
				return this.PartialView("payment-error", payment);
			}
			PayPalOrder payPalOrder = new PayPalOrder()
			{
				Amount = fees,
				Token = token,
				PayerID = PayerID,
				UserName = paymentInfo.APILoginID,
				Password = paymentInfo.TransactionKey,
				Signature = paymentInfo.MD5HashPhrase
			};
			string transactionID = Utility.PayPal.DoExpressCheckoutPayment(payPalOrder);
			payment.AuthCode = token;
			payment.TransactionID = (transactionID != null ? transactionID.ToString() : string.Empty);
			payment.PaymentMessage = Messages.PaymentSuccess;
			this.unitOfWork.PaymentRepository.Insert(payment);
			long? appliedCouponId = (long?)base.GetSession("AppliedCoupondId");
			if (this.ClassBookingInfo != null)
			{
				foreach (ChildBookingItem chilClass in this.ClassBookingInfo)
				{
					foreach (BookingShortInfo childBook in chilClass.Booking)
					{
						GenericRepository<Booking> bookingRepository = this.unitOfWork.BookingRepository;
						Booking booking = new Booking()
						{
							StudentId = chilClass.Child.Id,
							ClassId = new long?(childBook.Id),
							BookDate = DateTime.Today,
							CouponId = appliedCouponId
						};
						bookingRepository.Insert(booking);
					}
				}
			}
			if (this.CampBookingInfo != null)
			{
				foreach (ChildBookingItem chilCamp in this.CampBookingInfo)
				{
					foreach (BookingShortInfo childBook in chilCamp.Booking)
					{
						GenericRepository<Booking> genericRepository = this.unitOfWork.BookingRepository;
						Booking booking1 = new Booking()
						{
							StudentId = chilCamp.Child.Id,
							CampId = new long?(childBook.Id),
							BookDate = DateTime.Today,
							CouponId = appliedCouponId
						};
						genericRepository.Insert(booking1);
					}
				}
			}
			this.unitOfWork.Save();
			this.ClassBookingInfo = null;
			this.CampBookingInfo = null;
			System.Web.HttpContext.Current.Session["TotalFees"] = null;
			long currentAreaId = this.GetCurrentAreaId();
			((dynamic)ViewBag).HelpQuestions = this.unitOfWork.AdminFrontendRepository.GetAdminFrontendItems(currentAreaId);
			return base.View("payment-detail", payment);
		}

		[AllowAnonymous]
		[HttpGet]
		public ActionResult Register(string location)
		{
			APVariable terms = this.db.APVariables.FirstOrDefault<APVariable>((APVariable t) => t.Name == "TEARM");
			if (terms == null)
			{
				((dynamic)ViewBag).Terms = "No Terms & Condition Required";
			}
			else
			{
				((dynamic)ViewBag).Terms = terms.Value;
			}
			((dynamic)ViewBag).Locations = SelectLists.LocationsForFrontEnd(this.db, this.GetCurrentAreaId());
			((dynamic)ViewBag).SelectedMenuItem = "Home";
			((dynamic)ViewBag).Location = location;
			System.Web.HttpContext.Current.Session["Register.Parent"] = null;
			System.Web.HttpContext.Current.Session["Register.Students"] = null;
			return base.View();
		}

		[ActionName("Register")]
		[AllowAnonymous]
		[HttpPost]
		public ActionResult RegisterFinish(string location)
		{
			UserItem userItem = new UserItem()
			{
				Parent = (ParentItem)System.Web.HttpContext.Current.Session["Register.Parent"],
				Students = (List<StudentItem>)System.Web.HttpContext.Current.Session["Register.Students"]
			};
			UserItem user = userItem;
			if (this.ValidateRegistration(user.Parent.Email, user.Parent.Password, user.Parent.Password))
			{
				MembershipCreateStatus createStatus = this.CreateUser(user);
				if (createStatus == MembershipCreateStatus.Success)
				{
					base.SignIn(user.Parent.Email, false);
					this.SetSuccessMessages(Messages.FirstLoginMain, Messages.FirstLoginSub);
					System.Web.HttpContext.Current.Session["Register.Parent"] = null;
					System.Web.HttpContext.Current.Session["Register.Students"] = null;
					try
					{
						this.UserMailer.WelcomeToNewUser(user.Parent).Send(EmailUtil.CreateStmtClientWrapperForAdmin(this.db, this.GetCurrentAdmin()));
					}
					catch (Exception exception)
					{
						this.log.Error(exception);
					}
					return this.Redirect(this.Url.RouteUrl("Users", new { action = "account-info", location = location }));
				}
				ModelState.AddModelError("", UsersController.ErrorCodeToString(createStatus));
			}
			return base.View("Register");
		}

		[ActionName("password-reset")]
		[AllowAnonymous]
		[HttpGet]
		public ActionResult ResetPassword()
		{
			return base.View(new ResetPasswordForm());
		}

		[ActionName("password-reset")]
		[AllowAnonymous]
		[HttpPost]
		public ActionResult ResetPassword(ResetPasswordForm model)
		{
			Parent parent = this.unitOfWork.ParentRepository.Get((Parent t) => t.Email == model.Email, null, "").FirstOrDefault<Parent>();
			if (parent == null)
			{
				ModelState.AddModelError("", Messages.EmailNotFound);
				return base.View(model);
			}
			Guid guid = Guid.NewGuid();
			string randomPassword = guid.ToString("N").Substring(0, 8);
			parent.Password = global::BCrypt.Net.BCrypt.HashPassword(randomPassword, 10);
			this.unitOfWork.Save();
			try
			{
				this.UserMailer.PasswordReset(parent, randomPassword).Send(new SmtpClientWrapper(EmailUtil.GetSystemSmtpClient(this.db)));
			}
			catch (Exception exception)
			{
				this.log.Error(exception);
			}
			((dynamic)ViewBag).Message = Messages.ResetPasswordSuccess;
			return base.View(new ResetPasswordForm());
		}

		private ActionResult ReturnResponseErrorMsg(string msg)
		{
		    System.Web.HttpContext.Current.Response.StatusCode = 600;
			System.Web.HttpContext.Current.Response.TrySkipIisCustomErrors = true;
			return base.Content(msg);
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult SignupFrstStep(string location, ParentItem model)
		{
			if (this.unitOfWork.ParentRepository.Get((Parent t) => t.Email == model.Email, null, "").Count<Parent>() > 0)
			{
				ModelState.AddModelError("", "Email Address has been registed by other user.");
			}
			if (ModelState.IsValid)
			{
				System.Web.HttpContext.Current.Session["Register.Parent"] = model;
			}
			else
			{
				System.Web.HttpContext.Current.Response.StatusCode = 600;
				System.Web.HttpContext.Current.Response.TrySkipIisCustomErrors = true;
			}
			((dynamic)ViewBag).Locations = SelectLists.LocationsForFrontEnd(this.db, this.GetCurrentAreaId());
			return base.PartialView(model);
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult SignupScndStep(StudentItem model, string type)
		{
			if (ModelState.IsValid)
			{
				List<StudentItem> students = (List<StudentItem>)System.Web.HttpContext.Current.Session["Register.Students"];
				if (students == null)
				{
					students = new List<StudentItem>();
					System.Web.HttpContext.Current.Session["Register.Students"] = students;
				}
				students.Add(model);
			}
			else
			{
				System.Web.HttpContext.Current.Response.StatusCode = 600;
				System.Web.HttpContext.Current.Response.TrySkipIisCustomErrors = true;
			}
			ModelState.Clear();
			if (string.IsNullOrEmpty(type))
			{
				return base.PartialView(model);
			}
			return base.PartialView(new StudentItem());
		}

		[ActionName("student-info")]
		[Authorize]
		[HttpPost]
		public ActionResult StudentInfo()
		{
			return base.PartialView(this.unitOfWork.ParentRepository.GetByID(this.CurrentParent.Id).Students);
		}

		protected bool ValidateRegistration(string email, string password, string confirmPassword)
		{
			if (string.IsNullOrEmpty(email))
			{
				ModelState.AddModelError("email", "You must specify a email.");
			}
			if (password == null || password.Length < 6)
			{
				ModelStateDictionary modelState = ModelState;
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				object[] objArray = new object[] { 6 };
				modelState.AddModelError("password", string.Format(currentCulture, "You must specify a password of {0} or more characters.", objArray));
			}
			if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
			{
				ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
			}
			if (this.unitOfWork.ParentRepository.Get((Parent t) => t.Email == email, null, "").Count<Parent>() > 0)
			{
				ModelState.AddModelError("_FORM", "Email Address has been registed by other user.");
			}
			return ModelState.IsValid;
		}
	}
}