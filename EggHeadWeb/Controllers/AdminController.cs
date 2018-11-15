using AuthorizeNet;
using EggheadWeb.Common;
using EggheadWeb.DAL;
using EggheadWeb.Models.AdminModels;
using EggheadWeb.Models.Common;
using EggheadWeb.Models.UserModels;
using EggheadWeb.Security;
using EggheadWeb.Utility;
using EggHeadWeb.DatabaseContext;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
//using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static System.Linq.Expressions.Expression;
using Convert = System.Convert;

namespace EggheadWeb.Controllers
{
	public class AdminController : BaseAdminController
	{
		private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private DateTime firstSunday = new DateTime(1904, 1, 10);

		private UnitOfWork unitOfWork = new UnitOfWork();

		public AdminController() : base("admin")
		{
			base.LoginUrl = this.Url.RouteUrl("admin", new { action = "" });
		}

		private IQueryable<Assign> AssignScheduleSearch(ScheduleSearchForm form, Models.Common.ServiceType serviceType)
		{
			IQueryable<Assign> query = this.db.Assigns.AsQueryable<Assign>();
			if (form.DateFrom.HasValue)
			{
				query = 
					from a in query
					where a.Date >= form.DateFrom.Value
					select a;
			}
			if (form.DateTo.HasValue)
			{
				query = 
					from a in query
					where a.Date <= form.DateTo.Value
					select a;
			}
			switch (serviceType)
			{
				case Models.Common.ServiceType.Class:
				{
					query = 
						from a in query
						where a.Class != null
						select a;
					break;
				}
				case Models.Common.ServiceType.Camp:
				{
					query = 
						from a in query
						where a.Camp != null
						select a;
					break;
				}
				case Models.Common.ServiceType.Workshop:
				{
					query = 
						from a in query
						where a.Workshop != null
						select a;
					break;
				}
				case Models.Common.ServiceType.Birthday:
				{
					query = 
						from a in query
						where a.Birthday != null
						select a;
					break;
				}
			}
			return query;
		}

		[ActionName("birthday-delete")]
		[HttpPost]
		public ActionResult BirthdayDelete(long id)
		{
			try
			{
				Birthday birthday = this.GetBirthday(id);
				foreach (Assign assign in birthday.Assigns.ToList<Assign>())
				{
					this.db.Assigns.Remove(assign);
				}
				this.db.Birthdays.Remove(birthday);
				this.db.SaveChanges();
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.DeleteScheduleEvents(null, null, id.ToString(), null);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Birthday" });
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Birthday" });
			}
			return base.RedirectToRoute("Admin", new { action = "schedules", research = "true" });
		}

		[ActionName("birthday-detail")]
		[HttpGet]
		public ActionResult BirthdayDetail(long id)
		{
			ActionResult route;
			try
			{
				Birthday birthday = this.GetBirthday(id);
				birthday.UpdateCustomProperties();
				route = base.View(birthday);
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.RedirectToRoute("Admin", new { action = "schedules", research = "true" });
			}
			return route;
		}

		[ActionName("birthday-detail")]
		[HttpPost]
		public ActionResult BirthdayDetail(Birthday birthday)
		{
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(birthday);
			}
			try
			{
				Birthday dbBirthday = this.GetBirthday(birthday.Id);
				dbBirthday.Address = birthday.Address;
				dbBirthday.Age = birthday.Age;
				dbBirthday.ChildName = birthday.ChildName;
				dbBirthday.ContactNumber = birthday.ContactNumber;
				dbBirthday.Email = birthday.Email;
				dbBirthday.Notes = birthday.Notes;
				dbBirthday.ParentName = birthday.ParentName;
				dbBirthday.PartyDate = birthday.PartyDate;
				dbBirthday.PartyTime = birthday.PartyTime;
				dbBirthday.InstructorId = birthday.InstructorId;
				dbBirthday.AssistantId = birthday.AssistantId;
				foreach (Assign assign in dbBirthday.Assigns)
				{
					assign.Date = birthday.PartyDate;
					assign.InstructorId = birthday.InstructorId;
					assign.AssistantId = birthday.AssistantId;
				}
				this.db.SaveChanges();
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.SynsScheduleEvents(null, null, dbBirthday, null);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.UpdateSuccess, new object[] { "Birthday" });
				route = base.RedirectToRoute("Admin", new { action = "birthday-detail", id = dbBirthday.Id });
			}
			catch (Exception exception)
			{
				this.log.Error("Update birthday failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(birthday);
			}
			return route;
		}

		[ActionName("birthday-detail-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult BirthdayDetailExel(long? id)
		{
			Birthday birthday = this.GetBirthday(id.Value);
			((dynamic)ViewBag).FileName = birthday.ChildName;
			((dynamic)ViewBag).Detail = new List<string[]>();
			Assign assign = birthday.Assigns.First<Assign>();
			dynamic viewBag = ((dynamic)ViewBag).Detail;
			string[] parentName = new string[] { birthday.ParentName, birthday.ContactNumber, birthday.Email, birthday.Address, birthday.ChildName, null, null, null, null, null, null };
			parentName[5] = birthday.Age.ToString();
			parentName[6] = birthday.PartyDate.ToOneDigitMonth();
			parentName[7] = birthday.PartyTime.To12HoursString();
			parentName[8] = string.Concat(assign.Instructor.FirstName, " ", assign.Instructor.LastName);
			parentName[9] = (assign.AssistantId.HasValue ? string.Concat(assign.Instructor1.FirstName, " ", assign.Instructor1.LastName) : string.Empty);
			parentName[10] = birthday.Notes;
			viewBag.Add(parentName);
			return new EmptyResult();
		}

		[ActionName("booking-birthday")]
		[HttpGet]
		public ActionResult BookingBirthday()
		{
			if (base.User.Admin.AreaId.HasValue)
			{
				return base.View();
			}
			base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
			return base.View("AdminNotAssign");
		}

		[ActionName("booking-birthday")]
		[HttpPost]
		public ActionResult BookingBirthday(Birthday birthday)
		{
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(birthday);
			}
			try
			{
				Birthday birthday1 = new Birthday()
				{
					AreaId = base.User.Admin.AreaId.Value,
					ParentName = birthday.ParentName,
					ContactNumber = birthday.ContactNumber,
					Email = birthday.Email,
					Address = birthday.Address,
					ChildName = birthday.ChildName,
					Age = birthday.Age,
					PartyDate = birthday.PartyDate,
					PartyTime = birthday.PartyTime,
					Notes = birthday.Notes,
					InputDate = new DateTime?(DateTime.Today)
				};
				Birthday dbBirthday = birthday1;
				ICollection<Assign> assigns = dbBirthday.Assigns;
				Assign assign = new Assign()
				{
					Date = birthday.PartyDate,
					InstructorId = birthday.InstructorId,
					AssistantId = birthday.AssistantId
				};
				assigns.Add(assign);
				this.db.Birthdays.Add(dbBirthday);
				this.db.SaveChanges();
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.SynsScheduleEvents(null, null, dbBirthday, null);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.BookingSuccess, new object[] { "Birthday" });
				route = base.RedirectToRoute("Admin", new { action = "booking-birthday" });
			}
			catch (Exception exception)
			{
				this.log.Error("Booking birthday failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(birthday);
			}
			return route;
		}

		[ActionName("booking-camp")]
		[HttpGet]
		public ActionResult BookingCamp()
		{
			if (base.User.Admin.AreaId.HasValue)
			{
				return base.View();
			}
			base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
			return base.View("AdminNotAssign");
		}

		[ActionName("booking-camp")]
		[HttpPost]
		public ActionResult BookingCamp(Camp camp)
		{
		    //AdminController. variable;
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(camp);
			}
			try
			{
				Camp camp1 = new Camp()
				{
					LocationId = camp.LocationId,
					Name = camp.Name,
					OnlineName = camp.OnlineName,
					TimeStart = camp.TimeStart,
					TimeEnd = camp.TimeEnd,
					InstructorId = camp.InstructorId,
					AssistantId = camp.AssistantId,
					CanRegistOnline = camp.CanRegistOnline,
					IsOpen = camp.IsOpen,
					DisplayUntil = camp.NDisplayUntil,
					MaxEnroll = camp.NMaxEnroll.Value,
					Cost = camp.NCost.Value,
					OnlineDescription = camp.OnlineDescription,
					Notes = camp.Notes,
					Enrolled = new int?(0),
					InputDate = new DateTime?(DateTime.Today)
				};
				Camp gradeGroup = camp1;
				string str = camp.Dates;
				char[] chrArray = new char[] { ',' };
				List<string> dates = str.Split(chrArray).ToList<string>();
				dates.ForEach((string date) => gradeGroup.Assigns.Add(new Assign()
				{
					Date = Convert.ToDateTime(date),
					InstructorId = camp.InstructorId,
					AssistantId = camp.AssistantId
				}));
				gradeGroup.GradeGroup = new GradeGroup();
				camp.GradeIds.ForEach((int id) => gradeGroup.GradeGroup.Grades.Add(this.db.Grades.First<Grade>((Grade t) => (int)t.Id == (int)id)));
				this.db.Camps.Add(gradeGroup);
				this.db.SaveChanges();
				Location location = this.db.Locations.First<Location>((Location t) => t.Id == gradeGroup.LocationId);
				gradeGroup.Location = location;
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.SynsScheduleEvents(null, gradeGroup, null, null);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.BookingSuccess, new object[] { "Camp" });
				route = base.RedirectToRoute("Admin", new { action = "booking-camp" });
			}
			catch (Exception exception)
			{
				this.log.Error("Booking camp failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(camp);
			}
			return route;
		}

		[ActionName("booking-class")]
		[HttpGet]
		public ActionResult BookingClass()
		{
			if (base.User.Admin.AreaId.HasValue)
			{
				return base.View();
			}
			base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
			return base.View("AdminNotAssign");
		}

