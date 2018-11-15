using EggheadWeb.Common;
using EggheadWeb.Mailers;
using EggheadWeb.Models.Common;
using EggheadWeb.Security;
using EggheadWeb.Utility;
using Microsoft.CSharp.RuntimeBinder;
using Mvc.Mailer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace EggheadWeb.Controllers
{
	public class BaseAdminController : WithAuthenController
	{
		private const string SUPPER_ADMIN_CONTROLLER = "SuperAdmin";

		private IAdminMailer adminMailer = new EggheadWeb.Mailers.AdminMailer();

		public IAdminMailer AdminMailer
		{
			get
			{
				return this.adminMailer;
			}
			set
			{
				this.adminMailer = value;
			}
		}

		public BaseAdminController(string role) : base(role)
		{
		}

		private SmtpClientWrapper CreateStmtClientWrapper()
		{
			SmtpClientWrapper stmpClient = new SmtpClientWrapper(EmailUtil.GetSystemSmtpClient(this.db));
			stmpClient.InnerSmtpClient.Credentials = new NetworkCredential(base.User.Admin.Email, CryptoUtil.Decrypt(base.User.Admin.EmailPassword, base.User.Admin.Username));
			return stmpClient;
		}

		[ActionName("task-delete")]
		[HttpPost]
		public ActionResult DeleteTask(long taskId)
		{
			ActionResult actionResult;
			try
			{
				AdminTask adminTask = this.db.AdminTasks.SingleOrDefault<AdminTask>((AdminTask t) => t.Id == taskId);
				if (adminTask == null)
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
				}
				this.db.AdminTasks.Remove(adminTask);
				this.db.SaveChanges();
				dynamic viewBag = ViewBag;
				DbSet<AdminTask> adminTasks = this.db.AdminTasks;
				viewBag.RunningTasks = (
					from t in adminTasks
					where t.AdminId == this.User.Admin.Id && (int)t.Status == 1
					select t).ToList<AdminTask>();
				dynamic list = ViewBag;
				DbSet<AdminTask> adminTasks1 = this.db.AdminTasks;
				list.CompletedTasks = (
					from t in adminTasks1
					where t.AdminId == this.User.Admin.Id && (int)t.Status == 2
					select t).ToList<AdminTask>();
				actionResult = base.PartialView("AdminTasks");
			}
			catch (Exception exception)
			{
				System.Web.HttpContext.Current.Response.StatusCode = 600;
				actionResult = base.PartialView("AdminTasks");
			}
			return actionResult;
		}

		public ActionResult Email()
		{
			dynamic viewBag = ViewBag;
			IQueryable<AdminEmailTemplate> adminEmailTemplates = 
				from k in this.db.AdminEmailTemplates
				where k.AdminId == this.User.Admin.Id
				select k;
			viewBag.UserEmailTeplates = SearchResult.New<AdminEmailTemplate>(
				from g in adminEmailTemplates
				orderby g.Id
				select g, 1, 2147483647);
			if (base.User.IsInRole("superadmin"))
			{
				dynamic obj = ViewBag;
				DbSet<EmailTemplate> emailTemplates = this.db.EmailTemplates;
				obj.SystemEmailTeplates = SearchResult.New<object, EmailTemplate>(null, 
					from g in emailTemplates
					orderby g.Id
					select g, 1, 2147483647);
			}
			return base.View();
		}

		private JsonResult GetBarChartData(long? adminId, string fromDate, string toDate)
		{
			object[] objArray;
			DateTime barFromDate = Convert.ToDateTime(fromDate);
			DateTime barToDate = Convert.ToDateTime(toDate);
			IQueryable<Class> bookedClassesQ = this.db.Classes.AsQueryable<Class>();
			IQueryable<Camp> bookedCampsQ = this.db.Camps.AsQueryable<Camp>();
			IQueryable<Workshop> bookedWorkshopsQ = this.db.Workshops.AsQueryable<Workshop>();
			IQueryable<Birthday> bookedBirthdaysQ = this.db.Birthdays.AsQueryable<Birthday>();
			int index = 1;
			List<object> schoolBooked = new List<object>();
			List<object> classes = new List<object>();
			List<object> camps = new List<object>();
			List<object> workshops = new List<object>();
			List<object> birthdays = new List<object>();
			List<object> enrolledStudents = new List<object>();
			int maxY = 0;
			index = 1;
			if (adminId.HasValue && adminId.Value != (long)-1)
			{
				bookedClassesQ = 
					from t in bookedClassesQ
					where t.Location.Area.Admins.Any<Admin>((Admin x) => (long?)x.Id == adminId)
					select t;
				bookedCampsQ = 
					from t in bookedCampsQ
					where t.Location.Area.Admins.Any<Admin>((Admin x) => (long?)x.Id == adminId)
					select t;
				bookedWorkshopsQ = 
					from t in bookedWorkshopsQ
					where t.Location.Area.Admins.Any<Admin>((Admin x) => (long?)x.Id == adminId)
					select t;
				bookedBirthdaysQ = 
					from t in bookedBirthdaysQ
					where t.Area.Admins.Any<Admin>((Admin x) => (long?)x.Id == adminId)
					select t;
			}
			decimal adminCount = Convert.ToDecimal(this.db.Admins.Count<Admin>((Admin t) => !t.IsSuperAdmin));
			if (!adminId.HasValue && adminCount != new decimal(0))
			{
				for (DateTime i = barFromDate; i <= barToDate; i = i.AddDays(1))
				{
					long timestamps = MisUtil.GetJavascriptTimestamp(i);
					if (i < barToDate)
					{
						List<long> runningScholls = new List<long>();
						int studentCount = 0;
						List<Class> runningClasses = (
							from t in bookedClassesQ
							where t.Assigns.Any<Assign>((Assign x) => x.Date == i)
							select t).ToList<Class>();
						List<Camp> runningCamps = (
							from t in bookedCampsQ
							where t.Assigns.Any<Assign>((Assign x) => x.Date == i)
							select t).ToList<Camp>();
						List<Workshop> runningWorkshops = (
							from t in bookedWorkshopsQ
							where t.Assigns.Any<Assign>((Assign x) => x.Date == i)
							select t).ToList<Workshop>();
						List<Birthday> runningBirthdays = (
							from t in bookedBirthdaysQ
							where t.Assigns.Any<Assign>((Assign x) => x.Date == i)
							select t).ToList<Birthday>();
						List<Class> runningSchollByClasses = (
							from x in bookedClassesQ
							where (x.Assigns.Min<Assign, DateTime>((Assign t) => t.Date) <= i) && (x.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= i)
							select x).ToList<Class>();
						List<Camp> runningSchollByCamps = (
							from x in bookedCampsQ
							where (x.Assigns.Min<Assign, DateTime>((Assign t) => t.Date) <= i) && (x.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= i)
							select x).ToList<Camp>();
						List<Workshop> runningSchollByWorkshops = (
							from x in bookedWorkshopsQ
							where (x.Assigns.Min<Assign, DateTime>((Assign t) => t.Date) <= i) && (x.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= i)
							select x).ToList<Workshop>();
						foreach (Class x in runningSchollByClasses.ToList<Class>())
						{
							if (runningScholls.Contains(x.LocationId))
							{
								continue;
							}
							runningScholls.Add(x.LocationId);
						}
						foreach (Camp x in runningSchollByCamps.ToList<Camp>())
						{
							if (runningScholls.Contains(x.LocationId))
							{
								continue;
							}
							runningScholls.Add(x.LocationId);
						}
						foreach (Workshop x in runningSchollByWorkshops.ToList<Workshop>())
						{
							if (runningScholls.Contains(x.LocationId))
							{
								continue;
							}
							runningScholls.Add(x.LocationId);
						}
						foreach (Class x in runningClasses)
						{
							studentCount = studentCount + (x.Enrolled.HasValue ? x.Enrolled.Value : 0);
						}
						foreach (Camp x in runningCamps)
						{
							studentCount = studentCount + (x.Enrolled.HasValue ? x.Enrolled.Value : 0);
						}
						objArray = new object[] { timestamps, Math.Round(runningScholls.Count<long>() / adminCount, 1) };
						schoolBooked.Add(objArray);
						objArray = new object[] { timestamps, Math.Round(runningClasses.Count<Class>() / adminCount, 1) };
						classes.Add(objArray);
						objArray = new object[] { timestamps, Math.Round(runningCamps.Count<Camp>() / adminCount, 1) };
						camps.Add(objArray);
						objArray = new object[] { timestamps, Math.Round(runningWorkshops.Count<Workshop>() / adminCount, 1) };
						workshops.Add(objArray);
						objArray = new object[] { timestamps, Math.Round(runningBirthdays.Count<Birthday>() / adminCount, 1) };
						birthdays.Add(objArray);
						objArray = new object[] { timestamps, Math.Round(studentCount / adminCount, 1) };
						enrolledStudents.Add(objArray);
						if (runningScholls.Count<long>() > maxY)
						{
							maxY = runningScholls.Count<long>();
						}
						if (runningClasses.Count<Class>() > maxY)
						{
							maxY = runningClasses.Count<Class>();
						}
						if (runningWorkshops.Count<Workshop>() > maxY)
						{
							maxY = runningWorkshops.Count<Workshop>();
						}
						if (runningBirthdays.Count<Birthday>() > maxY)
						{
							maxY = runningBirthdays.Count<Birthday>();
						}
						if (studentCount > maxY)
						{
							maxY = studentCount;
						}
					}
					index++;
				}
			}
			else if (adminId.HasValue)
			{
				for (DateTime j = barFromDate; j <= barToDate; j = j.AddDays(1))
				{
					long timestamps = MisUtil.GetJavascriptTimestamp(j);
					if (j < barToDate)
					{
						List<long> runningScholls = new List<long>();
						int studentCount = 0;
						List<Class> runningClasses = (
							from t in bookedClassesQ
							where t.Assigns.Any<Assign>((Assign x) => x.Date == j)
							select t).ToList<Class>();
						List<Camp> runningCamps = (
							from t in bookedCampsQ
							where t.Assigns.Any<Assign>((Assign x) => x.Date == j)
							select t).ToList<Camp>();
						List<Workshop> runningWorkshops = (
							from t in bookedWorkshopsQ
							where t.Assigns.Any<Assign>((Assign x) => x.Date == j)
							select t).ToList<Workshop>();
						List<Birthday> runningBirthdays = (
							from t in bookedBirthdaysQ
							where t.Assigns.Any<Assign>((Assign x) => x.Date == j)
							select t).ToList<Birthday>();
						List<Class> runningSchollByClasses = (
							from x in bookedClassesQ
							where (x.Assigns.Min<Assign, DateTime>((Assign t) => t.Date) <= j) && (x.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= j)
							select x).ToList<Class>();
						List<Camp> runningSchollByCamps = (
							from x in bookedCampsQ
							where (x.Assigns.Min<Assign, DateTime>((Assign t) => t.Date) <= j) && (x.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= j)
							select x).ToList<Camp>();
						List<Workshop> runningSchollByWorkshops = (
							from x in bookedWorkshopsQ
							where (x.Assigns.Min<Assign, DateTime>((Assign t) => t.Date) <= j) && (x.Assigns.Max<Assign, DateTime>((Assign t) => t.Date) >= j)
							select x).ToList<Workshop>();
						foreach (Class x in runningSchollByClasses.ToList<Class>())
						{
							if (runningScholls.Contains(x.LocationId))
							{
								continue;
							}
							runningScholls.Add(x.LocationId);
						}
						foreach (Camp x in runningSchollByCamps.ToList<Camp>())
						{
							if (runningScholls.Contains(x.LocationId))
							{
								continue;
							}
							runningScholls.Add(x.LocationId);
						}
						foreach (Workshop x in runningSchollByWorkshops.ToList<Workshop>())
						{
							if (runningScholls.Contains(x.LocationId))
							{
								continue;
							}
							runningScholls.Add(x.LocationId);
						}
						foreach (Class x in runningClasses)
						{
							studentCount = studentCount + (x.Enrolled.HasValue ? x.Enrolled.Value : 0);
						}
						foreach (Camp x in runningCamps)
						{
							studentCount = studentCount + (x.Enrolled.HasValue ? x.Enrolled.Value : 0);
						}
						objArray = new object[] { timestamps, runningScholls.Count<long>() };
						schoolBooked.Add(objArray);
						objArray = new object[] { timestamps, runningClasses.Count<Class>() };
						classes.Add(objArray);
						objArray = new object[] { timestamps, runningCamps.Count<Camp>() };
						camps.Add(objArray);
						objArray = new object[] { timestamps, runningWorkshops.Count<Workshop>() };
						workshops.Add(objArray);
						objArray = new object[] { timestamps, runningBirthdays.Count<Birthday>() };
						birthdays.Add(objArray);
						objArray = new object[] { timestamps, studentCount };
						enrolledStudents.Add(objArray);
						if (runningScholls.Count<long>() > maxY)
						{
							maxY = runningScholls.Count<long>();
						}
						if (runningClasses.Count<Class>() > maxY)
						{
							maxY = runningClasses.Count<Class>();
						}
						if (runningWorkshops.Count<Workshop>() > maxY)
						{
							maxY = runningWorkshops.Count<Workshop>();
						}
						if (runningBirthdays.Count<Birthday>() > maxY)
						{
							maxY = runningBirthdays.Count<Birthday>();
						}
						if (studentCount > maxY)
						{
							maxY = studentCount;
						}
					}
					index++;
				}
			}
			List<object> data = new List<object>()
			{
				new { label = "School Booked", data = schoolBooked.ToArray() },
				new { label = "Classes", data = classes.ToArray() },
				new { label = "Camps", data = camps.ToArray() },
				new { label = "Birthdays", data = birthdays.ToArray() },
				new { label = "Workshops", data = workshops.ToArray() },
				new { label = "Enrolled Students", data = enrolledStudents.ToArray() }
			};
			return base.Json(new { Data = data, MaxY = maxY });
		}

		[ActionName("get-email-template")]
		[HttpPost]
		public ActionResult GetEmailTemplate(long id)
		{
			ActionResult actionResult;
			try
			{
				AdminEmailTemplate emailTemplate = this.GetUserEmailTemplate(id);
				actionResult = base.Json(new { ReturnStatus = "true", MailSubject = emailTemplate.MailSubject, MailBody = emailTemplate.MailBody });
			}
			catch
			{
				actionResult = base.Json(new { ReturnStatus = "false", MailSubject = string.Empty, MailBody = string.Empty });
			}
			return actionResult;
		}

		[ActionName("get-private-message")]
		[HttpPost]
		public ActionResult GetPrivateMessage(long id)
		{
			PrivateMessage message = this.db.PrivateMessages.SingleOrDefault<PrivateMessage>((PrivateMessage t) => t.Id == id);
			if (message == null)
			{
				return base.Json(new { ReturnStatus = "false", From = string.Empty, To = string.Empty, MessageSubject = string.Empty, ReceiveDate = string.Empty, FromAdminId = string.Empty, ToAdminId = string.Empty, MessageContent = string.Empty });
			}
			long? toAdminId = message.ToAdminId;
			long num = base.User.Admin.Id;
			if ((toAdminId.GetValueOrDefault() != num ? false : toAdminId.HasValue))
			{
				message.Unread = false;
				this.db.SaveChanges();
			}
			return base.Json(new { ReturnStatus = "false", From = (message.Admin != null ? StringUtil.GetFullName(message.Admin.FirstName, message.Admin.LastName) : message.FromAdminName), To = (message.Admin1 != null ? StringUtil.GetFullName(message.Admin1.FirstName, message.Admin1.LastName) : message.ToAdminName), MessageSubject = message.MessageSubject, ReceiveDate = message.SendDate.ToOneDigitMonthWithTime(), FromAdminId = (message.FromAdminId.HasValue ? message.FromAdminId.Value.ToString() : string.Empty), ToAdminId = (message.ToAdminId.HasValue ? message.ToAdminId.Value.ToString() : string.Empty), MessageContent = message.MessageContent });
		}

		private AdminEmailTemplate GetUserEmailTemplate(long id)
		{
			return this.db.AdminEmailTemplates.First<AdminEmailTemplate>((AdminEmailTemplate t) => (long)t.Id == id && t.AdminId == this.User.Admin.Id);
		}

		[AllowAnonymous]
		[HttpGet]
		public ActionResult Home()
		{
			string controller = RouteData.Values["controller"].ToString();
			if (!System.Web.HttpContext.Current.Request.IsAuthenticated)
			{
				return base.View("login");
			}
			if (base.User.Admin.IsSuperAdmin && controller != "SuperAdmin")
			{
				return base.RedirectToRoute("SuperAdmin", new { action = "Home" });
			}
			dynamic viewBag = ViewBag;
			IQueryable<AdminTask> adminTasks = 
				from t in this.db.AdminTasks
				where t.AdminId == this.User.Admin.Id && (int)t.Status == 1
				select t;
			viewBag.RunningTasks = (
				from t in adminTasks
				orderby t.DueDate
				select t).ToList<AdminTask>();
			dynamic list = ViewBag;
			IQueryable<AdminTask> adminTasks1 = 
				from t in this.db.AdminTasks
				where t.AdminId == this.User.Admin.Id && (int)t.Status == 2
				select t;
			list.CompletedTasks = (
				from t in adminTasks1
				orderby t.DueDate
				select t).ToList<AdminTask>();
			DateTime today = DateTime.Today;
			DateTime dateTime = DateTime.Today;
			DateTime startOfWeek = today.AddDays((double)((int)(DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday | DayOfWeek.Saturday) * (int)dateTime.DayOfWeek));
			((dynamic)ViewBag).BarChartFromDate = startOfWeek.ToOneDigitMonth();
			((dynamic)ViewBag).BarChartToDate = startOfWeek.AddDays(7).ToOneDigitMonth();
			Admin admin = base.User.Admin;
			if (!admin.IsSuperAdmin)
			{
				dynamic obj = ViewBag;
				long id = admin.Id;
				dynamic viewBag1 = ((dynamic)ViewBag).BarChartFromDate;
				obj.BarChartData = this.GetBarChartData(id, viewBag1, ((dynamic)ViewBag).BarChartToDate).Data;
			}
			else
			{
				dynamic obj1 = ViewBag;
				dynamic viewBag2 = ((dynamic)ViewBag).BarChartFromDate;
				obj1.BarChartData = this.GetBarChartData((dynamic)null, viewBag2, ((dynamic)ViewBag).BarChartToDate).Data;
			}
			dynamic list1 = ViewBag;
			IQueryable<PrivateMessage> privateMessages = 
				from t in this.db.PrivateMessages
				where t.ToAdminId == (long?)this.User.Admin.Id && t.Unread
				select t;
			list1.InboxMessages = (
				from t in privateMessages
				orderby t.SendDate descending
				select t).ToList<PrivateMessage>();
			dynamic obj2 = ViewBag;
			ICollection<PrivateMessage> privateMessages1 = base.User.Admin.PrivateMessages1;
			obj2.PMCount = privateMessages1.Count<PrivateMessage>((PrivateMessage t) => t.Unread);
			return base.View("home");
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult Login(LoginForm model)
		{
			if (!ModelState.IsValid)
			{
				return base.View();
			}
			if (base.ValidateUser(model.Username, model.Password))
			{
				base.SignIn(model.Username, model.SaveLogin);
				if (string.IsNullOrWhiteSpace(model.ReturnUrl))
				{
					return base.RedirectToAction("");
				}
				return this.Redirect(model.ReturnUrl);
			}
			((dynamic)ViewBag).Error = "Invalid username or password";
			return base.View();
		}

		[HttpGet]
		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();
			string userDicrectoryPath = Path.Combine( System.Web.HttpContext.Current.Server.MapPath( "~/Upload"), base.User.Admin.Username);
			if (Directory.Exists(userDicrectoryPath))
			{
				Directory.Delete(userDicrectoryPath, true);
			}
			return base.RedirectToAction("");
		}

		[ActionName("Private-message-display")]
		[HttpGet]
		public ActionResult PrivateMessageBoard()
		{
			dynamic viewBag = ViewBag;
			IQueryable<PrivateMessage> privateMessages = 
				from t in this.db.PrivateMessages
				where t.ToAdminId == (long?)this.User.Admin.Id
				select t;
			viewBag.InboxMessages = (
				from t in privateMessages
				orderby t.SendDate descending
				select t).ToList<PrivateMessage>();
			dynamic list = ViewBag;
			IQueryable<PrivateMessage> privateMessages1 = 
				from t in this.db.PrivateMessages
				where t.FromAdminId == (long?)this.User.Admin.Id
				select t;
			list.SentMessages = (
				from t in privateMessages1
				orderby t.SendDate descending
				select t).ToList<PrivateMessage>();
			return base.View("Private-message-display");
		}

		[ActionName("profile")]
		[Authorize(Roles="admin")]
		[HttpGet]
		public ActionResult Profile()
		{
			Admin user = this.db.Admins.First<Admin>((Admin t) => t.Id == this.User.Admin.Id);
			user.Password = Constants.STUB_PASSWORD;
			user.EmailPassword = Constants.STUB_PASSWORD;
			return base.View(user);
		}

		[ActionName("profile")]
		[Authorize(Roles="admin")]
		[HttpPost]
		public ActionResult Profile(Admin user)
		{
			ActionResult action;
			if (!ModelState.IsValid)
			{
				return base.View(user);
			}
			try
			{
				bool userNameChanged = false;
				Admin dbAdmin = this.db.Admins.First<Admin>((Admin t) => t.Id == this.User.Admin.Id);
				userNameChanged = dbAdmin.Username != user.Username;
				if (user.AreaId.HasValue)
				{
					if (this.db.Areas.First<Area>((Area t) => (long?)t.Id == user.AreaId).Admins.Any<Admin>((Admin x) => x.Id != user.Id))
					{
						base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.TerritoryDuplicate, new object[0]);
						action = base.View(user);
						return action;
					}
				}
				if (this.db.Admins.Any<Admin>((Admin t) => t.Id != user.Id && (t.Username == user.Username)))
				{
					string adminUsernameDuplicate = Messages.AdminUsernameDuplicate;
					object[] username = new object[] { user.Username };
					base.SetViewMessage(WithAuthenController.MessageType.Error, adminUsernameDuplicate, username);
					action = base.View(user);
				}
				else if (!this.db.Admins.Any<Admin>((Admin t) => t.Id != user.Id && (t.Email == user.Email)))
				{
					if (!string.IsNullOrWhiteSpace(user.EmailPassword) && user.EmailPassword != Constants.STUB_PASSWORD)
					{
						dbAdmin.EmailPassword = CryptoUtil.Encrypt(user.EmailPassword.Trim(), user.Username);
					}
					else if (userNameChanged)
					{
						string oldPassword = CryptoUtil.Decrypt(dbAdmin.EmailPassword, dbAdmin.Username);
						dbAdmin.EmailPassword = CryptoUtil.Encrypt(oldPassword, user.Username);
					}
					if (!string.IsNullOrWhiteSpace(user.Password) && user.Password != Constants.STUB_PASSWORD)
					{
						dbAdmin.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, 10);
					}
					dbAdmin.FirstName = user.FirstName;
					dbAdmin.LastName = user.LastName;
					dbAdmin.Address = user.Address;
					dbAdmin.City = user.City;
					dbAdmin.Email = user.Email;
					dbAdmin.PhoneNumber = user.PhoneNumber;
					dbAdmin.State = user.State;
					dbAdmin.Username = user.Username;
					dbAdmin.Zip = user.Zip;
					this.db.SaveChanges();
					base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.UpdateSuccess, new object[] { "Profile" });
					if (userNameChanged)
					{
						base.SignIn(dbAdmin.Username, true);
					}
					action = base.RedirectToAction("profile");
				}
				else
				{
					base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminEmailDuplicate, new object[0]);
					action = base.View(user);
				}
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				action = base.View(user);
			}
			return action;
		}

		[ActionName("private-message-refresh")]
		[HttpPost]
		public ActionResult RefreshPrivateMessageBoard()
		{
			dynamic viewBag = ViewBag;
			IQueryable<PrivateMessage> privateMessages = 
				from t in this.db.PrivateMessages
				where t.ToAdminId == (long?)this.User.Admin.Id
				select t;
			viewBag.InboxMessages = (
				from t in privateMessages
				orderby t.SendDate descending
				select t).ToList<PrivateMessage>();
			dynamic list = ViewBag;
			IQueryable<PrivateMessage> privateMessages1 = 
				from t in this.db.PrivateMessages
				where t.FromAdminId == (long?)this.User.Admin.Id
				select t;
			list.SentMessages = (
				from t in privateMessages1
				orderby t.SendDate descending
				select t).ToList<PrivateMessage>();
			dynamic obj = ViewBag;
			ICollection<PrivateMessage> privateMessages11 = base.User.Admin.PrivateMessages1;
			obj.PMCount = privateMessages11.Count<PrivateMessage>((PrivateMessage t) => t.Unread);
			return base.PartialView("PrivateMessages");
		}

		[ActionName("send-email-init")]
		[HttpPost]
		public virtual ActionResult SendEmail(string toAddresses, string toNames, string preAttachFile)
		{
			((dynamic)ViewBag).IsSuperAdmin = base.User.Admin.IsSuperAdmin;
			SendEmailForm sendEmailForm = new SendEmailForm()
			{
				From = base.User.Admin.Email,
				ToAddress = toAddresses,
				ToNames = toNames,
				PreAttachFile = preAttachFile
			};
			return base.View("send-email", sendEmailForm);
		}

		[ActionName("send-email")]
		[HttpPost]
		public ActionResult SendEmail(SendEmailForm model)
		{
			ActionResult actionResult;
			try
			{
				if (model.OnePerAddress)
				{
					foreach (MailMessage mailMessage in this.adminMailer.MailWithSeperateOnePerAddress(model))
					{
						mailMessage.Send(this.CreateStmtClientWrapper());
						mailMessage.Dispose();
					}
				}
				else
				{
					MailMessage mailMessage = this.AdminMailer.MailOne(model);
					mailMessage.Send(this.CreateStmtClientWrapper());
					mailMessage.Dispose();
				}
				if (!string.IsNullOrEmpty(model.FolderId))
				{
					Directory.Delete(Path.Combine( System.Web.HttpContext.Current.Server.MapPath( "~/Upload"), base.User.Admin.Username, model.FolderId), true);
				}
				actionResult = base.Json(new { ReturnStatus = "true", ErrorMessage = string.Empty });
			}
			catch (Exception exception)
			{
				Exception ex = exception;
				actionResult = base.Json(new { ReturnStatus = "false", ErrorMessage = ex.Message });
			}
			return actionResult;
		}

		[ActionName("reply-private-message")]
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SendPrivateMessage(PrivateMessage message)
		{
			ActionResult actionResult;
			try
			{
				Admin currentAdmin = base.User.Admin;
				Admin toAdmin = this.db.Admins.First<Admin>((Admin t) => (long?)t.Id == message.ToAdminId);
				PrivateMessage privateMessage = new PrivateMessage()
				{
					FromAdminId = new long?(currentAdmin.Id),
					ToAdminId = new long?(toAdmin.Id),
					MessageSubject = message.MessageSubject,
					MessageContent = message.MessageContent,
					SendDate = DateTime.Now,
					Unread = true,
					FromAdminName = StringUtil.GetFullName(currentAdmin.FirstName, currentAdmin.LastName),
					ToAdminName = StringUtil.GetFullName(toAdmin.FirstName, toAdmin.LastName)
				};
				this.db.PrivateMessages.Add(privateMessage);
				this.db.SaveChanges();
				actionResult = base.Json(new { ReturnStatus = "true" });
			}
			catch (Exception exception)
			{
				Exception e = exception;
				actionResult = base.Json(new { ReturnStatus = "false", ErrorMsg = e.Message });
			}
			return actionResult;
		}

		[ActionName("task-edit")]
		public ActionResult TaskEdit(AdminTask model)
		{
			if (!ModelState.IsValid)
			{
				System.Web.HttpContext.Current.Response.StatusCode = 600;
			}
			AdminTask dbTask = null;
			if (model.Id <= (long)0)
			{
				AdminTask adminTask = new AdminTask()
				{
					CreateDate = DateTime.Now,
					AdminId = base.User.Admin.Id,
					Status = 1
				};
				dbTask = adminTask;
				this.db.AdminTasks.Add(dbTask);
			}
			else
			{
				dbTask = this.db.AdminTasks.FirstOrDefault<AdminTask>((AdminTask t) => t.Id == model.Id);
			}
			dbTask.Name = model.Name;
			dbTask.DueDate = model.DueDate;
			dbTask.Priority = model.Priority;
			dbTask.Notes = model.Notes;
			this.db.SaveChanges();
			dynamic viewBag = ViewBag;
			DbSet<AdminTask> adminTasks = this.db.AdminTasks;
			viewBag.RunningTasks = (
				from t in adminTasks
				where t.AdminId == this.User.Admin.Id && (int)t.Status == 1
				select t).ToList<AdminTask>();
			dynamic list = ViewBag;
			DbSet<AdminTask> adminTasks1 = this.db.AdminTasks;
			list.CompletedTasks = (
				from t in adminTasks1
				where t.AdminId == this.User.Admin.Id && (int)t.Status == 2
				select t).ToList<AdminTask>();
			return base.PartialView("AdminTasks");
		}

		[ActionName("update-bar")]
		[HttpPost]
		public ActionResult UpdateBar(long? adminId, string fromDate, string toDate)
		{
			return this.GetBarChartData(adminId, fromDate, toDate);
		}

		[ActionName("email-update")]
		[HttpGet]
		public ActionResult UpdateEmail()
		{
			return base.View();
		}

		[ActionName("email-update")]
		[HttpPost]
		public ActionResult UpdateEmail(UpdateEmailPasswordForm form)
		{
			ActionResult action;
			if (!ModelState.IsValid)
			{
				return base.View(form);
			}
			try
			{
				Admin admin = this.db.Admins.First<Admin>((Admin t) => t.Username == this.User.Admin.Username);
				if (admin != null)
				{
					admin.EmailPassword = CryptoUtil.Encrypt(form.NewPassword, admin.Username);
				}
				this.db.SaveChanges();
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.UpdateSuccess, new object[] { "Email password" });
				action = base.RedirectToAction("email");
			}
			catch (Exception exception)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				action = base.View();
			}
			return action;
		}

		[ActionName("task-update-status")]
		[HttpPost]
		public ActionResult UpdateTaskStatus(long taskId, byte status)
		{
			ActionResult actionResult;
			try
			{
				AdminTask adminTask = this.db.AdminTasks.SingleOrDefault<AdminTask>((AdminTask t) => t.Id == taskId);
				if (adminTask == null)
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
				}
				adminTask.Status = status;
				this.db.SaveChanges();
				dynamic viewBag = ViewBag;
				DbSet<AdminTask> adminTasks = this.db.AdminTasks;
				viewBag.RunningTasks = (
					from t in adminTasks
					where t.AdminId == this.User.Admin.Id && (int)t.Status == 1
					select t).ToList<AdminTask>();
				dynamic list = ViewBag;
				DbSet<AdminTask> adminTasks1 = this.db.AdminTasks;
				list.CompletedTasks = (
					from t in adminTasks1
					where t.AdminId == this.User.Admin.Id && (int)t.Status == 2
					select t).ToList<AdminTask>();
				actionResult = base.PartialView("AdminTasks");
			}
			catch (Exception exception)
			{
				System.Web.HttpContext.Current.Response.StatusCode = 600;
				actionResult = base.PartialView("AdminTasks");
			}
			return actionResult;
		}

		[ActionName("upload-file")]
		[HttpPost]
		public ActionResult UploadFile(string qqFile, string folderId)
		{
			ActionResult actionResult;
			try
			{
				string fileName = Path.GetFileName(base.HttpContext.Request.Files[0].FileName).ToLower();
				string timeForderId = folderId;
				if (string.IsNullOrEmpty(folderId))
				{
					timeForderId = DateTime.Now.ToString("yyyyMMddhhmmss");
				}
				string folderPath = Path.Combine(base.User.Admin.Username, timeForderId);
				string saveLocation = Path.Combine( System.Web.HttpContext.Current.Server.MapPath( "~/Upload"), folderPath);
				if (!Directory.Exists(saveLocation))
				{
					Directory.CreateDirectory(saveLocation);
				}
				base.HttpContext.Request.Files[0].SaveAs(Path.Combine(saveLocation, fileName));
				actionResult = base.Json(new { ReturnStatus = "true", FileName = fileName, FilePath = Path.Combine(folderPath, fileName), FolderId = timeForderId });
			}
			catch (Exception exception)
			{
				Exception exUpload = exception;
				actionResult = base.Json(new { ReturnStatus = "false", ErrorMessage = exUpload.Message });
			}
			return actionResult;
		}

		[ActionName("email-delete")]
		[Authorize(Roles="admin")]
		[HttpPost]
		public ActionResult UserEmailDetelte(long id)
		{
			try
			{
				AdminEmailTemplate emailTemplate = this.GetUserEmailTemplate(id);
				this.db.AdminEmailTemplates.Remove(emailTemplate);
				this.db.SaveChanges();
				TempData.Add("Info", "Email template deleted.");
			}
			catch
			{
				TempData.Add("Error", "Error occured");
			}
			return base.RedirectToAction("email");
		}

		[ActionName("template-detail")]
		[Authorize(Roles="admin")]
		[HttpGet]
		public ActionResult UserEmailTemplateEdit(long? id)
		{
			ActionResult action;
			if (id.HasValue)
			{
				long? nullable = id;
				if ((nullable.GetValueOrDefault() <= (long)0 ? false : nullable.HasValue))
				{
					try
					{
						action = base.View(this.GetUserEmailTemplate(id.Value));
					}
					catch
					{
						TempData.Add("Error", "Error orcured.");
						action = base.RedirectToAction("email", new { research = true });
					}
					return action;
				}
			}
			AdminEmailTemplate adminEmailTemplate = new AdminEmailTemplate()
			{
				AdminId = base.User.Admin.Id
			};
			return base.View(adminEmailTemplate);
		}

		[ActionName("template-detail")]
		[Authorize(Roles="admin")]
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult UserEmailTemplateEdit(AdminEmailTemplate model)
		{
			if (!ModelState.IsValid)
			{
				return base.View(model);
			}
			if (model.Id <= 0)
			{
				this.db.AdminEmailTemplates.Add(model);
			}
			else
			{
				AdminEmailTemplate editTemplate = this.GetUserEmailTemplate((long)model.Id);
				editTemplate.Name = model.Name;
				editTemplate.MailSubject = model.MailSubject;
				editTemplate.MailBody = model.MailBody;
			}
			try
			{
				this.db.SaveChanges();
			}
			catch (DbEntityValidationException dbEntityValidationException)
			{
			}
			TempData.Add("Info", (model.Id > 0 ? "Email template updated." : "Email template added."));
			return base.RedirectToAction("email");
		}
	}
}