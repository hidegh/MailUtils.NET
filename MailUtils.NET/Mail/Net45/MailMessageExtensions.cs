using System;
using System.IO;
using System.Reflection;

namespace MailUtils.Mail.Net45
{
	public static class MailMessageExtensions
	{
		private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
		private static readonly Type MailWriter = typeof(System.Net.Mail.SmtpClient).Assembly.GetType("System.Net.Mail.MailWriter");
		private static readonly ConstructorInfo MailWriterConstructor = MailWriter.GetConstructor(Flags, null, new[] { typeof(Stream) }, null);
		private static readonly MethodInfo CloseMethod = MailWriter.GetMethod("Close", Flags);
		private static readonly MethodInfo SendMethod = typeof(System.Net.Mail.MailMessage).GetMethod("Send", Flags);

		/// <summary>
		/// A little hack to determine the number of parameters that we need to pass to the SaveMethod.
		/// </summary>
		private static readonly bool IsRunningInDotNetFourPointFive = SendMethod.GetParameters().Length == 3;

		/// <summary>
		/// The raw contents of this MailMessage as a MemoryStream.
		/// </summary>
		/// <param name="self">The caller.</param>
		/// <returns>A MemoryStream with the raw contents of this MailMessage.</returns>
		public static MemoryStream RawMessage(this System.Net.Mail.MailMessage self)
		{
			var result = new MemoryStream();
			var mailWriter = MailWriterConstructor.Invoke(new object[] { result });
			SendMethod.Invoke(self, Flags, null, IsRunningInDotNetFourPointFive ? new[] { mailWriter, true, true } : new[] { mailWriter, true }, null);
			result = new MemoryStream(result.ToArray());
			CloseMethod.Invoke(mailWriter, Flags, null, new object[] { }, null);
			return result;
		}
	}
}
