namespace MailUtils.Mail
{
	public interface IMailBuilder
	{
		IMailBuilder AddRecipentTo(params string[] to);
		IMailBuilder AddRecipentCc(params string[] cc);
		IMailBuilder AddRecipentBcc(params string[] bcc);

		IMailBuilder SetFrom(string from);
		IMailBuilder SetSubject(string subject);

		IMailBuilder SetHtmlBody(string html);
		IMailBuilder SetTextBody(string text);

		IMailBuilder AddAttachment(byte[] bytes, string name, string contentId = null);

		IMailBuilder AddLinkedResource(byte[] bytes, string contentId = null);

		object GetMessageObject();

		byte[] GetMessageBytes();
	}
}