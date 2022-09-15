using System.Configuration;
using System.Net.Configuration;
using System.Net.Mail;

namespace MailUtils.Mail
{
	/// <summary>
	/// This DTO ca be configured from:
	/// 1) System.Net.Configuration.SmtpSection
	/// 2) or from FinIS.Common.Utils.Mail.MailClientConfigurationSection
	/// 
	/// CONFIG EXAMPLE:
	/// <configuration>
	///		<configSections>
	///			<sectionGroup name="system.net">
	///				<sectionGroup name="mailSettings">
	///					<section name="customSmtp" type="FinIS.Common.Utils.Mail.MailClientConfigurationSection, FinIS.Common"/>
	///					<section name="customSmtp2" type="FinIS.Common.Utils.Mail.MailClientConfigurationSection, FinIS.Common"/>
	///				</sectionGroup>
	///			</sectionGroup>
	///		</configSections>
	/// </configuration>
	/// 
	/// <system.net>
	///		<mailSettings>
	///			<!-- the system s default smtp settings... -->
	///			<smtp />
	///			<!-- any custom smtp values of our kind / even in multiple sections if necessary... -->
	///			<customSmtp
	///				sslMode="implicitSsl"
	///			/>
	///			<customSmtp2 sslMode="noSsl" />
	///		</mailSettings>
	/// </system.net>
	/// 
	/// </summary>
	public class MailClientConfigurationDto
	{
		public const string DefaultSystemNetMailSettingsSmtpSection = "system.net/mailSettings/smtp";

		public string Host;
		public int Port;

		public SmtpDeliveryMethod DeliveryMethod;
		public string SpecifiedPickupDirectory;

		public string UserName;
		public string Password;
		public bool UseDefaultCredentials;

		public MailClientSslModeEnum SslMode;

		public int Timeout;

		public string From;

		/// <summary>
		/// ctor - will load default values from machine.config's (or app/web.config's) System.Net.Configuration.SmtpSection.
		/// </summary>
		/// <param name="loadNetDefaults"></param>
		public MailClientConfigurationDto(bool loadNetDefaults = true)
		{
			SetDefaults();
			LoadFromSystemNetMailSettingsStmpSection(DefaultSystemNetMailSettingsSmtpSection, false);
		}

		public void SetDefaults()
		{
			Host = "127.0.0.1";
			Port = 25;

			DeliveryMethod = SmtpDeliveryMethod.Network;
			SpecifiedPickupDirectory = string.Empty;

			UserName = string.Empty;
			Password = string.Empty;
			UseDefaultCredentials = true;

			SslMode = MailClientSslModeEnum.NoSsl;

			Timeout = 100000;

			From = string.Empty;
		}

		private object GetSection(string sectionName, bool exceptionWhenNotFound = true)
		{
			var section = ConfigurationManager.GetSection(sectionName);

			if (section == null && exceptionWhenNotFound)
				throw new ConfigurationErrorsException(string.Format("Configuration section: '{0}' was not found!", sectionName));

			return section;
		}

		/// <summary>
		/// Loads values from a System.Net.Configuration.SmtpSection section.
		/// Since the SmtpSection itself defines default values, all previous values will be overwritten.
		/// 
		/// NOTE:
		/// Some of the values (string.Empty or 0) could be interpreted as a value not-set...
		/// ...and so avoid overwriting of existing values inside DTO
		/// ...but for properties like enableSsl an exception/additional parameter would be needed to decide wheter to overwrite or not.
		/// 
		/// Since SmtpSection are used as base-values, i decided to simply overwrite everything and keeping things simple.
		/// 
		/// NOTE:
		/// Send() in Elmah's ErrorMailModule has also a problem with this - if useSSL is true for SmtpSection and it's not set in Elmah's section, then Elmah's default false value will override the value from SmtpSection.
		/// </summary>
		/// <param name="smtpSectionPath"></param>
		/// <param name="exceptionWhenNotFound"></param>
		public void LoadFromSystemNetMailSettingsStmpSection(string smtpSectionPath, bool exceptionWhenNotFound = true)
		{
			var section = GetSection(smtpSectionPath, exceptionWhenNotFound) as SmtpSection;
			if (section == null)
				return;

			// Set all values - .NET has everywhere it's defaults, so they will also be set, but NULL string vlaues will be changed to string.Empty
			Host = section.Network.Host ?? string.Empty;
			Port = section.Network.Port;

			DeliveryMethod = section.DeliveryMethod;
			SpecifiedPickupDirectory = section.SpecifiedPickupDirectory.PickupDirectoryLocation ?? string.Empty;

			UserName = section.Network.UserName ?? string.Empty;
			Password = section.Network.Password ?? string.Empty;
			UseDefaultCredentials = section.Network.DefaultCredentials;

			SslMode = section.Network.EnableSsl ? MailClientSslModeEnum.ExplicitSsl : MailClientSslModeEnum.NoSsl;

			Timeout = 100000;

			From = section.From ?? string.Empty;
		}

		/// <summary>
		/// Loads values from a FinIS.Common.Utils.Mail.MailClientConfigurationSection section.
		/// Since the section uses NULL / 0 / string.Empty as a masker for values not set, only those config values will be overwritten, which wese set in the given section.
		/// This allows us f.e. to use/load .NET's system.net|mailSettings|smtp settings and override only a part from it (f.e. changing only SslMode to Implicit and port to 465).
		/// </summary>
		/// <param name="settingsSectionPath"></param>
		/// <param name="exceptionWhenNotFound"></param>
		public void LoadFromMailClientSettingsSection(string settingsSectionPath, bool exceptionWhenNotFound = true)
		{
			var section = GetSection(settingsSectionPath, exceptionWhenNotFound) as MailClientConfigurationSection;
			if (section == null)
				return;

			//
			// NOW SET VALUES, BUT ONLY IF THEY'RE SET (thus overriding only the desired values from the current configuration).
			// 
			// NOTE:
			// Similar code is in the Send() method implementation inside Elmah's ErrorMailModule.
			// Our first MailBuilder (which was tightly bound with System.Net.Mail.SmtpClient) used similar approach to configure/override some of the .NET's system.net|mailSettings|smtp settings.
			//

			// Host, port
			var host = section.Host ?? string.Empty;
			if (host.Length > 0)
				Host = host;

			var port = section.Port;
			if (port > 0)
				Port = port;

			// Delivery mode
			var smtpDeliveryMethod = section.DeliveryMethod;
			if (smtpDeliveryMethod.HasValue)
				DeliveryMethod = smtpDeliveryMethod.Value;

			var specifiedPickupDirectory = section.PickupDirectoryLocation ?? string.Empty;
			if (specifiedPickupDirectory.Length > 0)
				SpecifiedPickupDirectory = specifiedPickupDirectory;

			// Credentials
			var userName = section.UserName ?? string.Empty;
			if (userName.Length > 0)
				UserName = userName;

			var password = section.Password ?? string.Empty;
			if (password.Length > 0)
				Password = password;

			var useDefaultCredentials = section.UseDefaultCredentials;
			if (useDefaultCredentials.HasValue)
				UseDefaultCredentials = useDefaultCredentials.Value;

			// Ssl
			var useSsl = section.SslMode;
			if (useSsl.HasValue)
				SslMode = useSsl.Value;

			// Timeout
			var timeout = section.Timeout;
			if (timeout > 0)
				Timeout = timeout;

			// From
			var from = section.From ?? string.Empty;
			if (from.Length > 0)
				From = from;
		}
	}
}
