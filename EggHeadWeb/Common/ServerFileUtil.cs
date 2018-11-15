using System;
using System.IO;
using System.Web;

namespace EggheadWeb.Common
{
	public class ServerFileUtil
	{
		public ServerFileUtil()
		{
		}

		public static string CreateUserTemporaryFolder(string username, string createdFolderId)
		{
			string timeForderId = createdFolderId;
			if (string.IsNullOrEmpty(createdFolderId))
			{
				timeForderId = DateTime.Now.ToString("yyyyMMddhhmmss");
			}
			string folderPath = Path.Combine(username, timeForderId);
			string saveLocation = Path.Combine( System.Web.HttpContext.Current.Server.MapPath( "~/Upload"), folderPath);
			if (!Directory.Exists(saveLocation))
			{
				Directory.CreateDirectory(saveLocation);
			}
			return saveLocation;
		}
	}
}