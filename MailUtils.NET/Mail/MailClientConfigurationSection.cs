using System.Configuration;
using System.Net.Mail;

namespace MailUtils.Mail
{
	/// <summary>
	/// Special configuration section, which instead of default values returns NULL, 0, string.Empty values.
	/// 
	/// NOTE:
	/// The defaults are not implemented at XML configuration-level, but at ConfigurationDto level.
	/// So we can use a concrete settings and override only part of the values from a different one (settings).
	/// </summary>
	public class MailClientConfigurationSection: ConfigurationSection
	{
		[ConfigurationProperty("host", IsRequired = false, DefaultValue = "")]
		public string Host
		{
			get { return (string) this["host"]; }
			set { this["host"] = value; }
		}

		[ConfigurationProperty("port", IsRequired = false, DefaultValue = 0)]
		public int Port
		{
			get { return (int)this["port"]; }
			set { this["port"] = value; }
		}
		
		//

		[ConfigurationProperty("deliveryMethod", IsRequired = false, DefaultValue = null)]
		public SmtpDeliveryMethod? DeliveryMethod
		{
			get { return (SmtpDeliveryMethod?)this["deliveryMethod"]; }
			set { this["deliveryMethod"] = value; }
		}

		[ConfigurationProperty("pickupDirectoryLocation", IsRequired = false, DefaultValue = "")]
		public string PickupDirectoryLocation
		{
			get { return (string)this["pickupDirectoryLocation"]; }
			set { this["pickupDirectoryLocation"] = value; }
		}

		//

		[ConfigurationProperty("userName", IsRequired = false, DefaultValue = "")]
		public string UserName
		{
			get { return (string)this["userName"]; }
			set { this["userName"] = value; }
		}		
		
		[ConfigurationProperty("password", IsRequired = false, DefaultValue = "")]
		public string Password
		{
			get { return (string)this["password"]; }
			set { this["password"] = value; }
		}

		[ConfigurationProperty("defaultCredentials", IsRequired = false, DefaultValue = null)]
		public bool? UseDefaultCredentials
		{
			get { return (bool?)this["defaultCredentials"]; }
			set { this["defaultCredentials"] = value; }
		}		

		//

		[ConfigurationProperty("sslMode", IsRequired = false, DefaultValue = null)]
		public MailClientSslModeEnum? SslMode
		{
			get { return (MailClientSslModeEnum?)this["sslMode"]; }
			set { this["sslMode"] = value; }
		}
	
		//

		[ConfigurationProperty("timeout", IsRequired = false, DefaultValue = 0)]
		public int Timeout
		{
			get { return (int)this["timeout"]; }
			set { this["timeout"] = value; }
		}

		//

		[ConfigurationProperty("from", IsRequired = false, DefaultValue = "")]
		public string From
		{
			get { return (string)this["from"]; }
			set { this["from"] = value; }
		}
	}
}
