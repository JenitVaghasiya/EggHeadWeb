using EggheadWeb.Models.AdminModels;
using EggheadWeb.Models.Common;
using EggHeadWeb.DatabaseContext;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace EggheadWeb.Common
{
	public class RosterPdfUtil
	{
		private const string DOCUMENT_AUTHOR = "PROFESSOR EGGHEAD SCIENCE ACADEMY";

		private const string DOCUMENT_TITLE = "ROSTERS";

		private const string DOCUMENT_CREATOR = "Egghead Web Application";

		private const string LOCATION = "LOCATION";

		private const string DAY = "DAY";

		private const string TIME = "TIME";

		private const string PRINTED = "PRINTED";

		private const string ADDRESS = "ADDRESS";

		private const string STUDENT_NOTES = "STUDENT NOTES: ";

		private const string STUDENT_COLUMN_TEXT = "Name/Age/Gender";

		private const string PARENT_COLUMN_TEXT = "PARENT";

		public static string ROSTER_HEADER_TEXT;

		public static string ROSTER_FOOTER_TEXT;

		static RosterPdfUtil()
		{
			RosterPdfUtil.ROSTER_HEADER_TEXT = "EGGHEAD ATTENDANCE SHEET";
			DateTime now = DateTime.Now;
			RosterPdfUtil.ROSTER_FOOTER_TEXT = string.Format("{0} © PROFESSOR EGGHEAD SCIENCE ACADEMY® ALL RIGHTS RESERVED.", now.Year);
		}

		public RosterPdfUtil()
		{
		}

		private static Phrase CreateHeaderPhrase(string name, string content, iTextSharp.text.Font font)
		{
			return new Phrase(string.Format("{0}: {1}", name, content), font);
		}

		private static void CreateRosterDetail(PdfWriter writer, Document document, RosterDetailPdf rosterData)
		{
			BaseFont normalFont = (writer.PageEvent as RosterPdfUtil.RosterEventHandler).NormalFont;
			BaseFont boldFont = (writer.PageEvent as RosterPdfUtil.RosterEventHandler).BoldFont;
			RosterDetailPdf roster2 = null;
			if ((int)rosterData.Dates.Length > 11)
			{
				RosterDetailPdf rosterDetailPdf = new RosterDetailPdf()
				{
					LocationName = rosterData.LocationName,
					Days = rosterData.Days,
					Time = rosterData.Time,
					Printed = rosterData.Printed,
					Address = rosterData.Address,
					ClassName = rosterData.ClassName,
					Dates = new string[0],
					Bookings = rosterData.Bookings,
					Headers = rosterData.Headers,
					LogoUrl = rosterData.LogoUrl,
					Type = rosterData.Type
				};
				roster2 = rosterDetailPdf;
				List<string> extDate = new List<string>();
				for (int i = 0; i < (int)rosterData.Dates.Length; i++)
				{
					if (i >= 11)
					{
						extDate.Add(rosterData.Dates[i]);
					}
				}
				rosterData.Dates = rosterData.Dates.Take<string>(11).ToArray<string>();
				roster2.Dates = extDate.ToArray();
			}
			PdfPTable headerTable = new PdfPTable(4);
			iTextSharp.text.Font headerFont = new iTextSharp.text.Font(boldFont, 11f);
			headerTable.AddCell(new PdfPCell(RosterPdfUtil.CreateHeaderPhrase("LOCATION", rosterData.LocationName, headerFont)));
			headerTable.AddCell(new PdfPCell(RosterPdfUtil.CreateHeaderPhrase("DAY", rosterData.Days, headerFont)));
			headerTable.AddCell(new PdfPCell(RosterPdfUtil.CreateHeaderPhrase("TIME", rosterData.Time, headerFont)));
			headerTable.AddCell(new PdfPCell(RosterPdfUtil.CreateHeaderPhrase("PRINTED", rosterData.Printed, headerFont)));
			PdfPCell addressCell = new PdfPCell(RosterPdfUtil.CreateHeaderPhrase("ADDRESS", rosterData.Address, headerFont))
			{
				FixedHeight = 16.2f,
				Colspan = 2
			};
			PdfPCell classNameCell = new PdfPCell(RosterPdfUtil.CreateHeaderPhrase(rosterData.Type.ToString().ToUpper(), rosterData.ClassName, headerFont))
			{
				Colspan = 2
			};
			headerTable.AddCell(addressCell);
			headerTable.AddCell(classNameCell);
			headerTable.HorizontalAlignment = 2;
			headerTable.TotalWidth = 702.9f;
			headerTable.LockedWidth = true;
			headerTable.SetWidths(new float[] { 183.15f, 173.25f, 173.25f, 173.25f });
			foreach (PdfPRow row in headerTable.Rows)
			{
				PdfPCell[] pdfPCellArray = row.GetCells();
				for (int j = 0; j < (int)pdfPCellArray.Length; j++)
				{
					PdfPCell cell1 = pdfPCellArray[j];
					if (cell1 != null)
					{
						cell1.PaddingLeft = 5.4f;
						cell1.VerticalAlignment = 5;
					}
				}
			}
			headerTable.SpacingAfter = 15f;
			document.Add(headerTable);
			PdfPTable detailTable = new PdfPTable(14);
			List<PdfPCell> cells = new List<PdfPCell>();
			PdfPCell noHeaderCell = new PdfPCell()
			{
				Border = 0
			};
			cells.Add(noHeaderCell);
			noHeaderCell.FixedHeight = 16.2f;
			PdfPCell detailHeader0 = new PdfPCell(new Phrase("Name/Age/Gender", new iTextSharp.text.Font(boldFont, 10f)))
			{
				HorizontalAlignment = 1
			};
			cells.Add(detailHeader0);
			PdfPCell dettailHeader1 = new PdfPCell(new Phrase("PARENT", new iTextSharp.text.Font(boldFont, 10f)))
			{
				HorizontalAlignment = 1
			};
			cells.Add(dettailHeader1);
			string[] dates = rosterData.Dates;
			for (int k = 0; k < (int)dates.Length; k++)
			{
				string date = dates[k];
				PdfPCell cell = new PdfPCell(new Phrase(date, new iTextSharp.text.Font(normalFont, 9f)))
				{
					HorizontalAlignment = 1
				};
				cells.Add(new PdfPCell(cell));
			}
			for (int i = 0; i < 11 - (int)rosterData.Dates.Length; i++)
			{
				cells.Add(new PdfPCell(new Phrase()));
			}
			foreach (PdfPCell cell in cells)
			{
				cell.HorizontalAlignment = 1;
				if (cells.IndexOf(cell) <= 0)
				{
					continue;
				}
				cell.BackgroundColor = new BaseColor(195, 214, 155);
			}
			detailTable.Rows.Add(new PdfPRow(cells.ToArray()));
			int no = 0;
			foreach (Booking booking in rosterData.Bookings)
			{
				cells.Clear();
				int num = no + 1;
				no = num;
				int num1 = num;
				PdfPCell noCell = new PdfPCell(new Phrase(num1.ToString(), new iTextSharp.text.Font(normalFont, 11f)))
				{
					Border = 0,
					HorizontalAlignment = 2,
					PaddingLeft = 0f,
					FixedHeight = 16.2f
				};
				cells.Add(noCell);
				object[] firstName = new object[] { booking.Student.FirstName, booking.Student.LastName, MisUtil.CalAge(booking.Student.BirthDate), booking.Student.GenderText[0] };
				string studentInfo = string.Format("{0} {1} / {2} / {3}", firstName);
				cells.Add(new PdfPCell(new Phrase(studentInfo, new iTextSharp.text.Font(normalFont, 11f))));
				cells.Add(new PdfPCell(new Phrase(StringUtil.GetFullName(booking.Student.Parent.FirstName, booking.Student.Parent.LastName), new iTextSharp.text.Font(normalFont, 11f))));
				string[] strArrays = rosterData.Dates;
				for (int m = 0; m < (int)strArrays.Length; m++)
				{
					string str = strArrays[m];
					cells.Add(new PdfPCell(new Phrase()));
				}
				for (int i = 0; i < 11 - (int)rosterData.Dates.Length; i++)
				{
					cells.Add(new PdfPCell(new Phrase()));
				}
				foreach (PdfPCell cell in cells)
				{
					if (cells.IndexOf(cell) == 0)
					{
						continue;
					}
					if (no % 2 != 0)
					{
						cell.BackgroundColor = new BaseColor(255, 255, 255);
					}
					else
					{
						cell.BackgroundColor = new BaseColor(234, 241, 221);
					}
				}
				detailTable.Rows.Add(new PdfPRow(cells.ToArray()));
			}
			for (int add = 1; add <= 16 - rosterData.Bookings.Count<Booking>(); add++)
			{
				cells.Clear();
				int num2 = no + add;
				PdfPCell noCell = new PdfPCell(new Phrase(num2.ToString(), new iTextSharp.text.Font(normalFont, 11f)))
				{
					Border = 0,
					HorizontalAlignment = 2,
					PaddingLeft = 0f,
					FixedHeight = 16.2f
				};
				cells.Add(noCell);
				cells.Add(new PdfPCell(new Phrase(string.Empty)));
				cells.Add(new PdfPCell(new Phrase(string.Empty)));
				string[] dates1 = rosterData.Dates;
				for (int p = 0; p < (int)dates1.Length; p++)
				{
					string str1 = dates1[p];
					cells.Add(new PdfPCell(new Phrase()));
				}
				for (int i = 0; i < 11 - (int)rosterData.Dates.Length; i++)
				{
					cells.Add(new PdfPCell(new Phrase()));
				}
				foreach (PdfPCell cell in cells)
				{
					if (cells.IndexOf(cell) == 0)
					{
						continue;
					}
					if ((no + add) % 2 != 0)
					{
						cell.BackgroundColor = new BaseColor(255, 255, 255);
					}
					else
					{
						cell.BackgroundColor = new BaseColor(234, 241, 221);
					}
				}
				detailTable.Rows.Add(new PdfPRow(cells.ToArray()));
			}
			detailTable.LockedWidth = true;
			detailTable.TotalWidth = 714.5f;
			detailTable.HorizontalAlignment = 1;
			detailTable.SetWidths(new float[] { 13.6f, 153.6f, 98.5f, 40.75f, 40.75f, 40.75f, 40.75f, 40.75f, 40.75f, 40.75f, 40.75f, 40.75f, 40.75f, 40.75f });
			foreach (PdfPRow row in detailTable.Rows)
			{
				List<PdfPCell> rowCells = row.GetCells().ToList<PdfPCell>();
				foreach (PdfPCell cell1 in rowCells)
				{
					if (cell1 == null)
					{
						continue;
					}
					cell1.VerticalAlignment = 5;
					if (rowCells.IndexOf(cell1) <= 0)
					{
						continue;
					}
					cell1.PaddingLeft = 5.4f;
				}
			}
			detailTable.SpacingAfter = 15f;
			document.Add(detailTable);
			PdfPTable noteTable = new PdfPTable(2);
			PdfPCell noteCell = new PdfPCell();
			List<Booking> bookingWithNotes = (
				from t in rosterData.Bookings
				where !string.IsNullOrEmpty(t.Student.Notes)
				select t).ToList<Booking>();
			noteCell.AddElement(new Phrase("STUDENT NOTES: ", new iTextSharp.text.Font(boldFont, 11f)));
			PdfPTable notedetailTable = new PdfPTable(2)
			{
				TotalWidth = 95f,
				HorizontalAlignment = 0
			};
			PdfPCell noteDetailCell = new PdfPCell();
			for (int i = 0; i < bookingWithNotes.Count<Booking>(); i += 2)
			{
				noteDetailCell = new PdfPCell(new Phrase(string.Format("{0}: {1}", StringUtil.GetFullName(bookingWithNotes[i].Student.FirstName, bookingWithNotes[i].Student.LastName), bookingWithNotes[i].Student.Notes), new iTextSharp.text.Font(normalFont, 11f)))
				{
					BorderWidth = 0f
				};
				notedetailTable.AddCell(noteDetailCell);
				noteDetailCell = (i + 1 >= bookingWithNotes.Count<Booking>() ? new PdfPCell(new Phrase("")) : new PdfPCell(new Phrase(string.Format("{0}: {1}", StringUtil.GetFullName(bookingWithNotes[i + 1].Student.FirstName, bookingWithNotes[i + 1].Student.LastName), bookingWithNotes[i + 1].Student.Notes), new iTextSharp.text.Font(normalFont, 11f))));
				noteDetailCell.BorderWidth = 0f;
				notedetailTable.AddCell(noteDetailCell);
			}
			notedetailTable.SpacingBefore = 5f;
			noteCell.AddElement(notedetailTable);
			noteCell.PaddingLeft = 5.4f;
			noteCell.FixedHeight = 96.2f;
			string logoPath = Path.Combine( System.Web.HttpContext.Current.Server.MapPath( "~/Content/images"), "logo_small_rotate16_cropted.png");
			System.Drawing.Image image = System.Drawing.Image.FromFile(logoPath);
			iTextSharp.text.Image pic = iTextSharp.text.Image.GetInstance(image, ImageFormat.Png);
			pic.ScaleAbsoluteHeight(96.2f);
			pic.ScaleAbsoluteWidth(119.25f);
			PdfPCell imageCell = new PdfPCell()
			{
				Border = 0,
				PaddingLeft = 20f
			};
			imageCell.AddElement(pic);
			noteTable.AddCell(noteCell);
			noteTable.AddCell(imageCell);
			noteTable.WidthPercentage = 94f;
			noteTable.SetWidths(new float[] { 600f, 129.25f });
			document.Add(noteTable);
			if (roster2 != null)
			{
				document.NewPage();
				RosterPdfUtil.CreateRosterDetail(writer, document, roster2);
			}
		}

		public static void CreateRostersDocument(PdfWriter writer, Document document, dynamic classes, dynamic camps)
		{
			dynamic obj;
			document.SetPageSize(PageSize.LETTER.Rotate());
			document.SetMargins(36f, 36f, 85.6f, 36f);
			document.AddTitle("ROSTERS");
			document.AddAuthor("PROFESSOR EGGHEAD SCIENCE ACADEMY");
			document.AddCreator("Egghead Web Application");
			writer.PageEvent = new RosterPdfUtil.RosterEventHandler();
			document.Open();
			if (classes != (dynamic)null)
			{
				int i = 0;
				while (true)
				{
					if (i >= classes.Count)
					{
						break;
					}
					CreateRosterDetail(writer, document, classes[i]);
					dynamic obj1 = i < classes.Count - 1;
					if (obj1)
					{
						obj = obj1;
					}
					else
					{
						dynamic obj2 = obj1;
						dynamic obj3 = camps != (dynamic)null;
						obj = obj2 | (!obj3 ? obj3 : obj3 & camps.Count > 0);
					}
					if (obj)
					{
						document.NewPage();
					}
					i++;
				}
			}
			if (camps != (dynamic)null)
			{
				int i = 0;
				while (true)
				{
					if (i >= camps.Count)
					{
						break;
					}
					RosterPdfUtil.CreateRosterDetail(writer, document, camps[i]);
					if (i < camps.Count - 1)
					{
						document.NewPage();
					}
					i++;
				}
			}
			document.Close();
		}

		internal class RosterEventHandler : PdfPageEventHelper
		{
			private PdfContentByte cb;

			public BaseFont BoldFont
			{
				get;
				set;
			}

			public BaseFont NormalFont
			{
				get;
				set;
			}

			public RosterEventHandler()
			{
			}

			public override void OnEndPage(PdfWriter writer, Document document)
			{
				base.OnEndPage(writer, document);
				iTextSharp.text.Rectangle pageSize = document.PageSize;
				this.cb.BeginText();
				this.cb.SetFontAndSize(this.NormalFont, 9f);
				this.cb.SetTextMatrix(pageSize.GetLeft(36f), pageSize.GetBottom(39.4f));
				this.cb.ShowText(RosterPdfUtil.ROSTER_FOOTER_TEXT);
				this.cb.EndText();
			}

			public override void OnOpenDocument(PdfWriter writer, Document document)
			{
				string[] strArrays = new string[] { System.Web.HttpContext.Current.Server.MapPath( "~/PDFFont") };
				string fontPath = Path.Combine(strArrays);
				this.NormalFont = BaseFont.CreateFont(Path.Combine(fontPath, "CALIBRI.ttf"), "Cp1252", true);
				this.BoldFont = BaseFont.CreateFont(Path.Combine(fontPath, "CALIBRIB.ttf"), "Cp1252", true);
				this.cb = writer.DirectContent;
			}

			public override void OnStartPage(PdfWriter writer, Document document)
			{
				base.OnStartPage(writer, document);
				iTextSharp.text.Rectangle pageSize = document.PageSize;
				PdfPTable headerTable = new PdfPTable(1);
				PdfPCell titleCell = new PdfPCell(new Phrase(RosterPdfUtil.ROSTER_HEADER_TEXT, new iTextSharp.text.Font(this.BoldFont, 18f)))
				{
					VerticalAlignment = 4,
					HorizontalAlignment = 2,
					Border = 0
				};
				headerTable.AddCell(titleCell);
				headerTable.HorizontalAlignment = 2;
				headerTable.TotalWidth = pageSize.Width - 72f;
				headerTable.WriteSelectedRows(0, -1, pageSize.GetLeft(45f), pageSize.GetTop(54f), this.cb);
			}
		}
	}
}