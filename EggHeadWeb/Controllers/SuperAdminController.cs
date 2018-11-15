using EggheadWeb.Common;
using EggheadWeb.Mailers;
using EggheadWeb.Models.AdminModels;
using EggheadWeb.Security;
using log4net;
using Mvc.Mailer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using BCrypt.Net;
using EggHeadWeb.DatabaseContext;
using EggheadWeb.Models.Common;

namespace EggheadWeb.Controllers
{
    [Authorize(Roles="superadmin")]
	public class SuperAdminController : BaseAdminController
	{
		protected const int MIN_PASSWORDL_ENGTH = 6;

		protected const int WORK_FACTOR = 10;

		private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private IUserMailer _userMailer;

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

		public SuperAdminController() : base("superadmin")
		{
			base.LoginUrl = this.Url.RouteUrl("SuperAdmin", new { action = "" });
			this._userMailer = new EggheadWeb.Mailers.UserMailer(this.db);
		}

		[ActionName("frontend")]
		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult FrontEnd()
		{
			dynamic viewBag = ViewBag;
			DbSet<Frontend> frontends = this.db.Frontends;
			viewBag.Frontends = SearchResult.New<object, Frontend>(null, 
				from k in frontends
				orderby k.Name
				select k, 1, 2147483647);
			return base.View();
		}

		[ActionName("frontend-delete")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult FrontendDelete(long id)
		{
			try
			{
				Frontend frontend = this.db.Frontends.SingleOrDefault<Frontend>((Frontend t) => (long)t.Id == id);
				if (frontend == null)
				{
					base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DataNotFound, new object[0]);
				}
				frontend.AdminFrontends.Clear();
				this.db.Frontends.Remove(frontend);
				this.db.SaveChanges();
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Frontend" });
			}
			catch (Exception exception)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Frontend" });
			}
			return base.RedirectToAction("frontend");
		}

		[ActionName("frontend-detail")]
		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult FrontendDetail(long? id)
		{
			if (!id.HasValue)
			{
				return base.View(new Frontend());
			}
			Frontend frontend = this.db.Frontends.SingleOrDefault<Frontend>((Frontend t) => (long)t.Id == id.Value);
			return base.View(frontend);
		}

