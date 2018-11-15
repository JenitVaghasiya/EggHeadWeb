using EggheadWeb.Utility;
using EggHeadWeb.DatabaseContext;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EggheadWeb.Security
{
	public class WithAuthenController : Controller
	{
		protected const int STATUS_CODE_VALIDATION = 600;

		public UrlHelper Url = new UrlHelper(System.Web.HttpContext.Current.Request.RequestContext);

		protected EggheadContext db = new EggheadContext();

		protected RequestFlow flow;

		protected long AdminAreaId
		{
			get
			{
				return this.User.Admin.AreaId.Value;
			}
		}

		public string LoginUrl
		{
			get;
			set;
		}

		public UserProvider MembershipProvider
		{
			get
			{
				return (UserProvider)Membership.Provider;
			}
		}

		public string Role
		{
			get;
			set;
		}

		public LoginUser User
		{
			get;
			private set;
		}

		public WithAuthenController(string role)
		{
			this.Role = role;
		}

		protected object GetSession(string name)
		{
			return System.Web.HttpContext.Current.Session[name];
		}

		protected override void OnAuthorization(AuthorizationContext context)
		{
		    HttpContext httpContext = System.Web.HttpContext.Current;
            if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated)
			{
				this.User = this.MembershipProvider.GetUser(this.db, httpContext.User.Identity, this.Role);
				this.MembershipProvider.User = this.User;
				httpContext.User = this.User;
			}
			UrlHelper urlHelper = new UrlHelper( httpContext.Request.RequestContext);
			if (httpContext.Request.IsAuthenticated)
			{
				if (!httpContext.User.IsInRole(this.Role))
				{
				   System.Web.HttpContext.Current.Response.RedirectToRoute( new RedirectResult(this.LoginUrl));
					return;
				}
			}
			else if ((context.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) ? false : !context.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)))
			{
				base.OnAuthorization(context);
			}
		}

		protected void ProcessPaymentWithPaypal()
		{
			APIContext apiContext = Configuration.GetAPIContext("");
			string payerId = System.Web.HttpContext.Current.Request.Params["PayerID"];
			if (!string.IsNullOrEmpty(payerId))
			{
				string guid = System.Web.HttpContext.Current.Request.Params["guid"];
				this.flow = Session[string.Concat("flow-", guid)] as RequestFlow;
				this.RegisterPaypalRequestFlow();
				flow.RecordApproval("PayPal payment approved successfully.");
				string paymentId = Session[guid] as string;
				PaymentExecution paymentExecution1 = new PaymentExecution();
				paymentExecution1.payer_id = payerId;
				PaymentExecution paymentExecution = paymentExecution1;
				PayPal.Api.Payment payment1 = new PayPal.Api.Payment();
				payment1.id = paymentId;
				PayPal.Api.Payment paymenta = payment1;
				this.flow.AddNewRequest("Execute PayPal payment", paymenta, "");
				PayPal.Api.Payment executedPayment = paymenta.Execute(apiContext, paymentExecution);
				this.flow.RecordResponse(executedPayment);
				return;
			}
			this.RegisterPaypalRequestFlow();
			ItemList itemList1 = new ItemList();
			List<Item> items = new List<Item>();
			Item item = new Item();
			item.name="Item Name";
			item.currency="USD";
			item.price="15";
			item.quantity="5";
			item.sku="sku";
			items.Add(item);
			itemList1.items =items;
			ItemList itemList = itemList1;
			Payer payer1 = new Payer();
			payer1.payment_method = "paypal";
			Payer payer = payer1;
			string baseURI = string.Concat(System.Web.HttpContext.Current.Request.Url.Scheme, "://", System.Web.HttpContext.Current.Request.Url.Authority, "/PaymentWithPayPal.aspx?");
			string s1 = Convert.ToString((new Random()).Next(100000));
			string redirectUrl = string.Concat(baseURI, "guid=", s1);
			RedirectUrls redirectUrl1 = new RedirectUrls();
			redirectUrl1.cancel_url = string.Concat(redirectUrl, "&cancel=true");
			redirectUrl1.return_url = (redirectUrl);
			RedirectUrls redirUrls = redirectUrl1;
			Details detail = new Details();
			detail.tax="15";
			detail.shipping="10";
			detail.subtotal="75";
			Details details = detail;
		    Amount amount1 = new Amount {currency = "USD", total = "100.00", details = details};
		    Amount amount = amount1;
			List<Transaction> transactionList = new List<Transaction>();
			Transaction transaction = new Transaction();
			transaction.description = "Transaction description.";
			transaction.invoice_number = EggheadWeb.Utility.Common.GetRandomInvoiceNumber();
			transaction.amount = amount;
			transaction.item_list = itemList;
			transactionList.Add(transaction);
		    PayPal.Api.Payment payment2 = new PayPal.Api.Payment
		    {
		        intent = "sale", payer = payer, transactions = transactionList, redirect_urls = redirUrls
		    };
		    PayPal.Api.Payment payment = payment2;
			this.flow.AddNewRequest("Create PayPal payment", payment, "");
			PayPal.Api.Payment createdPayment = payment.Create(apiContext);
			this.flow.RecordResponse(createdPayment);
			List<Links>.Enumerator links = createdPayment.links.GetEnumerator();
			while (links.MoveNext())
			{
				Links link = links.Current;
				if (!link.rel.ToLower().Trim().Equals("approval_url"))
				{
					continue;
				}
				this.flow.RecordRedirectUrl("Redirect to PayPal to approve the payment...", link.href);
			}
			Session.Add(s1, createdPayment.id);
			Session.Add(string.Concat("flow-", s1), this.flow);
		}

		protected void RegisterPaypalRequestFlow()
		{
			if (this.flow == null)
			{
				this.flow = new RequestFlow();
			}
		}

		protected void SetSession(string name, object value)
		{
			Session[name] = value;
		}

		protected void SetViewMessage(WithAuthenController.MessageType type, string message, params object[] parameters)
		{
			string msg = string.Format(message, parameters);
			if (type == WithAuthenController.MessageType.Info)
			{
				TempData.Add("Info", msg);
				return;
			}
		    TempData.Add( "Error", msg);
		}

		public void SignIn(string username, bool createPersistentCookie)
		{
			FormsAuthentication.SetAuthCookie(username, createPersistentCookie);
		}

		public void SignOut()
		{
			FormsAuthentication.SignOut();
		}

		public bool ValidateUser(string username, string password)
		{
			string role = this.Role;
			string str = role;
			if (role != null)
			{
				if (str == "superadmin")
				{
					return this.MembershipProvider.ValidateAdmin(this.db, username, password);
				}
				if (str == "admin")
				{
					return this.MembershipProvider.ValidateAdmin(this.db, username, password);
				}
				if (str == "parent")
				{
					return this.MembershipProvider.ValidateParent(this.db, username, password);
				}
			}
			return false;
		}

		protected enum MessageType
		{
			Info,
			Error
		}
	}
}