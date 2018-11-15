using EggheadWeb.Common;
using EggheadWeb.Models.AdminModels;
using EggheadWeb.Models.Common;
using EggheadWeb.Utility;
using Microsoft.CSharp.RuntimeBinder;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EggheadWeb.Controllers
{
	public class ExcelDownloadAttribute : ActionFilterAttribute
	{
		public ExcelDownloadAttribute()
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

		private void CreateRoseterDetailWorksheet(ExcelPackage packate, RosterDetailPdf rosterData)
		{
			ExcelWorksheet ws = packate.Workbook.Worksheets.Add(rosterData.ExportFileName);
			List<string[]> headers = new List<string[]>();
			string[] strArrays = new string[] { string.Format("LOCATION: {0}", rosterData.LocationName), string.Empty, string.Empty, string.Format("DAY: {0}", rosterData.Days), string.Empty, string.Empty, string.Empty, string.Format("TIME: {0}", rosterData.Time), string.Empty, string.Empty, string.Format("PRINTED : {0}", rosterData.Printed), string.Empty, string.Empty };
			headers.Add(strArrays);
			string[] strArrays1 = new string[] { string.Format("ADDRESS: {0}", rosterData.Address), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Format("{0}: {1}", rosterData.Type.ToString().ToUpper(), rosterData.ClassName), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
			headers.Add(strArrays1);
			headers.Add(new string[] { "" });
			List<string> rosterHeader = new List<string>();
			rosterHeader.AddRange(new string[] { "Name/Age/Gender", "PARENT" });
			rosterHeader.AddRange(rosterData.Dates);
			for (int i = 0; i < 11 - (int)rosterData.Dates.Length; i++)
			{
				rosterHeader.Add("");
			}
			headers.Add(rosterHeader.ToArray());
			int num2 = 0;
			var data = rosterData.Bookings.Select((Booking t) => {
				int num = num2 + 1;
				int num1 = num;
				num2 = num;
				return new { No = num1, StudentInfo = string.Format("{0} {1} / {2} / {3}", new object[] { t.Student.FirstName, t.Student.LastName, MisUtil.CalAge(t.Student.BirthDate), t.Student.GenderText[0] }), Parent = string.Concat(t.Student.Parent.FirstName, " ", t.Student.Parent.LastName) };
			}).ToList();
			if (data.Count < 16)
			{
				int maxBooking = data.Count;
				for (int index = 1; index <= 16 - maxBooking; index++)
				{
					data.Add(new { No = maxBooking + index, StudentInfo = "", Parent = "" });
				}
			}
			ws.Cells["H2:N2"].Merge = true;
			ws.Cells["H2:N2"].Style.Font.Size = 18f;
			ws.Cells["H2:N2"].Style.Font.Bold = true;
			ws.Cells["H2:N2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
			ws.Cells["H2:N2"].LoadFromText("EGGHEAD ATTENDANCE SHEET");
			ws.Cells["B5:D5"].Merge = true;
			ws.Cells["E5:H5"].Merge = true;
			ws.Cells["I5:K5"].Merge = true;
			ws.Cells["L5:N5"].Merge = true;
			ws.Cells["B5"].LoadFromArrays(headers);
			int headerLengh = headers.Max<string[]>((string[] t) => (int)t.Length);
			ws.Cells["B6:H6"].Merge = true;
			ws.Cells["I6:N6"].Merge = true;
			ws.Cells["B5:N6"].Style.Font.Bold = true;
			ws.Cells["B5:N6"].Style.Border.Left.Style = ExcelBorderStyle.Dashed;
			ws.Cells["B5:N6"].Style.Border.Top.Style = ExcelBorderStyle.Dashed;
			ws.Cells["B5:N6"].Style.Border.Right.Style = ExcelBorderStyle.Dashed;
			ws.Cells["B5:N6"].Style.Border.Bottom.Style = ExcelBorderStyle.Dashed;
			string.Format("B{0}", headers.Count + 4);
			string nextCellFrom = string.Format("A{0}", headers.Count + 5);
			if (data.Count > 0)
			{
				ws.Cells[nextCellFrom].LoadFromCollection(data, false);
			}
			ws.Cells[headers.Count + 4, 2, headers.Count + 4, 3].Style.Font.Bold = true;
			ws.Cells[headers.Count + 4, 2, headers.Count + 4, headerLengh + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
			ws.Cells[headers.Count + 4, 2, headers.Count + 4, headerLengh + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
			ws.Cells[headers.Count + 4, 2, headers.Count + 4 + data.Count, headerLengh + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
			ws.Cells[headers.Count + 4, 2, headers.Count + 4 + data.Count, headerLengh + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
			ws.Cells[headers.Count + 4, 2, headers.Count + 4 + data.Count, headerLengh + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
			ws.Cells[headers.Count + 4, 2, headers.Count + 4 + data.Count, headerLengh + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
			string noteCellsFrom = string.Format("B{0}", headers.Count + data.Count + 7);
			string noteCells = string.Format("B{0}:I{1}", headers.Count + data.Count + 7, headers.Count + data.Count + 11);
			ws.Cells[noteCells].Merge = true;
			ws.Cells[noteCells].Style.Font.Bold = true;
			ws.Cells[noteCellsFrom].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
			ws.Cells[noteCellsFrom].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
			ws.Cells[noteCells].Value = "STUDENT NOTES:";
			ws.Cells[noteCells].Style.Border.Left.Style = ExcelBorderStyle.Thin;
			ws.Cells[noteCells].Style.Border.Top.Style = ExcelBorderStyle.Thin;
			ws.Cells[noteCells].Style.Border.Right.Style = ExcelBorderStyle.Thin;
			ws.Cells[noteCells].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
			List<Booking> bookingWithNotes = (
				from t in rosterData.Bookings
				where !string.IsNullOrEmpty(t.Student.Notes)
				select t).ToList<Booking>();
			StringBuilder notes = new StringBuilder();
			for (int i = 0; i < bookingWithNotes.Count<Booking>(); i += 2)
			{
				notes.AppendLine();
				notes.AppendFormat("{0}: {1}", StringUtil.GetFullName(bookingWithNotes[i].Student.FirstName, bookingWithNotes[i].Student.LastName), bookingWithNotes[i].Student.Notes);
				if (i + 1 >= bookingWithNotes.Count<Booking>())
				{
					notes.Append(string.Empty);
				}
				else
				{
					notes.Append(new string(' ', 3));
					notes.AppendFormat("{0}: {1}", StringUtil.GetFullName(bookingWithNotes[i + 1].Student.FirstName, bookingWithNotes[i + 1].Student.LastName), bookingWithNotes[i + 1].Student.Notes);
				}
			}
			ExcelRichText ec = ws.Cells[noteCells].RichText.Add(notes.ToString());
			ec.Bold = false;
			string logoPath = Path.Combine( System.Web.HttpContext.Current.Server.MapPath( "~/Content/images"), "logo_small.png");
			Bitmap mb = new Bitmap(Image.FromFile(logoPath));
			ExcelPicture picture = null;
			if (mb != null)
			{
				picture = ws.Drawings.AddPicture("logo", ExcelDownloadAttribute.RotateImg(mb, 16f, Color.Transparent));
				picture.From.Column = 9;
				picture.From.Row = headers.Count + data.Count + 5;
				picture.SetSize(176, 147);
			}
		}

		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			string actionName = (string)filterContext.RouteData.Values["Action"];
			List<string[]> strArrays = new List<string[]>();
			string str = actionName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "territories-excel":
					{
						this.WriteSingleSheetExcelFile(filterContext);
						return;
					}
					case "locations-excel":
					{
						this.WriteSingleSheetExcelFile(filterContext);
						return;
					}
					case "instructors-excel":
					{
						this.WriteSingleSheetExcelFile(filterContext);
						return;
					}
					case "parents-excel":
					{
						this.WriteSingleSheetExcelFile(filterContext);
						return;
					}
					case "coupons-excel":
					{
						this.WriteSingleSheetExcelFile(filterContext);
						return;
					}
					case "payroll-excel":
					{
						this.WriteSingleSheetExcelFile(filterContext);
						return;
					}
					case "rosters-excel":
					{
						this.WriteRosterExcelFile(filterContext, "Rosters");
						return;
					}
					case "roster-detail-excel":
					{
						this.WriteRosterDetailSheetExcelFile(filterContext);
						return;
					}
					case "location-detail-excel":
					{
						this.WriteLocationDetailExcelFile(filterContext);
						return;
					}
					case "instructor-detail-excel":
					{
						this.WriteInstructorDetailExcelFile(filterContext);
						return;
					}
					case "parent-detail-excel":
					{
						this.WriteParentDetailExcelFile(filterContext);
						return;
					}
					case "class-detail-excel":
					case "camp-detail-excel":
					{
						this.WriteClassCampDetailExcelFile(filterContext);
						return;
					}
					case "birthday-detail-excel":
					{
						this.WriteBirthdayDetailExcelFile(filterContext);
						return;
					}
					case "workshop-detail-excel":
					{
						this.WriteWorkshopDetailExcelFile(filterContext);
						break;
					}
					default:
					{
						return;
					}
				}
			}
		}

		public static Bitmap RotateImg(Bitmap bmp, float angle, Color bkColor)
		{
			int w = bmp.Width;
			int h = bmp.Height;
			PixelFormat pf = PixelFormat.Undefined;
			pf = (bkColor != Color.Transparent ? bmp.PixelFormat : PixelFormat.Format32bppArgb);
			Bitmap tempImg = new Bitmap(w, h, pf);
			Graphics g = Graphics.FromImage(tempImg);
			g.Clear(bkColor);
			g.DrawImageUnscaled(bmp, 1, 1);
			g.Dispose();
			GraphicsPath path = new GraphicsPath();
			path.AddRectangle(new RectangleF(0f, 0f, (float)w, (float)h));
			Matrix mtrx = new Matrix();
			mtrx.Rotate(angle);
			RectangleF rct = path.GetBounds(mtrx);
			Bitmap newImg = new Bitmap(Convert.ToInt32(rct.Width), Convert.ToInt32(rct.Height), pf);
			g = Graphics.FromImage(newImg);
			g.Clear(bkColor);
			g.TranslateTransform(-rct.X, -rct.Y);
			g.RotateTransform(angle);
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			g.DrawImageUnscaled(tempImg, 0, 0);
			g.Dispose();
			tempImg.Dispose();
			return newImg;
		}

		private void WriteBirthdayDetailExcelFile(ResultExecutingContext filterContext)
		{
			string fileName = ((dynamic)filterContext.Controller.ViewBag).FileName as string;
			using (ExcelPackage pck = new ExcelPackage())
			{
				ExcelWorksheet detailWs = pck.Workbook.Worksheets.Add("Detail");
				pck.Workbook.Worksheets.Add("Schedule");
				List<string[]> headers = new List<string[]>();
				string[] strArrays = new string[] { "Parent Name", "Contact Number", "Email", "Address", "Child Name", "Age", "Party Date", "Time", "Instructor", "Assistant", "Notes" };
				headers.Add(strArrays);
				detailWs.Cells["B2"].LoadFromArrays(headers);
				dynamic detail = ((dynamic)filterContext.Controller.ViewBag).Detail;
				dynamic obj = detail != (dynamic)null;
				if ((!obj ? obj : obj & detail.Count > 0))
				{
					detailWs.Cells["B3"].LoadFromArrays(detail);
				}
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", fileName));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}

		private void WriteClassCampDetailExcelFile(ResultExecutingContext filterContext)
		{
			string fileName = ((dynamic)filterContext.Controller.ViewBag).FileName as string;
			using (ExcelPackage pck = new ExcelPackage())
			{
				ExcelWorksheet detailWs = pck.Workbook.Worksheets.Add("Detail");
				ExcelWorksheet scheduleWs = pck.Workbook.Worksheets.Add("Schedule");
				List<string[]> headers = new List<string[]>();
				string[] strArrays = new string[] { "Location", "Name", "Online Name", "Grades", "Time Start", "Time End", "Dates", "Instructor", "Assistant", "Online Registration", "Open To All", "Display Until", "Max Enroll #", "Total Cost", "Online Desicription", "Notes" };
				headers.Add(strArrays);
				detailWs.Cells["B2"].LoadFromArrays(headers);
				dynamic detail = ((dynamic)filterContext.Controller.ViewBag).Detail;
				dynamic obj = detail != (dynamic)null;
				if ((!obj ? obj : obj & detail.Count > 0))
				{
					detailWs.Cells["B3"].LoadFromArrays(detail);
				}
				ExcelDownloadAttribute.CreateCalendarWorksheet(filterContext, scheduleWs);
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", fileName));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}

		private void WriteInstructorDetailExcelFile(ResultExecutingContext filterContext)
		{
			string fileName = ((dynamic)filterContext.Controller.ViewBag).FileName as string;
			using (ExcelPackage pck = new ExcelPackage())
			{
				ExcelWorksheet detailWs = pck.Workbook.Worksheets.Add("Detail");
				ExcelWorksheet scheduleWs = pck.Workbook.Worksheets.Add("Schedule");
				List<string[]> headers = new List<string[]>();
				string[] strArrays = new string[] { "First Name", "Last Name", "Phone Number", "Pay", "Address", "City", "State", "Zip", "Email", "Notes", "Active" };
				headers.Add(strArrays);
				detailWs.Cells["B2"].LoadFromArrays(headers);
				dynamic detail = ((dynamic)filterContext.Controller.ViewBag).Detail;
				dynamic obj = detail != (dynamic)null;
				if ((!obj ? obj : obj & detail.Count > 0))
				{
					detailWs.Cells["B3"].LoadFromArrays(detail);
				}
				ExcelDownloadAttribute.CreateCalendarWorksheet(filterContext, scheduleWs);
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", fileName));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}

		private void WriteLocationDetailExcelFile(ResultExecutingContext filterContext)
		{
			string fileName = ((dynamic)filterContext.Controller.ViewBag).FileName as string;
			using (ExcelPackage pck = new ExcelPackage())
			{
				ExcelWorksheet detailWs = pck.Workbook.Worksheets.Add("Detail");
				ExcelWorksheet scheduleWs = pck.Workbook.Worksheets.Add("Schedule");
				List<string[]> headers = new List<string[]>();
				string[] strArrays = new string[] { "Name", "Display Name", "Phone Number", "Address", "City", "State", "Zip", "Email", "Contact Person", "Notes", "Active", "Can regist online" };
				headers.Add(strArrays);
				detailWs.Cells["B2"].LoadFromArrays(headers);
				dynamic detail = ((dynamic)filterContext.Controller.ViewBag).Detail;
				dynamic obj = detail != (dynamic)null;
				if ((!obj ? obj : obj & detail.Count > 0))
				{
					detailWs.Cells["B3"].LoadFromArrays(detail);
				}
				ExcelDownloadAttribute.CreateCalendarWorksheet(filterContext, scheduleWs);
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", fileName));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}

		private void WriteParentDetailExcelFile(ResultExecutingContext filterContext)
		{
			string fileName = ((dynamic)filterContext.Controller.ViewBag).FileName as string;
			using (ExcelPackage pck = new ExcelPackage())
			{
				ExcelWorksheet detailWs = pck.Workbook.Worksheets.Add("Detail");
				ExcelWorksheet childrenWs = pck.Workbook.Worksheets.Add("Children");
				ExcelWorksheet scheduleWs = pck.Workbook.Worksheets.Add("Schedule");
				List<string[]> headers = new List<string[]>();
				string[] strArrays = new string[] { "Location", "First Name", "Last Name", "Phone Number", "Address", "City", "State", "Zip", "Email", "Notes" };
				headers.Add(strArrays);
				detailWs.Cells["B2"].LoadFromArrays(headers);
				dynamic detail = ((dynamic)filterContext.Controller.ViewBag).Detail;
				dynamic obj = detail != (dynamic)null;
				if ((!obj ? obj : obj & detail.Count > 0))
				{
					detailWs.Cells["B3"].LoadFromArrays(detail);
				}
				headers.Clear();
				string[] strArrays1 = new string[] { "First Name", "Last Name", "Gender", "Grade", "Birth Date", "Notes" };
				headers.Add(strArrays1);
				dynamic children = ((dynamic)filterContext.Controller.ViewBag).Children;
				childrenWs.Cells["B2"].LoadFromArrays(headers);
				dynamic obj1 = children != (dynamic)null;
				if ((!obj1 ? obj1 : obj1 & children.Count > 0))
				{
					childrenWs.Cells["B3"].LoadFromCollection(children, false);
				}
				ExcelDownloadAttribute.CreateCalendarWorksheet(filterContext, scheduleWs);
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", fileName));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}

		private void WriteRosterDetailSheetExcelFile(ResultExecutingContext filterContext)
		{
			using (ExcelPackage pck = new ExcelPackage())
			{
				string fileName = (string)((dynamic)filterContext.Controller.ViewBag).FileName;
				this.CreateRoseterDetailWorksheet(pck, ((dynamic)filterContext.Controller.ViewBag).RosterDetail);
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", fileName));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}

		private void WriteRosterExcelFile(ResultExecutingContext filterContext, string fileName)
		{
			using (ExcelPackage pck = new ExcelPackage())
			{
				dynamic exportClasses = ((dynamic)filterContext.Controller.ViewBag).ClassRosters;
				dynamic exportCamps = ((dynamic)filterContext.Controller.ViewBag).CampRosters;
				foreach (dynamic klass in (IEnumerable)exportClasses)
				{
					this.CreateRoseterDetailWorksheet(pck, klass);
				}
				foreach (dynamic camp in (IEnumerable)exportCamps)
				{
					this.CreateRoseterDetailWorksheet(pck, camp);
				}
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", fileName));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}

		private void WriteSingleSheetExcelFile(ResultExecutingContext filterContext)
		{
			using (ExcelPackage pck = new ExcelPackage())
			{
				string fileName = (string)((dynamic)filterContext.Controller.ViewBag).FileName;
				string sheetName = (string)((dynamic)filterContext.Controller.ViewBag).SheetName;
				List<string[]> headers = (List<string[]>)((dynamic)filterContext.Controller.ViewBag).Headers;
				dynamic data = ((dynamic)filterContext.Controller.ViewBag).ExportData;
				ExcelWorksheet ws = pck.Workbook.Worksheets.Add(sheetName);
				ws.Cells["B2"].LoadFromArrays(headers);
				string nextCell = string.Format("B{0}", 2 + headers.Count);
				if (data.Count > 0)
				{
					ws.Cells[nextCell].LoadFromCollection(data, false);
				}
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", fileName));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}

		private void WriteWorkshopDetailExcelFile(ResultExecutingContext filterContext)
		{
			string fileName = ((dynamic)filterContext.Controller.ViewBag).FileName as string;
			using (ExcelPackage pck = new ExcelPackage())
			{
				ExcelWorksheet detailWs = pck.Workbook.Worksheets.Add("Detail");
				pck.Workbook.Worksheets.Add("Schedule");
				List<string[]> headers = new List<string[]>();
				string[] strArrays = new string[] { "Location", "Name", "Grades", "Time Start", "Time End", "Dates", "Instructor", "Assistant", "Total Cost", "Notes" };
				headers.Add(strArrays);
				detailWs.Cells["B2"].LoadFromArrays(headers);
				dynamic detail = ((dynamic)filterContext.Controller.ViewBag).Detail;
				dynamic obj = detail != (dynamic)null;
				if ((!obj ? obj : obj & detail.Count > 0))
				{
					detailWs.Cells["B3"].LoadFromArrays(detail);
				}
				filterContext.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				filterContext.HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", fileName));
				filterContext.HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
			}
		}
	}
}