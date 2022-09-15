namespace MailUtils.Mail
{
	public abstract class MailClientBase : IMailClient
	{
		protected MailClientConfigurationDto configDto;

		protected MailClientBase(MailClientConfigurationDto dto)
		{
			configDto = dto;
		}

		public abstract MailBuilderBase GetMailBuilder();

		public abstract void Send(object messageObject, bool ignoreErrorsWhenSending = false);
	}
}
