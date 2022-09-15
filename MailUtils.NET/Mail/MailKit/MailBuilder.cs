using System;
using System.IO;
using System.Linq;
using MailUtils.Extensions;
using MimeKit;

namespace MailUtils.Mail.MailKit
{
	public class MailBuilder : MailBuilderBase
	{
		protected MimeMessage mimeMessage;
		protected BodyBuilder bodyBuilder;

		public MailBuilder()
		{
			mimeMessage = new MimeMessage();
			bodyBuilder = new BodyBuilder();
		}

		public override IMailBuilder AddRecipentTo(params string[] to)
		{
			to.ToList().ForEach(i => mimeMessage.To.Add(InternetAddress.Parse(i)));
			return this;
		}

		public override IMailBuilder AddRecipentCc(params string[] cc)
		{
			cc.ToList().ForEach(i => mimeMessage.Cc.Add(InternetAddress.Parse(i)));
			return this;
		}

		public override IMailBuilder AddRecipentBcc(params string[] bcc)
		{
			bcc.ToList().ForEach(i => mimeMessage.Bcc.Add(InternetAddress.Parse(i)));
			return this;
		}

		public override IMailBuilder SetFrom(string from)
		{
			mimeMessage.From.Add(InternetAddress.Parse(from));
			return this;
		}

		public override IMailBuilder SetSubject(string subject)
		{
			// NOTE:
			// MimeKit handles non-printable characters in subject well (far more better than .NET).
			// The modifying of subject's text is only to have almost the same behaviour whether we use .NET or MimeKit.
			var modifiedSubject = GetConformedSubjectText(subject);
			mimeMessage.Subject = modifiedSubject;
			return this;
		}

		public override IMailBuilder SetHtmlBody(string html)
		{
			html = EnsureHtml(html);

			if (MailBuilderConfig.UseHtmlTitleForSubject && string.IsNullOrWhiteSpace(mimeMessage.Subject))
			{
				// set title...
				var title = html.GetHtmlTitle();

				if (!string.IsNullOrWhiteSpace(title))
				{
					var conformedTitle = GetConformedSubjectText(title);
					mimeMessage.Subject = conformedTitle;
				}
			}

			if (MailBuilderConfig.LogoImage != null && MailBuilderConfig.LogoImage.Length > 0)
			{
				var hasReference = HasEmbeddedResourceWithId(html, MailBuilderConfig.LogoCid);
				var resource = bodyBuilder.LinkedResources.FirstOrDefault(i => i.ContentId == MailBuilderConfig.LogoCid);

				if (hasReference && resource == null)
				{
					// add logo
					AddLinkedResource(MailBuilderConfig.LogoImage, MailBuilderConfig.LogoCid);
				}

				if (!hasReference && resource != null)
				{
					// remove logo
					bodyBuilder.LinkedResources.Remove(resource);
				}
			}

			// modify html only at the end, reason: PreMailer may create a non valid HTML and that would raise an exception inside finalHtml.GetHtmlTitle()
			var finalHtml = GetPreprocessedHtml(html);
			bodyBuilder.HtmlBody = finalHtml;

			return this;
		}

		public override IMailBuilder SetTextBody(string text)
		{
			bodyBuilder.TextBody = text;
			return this;
		}

		public override IMailBuilder AddAttachment(byte[] bytes, string name, string contentId = null)
		{
			var modifiedName = GetConformedAttachmentName(name);
			var attachment = bodyBuilder.Attachments.Add(modifiedName, bytes);

			if (contentId != null)
				attachment.ContentId = contentId;

			return this;
		}

		public override IMailBuilder AddLinkedResource(byte[] bytes, string contentId = null)
		{
			var name = Guid.NewGuid().ToString();
			var linkedResource = bodyBuilder.LinkedResources.Add(name, bytes);

			if (contentId != null)
				linkedResource.ContentId = contentId;

			return this;
		}

		public override object GetMessageObject()
		{
			mimeMessage.Body = bodyBuilder.ToMessageBody();
			return mimeMessage;
		}

		public override byte[] GetMessageBytes()
		{
			var messageObject = GetMessageObject();
			var mailMessage = messageObject as MimeMessage;

			var ms = new MemoryStream();
			mailMessage.WriteTo(ms);

			return ms.ToArray();
		}
	}
}
