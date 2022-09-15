using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace MailUtils.Mail.Net45
{
	public class MailClient : MailClientBase
	{
		public MailClient(MailClientConfigurationDto dto) : base(dto)
		{
			
		}

		public override MailBuilderBase GetMailBuilder()
		{
			return new MailUtils.Mail.Net45.MailBuilder();
		}

		public override void Send(object messageObject, bool ignoreErrorsWhenSending = false)
		{
			// mail message
			var mailMessage = messageObject as MailMessage;
			if (mailMessage == null)
				throw new ArgumentException("The messageObject must be type of System.Net.Mail.MailMessage!", "messageObject");

			if (!string.IsNullOrWhiteSpace(configDto.From) && string.IsNullOrWhiteSpace(mailMessage.From.Address))
				mailMessage.From = new MailAddress(configDto.From);

			using (var client = new SmtpClient())
			{
				// smtp client
				client.Host = configDto.Host;
				client.Port = configDto.Port;

				client.DeliveryMethod = configDto.DeliveryMethod;
				client.PickupDirectoryLocation = configDto.SpecifiedPickupDirectory;

				client.UseDefaultCredentials = configDto.UseDefaultCredentials;
				if (!string.IsNullOrWhiteSpace(configDto.UserName) && !string.IsNullOrWhiteSpace(configDto.Password) )
					client.Credentials = new NetworkCredential(configDto.UserName, configDto.Password);

				switch (configDto.SslMode)
				{
					case MailClientSslModeEnum.NoSsl:
						client.EnableSsl = false;
						break;
					case MailClientSslModeEnum.ExplicitSsl:
						client.EnableSsl = true;
						break;
					default:
						throw new ConfigurationErrorsException(string.Format("SSL mode: '{0}' not supported by '{1}'", configDto.SslMode, GetType().FullName));
				}

				client.Timeout= configDto.Timeout;

				// send
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