		[ActionName("booking-class")]
		[HttpPost]
		public ActionResult BookingClass(Class klass)
		{
			//AdminController.<>c__DisplayClass1a1 variable;
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(klass);
			}
			try
			{
				Class @class = new Class()
				{
					LocationId = klass.LocationId,
					Name = klass.Name,
					OnlineName = klass.OnlineName,
					TimeStart = klass.TimeStart,
					TimeEnd = klass.TimeEnd,
					InstructorId = klass.InstructorId,
					AssistantId = klass.AssistantId,
					CanRegistOnline = klass.CanRegistOnline,
					IsOpen = klass.IsOpen,
					DisplayUntil = klass.NDisplayUntil,
					MaxEnroll = klass.NMaxEnroll.Value,
					Cost = klass.NCost.Value,
					OnlineDescription = klass.OnlineDescription,
					Notes = klass.Notes,
					Enrolled = new int?(0),
					InputDate = new DateTime?(DateTime.Today)
				};
				Class gradeGroup = @class;
				string str = klass.Dates;
				char[] chrArray = new char[] { ',' };
				List<string> dates = str.Split(chrArray).ToList<string>();
				dates.ForEach((string date) => gradeGroup.Assigns.Add(new Assign()
				{
					Date = Convert.ToDateTime(date),
					InstructorId = klass.InstructorId,
					AssistantId = klass.AssistantId
				}));
				gradeGroup.GradeGroup = new GradeGroup();
				klass.GradeIds.ForEach((int id) => gradeGroup.GradeGroup.Grades.Add(this.db.Grades.First<Grade>((Grade t) => (int)t.Id == (int)id)));
				this.db.Classes.Add(gradeGroup);
				this.db.SaveChanges();
				Location location = this.db.Locations.First<Location>((Location t) => t.Id == gradeGroup.LocationId);
				gradeGroup.Location = location;
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.SynsScheduleEvents(gradeGroup, null, null, null);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.BookingSuccess, new object[] { "Class" });
				route = base.RedirectToRoute("Admin", new { action = "booking-class" });
			}
			catch (Exception exception)
			{
				this.log.Error("Booking class failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(klass);
			}
			return route;
		}

		[ActionName("booking-workshop")]
		[HttpGet]
		public ActionResult BookingWorkshop()
		{
			if (base.User.Admin.AreaId.HasValue)
			{
				return base.View();
			}
			base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
			return base.View("AdminNotAssign");
		}

		[ActionName("booking-workshop")]
		[HttpPost]
		public ActionResult BookingWorkshop(Workshop workshop)
		{
			//AdminController.<>c__DisplayClass197 variable;
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(workshop);
			}
			try
			{
				Workshop workshop1 = new Workshop()
				{
					LocationId = workshop.LocationId,
					Name = workshop.Name,
					TimeStart = workshop.TimeStart,
					TimeEnd = workshop.TimeEnd,
					InstructorId = workshop.InstructorId,
					AssistantId = workshop.AssistantId,
					Cost = workshop.NCost.Value,
					Notes = workshop.Notes,
					InputDate = new DateTime?(DateTime.Today)
				};
				Workshop gradeGroup = workshop1;
				string str = workshop.Dates;
				char[] chrArray = new char[] { ',' };
				List<string> dates = str.Split(chrArray).ToList<string>();
				dates.ForEach((string date) => gradeGroup.Assigns.Add(new Assign()
				{
					Date = Convert.ToDateTime(date),
					InstructorId = workshop.InstructorId,
					AssistantId = workshop.AssistantId
				}));
				gradeGroup.GradeGroup = new GradeGroup();
				workshop.GradeIds.ForEach((int id) => gradeGroup.GradeGroup.Grades.Add(this.db.Grades.First<Grade>((Grade t) => (int)t.Id == (int)id)));
				this.db.Workshops.Add(gradeGroup);
				this.db.SaveChanges();
				Location location = this.db.Locations.First<Location>((Location t) => t.Id == gradeGroup.LocationId);
				gradeGroup.Location = location;
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.SynsScheduleEvents(null, null, null, gradeGroup);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.BookingSuccess, new object[] { "Workshop" });
				route = base.RedirectToRoute("Admin", new { action = "booking-workshop" });
			}
			catch (Exception exception)
			{
				this.log.Error("Booking workshop failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(workshop);
			}
			return route;
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

		public ActionResult Calendar(CalendarSearchForm form)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (form == null)
			{
				CalendarSearchForm calendarSearchForm = new CalendarSearchForm()
				{
					ShowBirthdays = true,
					ShowCamps = true,
					ShowWorkshops = true,
					ShowClasses = true,
					Date = new DateTime?(DateTime.Today)
				};
				form = calendarSearchForm;
			}
			base.SetSession("Calendar.SearchForm", form);
			((dynamic)ViewBag).Calendar = this.PerformCalendarSearch(form);
			return base.View(form);
		}

		[ActionName("calendar-excel")]
		[CalendarExceLDownload(Filename="Calendars")]
		public ActionResult CalendarExcel()
		{
			CalendarSearchForm form = (CalendarSearchForm)base.GetSession("Calendar.SearchForm");
			if (form == null)
			{
				CalendarSearchForm calendarSearchForm = new CalendarSearchForm()
				{
					ShowBirthdays = true,
					ShowCamps = true,
					ShowWorkshops = true,
					ShowClasses = true,
					Date = new DateTime?(DateTime.Today)
				};
				form = calendarSearchForm;
			}
			((dynamic)ViewBag).CalendarSearchForm = form;
			((dynamic)ViewBag).Calendar = this.PerformCalendarSearch(form);
			return new EmptyResult();
		}

		[ActionName("calendar-schedule")]
		public ActionResult CalendarSchedule(CalendarSearchForm form)
		{
			base.SetSession("Calendar.Schedule.SearchForm", form);
			((dynamic)ViewBag).Calendar = this.PerformCalendarSearch(form);
			return base.PartialView(form);
		}

		private List<ChildBookingItem> CampBookingInfo(long parentId)
		{
			List<ChildBookingItem> listResult = new List<ChildBookingItem>();
			List<Student> listStudent = this.unitOfWork.StudentRepository.Get((Student r) => r.ParentId == parentId, null, "").ToList<Student>();
			if (listStudent != null)
			{
				long appliedCouponId = (long)0;
				foreach (Student student in listStudent)
				{
					List<BookingShortInfoTemp> listBookingTempItem = this.unitOfWork.BookingTempRepository.Get((BookingShortInfoTemp r) => (r.ServiceType == Constants.BOOK_SERVICE_TYPE_CAMP) && r.StudentId == (long?)student.Id, null, "").ToList<BookingShortInfoTemp>();
					if (listBookingTempItem != null && listBookingTempItem.Count > 0)
					{
						appliedCouponId = listBookingTempItem[0].CouponId.Value;
					}
					List<ChildBookingItem> childBookingItems = listResult;
					ChildBookingItem childBookingItem = new ChildBookingItem()
					{
						Child = student,
						Booking = (
							from item in listBookingTempItem
							select new BookingShortInfo()
							{
								Type = ServiceType.Class,
								Id = item.classId.Value,
								Name = item.Name,
								Dates = item.Dates,
								Cost = item.Cost.Value
							}).ToList<BookingShortInfo>()
					};
					childBookingItems.Add(childBookingItem);
				}
				base.SetSession("AdminAppliedCoupondId", appliedCouponId);
			}
			return listResult;
		}

		[ActionName("camp-delete")]
		[HttpPost]
		public ActionResult CampDelete(long id)
		{
			try
			{
				Camp camp = this.GetCamp(id);
				foreach (Assign assign in camp.Assigns.ToList<Assign>())
				{
					this.db.Assigns.Remove(assign);
				}
				camp.GradeGroup.Grades.Clear();
				this.db.GradeGroups.Remove(camp.GradeGroup);
				this.db.Camps.Remove(camp);
				this.db.SaveChanges();
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.DeleteScheduleEvents(null, id.ToString(), null, null);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Camp" });
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Camp" });
			}
			return base.RedirectToRoute("Admin", new { action = "schedules", research = "true" });
		}

		[ActionName("camp-detail")]
		[HttpGet]
		public ActionResult CampDetail(long id)
		{
			ActionResult route;
			try
			{
				Camp camp = this.GetCamp(id);
				camp.UpdateCustomProperties();
				route = base.View(camp);
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.RedirectToRoute("Admin", new { action = "schedules", research = "true" });
			}
			return route;
		}

		[ActionName("camp-detail")]
		[HttpPost]
		public ActionResult CampDetail(Camp camp)
		{
			ActionResult route;
			dynamic viewBag = ViewBag;
			CalendarSearchForm calendarSearchForm = new CalendarSearchForm()
			{
				ViewBy = CalendarViewType.Month,
				CampId = new long?(camp.Id),
				ShowCamps = true,
				Date = new DateTime?(DateTime.Today)
			};
			viewBag.Calendar = this.PerformCalendarSearch(calendarSearchForm);
			if (!ModelState.IsValid)
			{
				return base.View(camp);
			}
			try
			{
				Camp canRegistOnline = this.GetCamp(camp.Id);
				canRegistOnline.CanRegistOnline = camp.CanRegistOnline;
				canRegistOnline.Cost = camp.NCost.Value;
				canRegistOnline.DisplayUntil = camp.NDisplayUntil;
				canRegistOnline.IsOpen = camp.IsOpen;
				canRegistOnline.LocationId = this.GetLocation(camp.LocationId).Id;
				canRegistOnline.MaxEnroll = camp.NMaxEnroll.Value;
				canRegistOnline.Name = camp.Name;
				canRegistOnline.Notes = camp.Notes;
				canRegistOnline.OnlineDescription = camp.OnlineDescription;
				canRegistOnline.OnlineName = camp.OnlineName;
				canRegistOnline.TimeEnd = camp.TimeEnd;
				canRegistOnline.TimeStart = camp.TimeStart;
				canRegistOnline.Enrolled = camp.Enrolled;
				bool changeInstructor = canRegistOnline.InstructorId != camp.InstructorId;
				long? assistantId = canRegistOnline.AssistantId;
				long? nullable = camp.AssistantId;
				bool changeAssistant = (assistantId.GetValueOrDefault() != nullable.GetValueOrDefault() ? true : assistantId.HasValue != nullable.HasValue);
				if (changeInstructor)
				{
					canRegistOnline.InstructorId = camp.InstructorId;
				}
				if (changeAssistant)
				{
					canRegistOnline.AssistantId = camp.AssistantId;
				}
				canRegistOnline.Assigns.Clear();
				foreach (Assign assignList in camp.AssignList)
				{
					if (!assignList.NDate.HasValue)
					{
						continue;
					}
					if (!canRegistOnline.Assigns.Any<Assign>((Assign t) => t.Date == assignList.NDate.Value))
					{
						Assign assign = new Assign()
						{
							Date = assignList.NDate.Value,
							InstructorId = (changeInstructor ? camp.InstructorId : assignList.InstructorId),
							AssistantId = (changeAssistant ? camp.AssistantId : assignList.AssistantId)
						};
						canRegistOnline.Assigns.Add(assign);
					}
					else
					{
						base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DatesDuplicated, new object[0]);
						route = base.View(camp);
						return route;
					}
				}
				canRegistOnline.GradeGroup.Grades.Clear();
				foreach (byte gradeId in camp.GradeIds)
				{
					canRegistOnline.GradeGroup.Grades.Add(this.GetGrade((long)gradeId));
				}
				this.db.SaveChanges();
				Location location = this.db.Locations.First<Location>((Location t) => t.Id == canRegistOnline.LocationId);
				canRegistOnline.Location = location;
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.SynsScheduleEvents(null, canRegistOnline, null, null);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.UpdateSuccess, new object[] { "Camp" });
				route = base.RedirectToRoute("Admin", new { action = "camp-detail", id = canRegistOnline.Id });
			}
			catch (Exception exception)
			{
				this.log.Error("Update camp failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(camp);
			}
			return route;
		}

		[ActionName("camp-detail-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult CampDetailExel(long? id)
		{
			Camp camp = this.GetCamp(id.Value);
			((dynamic)ViewBag).FileName = camp.Name;
			((dynamic)ViewBag).Detail = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Detail;
			string[] displayName = new string[] { camp.Location.DisplayName, camp.Name, camp.OnlineName, ScheduleUtil.GetGradeListText(camp), camp.TimeStart.To12HoursString(), camp.TimeEnd.To12HoursString(), ScheduleUtil.GetDateListText(camp), string.Concat(camp.Instructor.FirstName, " ", camp.Instructor.LastName), null, null, null, null, null, null, null, null };
			displayName[8] = (camp.AssistantId.HasValue ? string.Concat(camp.Instructor1.FirstName, camp.Instructor1.LastName) : string.Empty);
			displayName[9] = (camp.CanRegistOnline ? "Yes" : "No");
			displayName[10] = (camp.IsOpen ? "Yes" : "No");
			displayName[11] = (camp.DisplayUntil.HasValue ? camp.DisplayUntil.Value.ToString("M/dd/yyyy") : string.Empty);
			displayName[12] = camp.MaxEnroll.ToString();
			displayName[13] = camp.Cost.ToString();
			displayName[14] = camp.OnlineDescription;
			displayName[15] = camp.Notes;
			viewBag.Add(displayName);
			CalendarSearchForm form = (CalendarSearchForm)base.GetSession("Calendar.Schedule.SearchForm");
			((dynamic)ViewBag).CalendarSearchForm = form;
			((dynamic)ViewBag).Calendar = this.PerformCalendarSearch(form);
			return new EmptyResult();
		}

		[ActionName("charge")]
		[Authorize(Roles="admin")]
		[HttpGet]
		public ActionResult Charge()
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			List<long> list = (
				from r in this.db.Locations
				where (long?)r.AreaId == this.User.Admin.AreaId
				select r.Id).ToList<long>();
			dynamic viewBag = ViewBag;
			List<Parent> parents = (
				from r in this.db.Parents
				where list.Contains(r.LocationId)
				select r).ToList<Parent>();
			viewBag.Parents = 
				from r in parents
				orderby r.FullName
				select r;
			return base.View(new Payment());
		}

		[ActionName("charge")]
		[Authorize(Roles="admin")]
		[HttpPost]
		public ActionResult Charge(Payment payment)
		{
			((dynamic)ViewBag).Parents = this.db.Parents.ToList<Parent>();
			Parent parent = (
				from r in this.db.Parents
				where (long?)r.Id == payment.AdminParentId
				select r).FirstOrDefault<Parent>();
			string empty = string.Empty;
			string parentName = string.Empty;
			if (parent != null)
			{
				string email = parent.Email;
				parentName = string.Format("{0} {1}", parent.FirstName, parent.LastName);
			}
			if (string.IsNullOrEmpty(payment.AdminAmount))
			{
				payment.PaymentMessage = "Ammount is required.";
				return base.View("Charge", payment);
			}
			decimal totalAmount = new decimal(0);
			if (!decimal.TryParse(payment.AdminAmount, out totalAmount))
			{
				payment.PaymentMessage = "Ammount should be numberic.";
				return base.View("Charge", payment);
			}
			string description = payment.Description;
			if (string.IsNullOrEmpty(description))
			{
				description = string.Format("Payment from Parent {0}", parentName);
			}
			AdminPaymentInfo paymentInfo = this.db.AdminPaymentInfoes.FirstOrDefault<AdminPaymentInfo>((AdminPaymentInfo t) => t.AdminId == this.User.Admin.Id);
			if (paymentInfo == null)
			{
				payment.PaymentMessage = "Payment informtion is empty. Please config your payment account.";
				return base.View("Charge", payment);
			}
			System.Web.HttpContext.Current.Session["totalAmount"] = totalAmount;
			string message = string.Empty;
			PayPalOrder payPalOrder = new PayPalOrder()
			{
				Amount = totalAmount,
				Name = string.Format("Payment from Parent {0}", parentName),
				Description = description,
				UserName = paymentInfo.APILoginID,
				Password = paymentInfo.TransactionKey,
				Signature = paymentInfo.MD5HashPhrase,
				OverrideAddress = false
			};
			PayPalOrder paypalOrder = payPalOrder;
			if (parent != null)
			{
				paypalOrder.OverrideAddress = true;
				paypalOrder.FirstName = parent.FirstName;
				paypalOrder.LastName = parent.LastName;
				paypalOrder.Address = parent.Address;
				paypalOrder.City = parent.City;
				paypalOrder.State = parent.State;
				paypalOrder.Zip = parent.Zip;
				paypalOrder.Email = parent.Email;
			}
			PayPalRedirect redirect = Utility.PayPal.ExpressCheckout(paypalOrder, out message);
			if (!string.IsNullOrEmpty(message))
			{
				payment.PaymentMessage = message;
				return base.View("Charge", payment);
			}
			System.Web.HttpContext.Current.Session["token"] = redirect.Token;
			return new RedirectResult(redirect.Url);
		}

		[ActionName("charge-fail")]
		[AllowAnonymous]
		[HttpGet]
		public ActionResult ChargeFail()
		{
			return base.View("ChargeFail");
		}

		[ActionName("charge-success")]
		[AllowAnonymous]
		[HttpGet]
		public ActionResult ChargeSuccess(string token, string PayerID)
		{
			this.log.InfoFormat("Charge success from admin site. Token: {0}, PayerID: {1}", token, PayerID);
			object sessionToken = System.Web.HttpContext.Current.Session["token"];
			if (sessionToken != null)
			{
			}
			AdminPaymentInfo paymentInfo = this.db.AdminPaymentInfoes.FirstOrDefault<AdminPaymentInfo>((AdminPaymentInfo t) => t.AdminId == this.User.Admin.Id);
			if (paymentInfo == null)
			{
				return base.View("ChargeFail");
			}
			decimal totalAmount = Convert.ToDecimal(System.Web.HttpContext.Current.Session["totalAmount"]);
			PayPalOrder payPalOrder = new PayPalOrder()
			{
				Amount = totalAmount,
				Token = token,
				PayerID = PayerID,
				UserName = paymentInfo.APILoginID,
				Password = paymentInfo.TransactionKey,
				Signature = paymentInfo.MD5HashPhrase
			};
			string reference = Utility.PayPal.DoExpressCheckoutPayment(payPalOrder);
			this.log.InfoFormat("Charge success from admin site. Transaction ID: {0}", reference);
			((dynamic)ViewBag).Reference = reference;
			return base.View("ChargeSuccess");
		}

		private List<ChildBookingItem> ClassBookingInfo(long parentId)
		{
			List<ChildBookingItem> listResult = new List<ChildBookingItem>();
			List<Student> listStudent = this.unitOfWork.StudentRepository.Get((Student r) => r.ParentId == parentId, null, "").ToList<Student>();
			if (listStudent != null)
			{
				long appliedCouponId = (long)0;
				foreach (Student student in listStudent)
				{
					List<BookingShortInfoTemp> listBookingTempItem = this.unitOfWork.BookingTempRepository.Get((BookingShortInfoTemp r) => (r.ServiceType == Constants.BOOK_SERVICE_TYPE_CLASS) && r.StudentId == (long?)student.Id, null, "").ToList<BookingShortInfoTemp>();
					if (listBookingTempItem != null && listBookingTempItem.Count > 0)
					{
						appliedCouponId = listBookingTempItem[0].CouponId.Value;
					}
					List<ChildBookingItem> childBookingItems = listResult;
					ChildBookingItem childBookingItem = new ChildBookingItem()
					{
						Child = student,
						Booking = (
							from item in listBookingTempItem
							select new BookingShortInfo()
							{
								Type = ServiceType.Class,
								Id = item.classId.Value,
								Name = item.Name,
								Dates = item.Dates,
								Cost = item.Cost.Value
							}).ToList<BookingShortInfo>()
					};
					childBookingItems.Add(childBookingItem);
				}
				base.SetSession("AdminAppliedCoupondId", appliedCouponId);
			}
			return listResult;
		}

		[ActionName("class-delete")]
		[HttpPost]
		public ActionResult ClassDelete(long id)
		{
			try
			{
				Class klass = this.GetClass(id);
				foreach (Assign assign in klass.Assigns.ToList<Assign>())
				{
					this.db.Assigns.Remove(assign);
				}
				klass.GradeGroup.Grades.Clear();
				this.db.GradeGroups.Remove(klass.GradeGroup);
				this.db.Classes.Remove(klass);
				this.db.SaveChanges();
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.DeleteScheduleEvents(id.ToString(), null, null, null);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Class" });
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Class" });
			}
			return base.RedirectToRoute("Admin", new { action = "schedules", research = "true" });
		}

		[ActionName("class-detail")]
		[HttpGet]
		public ActionResult ClassDetail(long id)
		{
			ActionResult route;
			try
			{
				Class klass = this.GetClass(id);
				klass.UpdateCustomProperties();
				route = base.View(klass);
			}
			catch (Exception exception)
			{
				this.log.Error("Get class failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.RedirectToRoute("Admin", new { action = "schedules", research = "true" });
			}
			return route;
		}

		[ActionName("class-detail")]
		[HttpPost]
		public ActionResult ClassDetail(Class klass)
		{
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(klass);
			}
			try
			{
				Class @class = this.GetClass(klass.Id);
				@class.CanRegistOnline = klass.CanRegistOnline;
				@class.Cost = klass.NCost.Value;
				@class.DisplayUntil = klass.NDisplayUntil;
				@class.IsOpen = klass.IsOpen;
				@class.LocationId = this.GetLocation(klass.LocationId).Id;
				@class.MaxEnroll = klass.NMaxEnroll.Value;
				@class.Name = klass.Name;
				@class.Notes = klass.Notes;
				@class.OnlineDescription = klass.OnlineDescription;
				@class.OnlineName = klass.OnlineName;
				@class.TimeEnd = klass.TimeEnd;
				@class.TimeStart = klass.TimeStart;
				@class.Enrolled = klass.Enrolled;
				bool changeInstructor = @class.InstructorId != klass.InstructorId;
				long? assistantId = @class.AssistantId;
				long? nullable = klass.AssistantId;
				bool changeAssistant = (assistantId.GetValueOrDefault() != nullable.GetValueOrDefault() ? true : assistantId.HasValue != nullable.HasValue);
				if (changeInstructor)
				{
					@class.InstructorId = klass.InstructorId;
				}
				if (changeAssistant)
				{
					@class.AssistantId = klass.AssistantId;
				}
				@class.Assigns.Clear();
				foreach (Assign assignList in klass.AssignList)
				{
					if (!assignList.NDate.HasValue)
					{
						continue;
					}
					if (!@class.Assigns.Any<Assign>((Assign t) => t.Date == assignList.NDate.Value))
					{
						Assign assign = new Assign()
						{
							Date = assignList.NDate.Value,
							InstructorId = (changeInstructor ? klass.InstructorId : assignList.InstructorId),
							AssistantId = (changeAssistant ? klass.AssistantId : assignList.AssistantId)
						};
						@class.Assigns.Add(assign);
					}
					else
					{
						base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DatesDuplicated, new object[0]);
						route = base.View(klass);
						return route;
					}
				}
				@class.GradeGroup.Grades.Clear();
				foreach (byte gradeId in klass.GradeIds)
				{
					@class.GradeGroup.Grades.Add(this.GetGrade((long)gradeId));
				}
				this.db.SaveChanges();
				Location location = this.db.Locations.First<Location>((Location t) => t.Id == @class.LocationId);
				@class.Location = location;
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.SynsScheduleEvents(@class, null, null, null);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.UpdateSuccess, new object[] { "Class" });
				route = base.RedirectToRoute("Admin", new { action = "class-detail", id = @class.Id });
			}
			catch (Exception exception)
			{
				this.log.Error("Update class failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(klass);
			}
			return route;
		}

		[ActionName("class-detail-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult ClassDetailExel(long? id)
		{
			Class klass = this.GetClass(id.Value);
			((dynamic)ViewBag).FileName = klass.Name;
			((dynamic)ViewBag).Detail = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Detail;
			string[] displayName = new string[] { klass.Location.DisplayName, klass.Name, klass.OnlineName, ScheduleUtil.GetGradeListText(klass), klass.TimeStart.To12HoursString(), klass.TimeEnd.To12HoursString(), ScheduleUtil.GetDateListText(klass), string.Concat(klass.Instructor.FirstName, " ", klass.Instructor.LastName), null, null, null, null, null, null, null, null };
			displayName[8] = (klass.AssistantId.HasValue ? string.Concat(klass.Instructor1.FirstName, klass.Instructor1.LastName) : string.Empty);
			displayName[9] = (klass.CanRegistOnline ? "Yes" : "No");
			displayName[10] = (klass.IsOpen ? "Yes" : "No");
			displayName[11] = (klass.DisplayUntil.HasValue ? klass.DisplayUntil.Value.ToString("M/dd/yyyy") : string.Empty);
			displayName[12] = klass.MaxEnroll.ToString();
			displayName[13] = klass.Cost.ToString();
			displayName[14] = klass.OnlineDescription;
			displayName[15] = klass.Notes;
			viewBag.Add(displayName);
			CalendarSearchForm form = (CalendarSearchForm)base.GetSession("Calendar.Schedule.SearchForm");
			((dynamic)ViewBag).CalendarSearchForm = form;
			((dynamic)ViewBag).Calendar = this.PerformCalendarSearch(form);
			return new EmptyResult();
		}

		[ActionName("coupons-delete")]
		[Authorize(Roles="admin")]
		[HttpPost]
		public ActionResult CouponDelete(long id)
		{
			try
			{
				Coupon coupon = this.GetCoupon(id);
				if (coupon != null)
				{
					this.db.Coupons.Remove(coupon);
					this.db.SaveChanges();
					base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Coupon" });
				}
				else
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
					return base.Content(Messages.DataNotFound);
				}
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Coupon" });
			}
			return base.RedirectToRoute("Admin", new { action = "coupons", research = "true" });
		}

		[ActionName("coupons-detail")]
		[Authorize(Roles="admin")]
		[HttpGet]
		public ActionResult CouponDetail(long? id)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!id.HasValue)
			{
				Coupon coupon1 = new Coupon()
				{
					AdminId = new long?(base.User.Admin.Id)
				};
				return base.View(coupon1);
			}
			Coupon coupon = this.GetCoupon(id.Value);
			coupon.NExpDate = new DateTime?(coupon.ExpDate);
			if (coupon == null)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DataNotFound, new object[0]);
				return base.RedirectToRoute("Admin", new { action = "coupons", research = "true" });
			}
			var abc = (
				from t in coupon.Bookings
				group t by new { ParentId = t.Student.ParentId, BookDate = t.BookDate } into g
				select g).ToList();
			dynamic viewBag = ViewBag;
			IEnumerable<CouponBookingInfo> couponBookingInfo = 
				from g in abc
				select new CouponBookingInfo()
				{
					Parent = this.db.Parents.First<Parent>((Parent t) => t.Id == g.Key.ParentId),
					BookDate = g.Key.BookDate,
					Classes = string.Join(", ", (
						from t in g
						where t.ClassId.HasValue
						select t into x
						select x.Class.Name).ToArray<string>()),
					Camps = string.Join(", ", (
						from t in g
						where t.CampId.HasValue
						select t into x
						select x.Camp.Name).ToArray<string>())
				};
			viewBag.BookingInfo = (
				from x in couponBookingInfo
				orderby x.BookDate descending
				select x).ToList<CouponBookingInfo>();
			return base.View(coupon);
		}

		[ActionName("coupons-detail")]
		[Authorize(Roles="admin")]
		[HttpPost]
		public ActionResult CouponDetail(Coupon coupon)
		{
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(coupon);
			}
			try
			{
				bool updateMode = coupon.Id > (long)0;
				if (!updateMode)
				{
					coupon.ExpDate = coupon.NExpDate.Value;
					this.db.Coupons.Add(coupon);
					this.db.SaveChanges();
				}
				else
				{
					Coupon dbCoupon = this.GetCoupon(coupon.Id);
					dbCoupon.Code = coupon.Code;
					dbCoupon.Description = coupon.Description;
					dbCoupon.Type = coupon.Type;
					dbCoupon.DiscountAmount = coupon.DiscountAmount;
					dbCoupon.ExpDate = coupon.NExpDate.Value;
					dbCoupon.MaxAvailable = coupon.MaxAvailable;
					dbCoupon.MaxUsesPerCustomer = coupon.MaxUsesPerCustomer;
				}
				base.SetViewMessage(WithAuthenController.MessageType.Info, (updateMode ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "Coupon" });
				route = base.RedirectToRoute("Admin", new { action = "coupons", research = "true" });
			}
			catch (Exception exception)
			{
				this.log.Error("Add/Update coupon failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(coupon);
			}
			return route;
		}

		[Authorize(Roles="admin")]
		[HttpGet]
		public ActionResult Coupons(CouponSearchForm form, int? page, bool? research)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!research.HasValue || !research.Value || base.GetSession("Coupons.SearchForm") == null)
			{
				base.SetSession("Coupons.SearchForm", form);
			}
			CouponSearchForm session = (CouponSearchForm)base.GetSession("Coupons.SearchForm");
			int? nullable = page;
			SearchResult<CouponSearchForm, Coupon> coupons = this.PerformCouponSearch(session, (nullable.HasValue ? nullable.GetValueOrDefault() : 1), new int?(2147483647));
			if (coupons.TotalItems != 0)
			{
				((dynamic)ViewBag).Coupons = coupons;
			}
			else
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.NoRecordFound, new object[] { "coupon" });
			}
			return base.View();
		}

		[ActionName("coupons-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult CouponsExcel()
		{
			CouponSearchForm sessionForm = (CouponSearchForm)base.GetSession("Coupons.SearchForm") ?? new CouponSearchForm();
			((dynamic)ViewBag).FileName = "Coupons.xlsx";
			((dynamic)ViewBag).SheetName = "Coupons";
			((dynamic)ViewBag).Headers = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Headers;
			string[] strArrays = new string[] { "Code", "Description", "Discount", "Max Avaiable", "Expiration Date" };
			viewBag.Add(strArrays);
			SearchResult<CouponSearchForm, Coupon> coupons = this.PerformCouponSearch(sessionForm, 1, new int?(2147483647));
			dynamic list = ViewBag;
			List<Coupon> pageItems = coupons.PageItems;
			list.ExportData = (
				from t in pageItems
				select new { Code = t.Code, Description = t.Description, DiscountAmount = (t.Type == 1 ? string.Concat(t.DiscountAmount.Value.ToString("0.00"), "$") : string.Concat(t.DiscountAmount.Value.ToString("0.00"), "%")), MaxAvailable = t.MaxAvailable, ExpDate = t.ExpDate.ToString("M/dd/yyy") }).ToList();
			return new EmptyResult();
		}

		private IQueryable<Camp> CreateRosterCampsQueryResult(RosterSearchForm form)
		{
			IQueryable<Camp> query = this.db.Camps.AsQueryable<Camp>();
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from c in query
					where (long?)c.Location.AreaId == this.User.Admin.AreaId
					select c;
			}
			query = 
				from c in query
				where c.Location.IsActive && c.Location.CanRegistOnline
				select c;
			if (form.LocationId.HasValue)
			{
				query = 
					from c in query
					where (long?)c.LocationId == (long?)form.LocationId
					select c;
			}
			query = (!form.StartDate.HasValue ? 
				from c in query
				where c.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= DateTime.Today
				select c : 
				from c in query
				where c.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= form.StartDate.Value
				select c);
			if (form.InstructorId.HasValue)
			{
				query = 
					from c in query
					where c.InstructorId == (long)form.InstructorId.Value
					select c;
			}
			if (!string.IsNullOrWhiteSpace(form.ClassName))
			{
				query = 
					from c in query
					where c.Name.ToLower().Contains(form.ClassName.Trim().ToLower()) || c.OnlineName.ToLower().Contains(form.ClassName.Trim().ToLower())
					select c;
			}
			query = 
				from c in query
				orderby c.Assigns.Min<Assign, DateTime>((Assign t) => t.Date)
				select c;
			return query;
		}

		private IQueryable<Class> CreateRosterClassesQuery(RosterSearchForm form)
		{
			IQueryable<Class> query = this.db.Classes.AsQueryable<Class>();
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from c in query
					where (long?)c.Location.AreaId == this.User.Admin.AreaId
					select c;
			}
			query = 
				from c in query
				where c.Location.IsActive && c.Location.CanRegistOnline
				select c;
			if (form.LocationId.HasValue)
			{
				query = 
					from c in query
					where (long?)c.LocationId == (long?)form.LocationId
					select c;
			}
			query = (!form.StartDate.HasValue ? 
				from c in query
				where c.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= DateTime.Today
				select c : 
				from c in query
				where c.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= form.StartDate.Value
				select c);
			if (form.InstructorId.HasValue)
			{
				query = 
					from c in query
					where c.InstructorId == (long)form.InstructorId.Value
					select c;
			}
			if (!string.IsNullOrWhiteSpace(form.ClassName))
			{
				query = 
					from c in query
					where c.Name.ToLower().Contains(form.ClassName.Trim().ToLower()) || c.OnlineName.ToLower().Contains(form.ClassName.Trim().ToLower())
					select c;
			}
			query = 
				from c in query
				orderby c.Assigns.Min<Assign, DateTime>((Assign t) => t.Date)
				select c;
			return query;
		}

		private RosterDetailPdf CreateRosterDetailExportData(long id, ServiceType type)
		{
			dynamic service = null;
			RosterDetailPdf model = new RosterDetailPdf();
			if (type == ServiceType.Class)
			{
				service = this.GetClass(id);
			}
			else if (type == ServiceType.Camp)
			{
				service = this.GetCamp(id);
			}
			model.Bookings = service.Bookings;
			model.LocationName = (string)service.Location.DisplayName;
			model.Days = (string)this.GetDaysOfService(service);
			model.Time = string.Format("{0} - {1}", ((TimeSpan)service.TimeStart).To12HoursString(), ((TimeSpan)service.TimeEnd).To12HoursString());
			model.Address = (string)StringUtil.GetFullAddress(service.Location.Address, service.Location.City, service.Location.State, service.Location.Zip);
			model.ClassName = (string)service.OnlineName;
			RosterDetailPdf rosterDetailPdf = model;
			string str = (string)ScheduleUtil.GetDateListText(service);
			char[] chrArray = new char[] { ',' };
			rosterDetailPdf.Dates = str.Split(chrArray);
			model.LogoUrl = string.Concat(string.Format("{0}://{1}{2}", System.Web.HttpContext.Current.Request.Url.Scheme, System.Web.HttpContext.Current.Request.Url.Authority, this.Url.Content("~")), "Content/Images/logo_small_rotate16.png");
			model.Printed = DateTime.Now.ToString("M/dd/yyyy");
			model.ExportFileName = (string)service.Name;
			return model;
		}

		private void DeleteAllTempBooking(long parentId)
		{
			List<Student> listStudent = this.unitOfWork.StudentRepository.Get((Student r) => r.ParentId == parentId, null, "").ToList<Student>();
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
		}

		[ActionName("frontend")]
		[Authorize(Roles="admin")]
		[HttpGet]
		public ActionResult FrontEnd()
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			List<Frontend> systemFrontends = this.db.Frontends.ToList<Frontend>();
			bool all = true;
			foreach (Frontend frontend in systemFrontends)
			{
				if (this.db.AdminFrontends.Any<AdminFrontend>((AdminFrontend k) => k.AdminId == this.User.Admin.Id && k.FrontendId == (int?)frontend.Id))
				{
					continue;
				}
				DbSet<AdminFrontend> adminFrontends = this.db.AdminFrontends;
				AdminFrontend adminFrontend = new AdminFrontend()
				{
					Name = frontend.Name,
					FrontendId = new int?(frontend.Id),
					MenuName = frontend.MenuName,
					IsActive = frontend.IsActive,
					AdminId = base.User.Admin.Id,
					OverridePageContent = frontend.PageContent
				};
				adminFrontends.Add(adminFrontend);
				all = false;
			}
			try
			{
				if (!all)
				{
					this.db.SaveChanges();
				}
				dynamic viewBag = ViewBag;
				IQueryable<AdminFrontend> adminFrontends1 = 
					from t in this.db.AdminFrontends
					where t.AdminId == this.User.Admin.Id
					select t;
				viewBag.Frontends = SearchResult.New<object, AdminFrontend>(null, 
					from k in adminFrontends1
					orderby k.Name
					select k, 1, 2147483647);
			}
			catch (Exception exception)
			{
				this.log.Error("Update frontend birthday failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
			}
			return base.View();
		}

		[ActionName("frontend-delete")]
		[Authorize(Roles="admin")]
		[HttpPost]
		public ActionResult FrontendDelete(long id)
		{
			try
			{
				AdminFrontend frontend = this.db.AdminFrontends.SingleOrDefault<AdminFrontend>((AdminFrontend t) => (long)t.Id == id && !t.FrontendId.HasValue);
				if (frontend != null)
				{
					this.db.AdminFrontends.Remove(frontend);
					this.db.SaveChanges();
					base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Frontend" });
				}
				else
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
					return base.Content(Messages.DataNotFound);
				}
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Frontend" });
			}
			return base.RedirectToRoute("Admin", new { action = "frontend" });
		}

		[ActionName("frontend-detail")]
		[Authorize(Roles="admin")]
		[HttpGet]
		public ActionResult FrontendDetail(long? id)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!id.HasValue)
			{
				return base.View(new AdminFrontend());
			}
			AdminFrontend adminFrontend = this.db.AdminFrontends.SingleOrDefault<AdminFrontend>((AdminFrontend t) => (long)t.Id == id.Value);
			return base.View(adminFrontend);
		}

		[ActionName("frontend-detail")]
		[Authorize(Roles="admin")]
		[HttpPost]
		public ActionResult FrontendDetail(AdminFrontend model)
		{
			if (!ModelState.IsValid)
			{
				return base.View(model);
			}
			AdminFrontend adminFrontend = null;
			if (model.Id <= 0)
			{
				AdminFrontend adminFrontend1 = new AdminFrontend()
				{
					AdminId = base.User.Admin.Id,
					MenuName = model.MenuName,
					Name = model.Name,
					IsActive = model.IsActive,
					OverridePageContent = model.OverridePageContent
				};
				adminFrontend = adminFrontend1;
				this.db.AdminFrontends.Add(adminFrontend);
				this.db.SaveChanges();
			}
			else
			{
				adminFrontend = this.db.AdminFrontends.SingleOrDefault<AdminFrontend>((AdminFrontend t) => t.Id == model.Id);
				adminFrontend.MenuName = model.MenuName;
				adminFrontend.Name = model.Name;
				adminFrontend.IsActive = model.IsActive;
				adminFrontend.OverridePageContent = model.OverridePageContent;
				this.db.SaveChanges();
			}
			base.SetViewMessage(WithAuthenController.MessageType.Info, (model.Id > 0 ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "Frontend" });
			return base.RedirectToAction("frontend");
		}

		[HttpPost]
		public JsonResult GetBillingByParentIdAJAX(long parentId)
		{
			var data = 
				from item in this.db.Parents
				where item.Id == parentId
				select new { Email = item.Email, FirstName = item.FirstName, LastName = item.LastName, Address = item.Address, City = item.City, State = item.State, Zip = item.Zip };
			return base.Json(data.FirstOrDefault());
		}

		private Birthday GetBirthday(long id)
		{
			if (!base.User.Admin.IsSuperAdmin)
			{
				return this.db.Birthdays.First<Birthday>((Birthday b) => b.Id == id);
			}
			return this.db.Birthdays.First<Birthday>((Birthday b) => b.Id == id);
		}

		private Camp GetCamp(long id)
		{
			Camp camp = (base.User.Admin.IsSuperAdmin ? this.db.Camps.First<Camp>((Camp c) => c.Id == id) : this.db.Camps.First<Camp>((Camp c) => c.Id == id && (long?)c.Location.AreaId == this.User.Admin.AreaId));
			camp.UpdateCustomProperties();
			return camp;
		}

		private Class GetClass(long id)
		{
			Class klass = (base.User.Admin.IsSuperAdmin ? this.db.Classes.First<Class>((Class c) => c.Id == id) : this.db.Classes.First<Class>((Class c) => c.Id == id && (long?)c.Location.AreaId == this.User.Admin.AreaId));
			klass.UpdateCustomProperties();
			return klass;
		}

		private Coupon GetCoupon(long id)
		{
			if (base.User.Admin.IsSuperAdmin)
			{
				return this.db.Coupons.FirstOrDefault<Coupon>((Coupon i) => i.Id == id);
			}
			return this.db.Coupons.FirstOrDefault<Coupon>((Coupon i) => i.Id == id && i.AdminId == (long?)this.User.Admin.Id);
		}

		private string GetDaysOfService(dynamic service)
		{
			List<DayOfWeek> days = new List<DayOfWeek>();
			foreach (dynamic assign in (IEnumerable)service.Assigns)
			{
				if (!(dynamic)(!days.Contains(assign.Date.DayOfWeek)))
				{
					continue;
				}
				days.Add(assign.Date.DayOfWeek);
			}
			return string.Join(", ", (
				from t in days
				orderby t
				select t.ToString()).ToArray<string>());
		}

		private Grade GetGrade(long id)
		{
			return this.db.Grades.FirstOrDefault<Grade>((Grade g) => (long)g.Id == id);
		}

		private Instructor GetInstructor(long id)
		{
			if (base.User.Admin.IsSuperAdmin)
			{
				return this.db.Instructors.FirstOrDefault<Instructor>((Instructor i) => i.Id == id);
			}
			return this.db.Instructors.FirstOrDefault<Instructor>((Instructor i) => i.Id == id && i.AreaId == this.AdminAreaId);
		}

		private Location GetLocation(long id)
		{
			if (base.User.Admin.IsSuperAdmin)
			{
				return this.db.Locations.FirstOrDefault<Location>((Location l) => l.Id == id);
			}
			return this.db.Locations.FirstOrDefault<Location>((Location l) => l.Id == id && l.AreaId == this.AdminAreaId);
		}

		private Parent GetParent(long id)
		{
			IQueryable<Parent> query = 
				from p in this.db.Parents
				where p.Id == id
				select p;
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from p in query
					where p.Location.AreaId == this.AdminAreaId
					select p;
			}
			return query.FirstOrDefault<Parent>();
		}

		private string GetPaymentDescription(List<ChildBookingItem> classBookingInfo, List<ChildBookingItem> campBookingInfo)
		{
			StringBuilder paymentDescription = new StringBuilder();
			List<long> classes = new List<long>();
			List<long> camps = new List<long>();
			if (classBookingInfo != null)
			{
				foreach (ChildBookingItem chilClass in classBookingInfo)
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
			if (campBookingInfo != null)
			{
				foreach (ChildBookingItem chilCamp in campBookingInfo)
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
				foreach (ChildBookingItem childBook in classBookingInfo)
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
				foreach (ChildBookingItem childBook in campBookingInfo)
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
			return paymentDescription.ToString();
		}

		private Student GetStudent(long id)
		{
			return this.db.Students.FirstOrDefault<Student>((Student s) => s.Id == id);
		}

		private Workshop GetWorkshop(long id)
		{
			Workshop workshop = (base.User.Admin.IsSuperAdmin ? this.db.Workshops.First<Workshop>((Workshop w) => w.Id == id) : this.db.Workshops.First<Workshop>((Workshop w) => w.Id == id && (long?)w.Location.AreaId == this.User.Admin.AreaId));
			workshop.UpdateCustomProperties();
			return workshop;
		}

		[ActionName("instructors-delete")]
		[HttpPost]
		public ActionResult InstructorDelete(long id)
		{
			try
			{
				Instructor instructor = this.GetInstructor(id);
				if (instructor != null)
				{
					this.db.Instructors.Remove(instructor);
					this.db.SaveChanges();
					GoogleContactHelper googleContactHelper = new GoogleContactHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
					googleContactHelper.DeleteContactFromInstructor(id.ToString());
					base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Instructor" });
				}
				else
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
					return base.Content(Messages.DataNotFound);
				}
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Instructor" });
			}
			return base.RedirectToRoute("Admin", new { action = "instructors", research = "true" });
		}

		[ActionName("instructors-detail")]
		[HttpGet]
		public ActionResult InstructorDetail(long? id)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!id.HasValue)
			{
				if (base.User.IsInRole("superadmin"))
				{
					Instructor instructor1 = new Instructor()
					{
						AreaId = base.User.Admin.AreaId.Value
					};
					return base.View(instructor1);
				}
				Instructor instructor2 = new Instructor()
				{
					AreaId = base.User.Admin.AreaId.Value
				};
				return base.View("instructors-detail-admin", instructor2);
			}
			Instructor instructor = this.GetInstructor(id.Value);
			if (instructor == null)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DataNotFound, new object[0]);
				return base.RedirectToRoute("Admin", new { action = "instructors", research = "true" });
			}
			if (base.User.IsInRole("superadmin"))
			{
				return base.View(instructor);
			}
			return base.View("instructors-detail-admin", instructor);
		}

		[ActionName("instructors-detail")]
		[HttpPost]
		public ActionResult InstructorDetail(Instructor model)
		{
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(model);
			}
			try
			{
				Instructor dbInstructor = null;
				if (model.Id <= (long)0)
				{
					dbInstructor = new Instructor();
					this.db.Instructors.Add(dbInstructor);
				}
				else
				{
					dbInstructor = this.GetInstructor(model.Id);
				}
				dbInstructor.AreaId = model.AreaId;
				dbInstructor.FirstName = model.FirstName;
				dbInstructor.LastName = model.LastName;
				dbInstructor.Address = model.Address;
				dbInstructor.City = model.City;
				dbInstructor.State = model.State;
				dbInstructor.Zip = model.Zip;
				dbInstructor.Email = model.Email;
				dbInstructor.PhoneNumber = model.PhoneNumber;
				dbInstructor.Pay = model.Pay;
				dbInstructor.IsActive = model.IsActive;
				dbInstructor.Note = model.Note;
				dbInstructor.CreatedBy = base.User.Admin.Id;
				dbInstructor.CreatedDate = DateTime.Now;
				this.db.SaveChanges();
				GoogleContactHelper googleContactHelper = new GoogleContactHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleContactHelper.SynContactFromInstructor(dbInstructor);
				base.SetViewMessage(WithAuthenController.MessageType.Info, (model.Id > (long)0 ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "Instructor" });
				route = base.RedirectToRoute("Admin", new { action = "instructors-detail", id = dbInstructor.Id });
			}
			catch (Exception exception)
			{
				this.log.Error("Add/update instructor failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = (!base.User.IsInRole("superadmin") ? base.View("instructors-detail-admin", model) : base.View(model));
			}
			return route;
		}

		[ActionName("instructor-detail-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult InstructorDetailExcel(long? id)
		{
			Instructor instructor = this.GetInstructor(id.Value);
			((dynamic)ViewBag).FileName = string.Format("{0} {1}", instructor.FirstName, instructor.LastName);
			((dynamic)ViewBag).Detail = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Detail;
			string[] firstName = new string[] { instructor.FirstName, instructor.LastName, instructor.PhoneNumber, instructor.Pay, instructor.Address, instructor.City, instructor.State, instructor.Zip, instructor.Email, instructor.Note, null };
			firstName[10] = (instructor.IsActive ? "Yes" : "No");
			viewBag.Add(firstName);
			CalendarSearchForm form = (CalendarSearchForm)base.GetSession("Calendar.Schedule.SearchForm");
			((dynamic)ViewBag).CalendarSearchForm = form;
			((dynamic)ViewBag).Calendar = this.PerformCalendarSearch(form);
			return new EmptyResult();
		}

		[HttpGet]
		public ActionResult Instructors(InstructorSearchForm form, int? page, bool? research)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!ModelState.IsValid)
			{
				return base.View(form);
			}
			if (!research.HasValue || !research.Value || base.GetSession("Instructors.SearchForm") == null)
			{
				base.SetSession("Instructors.SearchForm", form);
			}
			InstructorSearchForm sessionForm = (InstructorSearchForm)base.GetSession("Instructors.SearchForm");
			InstructorSearchForm instructorSearchForm = sessionForm;
			int? nullable = page;
			SearchResult<InstructorSearchForm, Instructor> instructors = this.PerformInstructorSearch(instructorSearchForm, (nullable.HasValue ? nullable.GetValueOrDefault() : 1), new int?(2147483647));
			if (instructors.TotalItems != 0)
			{
				((dynamic)ViewBag).Instructors = instructors;
			}
			else
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.NoRecordFound, new object[] { "instructor" });
			}
			return base.View("instructors", sessionForm);
		}

		[ActionName("instructors-all")]
		[HttpGet]
		public ActionResult InstructorsAll()
		{
			return base.RedirectToAction("instructors", new { page = 1, research = false, itemsPerPage = 2147483647 });
		}

		[ActionName("instructors-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult InstructorsExcel()
		{
			InstructorSearchForm sessionForm = (InstructorSearchForm)base.GetSession("Instructors.SearchForm") ?? new InstructorSearchForm();
			((dynamic)ViewBag).FileName = "Instructors.xlsx";
			((dynamic)ViewBag).SheetName = "Instructors";
			((dynamic)ViewBag).Headers = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Headers;
			string[] strArrays = new string[] { "Name", "Email", "Phone", "Pay", "Active" };
			viewBag.Add(strArrays);
			SearchResult<InstructorSearchForm, Instructor> instructors = this.PerformInstructorSearch(sessionForm, 1, new int?(2147483647));
			dynamic list = ViewBag;
			List<Instructor> pageItems = instructors.PageItems;
			list.ExportData = (
				from t in pageItems
				select new { Name = string.Concat(t.FirstName, " ", t.LastName), Email = t.Email, Phone = t.PhoneNumber, Pay = t.Pay, Active = (t.IsActive ? "Yes" : "No") }).ToList();
			return new EmptyResult();
		}

		[ActionName("locations-detail")]
		[HttpGet]
		public ActionResult LocationDetail(long? id)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!id.HasValue)
			{
				if (base.User.IsInRole("superadmin"))
				{
					Location location1 = new Location()
					{
						AreaId = base.AdminAreaId
					};
					return base.View(location1);
				}
				Location location2 = new Location()
				{
					AreaId = base.AdminAreaId
				};
				return base.View("locations-detail-admin", location2);
			}
			Location location = this.GetLocation(id.Value);
			if (location == null)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DataNotFound, new object[0]);
				return base.RedirectToRoute("Admin", new { action = "locations", research = "true" });
			}
			if (base.User.IsInRole("superadmin"))
			{
				return base.View(location);
			}
			return base.View("locations-detail-admin", location);
		}

		[ActionName("location-detail-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult LocationDetailExcel(long? id)
		{
			Location location = this.GetLocation(id.Value);
			((dynamic)ViewBag).FileName = location.DisplayName;
			((dynamic)ViewBag).Detail = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Detail;
			string[] name = new string[] { location.Name, location.DisplayName, location.PhoneNumber, location.Address, location.City, location.State, location.Zip, location.Email, location.ContactPerson, location.Note, null, null };
			name[10] = (location.IsActive ? "Yes" : "No");
			name[11] = (location.CanRegistOnline ? "Yes" : "No");
			viewBag.Add(name);
			CalendarSearchForm form = (CalendarSearchForm)base.GetSession("Calendar.Schedule.SearchForm");
			((dynamic)ViewBag).CalendarSearchForm = form;
			((dynamic)ViewBag).Calendar = this.PerformCalendarSearch(form);
			return new EmptyResult();
		}

		[HttpGet]
		public ActionResult Locations(LocationSearchForm form, int? page, bool? research)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!ModelState.IsValid)
			{
				return base.View(form);
			}
			if (!research.HasValue || !research.Value || base.GetSession("Locations.SearchForm") == null)
			{
				base.SetSession("Locations.SearchForm", form);
			}
			LocationSearchForm sessionForm = (LocationSearchForm)base.GetSession("Locations.SearchForm");
			LocationSearchForm locationSearchForm = sessionForm;
			int? nullable = page;
			SearchResult<LocationSearchForm, Location> locations = this.PerformLocationSearch(locationSearchForm, (nullable.HasValue ? nullable.GetValueOrDefault() : 1), new int?(2147483647));
			if (locations.TotalItems != 0)
			{
				((dynamic)ViewBag).Locations = locations;
			}
			else
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.NoRecordFound, new object[] { "location" });
			}
			return base.View("locations", sessionForm);
		}

		[ActionName("locations-all")]
		[HttpGet]
		public ActionResult LocationsAll()
		{
			return base.RedirectToAction("Locations", new { page = 1, research = false, itemsPerPage = 2147483647 });
		}

		[ActionName("locations-delete")]
		[HttpPost]
		public ActionResult LocationsDelete(long id)
		{
			try
			{
				Location location = this.GetLocation(id);
				if (location != null)
				{
					this.db.Locations.Remove(location);
					this.db.SaveChanges();
					GoogleContactHelper googleContactHelper = new GoogleContactHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
					googleContactHelper.DeleteContactFromLocation(id.ToString());
					base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Location" });
				}
				else
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
					return base.Content(Messages.DataNotFound);
				}
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Location" });
			}
			return base.RedirectToRoute("Admin", new { action = "locations", research = "true" });
		}

		[ActionName("locations-detail")]
		[HttpPost]
		public ActionResult LocationsDetail(Location model)
		{
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(model);
			}
			try
			{
				Location dbLocation = null;
				if (model.Id <= (long)0)
				{
					dbLocation = new Location();
					this.db.Locations.Add(dbLocation);
				}
				else
				{
					dbLocation = this.GetLocation(model.Id);
				}
				dbLocation.AreaId = model.AreaId;
				dbLocation.CanRegistOnline = model.CanRegistOnline;
				dbLocation.ContactPerson = model.ContactPerson;
				dbLocation.Email = model.Email;
				dbLocation.IsActive = model.IsActive;
				dbLocation.Name = model.Name;
				dbLocation.DisplayName = model.DisplayName;
				dbLocation.Note = model.Note;
				dbLocation.PhoneNumber = model.PhoneNumber;
				dbLocation.Address = model.Address;
				dbLocation.City = model.City;
				dbLocation.Zip = model.Zip;
				dbLocation.State = model.State;
				dbLocation.UpdatedBy = base.User.Admin.Id;
				dbLocation.UpdatedDate = DateTime.Now;
				this.db.SaveChanges();
				GoogleContactHelper googleContactHelper = new GoogleContactHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleContactHelper.SynContactFromLocation(dbLocation);
				base.SetViewMessage(WithAuthenController.MessageType.Info, (model.Id > (long)0 ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "Location" });
				route = base.RedirectToRoute("Admin", new { action = "locations-detail", id = dbLocation.Id });
			}
			catch (Exception exception)
			{
				this.log.Error("Add/update location failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(model);
			}
			return route;
		}

		[ActionName("locations-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult LocationsExcel()
		{
			LocationSearchForm sessionForm = (LocationSearchForm)base.GetSession("Locations.SearchForm") ?? new LocationSearchForm();
			((dynamic)ViewBag).FileName = "Locations.xlsx";
			((dynamic)ViewBag).SheetName = "Locations";
			((dynamic)ViewBag).Headers = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Headers;
			string[] strArrays = new string[] { "Location", "Address", "Email", "Phone", "Contact", "Active", "Online" };
			viewBag.Add(strArrays);
			SearchResult<LocationSearchForm, Location> locations = this.PerformLocationSearch(sessionForm, 1, new int?(2147483647));
			dynamic list = ViewBag;
			List<Location> pageItems = locations.PageItems;
			list.ExportData = (
				from t in pageItems
				select new { Location = t.DisplayName, Address = StringUtil.GetFullAddress(t.Address, t.City, t.State, t.Zip), Email = t.Email, Phone = t.PhoneNumber, Contact = t.ContactPerson, Active = (t.IsActive ? "Yes" : "No"), Online = (t.CanRegistOnline ? "Yes" : "No") }).ToList();
			return new EmptyResult();
		}

		private PayrollItem NewPayrollItem(Instructor instructor, PayrollSearchForm form)
		{
			//AdminController.;
            
		 //   ParameterExpression parameterExpression;
			//ParameterExpression[] parameterExpressionArray;
			//ParameterExpression parameterExpression1;
			//ParameterExpression parameterExpression2;
			//IQueryable<Assign> assignsAsInstructor = instructor.Assigns.AsQueryable<Assign>();
			//IQueryable<Assign> assignsAsAssitant = instructor.Assigns1.AsQueryable<Assign>();
			//long? locationId = form.LocationId;
			//if (locationId.HasValue)
   //         {
   //             parameterExpression = Parameter( typeof( Assign ) );
   //             long? nullable = null;
   //             long? nullable1 = null;
   //             long? nullable2 = null;
   //             BinaryExpression binaryExpression = Expression.OrElse( Expression.OrElse( Expression.OrElse( AndAlso( NotEqual( Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_ClassId" ).MethodHandle ) ), Constant( nullable, typeof( long? ) ) ), Equal( Convert( Property( Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Class" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Class ).GetMethod( "get_LocationId" ).MethodHandle ) ), typeof( long? ) ), Expression.Property( Field( Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_LocationId" ).MethodHandle ) ) ) ), AndAlso( NotEqual( Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_CampId" ).MethodHandle ) ), Constant( nullable1, typeof( long? ) ) ), Equal( Convert( Property( Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Camp" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Camp ).GetMethod( "get_LocationId" ).MethodHandle ) ), typeof( long? ) ), Expression.Property( Field( Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_LocationId" ).MethodHandle ) ) ) ) ), AndAlso( NotEqual( Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_WorkshopId" ).MethodHandle ) ), Constant( nullable2, typeof( long? ) ) ), Equal( Convert( Property( Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Workshop" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Workshop ).GetMethod( "get_LocationId" ).MethodHandle ) ), typeof( long? ) ), Expression.Property( Field( Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_LocationId" ).MethodHandle ) ) ))), NotEqual( Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Birthday" ).MethodHandle ) ), Constant( null, typeof( object ) ) ));
   //             parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //             assignsAsInstructor = assignsAsInstructor.Where<Assign>( Lambda<Func<Assign, bool>>( binaryExpression, parameterExpressionArray ) );
   //             parameterExpression1 = Parameter( typeof( Assign ), "a" );
   //             long? nullable3 = null;
   //             long? nullable4 = null;
   //             long? nullable5 = null;
   //             BinaryExpression binaryExpression1 = Expression.OrElse( Expression.OrElse( Expression.OrElse( AndAlso( NotEqual( Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_ClassId" ).MethodHandle ) ), Constant( nullable3, typeof( long? ) ) ), Equal( Convert( Property( Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Class" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Class ).GetMethod( "get_LocationId" ).MethodHandle ) ), typeof( long? ) ), Expression.Property( Field( Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController ).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_LocationId" ).MethodHandle ) ) ) ), AndAlso( NotEqual( Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_CampId" ).MethodHandle ) ), Constant( nullable4, typeof( long? ) ) ), Equal( Convert( Property( Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Camp" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Camp ).GetMethod( "get_LocationId" ).MethodHandle ) ), typeof( long? ) ), Expression.Property( Field( Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_LocationId" ).MethodHandle ) ) ) ) ), AndAlso( NotEqual( Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_WorkshopId" ).MethodHandle ) ), Constant( nullable5, typeof( long? ) ) ), Equal( Convert( Property( Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Workshop" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Workshop ).GetMethod( "get_LocationId" ).MethodHandle ) ), typeof( long? ) ), Expression.Property( Field( Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_LocationId" ).MethodHandle ) ) ))), NotEqual( Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Birthday" ).MethodHandle ) ), Constant( null, typeof( object ) ) ));
   //             ParameterExpression[ ] parameterExpressionArray1 = new ParameterExpression[ ] { parameterExpression1 };
   //             assignsAsAssitant = assignsAsAssitant.Where<Assign>( Lambda<Func<Assign, bool>>( binaryExpression1, parameterExpressionArray1 ) );
   //         }
   //         if (form.DateFrom.HasValue)
   //         {
   //             parameterExpression2 = Expression.Parameter( typeof( Assign ), "a" );
   //             BinaryExpression binaryExpression2 = Expression.GreaterThanOrEqual( Expression.Property( parameterExpression2, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) ), Expression.Property( Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_DateFrom" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime? ).GetMethod( "get_Value" ).MethodHandle, typeof( DateTime? ).TypeHandle ) ), false, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "op_GreaterThanOrEqual", new Type[ ] { typeof( DateTime ), typeof( DateTime ) } ).MethodHandle ));
   //             ParameterExpression[ ] parameterExpressionArray2 = new ParameterExpression[ ] { parameterExpression2 };
   //             assignsAsInstructor = assignsAsInstructor.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression2, parameterExpressionArray2 ) );
   //             ParameterExpression parameterExpression3 = Expression.Parameter( typeof( Assign ), "a" );
   //             BinaryExpression binaryExpression3 = Expression.GreaterThanOrEqual( Expression.Property( parameterExpression3, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) ), Expression.Property( Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_DateFrom" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime? ).GetMethod( "get_Value" ).MethodHandle, typeof( DateTime? ).TypeHandle ) ), false, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "op_GreaterThanOrEqual", new Type[ ] { typeof( DateTime ), typeof( DateTime ) } ).MethodHandle ));
   //             ParameterExpression[ ] parameterExpressionArray3 = new ParameterExpression[ ] { parameterExpression3 };
   //             assignsAsAssitant = assignsAsAssitant.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression3, parameterExpressionArray3 ) );
   //         }
   //         if (form.DateTo.HasValue)
   //         {
   //             ParameterExpression parameterExpression4 = Expression.Parameter( typeof( Assign ), "a" );
   //             BinaryExpression binaryExpression4 = Expression.LessThanOrEqual( Expression.Property( parameterExpression4, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) ), Expression.Property( Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_DateTo" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime? ).GetMethod( "get_Value" ).MethodHandle, typeof( DateTime? ).TypeHandle ) ), false, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "op_LessThanOrEqual", new Type[ ] { typeof( DateTime ), typeof( DateTime ) } ).MethodHandle ));
   //             ParameterExpression[ ] parameterExpressionArray4 = new ParameterExpression[ ] { parameterExpression4 };
   //             assignsAsInstructor = assignsAsInstructor.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression4, parameterExpressionArray4 ) );
   //             ParameterExpression parameterExpression5 = Expression.Parameter( typeof( Assign ), "a" );
   //             BinaryExpression binaryExpression5 = Expression.LessThanOrEqual( Expression.Property( parameterExpression5, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) ), Expression.Property( Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "form" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollSearchForm ).GetMethod( "get_DateTo" ).MethodHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime? ).GetMethod( "get_Value" ).MethodHandle, typeof( DateTime? ).TypeHandle ) ), false, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "op_LessThanOrEqual", new Type[ ] { typeof( DateTime ), typeof( DateTime ) } ).MethodHandle ));
   //             ParameterExpression[ ] parameterExpressionArray5 = new ParameterExpression[ ] { parameterExpression5 };
   //             assignsAsAssitant = assignsAsAssitant.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression5, parameterExpressionArray5 ) );
   //         }
   //         ParameterExpression parameterExpression6 = Expression.Parameter( typeof( Assign ), "t" );
   //         long? nullable6 = null;
   //         BinaryExpression binaryExpression6 = Expression.NotEqual( Expression.Property( parameterExpression6, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_ClassId" ).MethodHandle ) ), Expression.Constant( nullable6, typeof( long? ) ) );
   //         ParameterExpression[ ] parameterExpressionArray6 = new ParameterExpression[ ] { parameterExpression6 };
   //         IQueryable<Assign> assignsAsInstructorClasses = assignsAsInstructor.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression6, parameterExpressionArray6 ) );
   //         ParameterExpression parameterExpression7 = Expression.Parameter( typeof( Assign ), "t" );
   //         long? nullable7 = null;
   //         BinaryExpression binaryExpression7 = Expression.NotEqual( Expression.Property( parameterExpression7, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_CampId" ).MethodHandle ) ), Expression.Constant( nullable7, typeof( long? ) ) );
   //         ParameterExpression[ ] parameterExpressionArray7 = new ParameterExpression[ ] { parameterExpression7 };
   //         IQueryable<Assign> assignsAsInstructorCamps = assignsAsInstructor.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression7, parameterExpressionArray7 ) );
   //         ParameterExpression parameterExpression8 = Expression.Parameter( typeof( Assign ), "t" );
   //         long? nullable8 = null;
   //         BinaryExpression binaryExpression8 = Expression.NotEqual( Expression.Property( parameterExpression8, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_ClassId" ).MethodHandle ) ), Expression.Constant( nullable8, typeof( long? ) ) );
   //         ParameterExpression[ ] parameterExpressionArray8 = new ParameterExpression[ ] { parameterExpression8 };
   //         IQueryable<Assign> assignsAsAssitantClasses = assignsAsAssitant.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression8, parameterExpressionArray8 ) );
   //         ParameterExpression parameterExpression9 = Expression.Parameter( typeof( Assign ), "t" );
   //         long? nullable9 = null;
   //         BinaryExpression binaryExpression9 = Expression.NotEqual( Expression.Property( parameterExpression9, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_CampId" ).MethodHandle ) ), Expression.Constant( nullable9, typeof( long? ) ) );
   //         ParameterExpression[ ] parameterExpressionArray9 = new ParameterExpression[ ] { parameterExpression9 };
   //         IQueryable<Assign> assignsAsAssitantCamps = assignsAsAssitant.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression9, parameterExpressionArray9 ) );
		 //   PayrollItem payrollItem = new PayrollItem()
   //         {
   //             InstructorId = instructor.Id,
   //         	InstructorName = StringUtil.GetFullName( instructor.FirstName, instructor.LastName ),
   //         	TeachCount = assignsAsInstructorClasses.Count<Assign>( ) + assignsAsInstructorCamps.Count<Assign>( )
   //         };
   //         ParameterExpression parameterExpression10 = Expression.Parameter( typeof( Assign ), "a" );
   //         BinaryExpression binaryExpression10 = Expression.NotEqual( Expression.Property( parameterExpression10, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Birthday" ).MethodHandle ) ), Expression.Constant( null, typeof( object ) ) );
   //         ParameterExpression[ ] parameterExpressionArray10 = new ParameterExpression[ ] { parameterExpression10 };
   //         int num = assignsAsInstructor.Count<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression10, parameterExpressionArray10 ) );
   //         ParameterExpression parameterExpression11 = Expression.Parameter( typeof( Assign ), "a" );
   //         BinaryExpression binaryExpression11 = Expression.NotEqual( Expression.Property( parameterExpression11, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Birthday" ).MethodHandle ) ), Expression.Constant( null, typeof( object ) ) );
   //         ParameterExpression[ ] parameterExpressionArray11 = new ParameterExpression[ ] { parameterExpression11 };
   //         payrollItem.BirthdayCount = num + assignsAsAssitant.Count<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression11, parameterExpressionArray11 ) );
   //         ParameterExpression parameterExpression12 = Expression.Parameter( typeof( Assign ), "a" );
   //         BinaryExpression binaryExpression12 = Expression.NotEqual( Expression.Property( parameterExpression12, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Workshop" ).MethodHandle ) ), Expression.Constant( null, typeof( object ) ) );
   //         ParameterExpression[ ] parameterExpressionArray12 = new ParameterExpression[ ] { parameterExpression12 };
   //         int num1 = assignsAsInstructor.Count<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression12, parameterExpressionArray12 ) );
   //         ParameterExpression parameterExpression13 = Expression.Parameter( typeof( Assign ), "a" );
   //         BinaryExpression binaryExpression13 = Expression.NotEqual( Expression.Property( parameterExpression13, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Workshop" ).MethodHandle ) ), Expression.Constant( null, typeof( object ) ) );
   //         ParameterExpression[ ] parameterExpressionArray13 = new ParameterExpression[ ] { parameterExpression13 };
   //         payrollItem.WorkshopCount = num1 + assignsAsAssitant.Count<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression13, parameterExpressionArray13 ) );
   //         payrollItem.AssistCount = assignsAsAssitantClasses.Count<Assign>( ) + assignsAsAssitantCamps.Count<Assign>( );
   //         ParameterExpression parameterExpression14 = Expression.Parameter( typeof( Assign ), "t" );
   //         MemberExpression memberExpression = Expression.Property( parameterExpression14, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_ClassId" ).MethodHandle ) );
   //         ParameterExpression[ ] parameterExpressionArray14 = new ParameterExpression[ ] { parameterExpression14 };
   //         IQueryable<IGrouping<long?, Assign>> groupings = assignsAsInstructorClasses.GroupBy<Assign, long?>( Expression.Lambda<Func<Assign, long?>>( memberExpression, parameterExpressionArray14 ) );
   //         ParameterExpression parameterExpression15 = Expression.Parameter( typeof( IGrouping<long?, Assign> ), "group" );
   //         NewExpression newExpression = Expression.New( ( ConstructorInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( ".ctor" ).MethodHandle ), new Expression[ 0 ] );
   //         MemberBinding[ ] memberBindingArray = new MemberBinding[ ] { Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Type", new Type[ ] { typeof( ServiceType ) } ).MethodHandle ), Expression.Constant( ServiceType.Class, typeof( ServiceType ) ) ), Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Property( parameterExpression15, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( IGrouping<long?, Assign> ).GetMethod( "get_Key" ).MethodHandle, typeof( IGrouping<long?, Assign> ).TypeHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( long? ).GetMethod( "get_Value" ).MethodHandle, typeof( long? ).TypeHandle ) ) ), null, null, null, null };
   //         MethodInfo methodFromHandle = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceName", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "First", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         Expression[ ] expressionArray = new Expression[ ] { parameterExpression15 };
   //         memberBindingArray[ 2 ] = Expression.Bind( methodFromHandle, Expression.Property( Expression.Call( null, methodInfo, expressionArray ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Name" ).MethodHandle ) ) );
   //         memberBindingArray[ 3 ] = Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_InstructorId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "instructor" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Instructor ).GetMethod( "get_Id" ).MethodHandle ) ));
   //         MethodInfo methodFromHandle1 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Count", new Type[ ] { typeof( long ) } ).MethodHandle );
   //         MethodInfo methodInfo1 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Count", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         Expression[ ] expressionArray1 = new Expression[ ] { parameterExpression15 };
   //         memberBindingArray[ 4 ] = Expression.Bind( methodFromHandle1, Expression.Convert( Expression.Call( null, methodInfo1, expressionArray1 ), typeof( long ) ) );
   //         MethodInfo methodFromHandle2 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Dates", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo2 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Aggregate", new Type[ ] { typeof( IEnumerable<string> ), typeof( Func<string, string, string> ) } ).MethodHandle );
   //         Expression[ ] expressionArray2 = new Expression[ 2 ];
   //         MethodInfo methodFromHandle3 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Select", new Type[ ] { typeof( IEnumerable<Assign> ), typeof( Func<Assign, string> ) } ).MethodHandle );
   //         Expression[ ] expressionArray3 = new Expression[ ] { parameterExpression15, null };
   //         ParameterExpression parameterExpression16 = Expression.Parameter( typeof( Assign ), "x" );
   //         MemberExpression memberExpression1 = Expression.Property( parameterExpression16, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) );
   //         MethodInfo methodInfo3 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "ToString", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         Expression[ ] expressionArray4 = new Expression[ ] { Expression.Constant( "M/dd", typeof( string ) ) };
   //         MethodCallExpression methodCallExpression = Expression.Call( memberExpression1, methodInfo3, expressionArray4 );
   //         ParameterExpression[ ] parameterExpressionArray15 = new ParameterExpression[ ] { parameterExpression16 };
   //         expressionArray3[ 1 ] = Expression.Lambda<Func<Assign, string>>( methodCallExpression, parameterExpressionArray15 );
   //         expressionArray2[ 0 ] = Expression.Call( null, methodFromHandle3, expressionArray3 );
   //         parameterExpression = Expression.Parameter( typeof( string ), "a" );
   //         parameterExpression1 = Expression.Parameter( typeof( string ), "b" );
   //         BinaryExpression binaryExpression14 = Expression.Add( Expression.Add( parameterExpression, Expression.Constant( ", ", typeof( string ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) ), parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression, parameterExpression1 };
   //         expressionArray2[ 1 ] = Expression.Lambda<Func<string, string, string>>( binaryExpression14, parameterExpressionArray );
   //         memberBindingArray[ 5 ] = Expression.Bind( methodFromHandle2, Expression.Call( null, methodInfo2, expressionArray2 ) );
   //         MemberInitExpression memberInitExpression = Expression.MemberInit( newExpression, memberBindingArray );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression15 };
   //         IQueryable<PayrollItemDetail> payrollItemDetails = groupings.Select<IGrouping<long?, Assign>, PayrollItemDetail>( Expression.Lambda<Func<IGrouping<long?, Assign>, PayrollItemDetail>>( memberInitExpression, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "t" );
   //         MemberExpression memberExpression2 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_CampId" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<IGrouping<long?, Assign>> groupings1 = assignsAsInstructorCamps.GroupBy<Assign, long?>( Expression.Lambda<Func<Assign, long?>>( memberExpression2, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( IGrouping<long?, Assign> ), "group" );
   //         NewExpression newExpression1 = Expression.New( ( ConstructorInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( ".ctor" ).MethodHandle ), new Expression[ 0 ] );
   //         memberBindingArray = new MemberBinding[ ] { Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Type", new Type[ ] { typeof( ServiceType ) } ).MethodHandle ), Expression.Constant( ServiceType.Camp, typeof( ServiceType ) ) ), Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( IGrouping<long?, Assign> ).GetMethod( "get_Key" ).MethodHandle, typeof( IGrouping<long?, Assign> ).TypeHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( long? ).GetMethod( "get_Value" ).MethodHandle, typeof( long? ).TypeHandle ) ) ), null, null, null, null };
   //         MethodInfo methodFromHandle4 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceName", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo4 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "First", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 2 ] = Expression.Bind( methodFromHandle4, Expression.Property( Expression.Call( null, methodInfo4, expressionArray ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Name" ).MethodHandle ) ) );
   //         memberBindingArray[ 3 ] = Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_InstructorId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "instructor" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Instructor ).GetMethod( "get_Id" ).MethodHandle ) ));
   //         MethodInfo methodFromHandle5 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Count", new Type[ ] { typeof( long ) } ).MethodHandle );
   //         MethodInfo methodInfo5 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Count", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 4 ] = Expression.Bind( methodFromHandle5, Expression.Convert( Expression.Call( null, methodInfo5, expressionArray ), typeof( long ) ) );
   //         MethodInfo methodFromHandle6 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Dates", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo6 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Aggregate", new Type[ ] { typeof( IEnumerable<string> ), typeof( Func<string, string, string> ) } ).MethodHandle );
   //         expressionArray = new Expression[ 2 ];
   //         MethodInfo methodFromHandle7 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Select", new Type[ ] { typeof( IEnumerable<Assign> ), typeof( Func<Assign, string> ) } ).MethodHandle );
   //         expressionArray1 = new Expression[ ] { parameterExpression, null };
   //         parameterExpression1 = Expression.Parameter( typeof( Assign ), "x" );
   //         MemberExpression memberExpression3 = Expression.Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) );
   //         MethodInfo methodInfo7 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "ToString", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         expressionArray2 = new Expression[ ] { Expression.Constant( "M/dd", typeof( string ) ) };
   //         MethodCallExpression methodCallExpression1 = Expression.Call( memberExpression3, methodInfo7, expressionArray2 );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1 };
   //         expressionArray1[ 1 ] = Expression.Lambda<Func<Assign, string>>( methodCallExpression1, parameterExpressionArray );
   //         expressionArray[ 0 ] = Expression.Call( null, methodFromHandle7, expressionArray1 );
   //         parameterExpression1 = Expression.Parameter( typeof( string ), "a" );
   //         parameterExpression2 = Expression.Parameter( typeof( string ), "b" );
   //         BinaryExpression binaryExpression15 = Expression.Add( Expression.Add( parameterExpression1, Expression.Constant( ", ", typeof( string ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) ), parameterExpression2, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1, parameterExpression2 };
   //         expressionArray[ 1 ] = Expression.Lambda<Func<string, string, string>>( binaryExpression15, parameterExpressionArray );
   //         memberBindingArray[ 5 ] = Expression.Bind( methodFromHandle6, Expression.Call( null, methodInfo6, expressionArray ) );
   //         MemberInitExpression memberInitExpression1 = Expression.MemberInit( newExpression1, memberBindingArray );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<PayrollItemDetail> payrollItemDetails1 = payrollItemDetails.Union<PayrollItemDetail>( groupings1.Select<IGrouping<long?, Assign>, PayrollItemDetail>( Expression.Lambda<Func<IGrouping<long?, Assign>, PayrollItemDetail>>( memberInitExpression1, parameterExpressionArray ) ) );
   //         parameterExpression = Expression.Parameter( typeof( PayrollItemDetail ), "x" );
   //         MemberExpression memberExpression4 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "get_ServiceName" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         payrollItem.TeachDetails = payrollItemDetails1.OrderBy<PayrollItemDetail, string>( Expression.Lambda<Func<PayrollItemDetail, string>>( memberExpression4, parameterExpressionArray ) ).ToList<PayrollItemDetail>( );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "t" );
   //         MemberExpression memberExpression5 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_ClassId" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<IGrouping<long?, Assign>> groupings2 = assignsAsAssitantClasses.GroupBy<Assign, long?>( Expression.Lambda<Func<Assign, long?>>( memberExpression5, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( IGrouping<long?, Assign> ), "group" );
   //         NewExpression newExpression2 = Expression.New( ( ConstructorInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( ".ctor" ).MethodHandle ), new Expression[ 0 ] );
   //         memberBindingArray = new MemberBinding[ ] { Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Type", new Type[ ] { typeof( ServiceType ) } ).MethodHandle ), Expression.Constant( ServiceType.Class, typeof( ServiceType ) ) ), Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( IGrouping<long?, Assign> ).GetMethod( "get_Key" ).MethodHandle, typeof( IGrouping<long?, Assign> ).TypeHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( long? ).GetMethod( "get_Value" ).MethodHandle, typeof( long? ).TypeHandle ) ) ), null, null, null, null };
   //         MethodInfo methodFromHandle8 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceName", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo8 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "First", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 2 ] = Expression.Bind( methodFromHandle8, Expression.Property( Expression.Call( null, methodInfo8, expressionArray ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Name" ).MethodHandle ) ) );
   //         memberBindingArray[ 3 ] = Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_InstructorId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "instructor" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Instructor ).GetMethod( "get_Id" ).MethodHandle ) ));
   //         MethodInfo methodFromHandle9 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Count", new Type[ ] { typeof( long ) } ).MethodHandle );
   //         MethodInfo methodInfo9 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Count", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 4 ] = Expression.Bind( methodFromHandle9, Expression.Convert( Expression.Call( null, methodInfo9, expressionArray ), typeof( long ) ) );
   //         MethodInfo methodFromHandle10 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Dates", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo10 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Aggregate", new Type[ ] { typeof( IEnumerable<string> ), typeof( Func<string, string, string> ) } ).MethodHandle );
   //         expressionArray = new Expression[ 2 ];
   //         MethodInfo methodFromHandle11 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Select", new Type[ ] { typeof( IEnumerable<Assign> ), typeof( Func<Assign, string> ) } ).MethodHandle );
   //         expressionArray1 = new Expression[ ] { parameterExpression, null };
   //         parameterExpression1 = Expression.Parameter( typeof( Assign ), "x" );
   //         MemberExpression memberExpression6 = Expression.Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) );
   //         MethodInfo methodInfo11 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "ToString", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         expressionArray2 = new Expression[ ] { Expression.Constant( "M/dd", typeof( string ) ) };
   //         MethodCallExpression methodCallExpression2 = Expression.Call( memberExpression6, methodInfo11, expressionArray2 );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1 };
   //         expressionArray1[ 1 ] = Expression.Lambda<Func<Assign, string>>( methodCallExpression2, parameterExpressionArray );
   //         expressionArray[ 0 ] = Expression.Call( null, methodFromHandle11, expressionArray1 );
   //         parameterExpression1 = Expression.Parameter( typeof( string ), "a" );
   //         parameterExpression2 = Expression.Parameter( typeof( string ), "b" );
   //         BinaryExpression binaryExpression16 = Expression.Add( Expression.Add( parameterExpression1, Expression.Constant( ", ", typeof( string ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) ), parameterExpression2, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1, parameterExpression2 };
   //         expressionArray[ 1 ] = Expression.Lambda<Func<string, string, string>>( binaryExpression16, parameterExpressionArray );
   //         memberBindingArray[ 5 ] = Expression.Bind( methodFromHandle10, Expression.Call( null, methodInfo10, expressionArray ) );
   //         MemberInitExpression memberInitExpression2 = Expression.MemberInit( newExpression2, memberBindingArray );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<PayrollItemDetail> payrollItemDetails2 = groupings2.Select<IGrouping<long?, Assign>, PayrollItemDetail>( Expression.Lambda<Func<IGrouping<long?, Assign>, PayrollItemDetail>>( memberInitExpression2, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "t" );
   //         MemberExpression memberExpression7 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_CampId" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<IGrouping<long?, Assign>> groupings3 = assignsAsAssitantCamps.GroupBy<Assign, long?>( Expression.Lambda<Func<Assign, long?>>( memberExpression7, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( IGrouping<long?, Assign> ), "group" );
   //         NewExpression newExpression3 = Expression.New( ( ConstructorInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( ".ctor" ).MethodHandle ), new Expression[ 0 ] );
   //         memberBindingArray = new MemberBinding[ ] { Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Type", new Type[ ] { typeof( ServiceType ) } ).MethodHandle ), Expression.Constant( ServiceType.Camp, typeof( ServiceType ) ) ), Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( IGrouping<long?, Assign> ).GetMethod( "get_Key" ).MethodHandle, typeof( IGrouping<long?, Assign> ).TypeHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( long? ).GetMethod( "get_Value" ).MethodHandle, typeof( long? ).TypeHandle ) ) ), null, null, null, null };
   //         MethodInfo methodFromHandle12 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceName", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo12 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "First", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 2 ] = Expression.Bind( methodFromHandle12, Expression.Property( Expression.Call( null, methodInfo12, expressionArray ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Name" ).MethodHandle ) ) );
   //         memberBindingArray[ 3 ] = Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_InstructorId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "instructor" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Instructor ).GetMethod( "get_Id" ).MethodHandle ) ));
   //         MethodInfo methodFromHandle13 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Count", new Type[ ] { typeof( long ) } ).MethodHandle );
   //         MethodInfo methodInfo13 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Count", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 4 ] = Expression.Bind( methodFromHandle13, Expression.Convert( Expression.Call( null, methodInfo13, expressionArray ), typeof( long ) ) );
   //         MethodInfo methodFromHandle14 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Dates", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo14 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Aggregate", new Type[ ] { typeof( IEnumerable<string> ), typeof( Func<string, string, string> ) } ).MethodHandle );
   //         expressionArray = new Expression[ 2 ];
   //         MethodInfo methodFromHandle15 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Select", new Type[ ] { typeof( IEnumerable<Assign> ), typeof( Func<Assign, string> ) } ).MethodHandle );
   //         expressionArray1 = new Expression[ ] { parameterExpression, null };
   //         parameterExpression1 = Expression.Parameter( typeof( Assign ), "x" );
   //         MemberExpression memberExpression8 = Expression.Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) );
   //         MethodInfo methodInfo15 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "ToString", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         expressionArray2 = new Expression[ ] { Expression.Constant( "M/dd", typeof( string ) ) };
   //         MethodCallExpression methodCallExpression3 = Expression.Call( memberExpression8, methodInfo15, expressionArray2 );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1 };
   //         expressionArray1[ 1 ] = Expression.Lambda<Func<Assign, string>>( methodCallExpression3, parameterExpressionArray );
   //         expressionArray[ 0 ] = Expression.Call( null, methodFromHandle15, expressionArray1 );
   //         parameterExpression1 = Expression.Parameter( typeof( string ), "a" );
   //         parameterExpression2 = Expression.Parameter( typeof( string ), "b" );
   //         BinaryExpression binaryExpression17 = Expression.Add( Expression.Add( parameterExpression1, Expression.Constant( ", ", typeof( string ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) ), parameterExpression2, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1, parameterExpression2 };
   //         expressionArray[ 1 ] = Expression.Lambda<Func<string, string, string>>( binaryExpression17, parameterExpressionArray );
   //         memberBindingArray[ 5 ] = Expression.Bind( methodFromHandle14, Expression.Call( null, methodInfo14, expressionArray ) );
   //         MemberInitExpression memberInitExpression3 = Expression.MemberInit( newExpression3, memberBindingArray );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<PayrollItemDetail> payrollItemDetails3 = payrollItemDetails2.Union<PayrollItemDetail>( groupings3.Select<IGrouping<long?, Assign>, PayrollItemDetail>( Expression.Lambda<Func<IGrouping<long?, Assign>, PayrollItemDetail>>( memberInitExpression3, parameterExpressionArray ) ) );
   //         parameterExpression = Expression.Parameter( typeof( PayrollItemDetail ), "x" );
   //         MemberExpression memberExpression9 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "get_ServiceName" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         payrollItem.AssistDetails = payrollItemDetails3.OrderBy<PayrollItemDetail, string>( Expression.Lambda<Func<PayrollItemDetail, string>>( memberExpression9, parameterExpressionArray ) ).ToList<PayrollItemDetail>( );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "k" );
   //         locationId = null;
   //         BinaryExpression binaryExpression18 = Expression.NotEqual( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_WorkshopId" ).MethodHandle ) ), Expression.Constant( locationId, typeof( long? ) ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<Assign> assigns = assignsAsInstructor.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression18, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "t" );
   //         MemberExpression memberExpression10 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_WorkshopId" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<IGrouping<long?, Assign>> groupings4 = assigns.GroupBy<Assign, long?>( Expression.Lambda<Func<Assign, long?>>( memberExpression10, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( IGrouping<long?, Assign> ), "group" );
   //         NewExpression newExpression4 = Expression.New( ( ConstructorInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( ".ctor" ).MethodHandle ), new Expression[ 0 ] );
   //         memberBindingArray = new MemberBinding[ ] { Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Type", new Type[ ] { typeof( ServiceType ) } ).MethodHandle ), Expression.Constant( ServiceType.Workshop, typeof( ServiceType ) ) ), Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( IGrouping<long?, Assign> ).GetMethod( "get_Key" ).MethodHandle, typeof( IGrouping<long?, Assign> ).TypeHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( long? ).GetMethod( "get_Value" ).MethodHandle, typeof( long? ).TypeHandle ) ) ), null, null, null, null };
   //         MethodInfo methodFromHandle16 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceName", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo16 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "First", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 2 ] = Expression.Bind( methodFromHandle16, Expression.Property( Expression.Call( null, methodInfo16, expressionArray ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Name" ).MethodHandle ) ) );
   //         memberBindingArray[ 3 ] = Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_InstructorId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "instructor" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Instructor ).GetMethod( "get_Id" ).MethodHandle ) ));
   //         MethodInfo methodFromHandle17 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Count", new Type[ ] { typeof( long ) } ).MethodHandle );
   //         MethodInfo methodInfo17 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Count", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 4 ] = Expression.Bind( methodFromHandle17, Expression.Convert( Expression.Call( null, methodInfo17, expressionArray ), typeof( long ) ) );
   //         MethodInfo methodFromHandle18 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Dates", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo18 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Aggregate", new Type[ ] { typeof( IEnumerable<string> ), typeof( Func<string, string, string> ) } ).MethodHandle );
   //         expressionArray = new Expression[ 2 ];
   //         MethodInfo methodFromHandle19 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Select", new Type[ ] { typeof( IEnumerable<Assign> ), typeof( Func<Assign, string> ) } ).MethodHandle );
   //         expressionArray1 = new Expression[ ] { parameterExpression, null };
   //         parameterExpression1 = Expression.Parameter( typeof( Assign ), "x" );
   //         MemberExpression memberExpression11 = Expression.Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) );
   //         MethodInfo methodInfo19 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "ToString", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         expressionArray2 = new Expression[ ] { Expression.Constant( "M/dd", typeof( string ) ) };
   //         MethodCallExpression methodCallExpression4 = Expression.Call( memberExpression11, methodInfo19, expressionArray2 );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1 };
   //         expressionArray1[ 1 ] = Expression.Lambda<Func<Assign, string>>( methodCallExpression4, parameterExpressionArray );
   //         expressionArray[ 0 ] = Expression.Call( null, methodFromHandle19, expressionArray1 );
   //         parameterExpression1 = Expression.Parameter( typeof( string ), "a" );
   //         parameterExpression2 = Expression.Parameter( typeof( string ), "b" );
   //         BinaryExpression binaryExpression19 = Expression.Add( Expression.Add( parameterExpression1, Expression.Constant( ", ", typeof( string ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) ), parameterExpression2, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1, parameterExpression2 };
   //         expressionArray[ 1 ] = Expression.Lambda<Func<string, string, string>>( binaryExpression19, parameterExpressionArray );
   //         memberBindingArray[ 5 ] = Expression.Bind( methodFromHandle18, Expression.Call( null, methodInfo18, expressionArray ) );
   //         MemberInitExpression memberInitExpression4 = Expression.MemberInit( newExpression4, memberBindingArray );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<PayrollItemDetail> payrollItemDetails4 = groupings4.Select<IGrouping<long?, Assign>, PayrollItemDetail>( Expression.Lambda<Func<IGrouping<long?, Assign>, PayrollItemDetail>>( memberInitExpression4, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "k" );
   //         locationId = null;
   //         BinaryExpression binaryExpression20 = Expression.NotEqual( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_WorkshopId" ).MethodHandle ) ), Expression.Constant( locationId, typeof( long? ) ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<Assign> assigns1 = assignsAsAssitant.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression20, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "t" );
   //         MemberExpression memberExpression12 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_WorkshopId" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<IGrouping<long?, Assign>> groupings5 = assigns1.GroupBy<Assign, long?>( Expression.Lambda<Func<Assign, long?>>( memberExpression12, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( IGrouping<long?, Assign> ), "group" );
   //         NewExpression newExpression5 = Expression.New( ( ConstructorInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( ".ctor" ).MethodHandle ), new Expression[ 0 ] );
   //         memberBindingArray = new MemberBinding[ ] { Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Type", new Type[ ] { typeof( ServiceType ) } ).MethodHandle ), Expression.Constant( ServiceType.Workshop, typeof( ServiceType ) ) ), Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( IGrouping<long?, Assign> ).GetMethod( "get_Key" ).MethodHandle, typeof( IGrouping<long?, Assign> ).TypeHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( long? ).GetMethod( "get_Value" ).MethodHandle, typeof( long? ).TypeHandle ) ) ), null, null, null, null };
   //         MethodInfo methodFromHandle20 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceName", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo20 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "First", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 2 ] = Expression.Bind( methodFromHandle20, Expression.Property( Expression.Call( null, methodInfo20, expressionArray ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Name" ).MethodHandle ) ) );
   //         memberBindingArray[ 3 ] = Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_InstructorId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "instructor" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Instructor ).GetMethod( "get_Id" ).MethodHandle ) ));
   //         MethodInfo methodFromHandle21 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Count", new Type[ ] { typeof( long ) } ).MethodHandle );
   //         MethodInfo methodInfo21 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Count", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 4 ] = Expression.Bind( methodFromHandle21, Expression.Convert( Expression.Call( null, methodInfo21, expressionArray ), typeof( long ) ) );
   //         MethodInfo methodFromHandle22 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Dates", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo22 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Aggregate", new Type[ ] { typeof( IEnumerable<string> ), typeof( Func<string, string, string> ) } ).MethodHandle );
   //         expressionArray = new Expression[ 2 ];
   //         MethodInfo methodFromHandle23 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Select", new Type[ ] { typeof( IEnumerable<Assign> ), typeof( Func<Assign, string> ) } ).MethodHandle );
   //         expressionArray1 = new Expression[ ] { parameterExpression, null };
   //         parameterExpression1 = Expression.Parameter( typeof( Assign ), "x" );
   //         MemberExpression memberExpression13 = Expression.Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) );
   //         MethodInfo methodInfo23 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "ToString", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         expressionArray2 = new Expression[ ] { Expression.Constant( "M/dd", typeof( string ) ) };
   //         MethodCallExpression methodCallExpression5 = Expression.Call( memberExpression13, methodInfo23, expressionArray2 );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1 };
   //         expressionArray1[ 1 ] = Expression.Lambda<Func<Assign, string>>( methodCallExpression5, parameterExpressionArray );
   //         expressionArray[ 0 ] = Expression.Call( null, methodFromHandle23, expressionArray1 );
   //         parameterExpression1 = Expression.Parameter( typeof( string ), "a" );
   //         parameterExpression2 = Expression.Parameter( typeof( string ), "b" );
   //         BinaryExpression binaryExpression21 = Expression.Add( Expression.Add( parameterExpression1, Expression.Constant( ", ", typeof( string ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) ), parameterExpression2, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1, parameterExpression2 };
   //         expressionArray[ 1 ] = Expression.Lambda<Func<string, string, string>>( binaryExpression21, parameterExpressionArray );
   //         memberBindingArray[ 5 ] = Expression.Bind( methodFromHandle22, Expression.Call( null, methodInfo22, expressionArray ) );
   //         MemberInitExpression memberInitExpression5 = Expression.MemberInit( newExpression5, memberBindingArray );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<PayrollItemDetail> payrollItemDetails5 = payrollItemDetails4.Union<PayrollItemDetail>( groupings5.Select<IGrouping<long?, Assign>, PayrollItemDetail>( Expression.Lambda<Func<IGrouping<long?, Assign>, PayrollItemDetail>>( memberInitExpression5, parameterExpressionArray ) ) );
   //         parameterExpression = Expression.Parameter( typeof( PayrollItemDetail ), "x" );
   //         MemberExpression memberExpression14 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "get_ServiceName" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         payrollItem.WorkshopDetails = payrollItemDetails5.OrderBy<PayrollItemDetail, string>( Expression.Lambda<Func<PayrollItemDetail, string>>( memberExpression14, parameterExpressionArray ) ).ToList<PayrollItemDetail>( );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "k" );
   //         locationId = null;
   //         BinaryExpression binaryExpression22 = Expression.NotEqual( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_BirthdayId" ).MethodHandle ) ), Expression.Constant( locationId, typeof( long? ) ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<Assign> assigns2 = assignsAsInstructor.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression22, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "t" );
   //         MemberExpression memberExpression15 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_BirthdayId" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<IGrouping<long?, Assign>> groupings6 = assigns2.GroupBy<Assign, long?>( Expression.Lambda<Func<Assign, long?>>( memberExpression15, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( IGrouping<long?, Assign> ), "group" );
   //         NewExpression newExpression6 = Expression.New( ( ConstructorInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( ".ctor" ).MethodHandle ), new Expression[ 0 ] );
   //         memberBindingArray = new MemberBinding[ ] { Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Type", new Type[ ] { typeof( ServiceType ) } ).MethodHandle ), Expression.Constant( ServiceType.Birthday, typeof( ServiceType ) ) ), Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( IGrouping<long?, Assign> ).GetMethod( "get_Key" ).MethodHandle, typeof( IGrouping<long?, Assign> ).TypeHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( long? ).GetMethod( "get_Value" ).MethodHandle, typeof( long? ).TypeHandle ) ) ), null, null, null, null };
   //         MethodInfo methodFromHandle24 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceName", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo24 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "First", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 2 ] = Expression.Bind( methodFromHandle24, Expression.Property( Expression.Call( null, methodInfo24, expressionArray ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Name" ).MethodHandle ) ) );
   //         memberBindingArray[ 3 ] = Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_InstructorId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "instructor" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Instructor ).GetMethod( "get_Id" ).MethodHandle ) ));
   //         MethodInfo methodFromHandle25 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Count", new Type[ ] { typeof( long ) } ).MethodHandle );
   //         MethodInfo methodInfo25 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Count", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 4 ] = Expression.Bind( methodFromHandle25, Expression.Convert( Expression.Call( null, methodInfo25, expressionArray ), typeof( long ) ) );
   //         MethodInfo methodFromHandle26 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Dates", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo26 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Aggregate", new Type[ ] { typeof( IEnumerable<string> ), typeof( Func<string, string, string> ) } ).MethodHandle );
   //         expressionArray = new Expression[ 2 ];
   //         MethodInfo methodFromHandle27 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Select", new Type[ ] { typeof( IEnumerable<Assign> ), typeof( Func<Assign, string> ) } ).MethodHandle );
   //         expressionArray1 = new Expression[ ] { parameterExpression, null };
   //         parameterExpression1 = Expression.Parameter( typeof( Assign ), "x" );
   //         MemberExpression memberExpression16 = Expression.Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) );
   //         MethodInfo methodInfo27 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "ToString", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         expressionArray2 = new Expression[ ] { Expression.Constant( "M/dd", typeof( string ) ) };
   //         MethodCallExpression methodCallExpression6 = Expression.Call( memberExpression16, methodInfo27, expressionArray2 );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1 };
   //         expressionArray1[ 1 ] = Expression.Lambda<Func<Assign, string>>( methodCallExpression6, parameterExpressionArray );
   //         expressionArray[ 0 ] = Expression.Call( null, methodFromHandle27, expressionArray1 );
   //         parameterExpression1 = Expression.Parameter( typeof( string ), "a" );
   //         parameterExpression2 = Expression.Parameter( typeof( string ), "b" );
   //         BinaryExpression binaryExpression23 = Expression.Add( Expression.Add( parameterExpression1, Expression.Constant( ", ", typeof( string ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) ), parameterExpression2, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1, parameterExpression2 };
   //         expressionArray[ 1 ] = Expression.Lambda<Func<string, string, string>>( binaryExpression23, parameterExpressionArray );
   //         memberBindingArray[ 5 ] = Expression.Bind( methodFromHandle26, Expression.Call( null, methodInfo26, expressionArray ) );
   //         MemberInitExpression memberInitExpression6 = Expression.MemberInit( newExpression6, memberBindingArray );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<PayrollItemDetail> payrollItemDetails6 = groupings6.Select<IGrouping<long?, Assign>, PayrollItemDetail>( Expression.Lambda<Func<IGrouping<long?, Assign>, PayrollItemDetail>>( memberInitExpression6, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "k" );
   //         locationId = null;
   //         BinaryExpression binaryExpression24 = Expression.NotEqual( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_BirthdayId" ).MethodHandle ) ), Expression.Constant( locationId, typeof( long? ) ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<Assign> assigns3 = assignsAsAssitant.Where<Assign>( Expression.Lambda<Func<Assign, bool>>( binaryExpression24, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( Assign ), "t" );
   //         MemberExpression memberExpression17 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_BirthdayId" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<IGrouping<long?, Assign>> groupings7 = assigns3.GroupBy<Assign, long?>( Expression.Lambda<Func<Assign, long?>>( memberExpression17, parameterExpressionArray ) );
   //         parameterExpression = Expression.Parameter( typeof( IGrouping<long?, Assign> ), "group" );
   //         NewExpression newExpression7 = Expression.New( ( ConstructorInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( ".ctor" ).MethodHandle ), new Expression[ 0 ] );
   //         memberBindingArray = new MemberBinding[ ] { Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Type", new Type[ ] { typeof( ServiceType ) } ).MethodHandle ), Expression.Constant( ServiceType.Birthday, typeof( ServiceType ) ) ), Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( IGrouping<long?, Assign> ).GetMethod( "get_Key" ).MethodHandle, typeof( IGrouping<long?, Assign> ).TypeHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( long? ).GetMethod( "get_Value" ).MethodHandle, typeof( long? ).TypeHandle ) ) ), null, null, null, null };
   //         MethodInfo methodFromHandle28 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_ServiceName", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo28 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "First", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 2 ] = Expression.Bind( methodFromHandle28, Expression.Property( Expression.Call( null, methodInfo28, expressionArray ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Name" ).MethodHandle ) ) );
   //         memberBindingArray[ 3 ] = Expression.Bind( ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_InstructorId", new Type[ ] { typeof( long ) } ).MethodHandle ), Expression.Property( Expression.Field( Expression.Constant( variable ), FieldInfo.GetFieldFromHandle( typeof( AdminController.<> c__DisplayClass129 ).GetField( "instructor" ).FieldHandle ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Instructor ).GetMethod( "get_Id" ).MethodHandle ) ));
   //         MethodInfo methodFromHandle29 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Count", new Type[ ] { typeof( long ) } ).MethodHandle );
   //         MethodInfo methodInfo29 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Count", new Type[ ] { typeof( IEnumerable<Assign> ) } ).MethodHandle );
   //         expressionArray = new Expression[ ] { parameterExpression };
   //         memberBindingArray[ 4 ] = Expression.Bind( methodFromHandle29, Expression.Convert( Expression.Call( null, methodInfo29, expressionArray ), typeof( long ) ) );
   //         MethodInfo methodFromHandle30 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "set_Dates", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         MethodInfo methodInfo30 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Aggregate", new Type[ ] { typeof( IEnumerable<string> ), typeof( Func<string, string, string> ) } ).MethodHandle );
   //         expressionArray = new Expression[ 2 ];
   //         MethodInfo methodFromHandle31 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Enumerable ).GetMethod( "Select", new Type[ ] { typeof( IEnumerable<Assign> ), typeof( Func<Assign, string> ) } ).MethodHandle );
   //         expressionArray1 = new Expression[ ] { parameterExpression, null };
   //         parameterExpression1 = Expression.Parameter( typeof( Assign ), "x" );
   //         MemberExpression memberExpression18 = Expression.Property( parameterExpression1, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( Assign ).GetMethod( "get_Date" ).MethodHandle ) );
   //         MethodInfo methodInfo31 = ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( DateTime ).GetMethod( "ToString", new Type[ ] { typeof( string ) } ).MethodHandle );
   //         expressionArray2 = new Expression[ ] { Expression.Constant( "M/dd", typeof( string ) ) };
   //         MethodCallExpression methodCallExpression7 = Expression.Call( memberExpression18, methodInfo31, expressionArray2 );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1 };
   //         expressionArray1[ 1 ] = Expression.Lambda<Func<Assign, string>>( methodCallExpression7, parameterExpressionArray );
   //         expressionArray[ 0 ] = Expression.Call( null, methodFromHandle31, expressionArray1 );
   //         parameterExpression1 = Expression.Parameter( typeof( string ), "a" );
   //         parameterExpression2 = Expression.Parameter( typeof( string ), "b" );
   //         BinaryExpression binaryExpression25 = Expression.Add( Expression.Add( parameterExpression1, Expression.Constant( ", ", typeof( string ) ), ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) ), parameterExpression2, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( string ).GetMethod( "Concat", new Type[ ] { typeof( string ), typeof( string ) } ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression1, parameterExpression2 };
   //         expressionArray[ 1 ] = Expression.Lambda<Func<string, string, string>>( binaryExpression25, parameterExpressionArray );
   //         memberBindingArray[ 5 ] = Expression.Bind( methodFromHandle30, Expression.Call( null, methodInfo30, expressionArray ) );
   //         MemberInitExpression memberInitExpression7 = Expression.MemberInit( newExpression7, memberBindingArray );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         IQueryable<PayrollItemDetail> payrollItemDetails7 = payrollItemDetails6.Union<PayrollItemDetail>( groupings7.Select<IGrouping<long?, Assign>, PayrollItemDetail>( Expression.Lambda<Func<IGrouping<long?, Assign>, PayrollItemDetail>>( memberInitExpression7, parameterExpressionArray ) ) );
   //         parameterExpression = Expression.Parameter( typeof( PayrollItemDetail ), "x" );
   //         MemberExpression memberExpression19 = Expression.Property( parameterExpression, ( MethodInfo )MethodBase.GetMethodFromHandle( typeof( PayrollItemDetail ).GetMethod( "get_ServiceName" ).MethodHandle ) );
   //         parameterExpressionArray = new ParameterExpression[ ] { parameterExpression };
   //         payrollItem.BirthdayDetails = payrollItemDetails7.OrderBy<PayrollItemDetail, string>( Expression.Lambda<Func<PayrollItemDetail, string>>( memberExpression19, parameterExpressionArray ) ).ToList<PayrollItemDetail>( );
            return new PayrollItem();
		}

		private RosterDetailPdf NewRosterDetailPdf(Class klass, Camp camp)
		{
			RosterDetailPdf rosterDetail = new RosterDetailPdf();
			if (klass != null)
			{
				rosterDetail.Type = ServiceType.Class;
				rosterDetail.Bookings = (HashSet<Booking>)klass.Bookings;
				rosterDetail.LocationName = klass.Location.DisplayName;
				rosterDetail.Days = this.GetDaysOfService(klass);
				rosterDetail.Time = string.Format("{0} - {1}", klass.TimeStart.To12HoursString(), klass.TimeEnd.To12HoursString());
				rosterDetail.Address = StringUtil.GetFullAddress(klass.Location.Address, klass.Location.City, klass.Location.State, klass.Location.Zip);
				rosterDetail.ClassName = klass.OnlineName;
				string dateListText = ScheduleUtil.GetDateListText(klass);
				char[] chrArray = new char[] { ',' };
				rosterDetail.Dates = dateListText.Split(chrArray);
				rosterDetail.LogoUrl = string.Concat(string.Format("{0}://{1}{2}", System.Web.HttpContext.Current.Request.Url.Scheme, System.Web.HttpContext.Current.Request.Url.Authority, this.Url.Content("~")), "Content/Images/logo_small_rotate16.png");
				rosterDetail.Printed = DateTime.Today.ToString("M/dd/yyyy");
				rosterDetail.ExportFileName = klass.Name;
			}
			else if (camp != null)
			{
				rosterDetail.Type = ServiceType.Camp;
				rosterDetail.Bookings = (HashSet<Booking>)camp.Bookings;
				rosterDetail.LocationName = camp.Location.DisplayName;
				rosterDetail.Days = this.GetDaysOfService(camp);
				rosterDetail.Time = string.Format("{0} - {1}", camp.TimeStart.To12HoursString(), camp.TimeEnd.To12HoursString());
				rosterDetail.Address = StringUtil.GetFullAddress(camp.Location.Address, camp.Location.City, camp.Location.State, camp.Location.Zip);
				rosterDetail.ClassName = camp.OnlineName;
				string str = ScheduleUtil.GetDateListText(camp);
				char[] chrArray1 = new char[] { ',' };
				rosterDetail.Dates = str.Split(chrArray1);
				rosterDetail.LogoUrl = string.Concat(string.Format("{0}://{1}{2}", System.Web.HttpContext.Current.Request.Url.Scheme, System.Web.HttpContext.Current.Request.Url.Authority, this.Url.Content("~")), "Content/Images/logo_small_rotate16.png");
				rosterDetail.Printed = DateTime.Today.ToString("M/dd/yyyy");
				rosterDetail.ExportFileName = camp.Name;
			}
			return rosterDetail;
		}

		private ScheduleItem NewScheduleItem(Class klass, Camp camp, Birthday birthday, Workshop workshop)
		{
			if (klass != null)
			{
				return new ScheduleItem()
				{
					Class = klass.Name
				};
			}
			if (camp != null)
			{
				return new ScheduleItem();
			}
			if (birthday != null)
			{
				return new ScheduleItem();
			}
			if (workshop != null)
			{
				return new ScheduleItem();
			}
			return null;
		}

		private IGateway OpenGateway()
		{
			Admin admin = this.unitOfWork.AdminRepository.Get((Admin t) => t.Id == this.User.Admin.Id, null, "").SingleOrDefault<Admin>();
			AdminPaymentInfo paymentInfo = admin.AdminPaymentInfoes.FirstOrDefault<AdminPaymentInfo>();
			return new Gateway(paymentInfo.APILoginID, paymentInfo.TransactionKey, false);
		}

		[ActionName("parents-delete")]
		[HttpPost]
		public ActionResult ParentDelete(long id)
		{
			try
			{
				Parent parent = this.GetParent(id);
				if (parent != null)
				{
					foreach (Student student in parent.Students.ToList<Student>())
					{
						this.db.Students.Remove(student);
					}
					this.db.Parents.Remove(parent);
					this.db.SaveChanges();
					base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Parent" });
				}
				else
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
					return base.Content(Messages.DataNotFound);
				}
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Parent" });
			}
			return base.RedirectToRoute("Admin", new { action = "parents", research = "true" });
		}

		[ActionName("parents-detail")]
		[HttpGet]
		public ActionResult ParentDetail(long? id)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!id.HasValue)
			{
				if (base.User.IsInRole("superadmin"))
				{
					return base.View(new Parent());
				}
				return base.View("parents-detail-admin", new Parent());
			}
			Parent parent = this.GetParent(id.Value);
			if (parent == null)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DataNotFound, new object[0]);
				return base.RedirectToRoute("Admin", new { action = "parents", research = "true" });
			}
			if (base.User.IsInRole("superadmin"))
			{
				return base.View(parent);
			}
			return base.View("parents-detail-admin", parent);
		}

		[ActionName("parents-detail")]
		[HttpPost]
		public ActionResult ParentDetail(Parent model)
		{
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(model);
			}
			try
			{
				Parent dbParent = null;
				if (model.Id <= (long)0)
				{
					Parent parent = new Parent()
					{
						Password = BCrypt.Net.BCrypt.HashPassword(model.Password, 10)
					};
					dbParent = parent;
					this.db.Parents.Add(dbParent);
				}
				else
				{
					dbParent = this.GetParent(model.Id);
				}
				dbParent.LocationId = this.GetLocation(model.LocationId).Id;
				dbParent.FirstName = model.FirstName;
				dbParent.LastName = model.LastName;
				dbParent.Email = model.Email;
				dbParent.Address = model.Address;
				dbParent.City = model.City;
				dbParent.Zip = model.Zip;
				dbParent.State = model.State;
				dbParent.PhoneNumer = model.PhoneNumer;
				this.db.SaveChanges();
				base.SetViewMessage(WithAuthenController.MessageType.Info, (model.Id > (long)0 ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "Parent" });
				route = base.RedirectToRoute("Admin", new { action = "parents-detail", id = dbParent.Id });
			}
			catch (Exception exception)
			{
				this.log.Error("Add/Update parent failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = (!base.User.IsInRole("superadmin") ? base.View("parents-detail-admin", model) : base.View(model));
			}
			return route;
		}

		[ActionName("parent-detail-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult ParentDetailExcel(long? id)
		{
			Parent parent = this.GetParent(id.Value);
			((dynamic)ViewBag).FileName = string.Format("{0} {1}", parent.FirstName, parent.LastName);
			((dynamic)ViewBag).Detail = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Detail;
			string[] displayName = new string[] { parent.Location.DisplayName, parent.FirstName, parent.LastName, parent.PhoneNumer, parent.Address, parent.City, parent.State, parent.Zip, parent.Email, parent.Note };
			viewBag.Add(displayName);
			dynamic list = ViewBag;
			ICollection<Student> students = parent.Students;
			list.Children = (
				from t in students
				select new { FirstName = t.FirstName, LastName = t.LastName, Gender = t.GenderText, Grade = t.Grade.Name, BirthDate = t.BirthDate.ToString("M/dd/yyyy"), Notes = t.Notes }).ToList();
			CalendarSearchForm form = (CalendarSearchForm)base.GetSession("Calendar.Schedule.SearchForm");
			((dynamic)ViewBag).CalendarSearchForm = form;
			((dynamic)ViewBag).Calendar = this.PerformCalendarSearch(form);
			return new EmptyResult();
		}

		[HttpGet]
		public ActionResult Parents(ParentSearchForm form, int? page, bool? research)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!ModelState.IsValid)
			{
				return base.View(form);
			}
			if (!research.HasValue || !research.Value || base.GetSession("Parents.SearchForm") == null)
			{
				base.SetSession("Parents.SearchForm", form);
			}
			ParentSearchForm sessionForm = (ParentSearchForm)base.GetSession("Parents.SearchForm");
			ParentSearchForm parentSearchForm = sessionForm;
			int? nullable = page;
			SearchResult<ParentSearchForm, Parent> parents = this.PerformParentSearch(parentSearchForm, (nullable.HasValue ? nullable.GetValueOrDefault() : 1), new int?(2147483647));
			if (parents.TotalItems != 0)
			{
				((dynamic)ViewBag).Parents = parents;
			}
			else
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.NoRecordFound, new object[] { "parent" });
			}
			return base.View(sessionForm);
		}

		[ActionName("parents-all")]
		[HttpGet]
		public ActionResult ParentsAll()
		{
			return base.RedirectToAction("parents", new { page = 1, research = false, itemsPerPage = 2147483647 });
		}

		[ActionName("parents-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult ParentsExcel()
		{
			ParentSearchForm sessionForm = (ParentSearchForm)base.GetSession("Parents.SearchForm") ?? new ParentSearchForm();
			((dynamic)ViewBag).FileName = "Parents.xlsx";
			((dynamic)ViewBag).SheetName = "Parents";
			((dynamic)ViewBag).Headers = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Headers;
			string[] strArrays = new string[] { "Name", "Email", "Phone", "Location", "Last Login Date" };
			viewBag.Add(strArrays);
			SearchResult<ParentSearchForm, Parent> parents = this.PerformParentSearch(sessionForm, 1, new int?(2147483647));
			dynamic list = ViewBag;
			List<Parent> pageItems = parents.PageItems;
			list.ExportData = (
				from t in pageItems
				select new { Name = string.Concat(t.FirstName, " ", t.LastName), Email = t.Email, Phone = t.PhoneNumer, Location = t.Location.Name, LastLoginDate = (t.LastLoginDateTime.HasValue ? t.LastLoginDateTime.Value.ToString("M/dd/yyyy") : string.Empty) }).ToList();
			return new EmptyResult();
		}

		[Authorize(Roles="admin")]
		[HttpGet]
		public ActionResult Payment()
		{
			AdminPaymentInfo paymentInfo = this.db.AdminPaymentInfoes.FirstOrDefault<AdminPaymentInfo>((AdminPaymentInfo t) => t.AdminId == this.User.Admin.Id);
			if (paymentInfo != null)
			{
				return base.View(paymentInfo);
			}
			AdminPaymentInfo adminPaymentInfo = new AdminPaymentInfo()
			{
				AdminId = base.User.Admin.Id
			};
			return base.View(adminPaymentInfo);
		}

		[Authorize(Roles="admin")]
		[HttpPost]
		public ActionResult Payment(AdminPaymentInfo model)
		{
			ActionResult route;
			if (!ModelState.IsValid)
			{
				return base.View(model);
			}
			try
			{
				if (model.Id <= (long)0)
				{
					DbSet<AdminPaymentInfo> adminPaymentInfoes = this.db.AdminPaymentInfoes;
					AdminPaymentInfo adminPaymentInfo = new AdminPaymentInfo()
					{
						AdminId = model.AdminId,
						APILoginID = model.APILoginID,
						TransactionKey = model.TransactionKey,
						MD5HashPhrase = (!string.IsNullOrWhiteSpace(model.MD5HashPhrase) ? model.MD5HashPhrase : string.Empty),
						LastUpdateDate = new DateTime?(DateTime.Now)
					};
					adminPaymentInfoes.Add(adminPaymentInfo);
				}
				else
				{
					AdminPaymentInfo paymentInfo = this.db.AdminPaymentInfoes.FirstOrDefault<AdminPaymentInfo>((AdminPaymentInfo t) => t.AdminId == this.User.Admin.Id);
					paymentInfo.APILoginID = model.APILoginID;
					paymentInfo.TransactionKey = model.TransactionKey;
					paymentInfo.MD5HashPhrase = (!string.IsNullOrWhiteSpace(model.MD5HashPhrase) ? model.MD5HashPhrase : string.Empty);
					paymentInfo.LastUpdateDate = new DateTime?(DateTime.Now);
				}
				this.db.SaveChanges();
				base.SetViewMessage(WithAuthenController.MessageType.Info, (model.Id > (long)0 ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "Payment info" });
				route = base.RedirectToRoute("Admin", new { action = "payment" });
			}
			catch (Exception exception)
			{
				this.log.Error("Update payment failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.RedirectToRoute("Admin", new { action = "payment" });
			}
			return route;
		}

		[HttpGet]
		public ActionResult Payroll(PayrollSearchForm form, int? page)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!ModelState.IsValid)
			{
				return base.View();
			}
			base.SetSession("Payroll.SearchForm", form);
			dynamic viewBag = ViewBag;
			PayrollSearchForm payrollSearchForm = form;
			int? nullable = page;
			viewBag.Payrolls = this.PerformPayrollSearch(payrollSearchForm, (nullable.HasValue ? nullable.GetValueOrDefault() : 1), new int?(2147483647));
			return base.View(form);
		}

		[ActionName("payroll-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult PayrollExcel()
		{
			PayrollSearchForm sessionForm = (PayrollSearchForm)base.GetSession("Payroll.SearchForm") ?? new PayrollSearchForm();
			((dynamic)ViewBag).FileName = string.Format("Payroll.xlsx", new object[0]);
			((dynamic)ViewBag).SheetName = "Payroll";
			((dynamic)ViewBag).Headers = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Headers;
			string[] strArrays = new string[] { "Instructor", "Teach", "Assist", "Birthday", "Workshop" };
			viewBag.Add(strArrays);
			SearchResult<PayrollSearchForm, PayrollItem> payrollList = this.PerformPayrollSearch(sessionForm, 1, new int?(2147483647));
			dynamic list = ViewBag;
			List<PayrollItem> pageItems = payrollList.PageItems;
			list.ExportData = (
				from t in pageItems
				select new { Instructor = t.InstructorName, Address = t.TeachCount, Email = t.AssistCount, Phone = t.BirthdayCount, Contact = t.WorkshopCount }).ToList();
			return new EmptyResult();
		}

		[ActionName("payroll-init")]
		[HttpGet]
		public ActionResult PayrollInit()
		{
			if (base.User.Admin.AreaId.HasValue)
			{
				return base.View("payroll", new PayrollSearchForm());
			}
			base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
			return base.View("AdminNotAssign");
		}

		private Calendar PerformCalendarSearch(CalendarSearchForm form)
		{
			DateTime? date = form.Date;
			Calendar calendar = new Calendar((date.HasValue ? date.GetValueOrDefault() : DateTime.Today), form.ViewBy, form.ServiceTypes);
			IQueryable<Assign> query = this.db.Assigns.AsQueryable<Assign>();
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from a in query
					where a.Class != null && (long?)a.Class.Location.AreaId == this.User.Admin.AreaId || a.Camp != null && (long?)a.Camp.Location.AreaId == this.User.Admin.AreaId || a.Workshop != null && (long?)a.Workshop.Location.AreaId == this.User.Admin.AreaId || a.Birthday != null && (long?)a.Birthday.AreaId == this.User.Admin.AreaId
					select a;
			}
			if (form.LocationId.HasValue)
			{
				query = 
					from a in query
					where a.Class != null && (long?)a.Class.LocationId == form.LocationId || a.Camp != null && (long?)a.Camp.LocationId == form.LocationId || a.Workshop != null && (long?)a.Workshop.LocationId == form.LocationId
					select a;
			}
			if (form.InstructorId.HasValue && form.AssistantId.HasValue)
			{
				query = 
					from a in query
					where a.InstructorId == form.InstructorId.Value || a.AssistantId == (long?)form.AssistantId.Value
					select a;
			}
			else if (form.InstructorId.HasValue)
			{
				query = 
					from a in query
					where a.InstructorId == form.InstructorId.Value
					select a;
			}
			else if (form.AssistantId.HasValue)
			{
				query = 
					from a in query
					where a.AssistantId == (long?)form.AssistantId.Value
					select a;
			}
			if (form.ClassId.HasValue)
			{
				query = 
					from a in query
					where a.ClassId.HasValue && a.ClassId.Value == form.ClassId.Value
					select a;
			}
			if (form.CampId.HasValue)
			{
				query = 
					from a in query
					where a.CampId.HasValue && a.CampId.Value == form.CampId.Value
					select a;
			}
			if (form.BirthdayId.HasValue)
			{
				query = 
					from a in query
					where a.BirthdayId.HasValue && a.BirthdayId.Value == form.BirthdayId.Value
					select a;
			}
			if (form.WorkshopId.HasValue)
			{
				query = 
					from a in query
					where a.WorkshopId.HasValue && a.WorkshopId.Value == form.WorkshopId.Value
					select a;
			}
			if (form.ParentId.HasValue)
			{
				query = 
					from a in query
					where a.Class != null && a.Class.Bookings.Any<Booking>((Booking b) => b.Student.ParentId == form.ParentId.Value) || a.Camp != null && a.Camp.Bookings.Any<Booking>((Booking b) => b.Student.ParentId == form.ParentId.Value) || a.Birthday != null && a.Birthday.Bookings.Any<Booking>((Booking b) => b.Student.ParentId == form.ParentId.Value) || a.Workshop != null && a.Workshop.Bookings.Any<Booking>((Booking b) => b.Student.ParentId == form.ParentId.Value)
					select a;
			}
			if (form.StudentId.HasValue)
			{
				query = 
					from a in query
					where a.Class != null && a.Class.Bookings.Any<Booking>((Booking b) => b.StudentId == form.StudentId.Value) || a.Camp != null && a.Camp.Bookings.Any<Booking>((Booking b) => b.StudentId == form.StudentId.Value) || a.Birthday != null && a.Birthday.Bookings.Any<Booking>((Booking b) => b.StudentId == form.StudentId.Value) || a.Workshop != null && a.Workshop.Bookings.Any<Booking>((Booking b) => b.StudentId == form.StudentId.Value)
					select a;
			}
			query = 
				from a in query
				where (a.Date >= calendar.FromDate) && (a.Date <= calendar.ToDate)
				select a;
			calendar.Events = (
				from a in query.ToList<Assign>()
				orderby a.Date, a.TimeStart, a.TimeEnd
				select a).ToList<Assign>();
			if (form.ParentId.HasValue)
			{
				List<Assign> fakeAssign = new List<Assign>();
				foreach (Student student in this.db.Parents.FirstOrDefault<Parent>((Parent t) => (long?)t.Id == form.ParentId).Students.ToList<Student>())
				{
					Assign assign = new Assign();
					Birthday birthday = new Birthday()
					{
						ChildName = StringUtil.GetFullName(student.FirstName, student.LastName),
						ParentName = StringUtil.GetFullName(student.Parent.FirstName, student.Parent.LastName),
						PartyTime = new TimeSpan(12, 0, 0)
					};
					assign.Birthday = birthday;
					assign.BirthdayId = new long?((long)0);
					int year = calendar.FromDate.Year;
					int month = student.BirthDate.Month;
					DateTime birthDate = student.BirthDate;
					assign.Date = new DateTime(year, month, birthDate.Day);
					fakeAssign.Add(assign);
					if (calendar.FromDate.Year == calendar.ToDate.Year)
					{
						continue;
					}
					Assign nullable = new Assign();
					Birthday birthday1 = new Birthday()
					{
						ChildName = StringUtil.GetFullName(student.FirstName, student.LastName),
						ParentName = StringUtil.GetFullName(student.Parent.FirstName, student.Parent.LastName),
						PartyTime = new TimeSpan(12, 0, 0)
					};
					nullable.Birthday = birthday1;
					nullable.BirthdayId = new long?((long)0);
					int num = calendar.ToDate.Year;
					int month1 = student.BirthDate.Month;
					birthDate = student.BirthDate;
					nullable.Date = new DateTime(num, month1, birthDate.Day);
					fakeAssign.Add(nullable);
				}
				calendar.Events.AddRange(fakeAssign.Where(a => {
					if (a.Date < calendar.FromDate)
					{
						return false;
					}
					return a.Date <= calendar.ToDate;
				}).ToList());
			}
			return calendar;
		}

		private SearchResult<CouponSearchForm, Coupon> PerformCouponSearch(CouponSearchForm form, int page, int? itemsPerPage = null)
		{
			IQueryable<Coupon> query = this.db.Coupons.AsQueryable<Coupon>();
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from t in query
					where t.AdminId == (long?)this.User.Admin.Id
					select t;
			}
			if (!string.IsNullOrWhiteSpace(form.QuickSearch))
			{
				query = 
					from a in query
					where a.Code.ToLower().Contains(form.QuickSearch.Trim().ToLower())
					select a;
			}
			query = 
				from a in query
				orderby a.Id descending
				select a;
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<CouponSearchForm, Coupon>(form, query, page);
			}
			return SearchResult.New<CouponSearchForm, Coupon>(form, query, page, itemsPerPage.Value);
		}

		private SearchResult<InstructorSearchForm, Instructor> PerformInstructorSearch(InstructorSearchForm form, int page, int? itemsPerPage = null)
		{
			IQueryable<Instructor> query = this.db.Instructors.AsQueryable<Instructor>();
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from i in query
					where i.AreaId == this.User.Admin.AreaId.Value
					select i;
			}
			if (form != null)
			{
				if (!string.IsNullOrWhiteSpace(form.QuickSearch))
				{
					query = 
						from i in query
						where ((i.FirstName + " ") + i.LastName).ToLower().Contains(form.QuickSearch.Trim().ToLower())
						select i;
				}
				if (form.IsActive.HasValue)
				{
					query = 
						from i in query
						where i.IsActive == form.IsActive.Value
						select i;
				}
			}
			query = 
				from i in query
				orderby (i.FirstName + " ") + i.LastName
				select i;
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<InstructorSearchForm, Instructor>(form, query, page);
			}
			return SearchResult.New<InstructorSearchForm, Instructor>(form, query, page, itemsPerPage.Value);
		}

		private SearchResult<LocationSearchForm, Location> PerformLocationSearch(LocationSearchForm form, int page, int? itemsPerPage = null)
		{
			IQueryable<Location> query = this.db.Locations.AsQueryable<Location>();
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from l in query
					where (long?)l.AreaId == this.User.Admin.AreaId
					select l;
			}
			if (form != null)
			{
				if (!string.IsNullOrWhiteSpace(form.QuickSearch))
				{
					query = 
						from l in query
						where l.Name.ToLower().Contains(form.QuickSearch.Trim().ToLower()) || l.DisplayName.ToLower().Contains(form.QuickSearch.Trim().ToLower())
						select l;
				}
				if (form.IsActive.HasValue)
				{
					query = 
						from l in query
						where l.IsActive == form.IsActive.Value
						select l;
				}
				if (form.CanEnrollOnline.HasValue)
				{
					query = 
						from l in query
						where l.CanRegistOnline == form.CanEnrollOnline.Value
						select l;
				}
			}
			query = 
				from l in query
				orderby l.Name
				select l;
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<LocationSearchForm, Location>(form, query, page);
			}
			return SearchResult.New<LocationSearchForm, Location>(form, query, page, itemsPerPage.Value);
		}

		private SearchResult<ParentSearchForm, Parent> PerformParentSearch(ParentSearchForm form, int page, int? itemsPerPage = null)
		{
			IQueryable<Parent> query = this.db.Parents.AsQueryable<Parent>();
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from p in query
					where (long?)p.Location.AreaId == this.User.Admin.AreaId
					select p;
			}
			if (!string.IsNullOrWhiteSpace(form.QuickSearch))
			{
				query = 
					from p in query
					where ((p.FirstName + " ") + p.LastName).ToLower().Contains(form.QuickSearch.Trim().ToLower())
					select p;
			}
			if (form.LocationId.HasValue)
			{
				query = 
					from p in query
					where p.LocationId == form.LocationId.Value
					select p;
			}
			query = 
				from p in query
				orderby (p.FirstName + " ") + p.LastName
				select p;
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<ParentSearchForm, Parent>(form, query, page);
			}
			return SearchResult.New<ParentSearchForm, Parent>(form, query, page, itemsPerPage.Value);
		}

		private SearchResult<PayrollSearchForm, PayrollItem> PerformPayrollSearch(PayrollSearchForm form, int page, int? itemsPerPage = null)
		{
			IQueryable<Instructor> query = (
				from t in this.db.Instructors
				where (long?)t.AreaId == this.User.Admin.AreaId && t.IsActive
				select t).AsQueryable<Instructor>();
			if (form.InstructorId.HasValue)
			{
				query = 
					from i in query
					where i.Id == form.InstructorId.Value
					select i;
			}
			query = 
				from i in query
				orderby (i.FirstName + " ") + i.LastName
				select i;
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<PayrollSearchForm, Instructor, PayrollItem>(form, query, page, (Instructor item) => this.NewPayrollItem(item, form));
			}
			return SearchResult.New<PayrollSearchForm, Instructor, PayrollItem>(form, query, page, itemsPerPage.Value, (Instructor item) => this.NewPayrollItem(item, form));
		}

		private SearchResult<RosterSearchForm, Camp> PerformRosterCampSearch(RosterSearchForm form, int page, int? itemsPerPage = null)
		{
			IQueryable<Camp> query = this.CreateRosterCampsQueryResult(form);
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<RosterSearchForm, Camp>(form, query, page);
			}
			return SearchResult.New<RosterSearchForm, Camp>(form, query, page, itemsPerPage.Value);
		}

		private SearchResult<RosterSearchForm, RosterDetailPdf> PerformRosterCampSearchForExport(RosterSearchForm form, string campIds, int page, int? itemsPerPage = null)
		{
			List<long> nums = new List<long>();
			if (!string.IsNullOrEmpty(campIds))
			{
				char[] chrArray = new char[] { ';' };
				campIds.Split(chrArray).ToList<string>().ForEach((string id) => nums.Add(Convert.ToInt64(id)));
			}
			IQueryable<Camp> query = 
				from t in this.db.Camps
				where nums.Contains(t.Id)
				select t;
			query = 
				from c in query
				orderby c.Assigns.Min<Assign, DateTime>((Assign t) => t.Date)
				select c;
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<RosterSearchForm, Camp, RosterDetailPdf>(form, query, page, (Camp t) => this.NewRosterDetailPdf(null, t));
			}
			return SearchResult.New<RosterSearchForm, Camp, RosterDetailPdf>(form, query, page, itemsPerPage.Value, (Camp t) => this.NewRosterDetailPdf(null, t));
		}

		private SearchResult<RosterSearchForm, Class> PerformRosterClassSearch(RosterSearchForm form, int page, int? itemsPerPage = null)
		{
			IQueryable<Class> query = this.CreateRosterClassesQuery(form);
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<RosterSearchForm, Class>(form, query, page);
			}
			return SearchResult.New<RosterSearchForm, Class>(form, query, page, itemsPerPage.Value);
		}

		private SearchResult<RosterSearchForm, RosterDetailPdf> PerformRosterClassSearchForExport(RosterSearchForm form, string classIds, int page, int? itemsPerPage = null)
		{
			List<long> nums = new List<long>();
			if (!string.IsNullOrEmpty(classIds))
			{
				char[] chrArray = new char[] { ';' };
				classIds.Split(chrArray).ToList<string>().ForEach((string id) => nums.Add(Convert.ToInt64(id)));
			}
			IQueryable<Class> query = 
				from t in this.db.Classes
				where nums.Contains(t.Id)
				select t;
			query = 
				from c in query
				orderby c.Assigns.Min<Assign, DateTime>((Assign t) => t.Date)
				select c;
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<RosterSearchForm, Class, RosterDetailPdf>(form, query, page, (Class t) => this.NewRosterDetailPdf(t, null));
			}
			return SearchResult.New<RosterSearchForm, Class, RosterDetailPdf>(form, query, page, itemsPerPage.Value, (Class t) => this.NewRosterDetailPdf(t, null));
		}

		private SearchResult<ScheduleSearchForm, Birthday> PerformScheduleBirthdaySearch(ScheduleSearchForm form, int page, int? itemsPerPage = null)
		{
			//if (form.Type.HasValue && form.Type.Value != ServiceType.Birthday)
			//{
			//	return null;
			//}
			//IQueryable<Birthday> query = this.db.Birthdays.AsQueryable<Birthday>();
			//if (!base.User.Admin.IsSuperAdmin)
			//{
			//	query = 
			//		from b in query
			//		where (long?)b.AreaId == this.User.Admin.AreaId
			//		select b;
			//}
			//query = (!form.DateFrom.HasValue ? 
			//	from b in query
			//	where b.Assigns.Max<Assign, DateTime>((Assign a) => a.Date) >= DateTime.Today
			//	select b : 
			//	from b in query
			//	where b.Assigns.Max<Assign, DateTime>((Assign a) => a.Date) >= form.DateFrom.Value
			//	select b);
			//if (form.DateTo.HasValue)
			//{
			//	query = 
			//		from b in query
			//		where b.Assigns.Min<Assign, DateTime>((Assign a) => a.Date) <= form.DateTo.Value
			//		select b;
			//}
			//if (!form.AllDays)
			//{
			//	query = 
			//		from b in query
			//		where b.Assigns.Any<Assign>((Assign a) => form.SelectedDays.Contains(EntityFunctions.DiffDays((DateTime?)this.firstSunday, (DateTime?)a.Date).Value % 7))
			//		select b;
			//}
			//if (form.InstructorId.HasValue)
			//{
			//	query = 
			//		from c in query
			//		where c.Assigns.Any<Assign>((Assign a) => a.InstructorId == form.InstructorId.Value)
			//		select c;
			//}
			//query = 
			//	from b in query
			//	orderby b.Assigns.Min<Assign, DateTime>((Assign a) => a.Date)
			//	select b;
			//if (!itemsPerPage.HasValue)
			//{
			//	return SearchResult.New<ScheduleSearchForm, Birthday>(form, query, page);
			//}
		    return null;
			//return SearchResult.New<ScheduleSearchForm, Birthday>(form, query, page, itemsPerPage.Value);
		}

		private SearchResult<ScheduleSearchForm, Camp> PerformScheduleCampSearch(ScheduleSearchForm form, int page, int? itemsPerPage = null)
		{
			//if (form.Type.HasValue && form.Type.Value != ServiceType.Camp)
			//{
			//	return null;
			//}
			//IQueryable<Camp> query = this.db.Camps.AsQueryable<Camp>();
			//if (!base.User.Admin.IsSuperAdmin)
			//{
			//	query = 
			//		from c in query
			//		where (long?)c.Location.AreaId == this.User.Admin.AreaId
			//		select c;
			//}
			//query = (!form.DateFrom.HasValue ? 
			//	from c in query
			//	where c.Assigns.Max<Assign, DateTime>((Assign a) => a.Date) >= DateTime.Today
			//	select c : 
			//	from c in query
			//	where c.Assigns.Max<Assign, DateTime>((Assign a) => a.Date) >= form.DateFrom.Value
			//	select c);
			//if (form.DateTo.HasValue)
			//{
			//	query = 
			//		from c in query
			//		where c.Assigns.Min<Assign, DateTime>((Assign a) => a.Date) <= form.DateTo.Value
			//		select c;
			//}
			//if (!form.AllDays)
			//{
			//	query = 
			//		from c in query
			//		where c.Assigns.Any<Assign>((Assign a) => form.SelectedDays.Contains(EntityFunctions.DiffDays((DateTime?)this.firstSunday, (DateTime?)a.Date).Value % 7))
			//		select c;
			//}
			//if (form.LocationId.HasValue)
			//{
			//	query = 
			//		from c in query
			//		where (long?)c.LocationId == form.LocationId
			//		select c;
			//}
			//if (form.InstructorId.HasValue)
			//{
			//	query = 
			//		from c in query
			//		where c.InstructorId == form.InstructorId.Value
			//		select c;
			//}
			//query = 
			//	from c in query
			//	orderby c.Assigns.Min<Assign, DateTime>((Assign a) => a.Date)
			//	select c;
			//if (!itemsPerPage.HasValue)
			//{
			//	return SearchResult.New<ScheduleSearchForm, Camp>(form, query, page);
			//}
		    return null;
			//return SearchResult.New<ScheduleSearchForm, Camp>(form, query, page, itemsPerPage.Value);
		}

		private SearchResult<ScheduleSearchForm, Class> PerformScheduleClassSearch(ScheduleSearchForm form, int page, int? itemsPerPage = null)
		{
			if (form.Type.HasValue && form.Type.Value != ServiceType.Class)
			{
				return null;
			}
			IQueryable<Class> query = this.db.Classes.AsQueryable<Class>();
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from c in query
					where (long?)c.Location.AreaId == this.User.Admin.AreaId
					select c;
			}
			query = (!form.DateFrom.HasValue ? 
				from c in query
				where c.Assigns.Max<Assign, DateTime>((Assign a) => a.Date) >= DateTime.Today
				select c : 
				from c in query
				where c.Assigns.Max<Assign, DateTime>((Assign a) => a.Date) >= form.DateFrom.Value
				select c);
			if (form.DateTo.HasValue)
			{
				query = 
					from c in query
					where c.Assigns.Min<Assign, DateTime>((Assign a) => a.Date) <= form.DateTo.Value
					select c;
			}
			//if (!form.AllDays)
			//{
			//	query = 
			//		from c in query
			//		where c.Assigns.Any<Assign>((Assign a) => form.SelectedDays.Contains(EntityFunctions.DiffDays((DateTime?)this.firstSunday, (DateTime?)a.Date).Value % 7))
			//		select c;
			//}
			if (form.LocationId.HasValue)
			{
				query = 
					from c in query
					where (long?)c.LocationId == form.LocationId
					select c;
			}
			if (form.InstructorId.HasValue)
			{
				query = 
					from c in query
					where c.InstructorId == form.InstructorId.Value
					select c;
			}
			query = 
				from c in query
				orderby c.Assigns.Min<Assign, DateTime>((Assign a) => a.Date)
				select c;
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<ScheduleSearchForm, Class>(form, query, page);
			}
			return SearchResult.New<ScheduleSearchForm, Class>(form, query, page, itemsPerPage.Value);
		}

		private SearchResult<ScheduleSearchForm, ScheduleItem> PerformScheduleSearch(ScheduleSearchForm form, ServiceType? type, int page)
		{
			IQueryable<Assign> query = this.db.Assigns.AsQueryable();
			if (type.HasValue)
			{
				switch (type.Value)
				{
					case ServiceType.Class:
					{
						query = 
							from a in query
							where a.Class != null
							select a;
						break;
					}
					case ServiceType.Camp:
					{
						query = 
							from a in query
							where a.Camp != null
							select a;
						break;
					}
					case ServiceType.Workshop:
					{
						query = 
							from a in query
							where a.Workshop != null
							select a;
						break;
					}
					case ServiceType.Birthday:
					{
						query = 
							from a in query
							where a.Birthday != null
							select a;
						break;
					}
				}
			}
			var group = 
				(from a in query
				group a by new { Class = a.Class, Camp = a.Camp, Birthday = a.Birthday, Workshop = a.Workshop }).ToList();
		 //   if (!form.DateFrom.HasValue)
		 //   {
		 //       grouping = from g in grouping where g.Min<Assign, DateTime>( ( Assign a ) => a.Date ) <= form.DateTo.Value

   //         }
				
			//if (form.DateTo.HasValue)
			//{
			//	group = 
			//		from g in group
			//		where g.Min<Assign, DateTime>((Assign a) => a.Date) <= form.DateTo.Value
			//		select g;
			//}
			//group = 
			//	from g in group
			//	orderby g.Min<Assign, DateTime>((Assign a) => a.Date), g.Max<Assign, DateTime>((Assign a) => a.Date)
			//	select g;
		    return null;//.New(form, group, page, (g) => this.NewScheduleItem(g.Key.Class, g.Key.Camp, g.Key.Birthday, g.Key.Workshop));
		}

		private SearchResult<ScheduleSearchForm, Workshop> PerformScheduleWorkshopSearch(ScheduleSearchForm form, int page, int? itemsPerPage = null)
		{
			if (form.Type.HasValue && form.Type.Value != ServiceType.Workshop)
			{
				return null;
			}
			IQueryable<Workshop> query = this.db.Workshops.AsQueryable<Workshop>();
			if (!base.User.Admin.IsSuperAdmin)
			{
				query = 
					from w in query
					where (long?)w.Location.AreaId == this.User.Admin.AreaId
					select w;
			}
			query = (!form.DateFrom.HasValue ? 
				from w in query
				where w.Assigns.Max<Assign, DateTime>((Assign a) => a.Date) >= DateTime.Today
				select w : 
				from w in query
				where w.Assigns.Max<Assign, DateTime>((Assign a) => a.Date) >= form.DateFrom.Value
				select w);
			if (form.DateTo.HasValue)
			{
				query = 
					from w in query
					where w.Assigns.Min<Assign, DateTime>((Assign a) => a.Date) <= form.DateTo.Value
					select w;
			}
			//if (!form.AllDays)
			//{
			//	query = 
			//		from w in query
			//		where w.Assigns.Any<Assign>((Assign a) => form.SelectedDays.Contains(EntityFunctions.DiffDays((DateTime?)this.firstSunday, (DateTime?)a.Date).Value % 7))
			//		select w;
			//}
			if (form.LocationId.HasValue)
			{
				query = 
					from c in query
					where (long?)c.LocationId == form.LocationId
					select c;
			}
			if (form.InstructorId.HasValue)
			{
				query = 
					from c in query
					where c.InstructorId == form.InstructorId.Value
					select c;
			}
			query = 
				from w in query
				orderby w.Assigns.Min<Assign, DateTime>((Assign a) => a.Date)
				select w;
			if (!itemsPerPage.HasValue)
			{
				return SearchResult.New<ScheduleSearchForm, Workshop>(form, query, page);
			}
			return SearchResult.New<ScheduleSearchForm, Workshop>(form, query, page, itemsPerPage.Value);
		}

		[ActionName("roster-delete")]
		[HttpPost]
		public ActionResult RosterDelete(long id)
		{
			int? nullable;
			int? nullable1;
			try
			{
				Booking booking = this.db.Bookings.FirstOrDefault<Booking>((Booking t) => t.Id == id);
				if (booking.ClassId.HasValue)
				{
					Class klass = this.db.Classes.FirstOrDefault<Class>((Class t) => (long?)t.Id == booking.ClassId);
					Class @class = klass;
					int? enrolled = @class.Enrolled;
					if (enrolled.HasValue)
					{
						nullable1 = new int?(enrolled.GetValueOrDefault() - 1);
					}
					else
					{
						nullable1 = null;
					}
					@class.Enrolled = nullable1;
				}
				else if (booking.CampId.HasValue)
				{
					Camp camp = this.db.Camps.FirstOrDefault<Camp>((Camp t) => (long?)t.Id == booking.CampId);
					Camp camp1 = camp;
					int? enrolled1 = camp1.Enrolled;
					if (enrolled1.HasValue)
					{
						nullable = new int?(enrolled1.GetValueOrDefault() - 1);
					}
					else
					{
						nullable = null;
					}
					camp1.Enrolled = nullable;
				}
				this.db.Bookings.Remove(booking);
				this.db.SaveChanges();
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Booking" });
			}
			catch (Exception exception)
			{
				this.log.Error("Delete student from roster failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
			}
			return base.RedirectToAction("roster-detail", new { type = base.GetSession("Roster_Detail.Type"), id = base.GetSession("Roster_Detail.Id") });
		}

		[ActionName("roster-detail")]
		[HttpGet]
		public ActionResult RosterDetail(long id, ServiceType type)
		{
			base.SetSession("Roster_Detail.Id", id);
			base.SetSession("Roster_Detail.Type", type);
			if (type == ServiceType.Class)
			{
				Class @class = this.GetClass(id);
				((dynamic)ViewBag).Service = @class;
				IQueryable<Student> query = 
					from t in this.db.Students
					where t.Parent.Location.AreaId == this.AdminAreaId
					select t;
				query = 
					from t in query
					where !t.Bookings.Any<Booking>((Booking x) => x.ClassId == (long?)@class.Id)
					select t;
				dynamic viewBag = ViewBag;
				List<Student> list = (
					from t in query
					orderby (t.FirstName + " ") + t.LastName
					select t).ToList<Student>();
				viewBag.UnregistStudents = new SelectList(
					from i in list
					select new { Value = i.Id, Text = StringUtil.GetFullName(i.FirstName, i.LastName) }, "Value", "Text");
				Booking booking = new Booking()
				{
					ClassId = new long?(@class.Id),
					BookDate = DateTime.Today
				};
				return base.View(booking);
			}
			if (type != ServiceType.Camp)
			{
				return base.View(new Booking());
			}
			Camp camp = this.GetCamp(id);
			((dynamic)ViewBag).Service = camp;
			IQueryable<Student> squery = 
				from t in this.db.Students
				where t.Parent.Location.AreaId == this.AdminAreaId
				select t;
			squery = 
				from t in squery
				where !t.Bookings.Any<Booking>((Booking x) => x.CampId == (long?)camp.Id)
				select t;
			dynamic selectList = ViewBag;
			List<Student> students = (
				from t in squery
				orderby (t.FirstName + " ") + t.LastName
				select t).ToList<Student>();
			selectList.UnregistStudents = new SelectList(
				from i in students
				select new { Value = i.Id, Text = StringUtil.GetFullName(i.FirstName, i.LastName) }, "Value", "Text");
			Booking booking1 = new Booking()
			{
				CampId = new long?(camp.Id),
				BookDate = DateTime.Today
			};
			return base.View(booking1);
		}

		[ActionName("roster-detail")]
		[HttpPost]
		public ActionResult RosterDetail(Booking booking)
		{
			if (!ModelState.IsValid)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, "Please select student", new object[0]);
				return base.RedirectToAction("roster-detail", new { type = base.GetSession("Roster_Detail.Type"), id = base.GetSession("Roster_Detail.Id") });
			}
			try
			{
				this.db.Bookings.Add(booking);
				this.db.SaveChanges();
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.AddSuccess, new object[] { "Student" });
			}
			catch (Exception exception)
			{
				this.log.Error("Add student to roster failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
			}
			return base.RedirectToAction("roster-detail", new { type = base.GetSession("Roster_Detail.Type"), id = base.GetSession("Roster_Detail.Id") });
		}

		[ActionName("roster-detail-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult RosterDetailExcel(long id, ServiceType type)
		{
			RosterDetailPdf model = this.CreateRosterDetailExportData(id, type);
			((dynamic)ViewBag).FileName = string.Format("Roster_{0}.xlsx", model.ExportFileName);
			((dynamic)ViewBag).RosterDetail = model;
			return new EmptyResult();
		}

		[ActionName("roster-detail-pdf")]
		[HttpGet]
		[PDFDownload]
		public ActionResult RosterDetailPdf(long id, ServiceType type)
		{
			dynamic model = ((dynamic)ViewBag).RosterDetail = this.CreateRosterDetailExportData(id, type);
			((dynamic)ViewBag).RosterDetailPdf = model;
			dynamic viewBag = ViewBag;
			viewBag.FileName = string.Format("Roster_{0}", model.ExportFileName);
			return new EmptyResult();
		}

		[ActionName("roster-detail-print")]
		[HttpGet]
		public ActionResult RosterDetailPrint(long id, ServiceType type)
		{
			return this.PartialView("roster-detail-print", this.CreateRosterDetailExportData(id, type));
		}

		[HttpGet]
		public ActionResult Rosters(RosterSearchForm form, bool? research)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!research.HasValue || !research.Value || base.GetSession("Roster.SearchForm") == null)
			{
				base.SetSession("Roster.SearchForm", form);
			}
			RosterSearchForm sessionForm = (RosterSearchForm)base.GetSession("Roster.SearchForm");
			dynamic viewBag = ViewBag;
			RosterSearchForm rosterSearchForm = form;
			int? classPage = sessionForm.ClassPage;
			viewBag.Classes = this.PerformRosterClassSearch(rosterSearchForm, (classPage.HasValue ? classPage.GetValueOrDefault() : 1), new int?(2147483647));
			dynamic obj = ViewBag;
			RosterSearchForm rosterSearchForm1 = form;
			int? campPage = sessionForm.CampPage;
			obj.Camps = this.PerformRosterCampSearch(rosterSearchForm1, (campPage.HasValue ? campPage.GetValueOrDefault() : 1), new int?(2147483647));
			return base.View(form);
		}

		[ActionName("rosters-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult RostersExcel(string classIds, string campIds)
		{
			int num;
			int num1;
			((dynamic)ViewBag).FileName = "Rosters";
			RosterSearchForm form = (RosterSearchForm)base.GetSession("Roster.SearchForm");
			dynamic viewBag = ViewBag;
			RosterSearchForm rosterSearchForm = form;
			string str = classIds;
			int? classPage = form.ClassPage;
			num = (classPage.HasValue ? classPage.GetValueOrDefault() : 1);
			int? nullable = null;
			viewBag.ClassRosters = this.PerformRosterClassSearchForExport(rosterSearchForm, str, num, nullable).PageItems;
			dynamic pageItems = ViewBag;
			RosterSearchForm rosterSearchForm1 = form;
			string str1 = campIds;
			int? campPage = form.CampPage;
			num1 = (campPage.HasValue ? campPage.GetValueOrDefault() : 1);
			int? nullable1 = null;
			pageItems.CampRosters = this.PerformRosterCampSearchForExport(rosterSearchForm1, str1, num1, nullable1).PageItems;
			return new EmptyResult();
		}

		[ActionName("rosters-paging")]
		[HttpGet]
		public ActionResult RostersPaging(RosterSearchForm form, int page)
		{
			switch (form.Type.Value)
			{
				case ServiceType.Class:
				{
					return this.PartialView("rosters-classes", this.PerformRosterClassSearch(form, page, new int?(2147483647)));
				}
				case ServiceType.Camp:
				{
					return this.PartialView("rosters-camps", this.PerformRosterCampSearch(form, page, new int?(2147483647)));
				}
			}
			return new EmptyResult();
		}

		[ActionName("rosters-pdf")]
		[HttpGet]
		[PDFDownload]
		public ActionResult RostersPdf(string classIds, string campIds)
		{
			int num;
			int num1;
			((dynamic)ViewBag).FileName = "Rosters";
			RosterSearchForm form = (RosterSearchForm)base.GetSession("Roster.SearchForm");
			dynamic viewBag = ViewBag;
			RosterSearchForm rosterSearchForm = form;
			string str = classIds;
			int? classPage = form.ClassPage;
			num = (classPage.HasValue ? classPage.GetValueOrDefault() : 1);
			int? nullable = null;
			viewBag.ClassRosters = this.PerformRosterClassSearchForExport(rosterSearchForm, str, num, nullable).PageItems;
			dynamic pageItems = ViewBag;
			RosterSearchForm rosterSearchForm1 = form;
			string str1 = campIds;
			int? campPage = form.CampPage;
			num1 = (campPage.HasValue ? campPage.GetValueOrDefault() : 1);
			int? nullable1 = null;
			pageItems.CampRosters = this.PerformRosterCampSearchForExport(rosterSearchForm1, str1, num1, nullable1).PageItems;
			return new EmptyResult();
		}

		[ActionName("rosters-print")]
		[HttpGet]
		public ActionResult RostersPrint(string classIds, string campIds)
		{
			int num;
			int num1;
			RosterSearchForm form = (RosterSearchForm)base.GetSession("Roster.SearchForm");
			dynamic viewBag = ViewBag;
			RosterSearchForm rosterSearchForm = form;
			string str = classIds;
			int? classPage = form.ClassPage;
			num = (classPage.HasValue ? classPage.GetValueOrDefault() : 1);
			int? nullable = null;
			viewBag.ClassRosters = this.PerformRosterClassSearchForExport(rosterSearchForm, str, num, nullable).PageItems;
			dynamic pageItems = ViewBag;
			RosterSearchForm rosterSearchForm1 = form;
			string str1 = campIds;
			int? campPage = form.CampPage;
			num1 = (campPage.HasValue ? campPage.GetValueOrDefault() : 1);
			int? nullable1 = null;
			pageItems.CampRosters = this.PerformRosterCampSearchForExport(rosterSearchForm1, str1, num1, nullable1).PageItems;
			return base.PartialView("rosters-print");
		}

		[ActionName("schedules-excel")]
		[ScheduleExcelDownload(Filename="Schedules")]
		public ActionResult ScheduleExcel()
		{
			ScheduleSearchForm sessionForm = (ScheduleSearchForm)base.GetSession("Schuedules.SearchForm") ?? new ScheduleSearchForm();
			dynamic viewBag = ViewBag;
			List<Class> pageItems = this.PerformScheduleClassSearch(sessionForm, 1, new int?(2147483647)).PageItems;
			viewBag.Classes = (
				from t in pageItems
				select new { Location = t.Location.DisplayName, Class = t.Name, TimeStart = t.TimeStart.To12HoursString(), TimeEnd = t.TimeEnd.To12HoursString(), Instructor = ScheduleUtil.GetShortInstructorName(t.Instructor), Cost = t.Cost, Grades = ScheduleUtil.GetGradeListText(t), Enroll = (t.Enrolled.HasValue ? t.Enrolled.Value : 0), Dates = ScheduleUtil.GetDateListText(t) }).ToList();
			dynamic list = ViewBag;
			List<Camp> camps = this.PerformScheduleCampSearch(sessionForm, 1, new int?(2147483647)).PageItems;
			list.Camps = (
				from t in camps
				select new { Location = t.Location.DisplayName, Class = t.Name, TimeStart = t.TimeStart.To12HoursString(), TimeEnd = t.TimeEnd.To12HoursString(), Instructor = ScheduleUtil.GetShortInstructorName(t.Instructor), Cost = t.Cost, Grades = ScheduleUtil.GetGradeListText(t), Enroll = (t.Enrolled.HasValue ? t.Enrolled.Value : 0), Dates = ScheduleUtil.GetDateListText(t) }).ToList();
			dynamic obj = ViewBag;
			List<Birthday> birthdays = this.PerformScheduleBirthdaySearch(sessionForm, 1, new int?(2147483647)).PageItems;
			obj.Birthdays = (
				from t in birthdays
				select new { Parent = t.ParentName, ContactNumber = t.ContactNumber, Email = t.Email, Address = t.Address, ChidName = t.ChildName, Age = t.Age, PartyDate = t.PartyDate.ToString("M/dd/yyyy"), Time = t.PartyTime.To12HoursString() }).ToList();
			dynamic viewBag1 = ViewBag;
			List<Workshop> workshops = this.PerformScheduleWorkshopSearch(sessionForm, 1, new int?(2147483647)).PageItems;
			viewBag1.Workshops = (
				from t in workshops
				select new { Location = t.Location.DisplayName, Class = t.Name, TimeStart = t.TimeStart.To12HoursString(), TimeEnd = t.TimeEnd.To12HoursString(), Instructor = ScheduleUtil.GetShortInstructorName(t.Instructor), Cost = t.Cost, Grades = ScheduleUtil.GetGradeListText(t), Dates = ScheduleUtil.GetDateListText(t) }).ToList();
			return new EmptyResult();
		}

		[ActionName("schedules-paging")]
		[HttpGet]
		public ActionResult SchedulePaging(ScheduleSearchForm form, int page)
		{
			switch (form.Type.Value)
			{
				case ServiceType.Class:
				{
					return this.PartialView("schedules-classes", this.PerformScheduleClassSearch(form, page, new int?(2147483647)));
				}
				case ServiceType.Camp:
				{
					return this.PartialView("schedules-camps", this.PerformScheduleCampSearch(form, page, new int?(2147483647)));
				}
				case ServiceType.Workshop:
				{
					return this.PartialView("schedules-workshops", this.PerformScheduleWorkshopSearch(form, page, new int?(2147483647)));
				}
				case ServiceType.Birthday:
				{
					return this.PartialView("schedules-birthdays", this.PerformScheduleBirthdaySearch(form, page, new int?(2147483647)));
				}
			}
			return new EmptyResult();
		}

		[HttpGet]
		public ActionResult Schedules(ScheduleSearchForm form, bool? research)
		{
			if (!base.User.Admin.AreaId.HasValue)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminNotAssign, new object[0]);
				return base.View("AdminNotAssign");
			}
			if (!ModelState.IsValid)
			{
				TempData.Add("Error", "Please verify the search condition");
				return base.View(form);
			}
			if (!research.HasValue || !research.Value || base.GetSession("Schuedules.SearchForm") == null)
			{
				base.SetSession("Schuedules.SearchForm", form);
			}
			ScheduleSearchForm sessionForm = (ScheduleSearchForm)base.GetSession("Schuedules.SearchForm");
			if (form.Type.HasValue)
			{
				switch (form.Type.Value)
				{
					case ServiceType.Class:
					{
						sessionForm.ClassPage = form.ClassPage;
						break;
					}
					case ServiceType.Camp:
					{
						sessionForm.CampPage = form.CampPage;
						break;
					}
					case ServiceType.Workshop:
					{
						sessionForm.WorkshopPage = form.WorkshopPage;
						break;
					}
					case ServiceType.Birthday:
					{
						sessionForm.BirthdayPage = form.BirthdayPage;
						break;
					}
				}
			}
			ScheduleSearchForm scheduleSearchForm = sessionForm;
			int? classPage = sessionForm.ClassPage;
			SearchResult<ScheduleSearchForm, Class> classResult = this.PerformScheduleClassSearch(scheduleSearchForm, (classPage.HasValue ? classPage.GetValueOrDefault() : 1), new int?(2147483647));
			ScheduleSearchForm scheduleSearchForm1 = sessionForm;
			int? campPage = sessionForm.CampPage;
			SearchResult<ScheduleSearchForm, Camp> campResult = this.PerformScheduleCampSearch(scheduleSearchForm1, (campPage.HasValue ? campPage.GetValueOrDefault() : 1), new int?(2147483647));
			ScheduleSearchForm scheduleSearchForm2 = sessionForm;
			int? birthdayPage = sessionForm.BirthdayPage;
			SearchResult<ScheduleSearchForm, Birthday> birthdayResult = this.PerformScheduleBirthdaySearch(scheduleSearchForm2, (birthdayPage.HasValue ? birthdayPage.GetValueOrDefault() : 1), new int?(2147483647));
			ScheduleSearchForm scheduleSearchForm3 = sessionForm;
			int? workshopPage = sessionForm.WorkshopPage;
			SearchResult<ScheduleSearchForm, Workshop> workshopsResult = this.PerformScheduleWorkshopSearch(scheduleSearchForm3, (workshopPage.HasValue ? workshopPage.GetValueOrDefault() : 1), new int?(2147483647));
			((dynamic)ViewBag).Classes = classResult;
			((dynamic)ViewBag).Camps = campResult;
			((dynamic)ViewBag).Birthdays = birthdayResult;
			((dynamic)ViewBag).Workshops = workshopsResult;
			Dictionary<DayOfWeek, List<Class>> classByDays = new Dictionary<DayOfWeek, List<Class>>();
			Dictionary<DayOfWeek, List<Camp>> campByDays = new Dictionary<DayOfWeek, List<Camp>>();
			Dictionary<DayOfWeek, List<Workshop>> workshopByDays = new Dictionary<DayOfWeek, List<Workshop>>();
			Dictionary<DayOfWeek, List<Birthday>> birthdayByDays = new Dictionary<DayOfWeek, List<Birthday>>();
			foreach (int day in sessionForm.SelectedDays)
			{
				List<Class> classList = new List<Class>();
				List<Camp> campList = new List<Camp>();
				List<Workshop> workshopList = new List<Workshop>();
				List<Birthday> birthdayList = new List<Birthday>();
				foreach (Class item in classResult.PageItems)
				{
					if (!item.IsRunOnDay((DayOfWeek)day))
					{
						continue;
					}
					classList.Add(item);
				}
				foreach (Camp item in campResult.PageItems)
				{
					if (!item.IsRunOnDay((DayOfWeek)day))
					{
						continue;
					}
					campList.Add(item);
				}
				foreach (Workshop item in workshopsResult.PageItems)
				{
					if (!item.IsRunOnDay((DayOfWeek)day))
					{
						continue;
					}
					workshopList.Add(item);
				}
				foreach (Birthday item in birthdayResult.PageItems)
				{
					if (!item.IsRunOnDay((DayOfWeek)day))
					{
						continue;
					}
					birthdayList.Add(item);
				}
				classByDays.Add((DayOfWeek)day, classList);
				campByDays.Add((DayOfWeek)day, campList);
				workshopByDays.Add((DayOfWeek)day, workshopList);
				birthdayByDays.Add((DayOfWeek)day, birthdayList);
			}
			((dynamic)ViewBag).ClassByDays = classByDays;
			((dynamic)ViewBag).CampByDays = campByDays;
			((dynamic)ViewBag).WorkshopByDays = workshopByDays;
			((dynamic)ViewBag).BirthdayByDays = birthdayByDays;
			return base.View(sessionForm);
		}

		public override ActionResult SendEmail(string toAddresses, string toNames, string preAttachFile)
		{
			((dynamic)ViewBag).IsSuperAdmin = base.User.Admin.IsSuperAdmin;
			if (!preAttachFile.Contains("ROSTER/"))
			{
				SendEmailForm sendEmailForm = new SendEmailForm()
				{
					From = base.User.Admin.Email,
					ToAddress = toAddresses,
					ToNames = toNames,
					PreAttachFile = string.Empty
				};
				return base.View("send-email", sendEmailForm);
			}
			string[] rosterInfo = preAttachFile.Split(new char[] { '/' });
			RosterDetailPdf roster = this.CreateRosterDetailExportData(Convert.ToInt64(rosterInfo[2]), (rosterInfo[1].ToUpper() == ServiceType.Class.ToString().ToUpper() ? ServiceType.Class : ServiceType.Camp));
			Document document = new Document();
			string fileName = string.Format("Roster_{0}.pdf", roster.ClassName);
			string timeForderId = DateTime.Now.ToString("yyyyMMddhhmmss");
			string folderPath = Path.Combine(base.User.Admin.Username, timeForderId);
			string saveLocation = Path.Combine( System.Web.HttpContext.Current.Server.MapPath( "~/Upload"), folderPath);
			if (!Directory.Exists(saveLocation))
			{
				Directory.CreateDirectory(saveLocation);
			}
			PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(Path.Combine(saveLocation, fileName), FileMode.Create));
			if (roster.Type != ServiceType.Class)
			{
				RosterPdfUtil.CreateRostersDocument(writer, document, null, new List<RosterDetailPdf>()
				{
					roster
				});
			}
			else
			{
				RosterPdfUtil.CreateRostersDocument(writer, document, new List<RosterDetailPdf>()
				{
					roster
				}, null);
			}
			SendEmailForm sendEmailForm1 = new SendEmailForm()
			{
				From = base.User.Admin.Email,
				ToAddress = toAddresses,
				ToNames = toNames,
				PreAttachFile = Path.Combine(folderPath, fileName)
			};
			return base.View("send-email", sendEmailForm1);
		}

		[ActionName("students-detail")]
		[HttpGet]
		public ActionResult StudentDetail(long? id, long parentId)
		{
			if (!id.HasValue)
			{
				return base.PartialView(new Student()
				{
					ParentId = parentId
				});
			}
			Student student = this.GetStudent(id.Value);
			if (student != null)
			{
				return base.PartialView(student);
			}
			System.Web.HttpContext.Current.Response.StatusCode = 600;
			return base.Content(Messages.DataNotFound);
		}

		[ActionName("students-detail")]
		[HttpPost]
		public ActionResult StudentDetail(Student student)
		{
			if (!ModelState.IsValid)
			{
				System.Web.HttpContext.Current.Response.StatusCode = 600;
				return base.PartialView(student);
			}
			Student dbstudent = null;
			if (student.Id <= (long)0)
			{
				dbstudent = new Student();
				this.db.Students.Add(dbstudent);
			}
			else
			{
				dbstudent = this.GetStudent(student.Id);
			}
			dbstudent.ParentId = student.ParentId;
			dbstudent.FirstName = student.FirstName;
			dbstudent.LastName = student.LastName;
			dbstudent.Gender = student.Gender;
			dbstudent.GradeId = student.GradeId;
			dbstudent.BirthDate = student.BirthDate;
			dbstudent.Notes = student.Notes;
			this.db.SaveChanges();
			((dynamic)ViewBag).ParentId = student.ParentId;
			return this.PartialView("students", this.GetParent(student.ParentId).Students);
		}

		[ActionName("students-delete")]
		[HttpPost]
		public ActionResult StudentsDelete(long id)
		{
			ActionResult actionResult;
			try
			{
				Student student = this.GetStudent(id);
				if (student != null)
				{
					Parent parent = student.Parent;
					this.db.Students.Remove(student);
					this.db.SaveChanges();
					((dynamic)ViewBag).ParentId = parent.Id;
					actionResult = this.PartialView("students", parent.Students);
				}
				else
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
					actionResult = base.Content(Messages.DataNotFound);
				}
			}
			catch (Exception exception)
			{
				this.log.Error("Delete student failed.", exception);
				System.Web.HttpContext.Current.Response.StatusCode = 600;
				actionResult = base.Content(string.Format(Messages.DeleteFailByForeignKey, "student"));
			}
			return actionResult;
		}

		[ActionName("workshop-delete")]
		[HttpPost]
		public ActionResult WorkshopDelete(long id)
		{
			try
			{
				Workshop workshop = this.GetWorkshop(id);
				foreach (Assign assign in workshop.Assigns.ToList<Assign>())
				{
					this.db.Assigns.Remove(assign);
				}
				workshop.GradeGroup.Grades.Clear();
				this.db.GradeGroups.Remove(workshop.GradeGroup);
				this.db.Workshops.Remove(workshop);
				this.db.SaveChanges();
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.DeleteScheduleEvents(null, null, null, id.ToString());
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Workshop" });
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Workshop" });
			}
			return base.RedirectToRoute("Admin", new { action = "schedules", research = "true" });
		}

		[ActionName("workshop-detail")]
		[HttpGet]
		public ActionResult WorkshopDetail(long id)
		{
			ActionResult route;
			try
			{
				Workshop workshop = this.GetWorkshop(id);
				workshop.UpdateCustomProperties();
				route = base.View(workshop);
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.RedirectToRoute("Admin", new { action = "schedules", research = "true" });
			}
			return route;
		}

		[ActionName("workshop-detail")]
		[HttpPost]
		public ActionResult WorkshopDetail(Workshop workshop)
		{
			ActionResult route;
			dynamic viewBag = ViewBag;
			CalendarSearchForm calendarSearchForm = new CalendarSearchForm()
			{
				Date = new DateTime?(DateTime.Today),
				ShowWorkshops = true,
				WorkshopId = new long?(workshop.Id),
				ViewBy = CalendarViewType.Month
			};
			viewBag.Calendar = this.PerformCalendarSearch(calendarSearchForm);
			if (!ModelState.IsValid)
			{
				return base.View(workshop);
			}
			try
			{
				Workshop value = this.GetWorkshop(workshop.Id);
				value.Cost = workshop.NCost.Value;
				value.LocationId = this.GetLocation(workshop.LocationId).Id;
				value.Name = workshop.Name;
				value.Notes = workshop.Notes;
				value.TimeEnd = workshop.TimeEnd;
				value.TimeStart = workshop.TimeStart;
				bool changeInstructor = value.InstructorId != workshop.InstructorId;
				long? assistantId = value.AssistantId;
				long? nullable = workshop.AssistantId;
				bool changeAssistant = (assistantId.GetValueOrDefault() != nullable.GetValueOrDefault() ? true : assistantId.HasValue != nullable.HasValue);
				if (changeInstructor)
				{
					value.InstructorId = workshop.InstructorId;
				}
				if (changeAssistant)
				{
					value.AssistantId = workshop.AssistantId;
				}
				value.Assigns.Clear();
				foreach (Assign assignList in workshop.AssignList)
				{
					if (!assignList.NDate.HasValue)
					{
						continue;
					}
					if (!value.Assigns.Any<Assign>((Assign t) => t.Date == assignList.NDate.Value))
					{
						Assign assign = new Assign()
						{
							Date = assignList.NDate.Value,
							InstructorId = (changeInstructor ? workshop.InstructorId : assignList.InstructorId),
							AssistantId = (changeAssistant ? workshop.AssistantId : assignList.AssistantId)
						};
						value.Assigns.Add(assign);
					}
					else
					{
						base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DatesDuplicated, new object[0]);
						route = base.View(workshop);
						return route;
					}
				}
				value.GradeGroup.Grades.Clear();
				foreach (byte gradeId in workshop.GradeIds)
				{
					value.GradeGroup.Grades.Add(this.GetGrade((long)gradeId));
				}
				this.db.SaveChanges();
				Location location = this.db.Locations.First<Location>((Location t) => t.Id == value.LocationId);
				value.Location = location;
				GoogleCalendarHelper googleTaskHelper = new GoogleCalendarHelper(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
				googleTaskHelper.SynsScheduleEvents(null, null, null, value);
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.UpdateSuccess, new object[] { "Workshop" });
				route = base.RedirectToRoute("Admin", new { action = "workshop-detail", id = value.Id });
			}
			catch (Exception exception)
			{
				this.log.Error("Update workshop failed.", exception);
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				route = base.View(workshop);
			}
			return route;
		}

		[ActionName("workshop-detail-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult WorkshopDetailExel(long? id)
		{
			Workshop workshop = this.GetWorkshop(id.Value);
			((dynamic)ViewBag).FileName = workshop.Name;
			((dynamic)ViewBag).Detail = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Detail;
			string[] displayName = new string[] { workshop.Location.DisplayName, workshop.Name, ScheduleUtil.GetGradeListText(workshop), workshop.TimeStart.To12HoursString(), workshop.TimeEnd.To12HoursString(), ScheduleUtil.GetDateListText(workshop), string.Concat(workshop.Instructor.FirstName, " ", workshop.Instructor.LastName), null, null, null };
			displayName[7] = (workshop.AssistantId.HasValue ? string.Concat(workshop.Instructor1.FirstName, workshop.Instructor1.LastName) : string.Empty);
			displayName[8] = workshop.Cost.ToString();
			displayName[9] = workshop.Notes;
			viewBag.Add(displayName);
			return new EmptyResult();
		}
	}
}