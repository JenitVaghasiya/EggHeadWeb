using PayPal.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using PayPal.Api;

namespace EggheadWeb.Utility
{
	public class RequestFlow
	{
		public string Description
		{
			get;
			set;
		}

		public List<RequestFlowItem> Items
		{
			get;
			private set;
		}

		public RequestFlow()
		{
			this.Items = new List<RequestFlowItem>();
		}

		public void AddNewRequest(string title, IPayPalSerializableObject requestObject = null, string description = "")
		{
			List<RequestFlowItem> items = this.Items;
			RequestFlowItem requestFlowItem = new RequestFlowItem()
			{
				Request = (requestObject == null ? string.Empty : EggheadWeb.Utility.Common.FormatJsonString(requestObject.ConvertToJson())),
				Title = title,
				Description = description
			};
			items.Add(requestFlowItem);
		}

		public void RecordActionSuccess(string message)
		{
			if (this.Items.Any<RequestFlowItem>())
			{
				this.Items.Last<RequestFlowItem>().RecordSuccess(message);
			}
		}

		public void RecordApproval(string message)
		{
			if (this.Items.Any<RequestFlowItem>())
			{
				RequestFlowItem requestFlowItem = this.Items.Last<RequestFlowItem>();
				requestFlowItem.Title = string.Concat(requestFlowItem.Title, " (Approved!)");
				this.Items.Last<RequestFlowItem>().RedirectUrlText = message;
				this.Items.Last<RequestFlowItem>().IsRedirectApproved = true;
			}
		}

		public void RecordException(Exception ex)
		{
			if (ex != null)
			{
				if (!this.Items.Any<RequestFlowItem>())
				{
					this.Items.Add(new RequestFlowItem());
				}
				this.Items.Last<RequestFlowItem>().RecordException(ex);
			}
		}

		public void RecordImage(Image image)
		{
			if (this.Items.Any<RequestFlowItem>())
			{
				string filename = "Images/sample.png";
			    string serverRoot = System.Web.HttpContext.Current.Server.MapPath( null);
				image.Save(Path.Combine(serverRoot, filename));
				this.Items.Last<RequestFlowItem>().ImagePath = filename;
			}
		}

		public void RecordRedirectUrl(string text, string redirectUrl)
		{
			if (this.Items.Any<RequestFlowItem>())
			{
				this.Items.Last<RequestFlowItem>().RedirectUrlText = text;
				this.Items.Last<RequestFlowItem>().RedirectUrl = redirectUrl;
			}
		}

		public void RecordResponse(IPayPalSerializableObject responseObject)
		{
			if (responseObject != null && this.Items.Any<RequestFlowItem>())
			{
				this.Items.Last<RequestFlowItem>().Response = EggheadWeb.Utility.Common.FormatJsonString(responseObject.ConvertToJson());
			}
		}
	}
}