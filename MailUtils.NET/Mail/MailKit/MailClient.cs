using System;
using System.Configuration;
using System.IO;
using System.Net;
using MailKit.Net.Smtp;
using MimeKit;

namespace MailUtils.Mail.MailKit
{
	public class MailClient : MailClientBase
	{
		public MailClient(MailClientConfigurationDto dto)
			: base(dto)
		{

		}

		public override MailBuilderBase GetMailBuilder()
		{
			return new MailUtils.Mail.MailKit.MailBuilder();
		}

		public override void Send(object messageObject, bool ignoreErrorsWhenSending = false)
		{
			// mail message
			var mailMessage = messageObject as MimeMessage;
			if (mailMessage == null)
				throw new ArgumentException("The messageObject must be type of MimeKit.MimeMessage!", "messageObject");

			if (!string.IsNullOrWhiteSpace(configDto.From) && mailMessage.From.Count == 0)
				mailMessage.From.Add(InternetAddress.Parse(configDto.From));

			// check host and port
			if (string.IsNullOrWhiteSpace(configDto.Host))
				throw new ConfigurationErrorsException("The host name cannot be empty.");

			if (configDto.Port < 0 || configDto.Port > 65535)
				throw new ConfigurationErrorsException("The port number is out of range (allowed is 0-65535).");

			// pickup directories vs. transfer
			var pickupDirectory = string.Empty;

			switch (configDto.DeliveryMethod)
			{
				case System.Net.Mail.SmtpDeliveryMethod.Network:
					break;
				case System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory:
					pickupDirectory = configDto.SpecifiedPickupDirectory;
					break;
				case System.Net.Mail.SmtpDeliveryMethod.PickupDirectoryFromIis:
					pickupDirectory = Helpers.GetIisPickupDirectory();
					break;
				default:
					throw new ConfigurationErrorsException(string.Format("Delivery method: '{0}' not supported by '{1}'", configDto.SslMode, GetType().FullName));
			}

			if (!string.IsNullOrWhiteSpace(pickupDirectory) && !System.IO.Path.IsPathRooted(pickupDirectory))
				throw new ConfigurationErrorsException("PickupDirectoryLocation must not be a relative directory - it must be rooted!");

			// transfer mode
			switch (configDto.SslMode)
			{
				case MailClientSslModeEnum.NoSsl:
					break;
				case MailClientSslModeEnum.ExplicitSsl:
					break;
				case MailClientSslModeEnum.ImplicitSsl:
					break;
				default:
					throw new ConfigurationErrorsException(string.Format("SSL mode: '{0}' not supported by '{1}'", configDto.SslMode, GetType().FullName));
			}

			// when pickup directory is defined, we will write message to the directory instead of sending
			if (!string.IsNullOrWhiteSpace(pickupDirectory))
			{
				var fileName = Guid.NewGuid().ToString() + ".eml";
				var fullPath = System.IO.Path.Combine(pickupDirectory, fileName);

				try
				{
					using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
					{
						mailMessage.WriteTo(fs);
					}
				}
				catch
				{
					// NOTE: writing to iis/other pickup directory is counted as sending, so therefore the try/catch block...
					if (!ignoreErrorsWhenSending)
						throw;
				}

				return;
			}

			// send message
			using (var client = new SmtpClient())
			{
				// timeout
				var timeout = configDto.Timeout;
				client.Timeout = timeout;

				// connect - if port is 0 then port 25 or 465 will be used, depending on smtp/smtps
				var host = configDto.Host;
				var port = configDto.Port;
				var implicitSsl = configDto.SslMode == MailClientSslModeEnum.ImplicitSsl;
				var explicitSsl = configDto.SslMode == MailClientSslModeEnum.ExplicitSsl;

				var protocolSmtp = "smtp";
				var protocolSmtps = "smtps";
				var format = (port > 0) ? "{0}://{1}:{2}/?starttls={3}" : "{0}://{1}/?starttls={3}";
				var uri = string.Format(format,
					implicitSsl ? protocolSmtps : protocolSmtp,
					host,
					port,
					explicitSsl
					);

				client.Connect(new Uri(uri));

				// authenticate (same logic as in .NET SmtpClient)
				//
				// NOTE:
				//	SmtpClient.UseDefaultCredentials Property:
				//	If the UseDefaultCredentials property is set to false, then the value set in the Credentials property will be used for the credentials when connecting to the server.
				//	If the UseDefaultCredentials property is set to false and the Credentials property has not been set, then mail is sent to the server anonymously.
				var useDefaultCredentials = configDto.UseDefaultCredentials;
				var userName = configDto.UserName;
				var password = configDto.Password;

				if (useDefaultCredentials)
				{
					// authenticate with default credentials...
					client.Authenticate(CredentialCache.DefaultNetworkCredentials.UserName, CredentialCache.DefaultNetworkCredentials.Password);
				}
				else
				{
					// when credentials are set, authenticate with them...
					if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
						client.Authenticate(userName, password);
				}

				// send it!
				try
				{
					client.Send(mailMessage);
				}
				catch
				{
					if (!ignoreErrorsWhenSending)
						throw;
				}
			}
		}
	}
}
