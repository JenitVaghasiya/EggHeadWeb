using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	public class MainTaskbarItem
	{
		public bool Add
		{
			get;
			set;
		}

		public string AddAction
		{
			get;
			set;
		}

		public bool Email
		{
			get;
			set;
		}

		public string EmailToAddress
		{
			get;
			set;
		}

		public string EmailToName
		{
			get;
			set;
		}

		public string ExcelAction
		{
			get;
			set;
		}

		public bool ExcelExport
		{
			get;
			set;
		}

		public bool FixedEmail
		{
			get;
			set;
		}

		public string PdfAction
		{
			get;
			set;
		}

		public bool PdfExport
		{
			get;
			set;
		}

		public bool Print
		{
			get;
			set;
		}

		public bool Save
		{
			get;
			set;
		}

		public string SendMailAction
		{
			get;
			set;
		}

		public MainTaskbarItem()
		{
		}
	}
}