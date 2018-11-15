using EggheadWeb.Models.AdminModels;
using EggheadWeb.Models.Common;
using EggheadWeb.Utility;
using Microsoft.CSharp.RuntimeBinder;
using OfficeOpenXml;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace EggheadWeb.Controllers
{
	public class CalendarExceLDownloadAttribute : ActionFilterAttribute
	{
		public string Filename
		{
			get;
			set;
		}

		public CalendarExceLDownloadAttribute()
		{
		}

		private static void CreateCalendarWorksheet(ResultExecutingContext filterContext, ExcelWorksheet wb)
		{
			dynamic customEvents;
			CalendarSearchForm form = (CalendarSearchForm)((dynamic)filterContext.Controller.ViewBag).CalendarSearchForm;
			Calendar calendar = (Calendar)((dynamic)filterContext.Controller.ViewBag).Calendar;
			if (form.ViewBy == CalendarViewType.Day)
			{
				ExcelRange item = wb.Cells["B2"];
				string[][] strArrays = new string[1][];
				string[] str = new string[] { "Date:", null };
				str[1] = calendar.FromDate.ToString("M/dd/yyyy");
				strArrays[0] = str;
				item.LoadFromArrays(strArrays);
				customEvents = (
					from t in calendar.Events
					select new { TimeStart = t.TimeStart.To12HoursString(), TimeEnd = t.TimeEnd.To12HoursString(), Event = t.Name, Type = t.Type.ToString() }).ToList();
			}
			else if (form.ViewBy != CalendarViewType.Week)
			{
				ExcelRange excelRange = wb.Cells["B2"];
				string[][] strArrays1 = new string[1][];
				string[] str1 = new string[] { "Month:", null };
				str1[1] = calendar.FromDate.ToString("M/yyyy");
				strArrays1[0] = str1;
				excelRange.LoadFromArrays(strArrays1);
				customEvents = (
					from t in calendar.Events
					select new { Date = t.Date.ToString("M/dd"), TimeStart = t.TimeStart.To12HoursString(), TimeEnd = t.TimeEnd.To12HoursString(), Event = t.Name, Type = t.Type.ToString() }).ToList();
			}
			else
			{
				ExcelRange item1 = wb.Cells["B2"];
				string[][] strArrays2 = new string[1][];
				string[] strArrays3 = new string[] { "Week:", null };
				string str2 = calendar.FromDate.ToString("M/dd/yyyy");
				DateTime toDate = calendar.ToDate;
				strArrays3[1] = string.Concat(str2, " - ", toDate.ToString("M/dd/yyyy"));
				strArrays2[0] = strArrays3;
				item1.LoadFromArrays(strArrays2);
				customEvents = (
					from t in calendar.Events
					select new { Date = t.Date.ToString("M/dd"), TimeStart = t.TimeStart.To12HoursString(), TimeEnd = t.TimeEnd.To12HoursString(), Event = t.Name, Type = t.Type.ToString() }).ToList();
			}
			wb.Cells["B4"].LoadFromCollection(customEvents, true);
		}

		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			using (ExcelPackage pck = new ExcelPackage())
			{
				ExcelWorksheet wb = pck.Workbook.Worksheets.Add("Events");
				CalendarExceLDownloadAttribute.CreateCalendarWorksheet(filterContext, wb);
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", this.Filename ?? (string)((dynamic)filterContext.Controller.ViewBag).ExcelFilename));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}
	}
}