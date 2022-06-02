using System.IO;
using ASC.Common.Notify.Patterns;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;
using Textile;
using Textile.Blocks;

namespace ASC.Notify.Textile
{
    public class TextileStyler:IPatternStyler
    {
        static TextileStyler()
        {
            BlockAttributesParser.Styler = new StyleReader(LoadResourceString("style.css"));
            Master = Resources.TemplateResource.HtmlMaster;
        }

        protected static string Master { get; set; }

        public void ApplyFormating(NoticeMessage message)
        {
            var output = new StringBuilderTextileFormatter();
            var formatter = new TextileFormatter(output);
            formatter.Format(message.Body);
            message.Body = Master.Replace("%CONTENT%", output.GetFormattedText());
        }

        internal static Stream LoadResource(string name)
        {
            return typeof(TextileStyler).Assembly.GetManifestResourceStream(string.Format("ASC.Notify.Textile.Resources.{0}",name));
        }

        internal static string LoadResourceString(string name)
        {
            using (var reader = new StreamReader(LoadResource(name)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}