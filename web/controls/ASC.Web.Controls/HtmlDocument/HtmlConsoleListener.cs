// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Diagnostics;

namespace ASC.Web.Controls
{
    internal class HtmlConsoleListener : System.Diagnostics.TraceListener
    {
        public override void WriteLine(string Message)
        {
            Write(Message + "\n");
        }

        public override void Write(string Message)
        {
            Write(Message, "");
        }

        public override void Write(string Message, string Category)
        {
            Console.Write("T:" + Category + ": " + Message);
        }

        public override void WriteLine(string Message, string Category)
        {
            Write(Message + "\n", Category);
        }
    }

}
