using Microsoft.CSharp.RuntimeBinder;
using OfficeOpenXml;
using System;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace EggheadWeb.Controllers
{
	public class ScheduleExcelDownloadAttribute : ActionFilterAttribute
	{
		public string Filename
		{
			get;
			set;
		}

		public ScheduleExcelDownloadAttribute()
		{
		}

		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			using (ExcelPackage pck = new ExcelPackage())
			{
				ExcelWorksheet classWs = pck.Workbook.Worksheets.Add("Classes");
				ExcelWorksheet campWs = pck.Workbook.Worksheets.Add("Camps");
				ExcelWorksheet birthdayWs = pck.Workbook.Worksheets.Add("Birthdays");
				ExcelWorksheet workshops = pck.Workbook.Worksheets.Add("Workshops");
				ExcelRange item = classWs.Cells["B2"];
				string[][] strArrays = new string[1][];
				string[] strArrays1 = new string[] { "Location", "Class", "Time(S)", "Time(E)", "Instructor", "$", "Grades", "Enroll", "Dates" };
				strArrays[0] = strArrays1;
				item.LoadFromArrays(strArrays);
				if (((dynamic)filterContext.Controller).Classes.Count > 0)
				{
					classWs.Cells["B3"].LoadFromCollection(((dynamic)filterContext.Controller.ViewBag).Classes, false);
				}
				ExcelRange excelRange = campWs.Cells["B2"];
				string[][] strArrays2 = new string[1][];
				string[] strArrays3 = new string[] { "Location", "Class", "Time(S)", "Time(E)", "Instructor", "$", "Grades", "Enroll", "Dates" };
				strArrays2[0] = strArrays3;
				excelRange.LoadFromArrays(strArrays2);
				if (((dynamic)filterContext.Controller.ViewBag()).Camps.Count > 0)
				{
					campWs.Cells["B3"].LoadFromCollection(((dynamic)filterContext.Controller.ViewBag).Camps, false);
				}
				ExcelRange item1 = birthdayWs.Cells["B2"];
				string[][] strArrays4 = new string[1][];
				string[] strArrays5 = new string[] { "Parent", "Contact Number", "Email", "Address", "Child", "Age", "Party Date", "Time" };
				strArrays4[0] = strArrays5;
				item1.LoadFromArrays(strArrays4);
				if (((dynamic)filterContext.Controller.ViewBag).Birthdays.Count > 0)
				{
					birthdayWs.Cells["B3"].LoadFromCollection(((dynamic)filterContext.Controller.ViewBag).Birthdays, false);
				}
				ExcelRange excelRange1 = workshops.Cells["B2"];
				string[][] strArrays6 = new string[1][];
				string[] strArrays7 = new string[] { "Location", "Workshop", "Time(S)", "Time(E)", "Instructor", "$", "Grades", "Dates" };
				strArrays6[0] = strArrays7;
				excelRange1.LoadFromArrays(strArrays6);
				if (((dynamic)filterContext.Controller.ViewBag).Workshops.Count > 0)
				{
					workshops.Cells["B3"].LoadFromCollection(((dynamic)filterContext.Controller.ViewBag).Workshops, false);
				}
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", this.Filename ?? (string)((dynamic)filterContext.Controller.ViewBag).ExcelFilename));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}
	}
}