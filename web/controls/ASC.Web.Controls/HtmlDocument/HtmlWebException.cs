// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;

namespace ASC.Web.Controls
{
    /// <summary>
    /// Represents an exception thrown by the HtmlWeb utility class.
    /// </summary>
    public class HtmlWebException : Exception
    {
        /// <summary>
        /// Creates an instance of the HtmlWebException.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        public HtmlWebException(string message)
            : base(message)
        {
        }
    }
}
