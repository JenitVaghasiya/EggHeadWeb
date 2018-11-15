using EggheadWeb.Models.AdminModels;
using EggheadWeb.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace EggheadWeb.Models.Common
{
	public static class SelectLists
	{
		public static SelectList AdminEmailTemplates(EggheadEntities db)
		{
			IQueryable<AdminEmailTemplate> query = 
				from l in db.AdminEmailTemplates
				where l.AdminId == UserProvider.Current.Admin.Id
				select l;
			query = 
				from l in query
				orderby l.Name
				select l;
			return new SelectList(
				from l in query
				select new { Value = l.Id, Text = l.Name }, "Value", "Text");
		}

		public static SelectList Admins(EggheadEntities db)
		{
			return new SelectList(
				from t in db.Admins
				where !t.IsSuperAdmin
				select t into a
				select new { Value = a.Id, Text = (a.FirstName + " ") + a.LastName }, "Value", "Text");
		}

		public static SelectList AdminsForBarChars(EggheadEntities db)
		{
			SelectList select = new SelectList(
				from t in db.Admins
				where !t.IsSuperAdmin
				select t into a
				select new { Value = a.Id, Text = (a.FirstName + " ") + a.LastName }, "Value", "Text");
			List<SelectListItem> list = select.ToList<SelectListItem>();
			SelectListItem selectListItem = new SelectListItem();
			selectListItem.Value = "-1";
			selectListItem.Text = "Total";
			list.Insert(0, selectListItem);
			return new SelectList(list, "Value", "Text");
		}

		public static SelectList AdminsForSendMessage(EggheadEntities db)
		{
			long id = (HttpContext.Current.User as LoginUser).Admin.Id;
			IOrderedQueryable<Admin> query = 
				from t in db.Admins
				where t.Id != id
				select t into l
				orderby l.FirstName
				select l;
			return new SelectList(
				from l in query
				select new { Value = l.Id, Text = (l.FirstName + " ") + l.LastName }, "Value", "Text");
		}

		public static SelectList Areas(EggheadEntities db)
		{
			return new SelectList(
				from a in db.Areas
				select new { Value = a.Id, Text = a.DisplayName }, "Value", "Text");
		}

		public static SelectList BooleanStatus()
		{
			object[] variable = new object[] { new { Value = true, Text = "Yes" }, new { Value = false, Text = "No" } };
			return new SelectList(variable, "Value", "Text");
		}

		public static SelectList CalendarViewTypes()
		{
			object[] variable = new object[] { new { Value = CalendarViewType.Day, Text = "Day" }, new { Value = CalendarViewType.Week, Text = "Week" }, new { Value = CalendarViewType.Month, Text = "Month" } };
			return new SelectList(variable, "Value", "Text");
		}

		public static SelectList CouponTypes()
		{
			object[] variable = new object[] { new { Value = 1, Text = "Fixed $ Amount" }, new { Value = 2, Text = "Percentage % Discount" } };
			return new SelectList(variable, "Value", "Text");
		}

		public static SelectList DaysOfWeek()
		{
			object[] objArray = new object[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
			return new SelectList(objArray);
		}

		public static SelectList FrontendMenus(EggheadEntities db)
		{
			IQueryable<Frontend> query = 
				from i in db.Frontends
				where i.IsActive
				select i;
			return new SelectList(
				from i in query
				select new { Value = i.Id, Text = i.MenuName }, "Value", "Text");
		}

		public static SelectList Genders()
		{
			object[] variable = new object[] { new { Value = "M", Text = "Male" }, new { Value = "F", Text = "Female" } };
			return new SelectList(variable, "Value", "Text");
		}

		public static SelectList Grades(EggheadEntities db)
		{
			return new SelectList(
				from g in db.Grades
				orderby g.Id
				select new { Value = g.Id, Text = g.Name }, "Value", "Text");
		}

		public static SelectList Instructors(EggheadEntities db, long? id = null)
		{
			IQueryable<Instructor> query = 
				from i in db.Instructors
				where i.IsActive
				select i;
			if (!UserProvider.Current.Admin.IsSuperAdmin)
			{
				query = 
					from i in query
					where (long?)i.AreaId == UserProvider.Current.Admin.AreaId
					select i;
			}
			query = 
				from i in query
				orderby (i.FirstName + " ") + i.LastName
				select i;
			if (!id.HasValue)
			{
				return new SelectList(
					from i in query
					select new { Value = i.Id, Text = (i.FirstName + " ") + i.LastName }, "Value", "Text", null);
			}
			return new SelectList(
				from i in query
				select new { Value = i.Id, Text = (i.FirstName + " ") + i.LastName }, "Value", "Text", (object)id.Value);
		}

		public static SelectList ItemsPerPages(EggheadEntities db)
		{
			APVariable itemPerPages = db.APVariables.FirstOrDefault<APVariable>((APVariable t) => t.Name == "SEARCH_RESULT_ITEMS_PER_PAGE");
			if (itemPerPages == null)
			{
				object[] variable = new object[] { new { Value = 10, Text = "10" }, new { Value = 20, Text = "20" }, new { Value = 50, Text = "50" }, new { Value = 2147483647, Text = "All" } };
				return new SelectList(variable, "Value", "Text");
			}
			string value = itemPerPages.Value;
			char[] chrArray = new char[] { ',' };
			return new SelectList(
				from i in value.Split(chrArray)
				select new { Value = (i == "All" ? 2147483647 : Convert.ToInt32(i)), Text = i }, "Value", "Text");
		}

		public static SelectList Locations(EggheadEntities db, bool? enrollable = null)
		{
			IQueryable<Location> query = 
				from l in db.Locations
				where l.IsActive
				select l;
			if (!UserProvider.Current.Admin.IsSuperAdmin)
			{
				query = 
					from l in query
					where (long?)l.AreaId == UserProvider.Current.Admin.AreaId
					select l;
			}
			if (enrollable.HasValue)
			{
				query = 
					from l in query
					where l.CanRegistOnline == enrollable.Value
					select l;
			}
			query = 
				from l in query
				orderby l.Name
				select l;
			return new SelectList(
				from l in query
				select new { Value = l.Id, Text = l.DisplayName }, "Value", "Text");
		}

		public static SelectList LocationsForFrontEnd(EggheadEntities db, long areaId)
		{
			IQueryable<Location> query = 
				from l in db.Locations
				where l.IsActive && l.CanRegistOnline && l.AreaId == areaId
				select l;
			query = 
				from l in query
				orderby l.Name
				select l;
			return new SelectList(
				from l in query
				select new { Value = l.Id, Text = l.Name }, "Value", "Text");
		}

		public static SelectList ServiceTypes()
		{
			object[] variable = new object[] { new { Value = ServiceType.Class, Text = "Class" }, new { Value = ServiceType.Camp, Text = "Camp" }, new { Value = ServiceType.Birthday, Text = "Birthday" }, new { Value = ServiceType.Workshop, Text = "Workshop" } };
			return new SelectList(variable, "Value", "Text");
		}

		public static SelectList SystemEmailTemplateTypes()
		{
			object[] variable = new object[] { new { Value = 1, Text = "New parent notification" }, new { Value = 2, Text = "Parent password reset notification" }, new { Value = 3, Text = "Enroll class notification" } };
			return new SelectList(variable, "Value", "Text");
		}

		public static SelectList TaksPriorities()
		{
			object[] variable = new object[] { new { Value = 1, Text = "Low" }, new { Value = 2, Text = "Medium" }, new { Value = 3, Text = "High" } };
			return new SelectList(variable, "Value", "Text");
		}

		public static SelectList TimeSufixes()
		{
			object[] variable = new object[] { new { Value = "AM", Text = "AM" }, new { Value = "PM", Text = "PM" } };
			return new SelectList(variable, "Value", "Text");
		}

		public static SelectList UnassignedAreas(EggheadEntities db)
		{
			return new SelectList(
				from a in db.Areas
				where a.Admins.Count<Admin>() == 0
				select new { Value = a.Id, Text = a.DisplayName }, "Value", "Text");
		}
	}
}