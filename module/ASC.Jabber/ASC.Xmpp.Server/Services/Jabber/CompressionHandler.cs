﻿using System;
using System.IO;
using agsXMPP.protocol.extensions.compression;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using agsXMPP.Xml.Dom;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Compress))]
	class CompressionHandler : XmppStreamHandler
	{
		public override void ElementHandle(XmppStream stream, Element element, XmppHandlerContext context)
		{
			context.Sender.SendTo(stream, new Compressed());
			var connection = context.Sender.GetXmppConnection(stream.ConnectionId);
			connection.SetStreamTransformer(new GZipTransformer());
			context.Sender.ResetStream(stream);
		}

		public override void OnRegister(IServiceProvider serviceProvider)
		{
			//ManagedZLib.ManagedZLib.Initialize();
		}

		public override void OnUnregister(IServiceProvider serviceProvider)
		{
			//ManagedZLib.ManagedZLib.Terminate();
		}
	}

	class GZipTransformer : IStreamTransformer
	{
		public Stream TransformInputStream(Stream inputStream)
		{
			//return new DeflateStream(inputStream, CompressionMode.Decompress, true);
			//return new GZipStream(inputStream, CompressionMode.Decompress, true);
			//return new ZipInputStream(inputStream);
			//return new ManagedZLib.CompressionStream(inputStream, ManagedZLib.CompressionOptions.Decompress) {  };
			throw new NotImplementedException();
		}

		public Stream TransformOutputStream(Stream outputStream)
		{
			//return new DeflateStream(outputStream, CompressionMode.Compress, true);
			//return new GZipStream(outputStream, CompressionMode.Compress, true);
			//return new ZipOutputStream(outputStream);
			//return new ManagedZLib.CompressionStream(outputStream, ManagedZLib.CompressionOptions.Compress);
			throw new NotImplementedException();
		}
	}
}
