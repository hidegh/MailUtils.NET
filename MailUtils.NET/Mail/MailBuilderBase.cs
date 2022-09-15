using System;
using System.IO;
using System.Text.RegularExpressions;
using MailUtils.Extensions;

namespace MailUtils.Mail
{
	/// <summary>
	/// Images and Linked resources - are used together with HTML mail format.
	/// 
	/// Embedded images - most email clients will block them (use linked resources)...
	/// To insert embedded image (base 64): <img alt="Embedded Image" height="128" width="128" src="data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEASABIAAD....snip..." />
	/// 
	/// Linked resources and attachment's (if their content id is set) can be referenced like this:
	/// <img src="cid:contentId" alt="link to the image with the given content id" />
	/// <a href="cid:contentId">link to the logo with the given content id</a>
	/// 
	/// Regex to match all img sources: <img\s+[^>]*?src=["'](?<src>[^"']+)
	/// Regex to match all href's: <a\s+[^>]*?href\s*=\s*["'](?<src>[^"']+)
	/// 
	/// Regex to match our cid's:
	/// (<img\s+[^>]*?src=["']cid:logo["'])|(<a\s+[^>]*?href=["']cid:logo["'])
	/// </summary>
	public abstract class MailBuilderBase : IMailBuilder, IDisposable
	{
		protected bool HasEmbeddedResourceWithId(string html, string id)
		{
			var regex = new Regex(string.Format(@"(<img\s+[^>]*?src=[""']cid:{0}[""'])|(<a\s+[^>]*?href=[""']cid:{0}[""'])", id));
			var match = regex.Match(html);
			return match.Success;
		}

		/// <summary>
		/// There are problems with non-printable characters inside subject...so we must slightly modify mails subject.
		/// </summary>
		/// <param name="subject"></param>
		/// <returns></returns>
		protected string GetConformedSubjectText(string subject)
		{
			// NOTE: there are problems with non-printable characters inside subject...
			var adjustedText = subject
				.ReplaceNonPrintableCharacters(" ")
				.TrimWhitespaces()
				.SingletonizeSpaces();

			return adjustedText;
		}

		/// <summary>
		/// Outlook's weird behaviour of handling accents and long filenames forces us to take some action...
		/// </summary>
		/// <param name="originalName"></param>
		/// <returns></returns>
		protected string GetConformedAttachmentName(string originalName)
		{
			// NOTE: must trim name, if last char is space, it'll mess up extension in mail
			var adjustedName = originalName.Trim();

			// accents
			if (MailBuilderConfig.AttachmentNameAccentStripping)
				adjustedName = adjustedName.StripAccents();

			// max length...
			var maxLength = MailBuilderConfig.AttachmentNameMaximizeLengthTo;
			if (maxLength > 0 && adjustedName.Length > maxLength)
			{
				var extension = Path.GetExtension(adjustedName);
				var fileName = adjustedName.MaxLength(maxLength - extension.Length);

				adjustedName = fileName + extension;
			}

			return adjustedName;
		}

		/// <summary>
		/// If no text (html) is present, an empty, but valid HTML content will be returned.
		/// Otherwise the original text (html) is returned.
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		protected string EnsureHtml(string html)
		{
			if (string.IsNullOrWhiteSpace(html))
			{
				return @"<!DOCTYPE html>
<html>
<head>
	<title></title>
</head>
<body>

</body>
</html>
";
			}

			return html;
		}

		/// <summary>
		/// Inisde emails styles must be defined inline...
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		protected string GetPreprocessedHtml(string html)
		{
			var useCssInliner = MailBuilderConfig.UseCssInliner;
			var defaultCss = MailBuilderConfig.DefaultCss;

			if (useCssInliner && !string.IsNullOrWhiteSpace(defaultCss))
			{
				// append our default CSS to the HTML
				if (!string.IsNullOrWhiteSpace(defaultCss))
					html = html.AppendCssToHtml(defaultCss);

				// move CSS to inline style setting
				var r = PreMailer.Net.PreMailer.MoveCssInline(html);
				html = r.Html;
			}

			return html;
		}

		//
		//
		//

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~MailBuilderBase()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// free managed resources
				// ...
			}

			// free native resources if there are any - also add ~MailBuilderBase() { Dispose (false); } if there are unmanaged resources
			// ...
		}

		//
		//
		//

		public abstract IMailBuilder AddRecipentTo(params string[] to);
		public abstract IMailBuilder AddRecipentCc(params string[] cc);
		public abstract IMailBuilder AddRecipentBcc(params string[] bcc);
		public abstract IMailBuilder SetFrom(string @from);

		public abstract IMailBuilder SetSubject(string subject);
		
		public abstract IMailBuilder SetHtmlBody(string html);
		public abstract IMailBuilder SetTextBody(string text);
		
		public abstract IMailBuilder AddAttachment(byte[] bytes, string name, string contentId = null);
		
		public abstract IMailBuilder AddLinkedResource(byte[] bytes, string contentId = null);
	
		public abstract object GetMessageObject();
		public abstract byte[] GetMessageBytes();
	}
}
