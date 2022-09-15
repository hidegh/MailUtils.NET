namespace MailUtils.Mail
{
	public interface IMailClient
	{
		MailBuilderBase GetMailBuilder();

		void Send(object messageObject, bool ignoreErrorsWhenSending = false);
	}
}