		[ActionName("frontend-detail")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult FrontendDetail(Frontend model)
		{
			if (!ModelState.IsValid)
			{
				return base.View(model);
			}
			Frontend frontend = null;
			if (model.Id <= 0)
			{
				Frontend frontend1 = new Frontend()
				{
					MenuName = model.MenuName,
					Name = model.Name,
					IsActive = model.IsActive,
					PageContent = model.PageContent
				};
				frontend = frontend1;
				this.db.Frontends.Add(frontend);
				this.db.SaveChanges();
			}
			else
			{
				frontend = this.db.Frontends.SingleOrDefault<Frontend>((Frontend t) => t.Id == model.Id);
				frontend.MenuName = model.MenuName;
				frontend.Name = model.Name;
				frontend.IsActive = model.IsActive;
				frontend.PageContent = model.PageContent;
				this.db.SaveChanges();
			}
			base.SetViewMessage(WithAuthenController.MessageType.Info, (model.Id > 0 ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "Frontend" });
			return base.RedirectToAction("frontend");
		}

		private Area GetArea(long id)
		{
			return this.db.Areas.FirstOrDefault<Area>((Area a) => a.Id == id);
		}

		private Grade GetGrade(long id)
		{
			return this.db.Grades.FirstOrDefault<Grade>((Grade g) => (long)g.Id == id);
		}

		private Admin GetUser(long id)
		{
			return this.db.Admins.FirstOrDefault<Admin>((Admin a) => a.Id == id);
		}

		[ActionName("grades")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult GradeAdd(Grade grade)
		{
			if (!ModelState.IsValid)
			{
				return base.View(grade);
			}
			DbSet<Grade> grades = this.db.Grades;
			Grade grade1 = new Grade()
			{
				Name = grade.Name
			};
			grades.Add(grade1);
			this.db.SaveChanges();
			base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.AddSuccess, new object[] { "Grade" });
			return base.RedirectToAction("grades");
		}

		[ActionName("grades-delete")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult GradeDelete(long id)
		{
			try
			{
				Grade grade = this.GetGrade(id);
				if (grade != null)
				{
					this.db.Grades.Remove(grade);
					this.db.SaveChanges();
					base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Grade" });
				}
				else
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
					return base.Content(Messages.DataNotFound);
				}
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Grade" });
			}
			return base.RedirectToAction("grades");
		}

		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult Grades()
		{
			dynamic viewBag = ViewBag;
			DbSet<Grade> grades = this.db.Grades;
			viewBag.Grades = EggheadWeb.Models.Common.SearchResult.New<Grade>(
				from g in grades
				orderby g.Id
				select g, 1, 2147483647);
			return base.View();
		}

		private EggheadWeb.Models.Common.SearchResult<AreaSearchForm, Area> PerformAreaSearch(AreaSearchForm form, int page, int? itemsPerPage = null)
		{
			IQueryable<Area> query = this.db.Areas.AsQueryable<Area>();
			if (!string.IsNullOrWhiteSpace(form.QuickSearch))
			{
				query = 
					from a in query
					where a.Name.ToLower().Contains(form.QuickSearch.Trim().ToLower()) || a.DisplayName.ToLower().Contains(form.QuickSearch.Trim().ToLower()) || a.DisplayName.ToLower().Contains(form.QuickSearch.Trim().ToLower()) || a.Description.ToLower().Contains(form.QuickSearch.Trim().ToLower()) || a.Admins.Any<Admin>((Admin t) => ((t.FirstName + " ") + t.LastName).ToLower().Contains(form.QuickSearch.Trim().ToLower()))
					select a;
			}
			query = 
				from a in query
				orderby a.Name
				select a;
			if (!itemsPerPage.HasValue)
			{
				return EggheadWeb.Models.Common.SearchResult.New<AreaSearchForm, Area>(form, query, page);
			}
			return EggheadWeb.Models.Common.SearchResult.New<AreaSearchForm, Area>(form, query, page, itemsPerPage.Value);
		}

		private EggheadWeb.Models.Common.SearchResult<UserSearchForm, Admin> PerformUserSearch(UserSearchForm form, int page)
		{
			IQueryable<Admin> query = this.db.Admins.AsQueryable<Admin>();
			if (!string.IsNullOrWhiteSpace(form.QuickSearch))
			{
				query = 
					from a in query
					where ((a.FirstName + " ") + a.LastName).ToLower().Contains(form.QuickSearch.Trim().ToLower()) || a.Username.ToLower().Contains(form.QuickSearch.Trim().ToLower())
					select a;
			}
			if (form.IsSuperAdmin.HasValue)
			{
				query = 
					from t in query
					where (bool?)t.IsSuperAdmin == form.IsSuperAdmin
					select t;
			}
			query = 
				from a in query
				orderby (a.FirstName + " ") + a.LastName
				select a;
			return SearchResult.New<UserSearchForm, Admin>(form, query, page);
		}

		[ActionName("system-template-delete")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult SystemEmailDetele(long id)
		{
			try
			{
				EmailTemplate emailTemplate = this.db.EmailTemplates.SingleOrDefault<EmailTemplate>((EmailTemplate t) => (long)t.Id == id);
				this.db.EmailTemplates.Remove(emailTemplate);
				this.db.SaveChanges();
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Template" });
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Template" });
			}
			return base.RedirectToAction("email");
		}

		[ActionName("system-template-detail")]
		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult SystemEmailEdit(long? id)
		{
			if (!id.HasValue)
			{
				return base.View(new EmailTemplate());
			}
			EmailTemplate emailTemplate = this.db.EmailTemplates.SingleOrDefault<EmailTemplate>((EmailTemplate t) => (long?)((long)t.Id) == id);
			return base.View(emailTemplate);
		}

		[ActionName("system-template-detail")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SystemEmailEdit(EmailTemplate model)
		{
			if (!ModelState.IsValid)
			{
				return base.View(model);
			}
			if (this.db.EmailTemplates.Any<EmailTemplate>((EmailTemplate t) => t.Type == model.Type && t.Id != model.Id))
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.TemplateExist, new object[0]);
				return base.RedirectToAction("email");
			}
			bool updateMode = model.Id > 0;
			if (!updateMode)
			{
				this.db.EmailTemplates.Add(model);
			}
			else
			{
				EmailTemplate editTemplate = this.db.EmailTemplates.SingleOrDefault<EmailTemplate>((EmailTemplate t) => t.Id == model.Id);
				editTemplate.Type = model.Type;
				editTemplate.Name = model.Name;
				editTemplate.MailSubject = model.MailSubject;
				editTemplate.MailBody = model.MailBody;
			}
			this.db.SaveChanges();
			base.SetViewMessage(WithAuthenController.MessageType.Info, (updateMode ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "Template" });
			return base.RedirectToAction("email");
		}

		[ActionName("system-email-setting")]
		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult SystemEmailSetting()
		{
			List<APVariable> apVariables = this.db.APVariables.ToList<APVariable>();
			SystemEmailSetting systemEmailSetting = new SystemEmailSetting()
			{
				SmtpHost = apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_HOST").Value,
				SmtpPort = Convert.ToInt32(apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_PORT").Value),
				UseSSL = Convert.ToBoolean(apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_ENABLE_SSL").Value),
				Email = apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_USER_NAME").Value,
				Password = apVariables.First<APVariable>((APVariable t) => t.Name == "SYSTEM_EMAIL_PASSWORD").Value
			};
			return base.View("System-email-setting", systemEmailSetting);
		}

		[ActionName("terms")]
		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult TermInfo()
		{
			APVariable term = this.db.APVariables.FirstOrDefault<APVariable>((APVariable t) => t.Name == "TEARM") ?? new APVariable()
			{
				Name = "TEARM"
			};
			return base.View("Terms-Detail", term);
		}

		[ActionName("terms")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult TermInfo(APVariable term)
		{
			ActionResult actionResult;
			if (!ModelState.IsValid)
			{
				return base.View(term.Name);
			}
			try
			{
				APVariable dbTerm = this.db.APVariables.FirstOrDefault<APVariable>((APVariable t) => t.Name == "TEARM");
				if (dbTerm != null)
				{
					base.TryUpdateModel<APVariable>(dbTerm);
				}
				else
				{
					this.db.APVariables.Add(term);
				}
				this.db.SaveChanges();
				base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.UpdateSuccess, new object[] { "Terms & Condition" });
				return base.RedirectToAction("frontend");
			}
			catch (DbEntityValidationException dbEntityValidationException)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				actionResult = View(term.Name);
			}
			return actionResult;
		}

		[ActionName("territories")]
		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult Territories(AreaSearchForm form, int? page, bool? research)
		{
			if (!research.HasValue || !research.Value || base.GetSession("Areas.SearchForm") == null)
			{
				base.SetSession("Areas.SearchForm", form);
			}
			AreaSearchForm session = (AreaSearchForm)base.GetSession("Areas.SearchForm");
			int? nullable = page;
			SearchResult<AreaSearchForm, Area> territories = this.PerformAreaSearch(session, (nullable.HasValue ? nullable.GetValueOrDefault() : 1), null);
			if (territories.TotalItems != 0)
			{
				((dynamic)ViewBag).Territories = territories;
			}
			else
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.NoRecordFound, new object[] { "territory" });
			}
			return base.View();
		}

