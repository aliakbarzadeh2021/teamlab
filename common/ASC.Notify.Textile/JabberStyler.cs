using System;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Common.Notify.Patterns;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;

namespace ASC.Notify.Textile
{
    public class JabberStyler : IPatternStyler
    {
        static readonly Regex linkReplacer = new Regex(@"""(?<text>[\w\W]+?)"":""(?<link>[^""]+)""", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex textileReplacer = new Regex(@"(h1\.|h2\.|\*|h3\.|p\.)", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex BrReplacer = new Regex(@"<br\s*\/*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Singleline);
        static readonly Regex TagReplacer = new Regex(@"<(.|\n)*?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.Singleline);

        public void ApplyFormating(NoticeMessage message)
        {
            if (!string.IsNullOrEmpty(message.Body))
            {
                var lines = message.Body.Split(new[] {Environment.NewLine, "\n"}, StringSplitOptions.RemoveEmptyEntries);
                string body = string.Empty;
                foreach(var line in lines)
                    body += linkReplacer.Replace(line, EvalLink) + Environment.NewLine;                   
                body = textileReplacer.Replace(HttpUtility.HtmlDecode(body), ""); //Kill textile markup
                body = BrReplacer.Replace(body, Environment.NewLine);
                body = TagReplacer.Replace(body, Environment.NewLine);
                message.Body = body;
            }
        }

        private static string EvalLink(Match match)
        {
            if (match.Success)
            {
                if (match.Groups["text"].Success && match.Groups["link"].Success)
                {
                    if (match.Groups["text"].Value.Equals(match.Groups["link"].Value,StringComparison.OrdinalIgnoreCase))
                    {
                        return match.Groups["text"].Value;
                    }
                    return match.Groups["text"].Value + string.Format(" ( {0} )", match.Groups["link"].Value);
                }
            }
            return match.Value;
        }
    }
}