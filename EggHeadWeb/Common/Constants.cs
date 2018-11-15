using System;

namespace EggheadWeb.Common
{
	public class Constants
	{
		public const int CW_Name = 150;

		public const int CW_Address = 200;

		public const int CW_Email = 150;

		public const int CW_PhoneNumber = 150;

		public const int CW_Location = 200;

		public const int CW_City = 150;

		public const int CW_State = 150;

		public const int CW_Checkbox = 40;

		public const int CW_Date = 100;

		public const int CW_Grade = 70;

		public const int CW_Gender = 50;

		public const int CW_Note = 150;

		public const int CW_Time = 70;

		public const int CW_Pay = 50;

		public const int CW_Schedule_Instructor = 100;

		public const string REGEX_Username = "^[A-Za-z0-9_]{3,15}$";

		public const string REGEX_Password = "^[A-Za-z0-9_@#$%]{3,20}$";

		public static string SELECT_LOCATION;

		public static string SELECT_INSTRUCTOR;

		public static string SELECT_ASSISTANT;

		public static string SELECT_RECEPIENT;

		public static string SELECT_TYPE;

		public static string SELECT_TEMPLATE;

		public static string SELECT;

		public static string STUB_PASSWORD;

		public static string BOOK_SERVICE_TYPE_CLASS;

		public static string BOOK_SERVICE_TYPE_CAMP;

		static Constants()
		{
			Constants.SELECT_LOCATION = "--Select Location--";
			Constants.SELECT_INSTRUCTOR = "--Select Instructor--";
			Constants.SELECT_ASSISTANT = "--Select Assistant--";
			Constants.SELECT_RECEPIENT = "--Select Recipient--";
			Constants.SELECT_TYPE = "--Select Type--";
			Constants.SELECT_TEMPLATE = "--Select Template--";
			Constants.SELECT = "--Select--";
			Constants.STUB_PASSWORD = "*****";
			Constants.BOOK_SERVICE_TYPE_CLASS = "Class";
			Constants.BOOK_SERVICE_TYPE_CAMP = "Camp";
		}

		public Constants()
		{
		}
	}
}