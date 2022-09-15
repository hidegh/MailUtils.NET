namespace MailUtils.Mail
{
	/// <summary>
	/// Config. setting used by mail builders.
	/// </summary>
	public class MailBuilderConfig
	{
		/// <summary>
		/// TRUE will remove culture specific accents from the file name.
		/// Some MIME parsers/creators may generate a MIME that is not fully complatible with outlook, leading to filenames like "Untitled document.dat"...
		/// </summary>
		public static bool AttachmentNameAccentStripping { get; set; }

		/// <summary>
		/// If attachment name is too long, some MIME parsers/creators may generate a MIME that is not fully complatible with outlook, leading to filenames like "Untitled document.dat"...
		/// We can avoid this if we keep attachment name short (default: 64 chars).
		/// </summary>
		public static int AttachmentNameMaximizeLengthTo { get; set; }

		/// <summary>
		/// If we use HTML and call SetBody() the mail's subject will be set from the HTML title when this property is TRUE and mail's subject was not set before...
		/// </summary>
		public static bool UseHtmlTitleForSubject { get; set; }

		/// <summary>
		/// If we use HTML and call SetBody() the CSS will be moved to inline style attributes if this property is set.
		/// </summary>
		public static bool UseCssInliner { get; set; }

		/// <summary>
		/// If UseCssInliner is set, this CSS (string) will be appended to the HTML prior moving CSS attributes to inline style.
		/// </summary>
		public static string DefaultCss { get; set; }

		/// <summary>
		/// Logo image bytes...
		/// </summary>
		public static byte[] LogoImage { get; set; }

		/// <summary>
		/// Logo content id
		/// </summary>
		public static string  LogoCid { get; set; }

		/// <summary>
		/// ctor - with defaults
		/// </summary>
		static MailBuilderConfig()
		{
			AttachmentNameAccentStripping = true;
			AttachmentNameMaximizeLengthTo = 64;

			UseHtmlTitleForSubject = true;

			UseCssInliner = true;
			DefaultCss = string.Empty;

			LogoImage = new byte[0];
			LogoCid = "logo";
		}
	}
}
