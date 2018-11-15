using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EggheadWeb.Common
{
	public static class CryptoUtil
	{
		private readonly static byte[] salt;

		static CryptoUtil()
		{
			CryptoUtil.salt = Encoding.ASCII.GetBytes("Ent3r your oWn S@lt v@lu# h#r3");
		}

		public static string Decrypt(string encryptedText, string encryptionPassword)
		{
			byte[] descryptedBytes;
			RijndaelManaged algorithm = CryptoUtil.GetAlgorithm(encryptionPassword);
			using (ICryptoTransform decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV))
			{
				descryptedBytes = CryptoUtil.InMemoryCrypt(Convert.FromBase64String(encryptedText), decryptor);
			}
			return Encoding.UTF8.GetString(descryptedBytes);
		}

		public static string Encrypt(string textToEncrypt, string encryptionPassword)
		{
			byte[] encryptedBytes;
			RijndaelManaged algorithm = CryptoUtil.GetAlgorithm(encryptionPassword);
			using (ICryptoTransform encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV))
			{
				byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);
				encryptedBytes = CryptoUtil.InMemoryCrypt(bytesToEncrypt, encryptor);
			}
			return Convert.ToBase64String(encryptedBytes);
		}

		private static RijndaelManaged GetAlgorithm(string encryptionPassword)
		{
			Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encryptionPassword, CryptoUtil.salt);
			RijndaelManaged algorithm = new RijndaelManaged();
			int bytesForKey = algorithm.KeySize / 8;
			int bytesForIV = algorithm.BlockSize / 8;
			algorithm.Key = key.GetBytes(bytesForKey);
			algorithm.IV = key.GetBytes(bytesForIV);
			return algorithm;
		}

		private static byte[] InMemoryCrypt(byte[] data, ICryptoTransform transform)
		{
			MemoryStream memory = new MemoryStream();
			using (Stream stream = new CryptoStream(memory, transform, CryptoStreamMode.Write))
			{
				stream.Write(data, 0, (int)data.Length);
			}
			return memory.ToArray();
		}
	}
}