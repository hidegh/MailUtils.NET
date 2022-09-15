using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using MailUtils.Extensions;

namespace MailUtils.Mail.Net45
{
	public class MailBuilder : MailBuilderBase
	{
		protected MailMessage mailMessage;

		protected string plainText;

		protected string htmlText;
		protected List<LinkedResource> linkedResourceCollection;

		public MailBuilder()
		{
			mailMessage = new MailMessage();
			mailMessage.Body = "";

			mailMessage.HeadersEncoding = Encoding.UTF8;
			mailMessage.BodyEncoding = Encoding.UTF8;
			mailMessage.BodyTransferEncoding = TransferEncoding.Unknown;

			plainText = null;
			htmlText = null;
			linkedResourceCollection = new List<LinkedResource>();
		}

		public override IMailBuilder AddRecipentTo(params string[] to)
		{
			to.ToList().ForEach(i => mailMessage.To.Add(i));
			return this;
		}

		public override IMailBuilder AddRecipentCc(params string[] cc)
		{
			cc.ToList().ForEach(i => mailMessage.CC.Add(i));
			return this;
		}

		public override IMailBuilder AddRecipentBcc(params string[] bcc)
		{
			bcc.ToList().ForEach(i => mailMessage.Bcc.Add(i));
			return this;
		}

		public override IMailBuilder SetFrom(string from)
		{
			mailMessage.From = new MailAddress(from);
			return this;
		}

		public override IMailBuilder SetSubject(string subject)
		{
			var modifiedSubject = GetConformedSubjectText(subject);
			mailMessage.Subject = modifiedSubject;
			return this;
		}

		public override IMailBuilder SetHtmlBody(string html)
		{
			html = EnsureHtml(html);

			if (MailBuilderConfig.UseHtmlTitleForSubject && string.IsNullOrWhiteSpace(mailMessage.Subject))
			{
				// set title...
				var title = html.GetHtmlTitle();

				if (!string.IsNullOrWhiteSpace(title))
				{
					var conformedTitle = GetConformedSubjectText(title);
					mailMessage.Subject = conformedTitle;
				}
			}

			if (MailBuilderConfig.LogoImage != null && MailBuilderConfig.LogoImage.Length > 0)
			{
				var hasReference = HasEmbeddedResourceWithId(html, MailBuilderConfig.LogoCid);
				var resource = linkedResourceCollection.FirstOrDefault(i => i.ContentId == MailBuilderConfig.LogoCid);

				if (hasReference && resource == null)
				{
					// add logo
					AddLinkedResource(MailBuilderConfig.LogoImage, MailBuilderConfig.LogoCid);
				}

				if (!hasReference && resource != null)
				{
					// remove logo
					linkedResourceCollection.Remove(resource);
				}
			}

			// modify html only at the end, reason: PreMailer may create a non valid HTML and that would raise an exception inside finalHtml.GetHtmlTitle()
			var finalHtml = GetPreprocessedHtml(html);
			htmlText = finalHtml;

			return this;
		}

		public override IMailBuilder SetTextBody(string text)
		{
			plainText = text;
			return this;
		}

		public override IMailBuilder AddAttachment(byte[] bytes, string name, string contentId = null)
		{
			var modifiedName = GetConformedAttachmentName(name);

			var attachment = new Attachment(new MemoryStream(bytes), modifiedName);

			if (contentId != null)
				attachment.ContentId = contentId;

			mailMessage.Attachments.Add(attachment);
			return this;
		}

		public override IMailBuilder AddLinkedResource(byte[] bytes, string contentId = null)
		{
			var linkedResource = new LinkedResource(new MemoryStream(bytes));

			if (contentId != null)
				linkedResource.ContentId = contentId;

			linkedResourceCollection.Add(linkedResource);
			return this;
		}

		public override object GetMessageObject()
		{
			// we overwrite previous views... (only our views will/can be there, so we can delete previous ones)
			mailMessage.AlternateViews.Clear();

			// add html and plain text view along with linked resources...
			if (!string.IsNullOrWhiteSpace(htmlText))
			{
				var htmlView = AlternateView.CreateAlternateViewFromString(htmlText, mailMessage.BodyEncoding, MediaTypeNames.Text.Html);
				mailMessage.AlternateViews.Add(htmlView);

				if (linkedResourceCollection.Count > 0)
				{
					foreach (var lr in linkedResourceCollection)
						htmlView.LinkedResources.Add(lr);
				}
			}

			if (!string.IsNullOrWhiteSpace(plainText))
			{
				var plainView = AlternateView.CreateAlternateViewFromString(plainText, mailMessage.BodyEncoding, MediaTypeNames.Text.Plain);
				mailMessage.AlternateViews.Add(plainView);
			}

			return mailMessage;
		}

		public override byte[] GetMessageBytes()
		{
			var messageObject = GetMessageObject();
			var mailMessage = messageObject as MailMessage;

			var ms = mailMessage.RawMessage();
			return ms.ToArray();
		}
	}
}
