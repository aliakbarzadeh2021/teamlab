using System;
using System.Runtime.Serialization;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using StanzaError = agsXMPP.protocol.client.Error;
using StreamError = agsXMPP.protocol.Error;

namespace ASC.Xmpp.Server
{
	public class JabberException : Exception
	{
		private StreamErrorCondition streamErrorCondition;

		private ErrorCode errorCode;

		public bool CloseStream
		{
			get;
			private set;
		}

		public bool StreamError
		{
			get;
			private set;
		}

		public JabberException(string message, Exception innerException)
			: base(message, innerException)
		{
			StreamError = false;
			errorCode = ErrorCode.InternalServerError;
		}

		public JabberException(StreamErrorCondition streamErrorCondition)
			: this(streamErrorCondition, true)
		{

		}

		public JabberException(StreamErrorCondition streamErrorCondition, bool closeStream)
			: base()
		{
			StreamError = true;
			CloseStream = closeStream;
			this.streamErrorCondition = streamErrorCondition;
		}

		public JabberException(ErrorCode errorCode)
			: base()
		{
			StreamError = false;
			CloseStream = false;
			this.errorCode = errorCode;
		}

		protected JabberException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public virtual Element ToElement()
		{
			return StreamError ? (Element)new StreamError(streamErrorCondition) : (Element)new StanzaError(errorCode);
		}
	}
}