		[ActionName("territories-excel")]
		[ExcelDownload]
		[HttpGet]
		public ActionResult TerritoriesExcel()
		{
			AreaSearchForm sessionForm = (AreaSearchForm)base.GetSession("Areas.SearchForm") ?? new AreaSearchForm();
			((dynamic)ViewBag).FileName = "Territories.xlsx";
			((dynamic)ViewBag).SheetName = "Territories";
			((dynamic)ViewBag).Headers = new List<string[]>();
			dynamic viewBag = ((dynamic)ViewBag).Headers;
			string[] strArrays = new string[] { "Territory Name", "URL Name", "State", "Administrator", "Description" };
			viewBag.Add(strArrays);
			dynamic list = ViewBag;
			var pageItems = 
				from t in this.PerformAreaSearch(sessionForm, 1, new int?(2147483647)).PageItems
				select new { Name = t.DisplayName, URLName = t.Name, State = t.State, Administrator = (t.Admins.Count > 0 ? StringUtil.GetFullName(t.Admins.First<Admin>().FirstName, t.Admins.First<Admin>().LastName) : string.Empty), Description = t.Description };
			list.ExportData = (
				from t in pageItems
				orderby t.Name
				select t).ToList();
			return new EmptyResult();
		}

		[ActionName("territories-delete")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult TerritoryDelete(long id)
		{
			try
			{
				Area area = this.GetArea(id);
				if (area != null)
				{
					area.Admins.Clear();
					foreach (Instructor instructor in area.Instructors.ToList<Instructor>())
					{
						this.db.Instructors.Remove(instructor);
					}
					foreach (Location location in area.Locations.ToList<Location>())
					{
						this.db.Locations.Remove(location);
					}
					this.db.Areas.Remove(area);
					this.db.SaveChanges();
					base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "Territory" });
				}
				else
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
					return base.Content(Messages.DataNotFound);
				}
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "Territory" });
			}
			return base.RedirectToAction("territories", new { research = "true" });
		}

		[ActionName("territories-detail")]
		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult TerritoryDetail(long? id)
		{
			if (!id.HasValue)
			{
				return base.View(new Area());
			}
			Area area = this.GetArea(id.Value);
			if (area != null)
			{
				return base.View(area);
			}
			base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DataNotFound, new object[0]);
			return base.RedirectToAction("territories", new { research = "true" });
		}

		[ActionName("territories-detail")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult TerritoryDetail(Area model)
		{
			ActionResult action;
			if (!ModelState.IsValid)
			{
				return base.View(model);
			}
			try
			{
				bool updateMode = model.Id > (long)0;
				if (!this.db.Areas.Any<Area>((Area t) => t.Id != model.Id && (t.Name == model.Name)))
				{
					if (!updateMode)
					{
						this.db.Areas.Add(model);
					}
					else
					{
						Area dbArea = this.GetArea(model.Id);
						dbArea.Description = model.Description;
						dbArea.DisplayName = model.DisplayName;
						dbArea.Name = model.Name;
						dbArea.State = model.State;
					}
					this.db.SaveChanges();
					base.SetViewMessage(WithAuthenController.MessageType.Info, (updateMode ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "Territory" });
					action = base.RedirectToAction("territories", new { research = "true" });
				}
				else
				{
					base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AreaURLNameDuplicate, new object[0]);
					action = base.View(model);
				}
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				action = base.View(model);
			}
			return action;
		}

		[ActionName("Users-delete")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult UserDelete(long id)
		{
			try
			{
				Admin user = this.GetUser(id);
				if (user != null)
				{
					foreach (AdminTask task in user.AdminTasks.ToList<AdminTask>())
					{
						this.db.AdminTasks.Remove(task);
					}
					foreach (AdminFrontend frontend in user.AdminFrontends.ToList<AdminFrontend>())
					{
						this.db.AdminFrontends.Remove(frontend);
					}
					foreach (AdminEmailTemplate template in user.AdminEmailTemplates.ToList<AdminEmailTemplate>())
					{
						this.db.AdminEmailTemplates.Remove(template);
					}
					foreach (AdminPaymentInfo paymentInfo in user.AdminPaymentInfoes.ToList<AdminPaymentInfo>())
					{
						this.db.AdminPaymentInfoes.Remove(paymentInfo);
					}
					user.Coupons.Clear();
					user.PrivateMessages.Clear();
					user.PrivateMessages1.Clear();
					this.db.Admins.Remove(user);
					this.db.SaveChanges();
					base.SetViewMessage(WithAuthenController.MessageType.Info, Messages.DeleteSuccess, new object[] { "User" });
				}
				else
				{
					System.Web.HttpContext.Current.Response.StatusCode = 600;
					return base.Content(Messages.DataNotFound);
				}
			}
			catch (Exception exception)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DeleteFailByForeignKey, new object[] { "User" });
			}
			return base.RedirectToAction("users", new { research = "true" });
		}

		[ActionName("Users-edit")]
		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult UserEdit(long? id)
		{
			if (!id.HasValue && base.User.Admin.IsSuperAdmin)
			{
				return base.View(new Admin());
			}
			if (!id.HasValue)
			{
				return base.RedirectToAction("users-edit", new { id = base.User.Admin.Id });
			}
			Admin user = this.GetUser(id.Value);
			if (user == null && base.User.Admin.IsSuperAdmin)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DataNotFound, new object[0]);
				return base.RedirectToAction("users", new { research = "true" });
			}
			if (user == null)
			{
				return base.RedirectToAction("users-edit", new { id = base.User.Admin.Id });
			}
			if (base.User.Admin.IsSuperAdmin || user.Id == base.User.Admin.Id)
			{
				return base.View(user);
			}
			return base.RedirectToAction("users-edit", new { id = base.User.Admin.Id });
		}

		[ActionName("Users-edit")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult UserEdit(Admin user)
		{
			ActionResult action;
			if (!ModelState.IsValid)
			{
				return base.View(user);
			}
			try
			{
				bool updateMode = user.Id > (long)0;
				Admin dbAdmin = null;
				bool userNameChanged = false;
				if (!updateMode)
				{
					if (user.AreaId.HasValue)
					{
						if (this.db.Areas.First(t => (long?)t.Id == user.AreaId).Admins.Count > 0)
						{
							base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.TerritoryDuplicate, new object[0]);
							action = base.View(user);
							return action;
						}
					}
					if (db.Admins.Any(t => t.Id != user.Id && (t.Username == user.Username)))
					{
						string adminUsernameDuplicate = Messages.AdminUsernameDuplicate;
						object[] username = new object[] { user.Username };
						base.SetViewMessage(WithAuthenController.MessageType.Error, adminUsernameDuplicate, username);
						action = base.View(user);
						return action;
					}
					else if (!this.db.Admins.Any(t => t.Email == user.Email))
					{
						Admin admin = new Admin()
						{
							Password = BCrypt.Net.BCrypt.HashPassword(user.Password, 10),
							EmailPassword = CryptoUtil.Encrypt(user.EmailPassword.Trim(), user.Username)
						};
						dbAdmin = admin;
						this.db.Admins.Add(dbAdmin);
					}
					else
					{
						base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminEmailDuplicate, new object[0]);
						action = base.View(user);
						return action;
					}
				}
				else
				{
					dbAdmin = this.GetUser(user.Id);
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
						string str = Messages.AdminUsernameDuplicate;
						object[] objArray = new object[] { user.Username };
						base.SetViewMessage(WithAuthenController.MessageType.Error, str, objArray);
						action = base.View(user);
						return action;
					}
					else if (!this.db.Admins.Any<Admin>((Admin t) => t.Id != user.Id && (t.Email == user.Email)))
					{
						if (!string.IsNullOrWhiteSpace(user.EmailPassword))
						{
							dbAdmin.EmailPassword = CryptoUtil.Encrypt(user.EmailPassword.Trim(), user.Username);
						}
						else if (userNameChanged)
						{
							string oldPassword = CryptoUtil.Decrypt(dbAdmin.EmailPassword, dbAdmin.Username);
							dbAdmin.EmailPassword = CryptoUtil.Encrypt(oldPassword, user.Username);
						}
						if (!string.IsNullOrWhiteSpace(user.Password))
						{
							dbAdmin.Password = global::BCrypt.Net.BCrypt.HashPassword( user.Password, 10);
						}
					}
					else
					{
						base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.AdminEmailDuplicate, new object[0]);
						action = base.View(user);
						return action;
					}
				}
				dbAdmin.FirstName = user.FirstName;
				dbAdmin.LastName = user.LastName;
				dbAdmin.Address = user.Address;
				dbAdmin.AreaId = user.AreaId;
				dbAdmin.City = user.City;
				dbAdmin.Email = user.Email;
				dbAdmin.PhoneNumber = user.PhoneNumber;
				dbAdmin.State = user.State;
				dbAdmin.Username = user.Username;
				dbAdmin.Zip = user.Zip;
				this.db.SaveChanges();
				base.SetViewMessage(WithAuthenController.MessageType.Info, (updateMode ? Messages.UpdateSuccess : Messages.AddSuccess), new object[] { "User" });
				action = base.RedirectToAction("users", new { research = "true" });
			}
			catch
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.ErrorOccured, new object[0]);
				action = base.View(user);
			}
			return action;
		}

		[ActionName("Users-ResetPass")]
		[Authorize(Roles="superadmin")]
		[HttpGet]
		public ActionResult UserResetPass(long? id)
		{
			if (!id.HasValue && base.User.Admin.IsSuperAdmin)
			{
				return base.View(new Admin());
			}
			if (!id.HasValue)
			{
				return base.RedirectToAction("users-resetpass", new { id = base.User.Admin.Id });
			}
			Admin user = this.GetUser(id.Value);
			if (user == null && base.User.Admin.IsSuperAdmin)
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.DataNotFound, new object[0]);
				return base.RedirectToAction("users", new { research = "true" });
			}
			if (user == null)
			{
				return base.RedirectToAction("users-resetpass", new { id = base.User.Admin.Id });
			}
			if (base.User.Admin.IsSuperAdmin || user.Id == base.User.Admin.Id)
			{
				return base.View(user);
			}
			return base.RedirectToAction("users-resetpass", new { id = base.User.Admin.Id });
		}

		[ActionName("Users-ResetPass")]
		[Authorize(Roles="superadmin")]
		[HttpPost]
		public ActionResult UserResetPass(Admin user)
		{
			Admin userAdmin = (
				from r in this.db.Admins
				where r.Id == user.Id
				select r).FirstOrDefault<Admin>();
			userAdmin.Password = global::BCrypt.Net.BCrypt.HashPassword(user.Password, 10);
			this.db.SaveChanges();
			try
			{
				this.UserMailer.UserAdminPasswordReset(userAdmin, user.Password).Send(new SmtpClientWrapper(EmailUtil.GetSystemSmtpClient(this.db)));
			}
			catch (Exception exception)
			{
				this.log.Error(exception);
			}
			((dynamic)ViewBag).Message = Messages.ResetPasswordSuccess;
			return base.View(userAdmin);
		}

		[HttpGet]
		public ActionResult Users(UserSearchForm form, int? page, bool? research)
		{
			if (!research.HasValue || !research.Value || base.GetSession("Users.SearchForm") == null)
			{
				base.SetSession("Users.SearchForm", form);
			}
			UserSearchForm session = (UserSearchForm)base.GetSession("Users.SearchForm");
			int? nullable = page;
			SearchResult<UserSearchForm, Admin> users = this.PerformUserSearch(session, (nullable.HasValue ? nullable.GetValueOrDefault() : 1));
			if (users.TotalItems != 0)
			{
				((dynamic)ViewBag).Users = users;
			}
			else
			{
				base.SetViewMessage(WithAuthenController.MessageType.Error, Messages.NoRecordFound, new object[] { "user" });
			}
			return base.View();
		}
	}
}