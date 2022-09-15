namespace MailUtils.Mail
{
	public enum MailClientSslModeEnum
	{
		NoSsl,
		ExplicitSsl,

		/// <summary>
		/// NOTE: still nmot supported in .net 4.5.2 
		/// </summary>
		ImplicitSsl
	}
}
