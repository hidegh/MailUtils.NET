using System;
using System.Linq;
using System.Reflection;

namespace MailUtils.Mail
{
	public class Helpers
	{
		/// <summary>
		/// NOTE: “Cannot get IIS pickup directory.” error
		/// Unfortunately, this exception is raised when any kind of problem occurs, while trying to determine the location of IIS/SMTP pickup directory.
		/// A common cause is simply missing IIS SMTP service.
		/// 
		/// The pickup directory is stored in the IIS Metabase, so if the account that your web-app runs as does not have access to the required nodes, this error can be thrown. Metabase permissions are separate from file permissions, so you explore it with Metabase explorer (part of the IIS resource kit).
		/// These nodes need to have read permission given to your web-app user: \LM, \LM\Smtpsrv\ and \LM\Smtpsrv\1
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.Net.Mail.SmtpException">Cannot get IIS pickup directory.at System.Net.Mail.IisPickupDirectory.GetPickupDirectory().</exception>
		public static string GetIisPickupDirectory()
		{
			// get system.dll
			var systemAssembly = AppDomain.CurrentDomain
				.GetAssemblies()
				.First(x => x.GetName().Name == "System");

			// get iisPickupType from a fully qualified assembly name...
			var iisPickupType = Type.GetType(
				string.Format("System.Net.Mail.IisPickupDirectory, {0}", systemAssembly.GetName().FullName),
				true);

			// get pickup folder
			var pickupFolder = (string) iisPickupType.InvokeMember(
				"GetPickupDirectory",
				BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic,
				null, null, null);

			// return pickup folder
			return pickupFolder;
		}
	}
}
