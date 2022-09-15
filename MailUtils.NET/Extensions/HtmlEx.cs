using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace MailUtils.Extensions
{
	public static class HtmlEx
	{
		private static Regex startsWithHtmlRegex = new Regex(@"\A\s*(<!DOCTYPE HTML[^>]*>\s*)?<HTML[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
		private static Regex endsWithHtmlRegex = new Regex(@"</HTML>\s*\z", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

		/// <summary>
		/// Check's if given string is a HTML (starts with HTML like syntax and ends with HTML tag + whitespaces)...
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsHtml(this string text)
		{
			var ms = startsWithHtmlRegex.Match(text);
			if (!ms.Success)
				return false;

			var me = endsWithHtmlRegex.Match(text);
			if (!me.Success)
				return false;

			return true;
		}

		/// <summary>
		/// If given input is HTML, this fnc. will return the content of the TITLE tag.
		/// </summary>
		/// <param name="htmlString"></param>
		/// <returns></returns>
		public static string GetHtmlTitle(this string htmlString)
		{
			// check if it's HTML
			if (!htmlString.IsHtml())
				throw new ArgumentException("htmlString");

			// load xml doc.
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(htmlString);

			// get & return title
			var titleNode = xmlDoc.SelectSingleNode("//html/head/title");
			if (titleNode != null)
				return titleNode.InnerText;

			// return empty string if no title found
			return "";
		}

		/// <summary>
		/// Appends CSS into the given HTML.
		/// </summary>
		/// <param name="htmlString"></param>
		/// <param name="cssString"></param>
		/// <returns></returns>
		public static string AppendCssToHtml(this string htmlString, string cssString)
		{
			// check if it's HTML
			if (!htmlString.IsHtml())
				throw new ArgumentException("htmlString");

			// load xml doc.
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(htmlString);

			// main node
			var htmlNode = xmlDoc.SelectSingleNode("//html");
			if (htmlNode == null)
				throw new ArgumentException("htmlString");

			// ensure head-node
			var headNode = xmlDoc.SelectSingleNode("//html/head");
			if (headNode == null)
			{
				headNode = xmlDoc.CreateElement("head");
				htmlNode.AppendChild(headNode);
			}

			// add custom style-node (css)
			var customCssNode = xmlDoc.CreateElement("style");

			var customCssNodeTypeAttribute = xmlDoc.CreateAttribute("type");
			customCssNodeTypeAttribute.Value = "text/css";

			customCssNode.Attributes.Append(customCssNodeTypeAttribute);

			customCssNode.InnerXml = cssString;

			headNode.AppendChild(customCssNode);

			// return result
			using (var sw = new StringWriter())
			{
				var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { OmitXmlDeclaration = true });
				xmlDoc.Save(xmlWriter);

				return sw.ToString();
			}
		}
	}
}
