using FluentValidation.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace EggheadWeb.Models.Common
{
	[Validator(typeof(PrivateMessageValidatior))]
	public class PrivateMessage
	{
		public virtual EggheadWeb.Models.Common.Admin Admin
		{
			get;
			set;
		}

		public virtual EggheadWeb.Models.Common.Admin Admin1
		{
			get;
			set;
		}

		public long? FromAdminId
		{
			get;
			set;
		}

		public string FromAdminName
		{
			get;
			set;
		}

		public long Id
		{
			get;
			set;
		}

		public string MessageContent
		{
			get;
			set;
		}

		public string MessageSubject
		{
			get;
			set;
		}

		public DateTime SendDate
		{
			get;
			set;
		}

		public long? ToAdminId
		{
			get;
			set;
		}

		public string ToAdminName
		{
			get;
			set;
		}

		public bool Unread
		{
			get;
			set;
		}

		public PrivateMessage()
		{
		}
	}
}