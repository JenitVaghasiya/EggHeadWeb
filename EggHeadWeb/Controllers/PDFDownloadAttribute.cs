using EggheadWeb.Common;
using EggheadWeb.Models.AdminModels;
using EggheadWeb.Models.Common;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EggheadWeb.Controllers
{
	public class PDFDownloadAttribute : ActionFilterAttribute
	{
		public PDFDownloadAttribute()
		{
		}

		public void CreateRosterDetailPDF(ResultExecutingContext filterContext)
		{
			string name = ((dynamic)filterContext.Controller.ViewBag).FileName as string;
			HttpResponse Response = HttpContext.Current.Response;
			Document document = new Document();
			PdfWriter writer = PdfWriter.GetInstance(document, Response.OutputStream);
			Response.Clear();
			Response.ContentType = "application/octet-stream";
			Response.AddHeader("Content-Disposition", string.Concat("attachment; filename=\"", name, ".pdf\""));
			RosterDetailPdf roster = ((dynamic)filterContext.Controller.ViewBag).RosterDetailPdf as RosterDetailPdf;
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
			PdfWriter.GetInstance(document, Response.OutputStream);
		}

		public void CreateRostersPDF(ResultExecutingContext filterContext)
		{
			string name = ((dynamic)filterContext.Controller.ViewBag).FileName as string;
			HttpResponse Response = HttpContext.Current.Response;
			Document document = new Document();
			PdfWriter writer = PdfWriter.GetInstance(document, Response.OutputStream);
			Response.Clear();
			Response.ContentType = "application/octet-stream";
			Response.AddHeader("Content-Disposition", string.Concat("attachment; filename=\"", name, ".pdf\""));
		    RosterPdfUtil.CreateRostersDocument(writer, document, ((dynamic)filterContext.Controller.ViewBag).ClassRosters, ((dynamic)filterContext.Controller.ViewBag).CampRosters);
			PdfWriter.GetInstance(document, Response.OutputStream);
		}

		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			string actionName = (string)filterContext.RouteData.Values["Action"];
			List<string[]> strArrays = new List<string[]>();
			string str = actionName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "roster-detail-pdf")
				{
					this.CreateRosterDetailPDF(filterContext);
					return;
				}
				if (str1 != "rosters-pdf")
				{
					return;
				}
				this.CreateRostersPDF(filterContext);
			}
		}
	}
}