using log4net;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;

namespace EggheadWeb.Utility
{
	public class PayPal
	{
		private static ILog log;

		static PayPal()
		{
			PayPal.log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		}

		public PayPal()
		{
		}

		public static string DoExpressCheckoutPayment(PayPalOrder order)
		{
			NameValueCollection values = new NameValueCollection();
			values["METHOD"] = "DoExpressCheckoutPayment";
			values["RETURNURL"] = (string.IsNullOrEmpty(order.ReturnUrl) ? PayPalSettings.ReturnUrl : order.ReturnUrl);
			values["CANCELURL"] = (string.IsNullOrEmpty(order.CancelUrl) ? PayPalSettings.CancelUrl : order.CancelUrl);
			values["USER"] = order.UserName;
			values["PWD"] = order.Password;
			values["SIGNATURE"] = order.Signature;
			values["VERSION"] = "85.0";
			values["PAYMENTREQUEST_0_PAYMENTACTION"] = "Sale";
			values["PAYMENTREQUEST_0_CURRENCYCODE"] = "USD";
			decimal amount = order.Amount;
			values["PAYMENTREQUEST_0_AMT"] = amount.ToString("0.00", CultureInfo.InvariantCulture);
			values["TOKEN"] = order.Token;
			values["PAYERID"] = order.PayerID;
			values = Utility.PayPal.Submit(values);
			string ack = values["ACK"].ToLower();
			if (!(ack == "success") && !(ack == "successwithwarning"))
			{
				throw new Exception(values["L_LONGMESSAGE0"]);
			}
			return values["PAYMENTINFO_0_TRANSACTIONID"];
		}

		public static PayPalRedirect ExpressCheckout(PayPalOrder order, out string message)
		{
			NameValueCollection values = new NameValueCollection();
			message = string.Empty;
			values["METHOD"] = "SetExpressCheckout";
			values["RETURNURL"] = (string.IsNullOrEmpty(order.ReturnUrl) ? PayPalSettings.ReturnUrl : order.ReturnUrl);
			values["CANCELURL"] = (string.IsNullOrEmpty(order.CancelUrl) ? PayPalSettings.CancelUrl : order.CancelUrl);
			values["USER"] = order.UserName;
			values["PWD"] = order.Password;
			values["SIGNATURE"] = order.Signature;
			values["VERSION"] = "85.0";
			values["SOLUTIONTYPE"] = "Sole";
			values["PAYMENTREQUEST_0_PAYMENTACTION"] = "Sale";
			values["PAYMENTREQUEST_0_CURRENCYCODE"] = "USD";
			decimal amount = order.Amount;
			values["PAYMENTREQUEST_0_AMT"] = amount.ToString("0.00", CultureInfo.InvariantCulture);
			decimal num = order.Amount;
			values["PAYMENTREQUEST_0_ITEMAMT"] = num.ToString("0.00", CultureInfo.InvariantCulture);
			PayPal.log.Debug(string.Concat("Input Payment Description: ", order.Description));
			if (order.Description != null && order.Description.Length > 127)
			{
				order.Description = order.Description.Substring(0, 127);
			}
			PayPal.log.Debug(string.Concat("Payment Description: ", order.Description));
			values["L_PAYMENTREQUEST_0_NAME0"] = order.Name;
			values["L_PAYMENTREQUEST_0_DESC0"] = order.Description;
			values["L_PAYMENTREQUEST_0_NUMBER0"] = "1";
			values["L_PAYMENTREQUEST_0_QTY0"] = "1";
			decimal amount1 = order.Amount;
			values["L_PAYMENTREQUEST_0_AMT0"] = amount1.ToString("0.00", CultureInfo.InvariantCulture);
			if (order.OverrideAddress)
			{
				values["ADDROVERRIDE"] = "1";
				values["PAYMENTREQUEST_0_SHIPTONAME"] = string.Format("{0} {1}", order.FirstName, order.LastName);
				values["PAYMENTREQUEST_0_SHIPTOSTREET"] = order.Address;
				values["PAYMENTREQUEST_0_SHIPTOSTREET2"] = string.Empty;
				values["PAYMENTREQUEST_0_SHIPTOCITY"] = order.City;
				values["PAYMENTREQUEST_0_SHIPTOSTATE"] = order.State;
				values["PAYMENTREQUEST_0_SHIPTOZIP"] = order.Zip;
				values["PAYMENTREQUEST_0_SHIPTOCOUNTRYCODE"] = "US";
				values["PAYMENTREQUEST_0_SHIPTOPHONENUM"] = string.Empty;
			}
			PayPal.log.Debug(string.Concat("Payment Description: ", order.Description));
			values = Utility.PayPal.Submit(values);
			string ack = values["ACK"].ToLower();
			if (!(ack == "success") && !(ack == "successwithwarning"))
			{
				if (order.OverrideAddress)
				{
					order.OverrideAddress = false;
					return Utility.PayPal.ExpressCheckout(order, out message);
				}
				message = values["L_LONGMESSAGE0"];
				return new PayPalRedirect();
			}
			message = string.Empty;
			PayPalRedirect payPalRedirect = new PayPalRedirect()
			{
				Token = values["TOKEN"],
				Url = string.Format("https://{0}/cgi-bin/webscr?cmd=_express-checkout&token={1}", PayPalSettings.CgiDomain, values["TOKEN"])
			};
			return payPalRedirect;
		}

		private static NameValueCollection Submit(NameValueCollection values)
		{
			NameValueCollection nameValueCollection;
			string data = string.Join("&", 
				from string key in values
				select string.Format("{0}={1}", key, HttpUtility.UrlEncode(values[key])));
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("https://{0}/nvp", PayPalSettings.ApiDomain));
			request.Method = "POST";
			request.ContentLength = (long)data.Length;
			using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
			{
				writer.Write(data);
			}
			using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
			{
				nameValueCollection = HttpUtility.ParseQueryString(reader.ReadToEnd());
			}
			return nameValueCollection;
		}
	}
}