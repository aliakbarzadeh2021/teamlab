﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using agsXMPP.Xml;
using ASC.Xmpp.Server.Statistics;
using ASC.Xmpp.Server.Utils;
using agsXMPP.Xml.Dom;
using log4net;

namespace ASC.Xmpp.Server.Gateway
{
    class TcpXmppConnection : IXmppConnection
    {
        private readonly long _maxPacket;
        protected Stream sendStream;

        protected Stream recieveStream;

        private byte[] buffer;
        private long packetSize = 0;

        private StreamParser streamParser;

        private bool streamEndFired;

        private bool closed;

        private static readonly ILog log = LogManager.GetLogger(typeof(TcpXmppConnection));

        private EndPoint remoteEndPoint;

        private ICollection<Node> notSendedBuffer = new List<Node>();


        public TcpXmppConnection(Socket socket, long maxPacket)
        {
            _maxPacket = maxPacket;
            if (socket == null) throw new ArgumentNullException("socket");

            Id = UniqueId.CreateNewId();
            streamEndFired = false;
            closed = false;

            streamParser = new StreamParser();
            streamParser.Reset();
            streamParser.OnStreamStart += StreamParserOnStreamStart;
            streamParser.OnStreamElement += StreamParserOnStreamElement;
            streamParser.OnStreamEnd += StreamParserOnStreamEnd;

            buffer = new byte[socket.ReceiveBufferSize];
            remoteEndPoint = socket.RemoteEndPoint;

            sendStream = recieveStream = new NetworkStream(socket, true);

            log.DebugFormat("Create new connection {0} with {1}", Id, remoteEndPoint);
        }

        #region IXmppConnection Members

        public string Id
        {
            get;
            private set;
        }

        public void Reset()
        {
            streamParser.Reset();
        }

        public void Close()
        {
            lock (this)
            {
                if (closed) return;
                closed = true;

                try
                {
                    OnStreamEnd();
                }
                catch { }
                try
                {
                    var handler = Closed;
                    if (handler != null) handler(this, new XmppConnectionCloseEventArgs());
                }
                catch { }

                try
                {
                    sendStream.Close();
                    sendStream = null;
                    recieveStream.Close();
                    recieveStream = null;
                }
                catch { }

                log.DebugFormat("Close connection {0} with {1}", Id, remoteEndPoint);
            }
        }

        public void Send(Node node, Encoding encoding)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (encoding == null) throw new ArgumentNullException("encoding");
            Send(encoding.GetBytes(node.ToString(encoding)), node);
        }

        public void Send(string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            if (encoding == null) throw new ArgumentNullException("encoding");

            Send(encoding.GetBytes(text), null);
        }

        public void BeginReceive()
        {
            try
            {
                if (recieveStream != null && recieveStream.CanRead)
                {
                    recieveStream.BeginRead(buffer, 0, buffer.Length, BeginReadCallback, recieveStream);
                }
            }
            catch (ObjectDisposedException) { }
        }

        public void SetStreamTransformer(IStreamTransformer transformer)
        {
            //recieveStream = transformer.TransformInputStream(recieveStream);
            //sendStream = transformer.TransformOutputStream(sendStream);
        }

        public event EventHandler<XmppStreamStartEventArgs> XmppStreamStart;

        public event EventHandler<XmppStreamEndEventArgs> XmppStreamEnd;

        public event EventHandler<XmppStreamEventArgs> XmppStreamElement;

        public event EventHandler<XmppConnectionCloseEventArgs> Closed;

        #endregion


        private void BeginReadCallback(IAsyncResult asyncResult)
        {
            try
            {
                var stream = (Stream)asyncResult.AsyncState;
                int readed = stream.EndRead(asyncResult);
                if (0 < readed)
                {
                    streamParser.Push(buffer, 0, readed);
                    BeginReceive();
                    NetStatistics.ReadBytes(readed);
                    packetSize += readed;
                    if (packetSize > _maxPacket)
                    {
                        throw new ArgumentException("request-too-large");
                    }
                }
                else
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                LogErrorAndCloseConnection(e, "BeginReadCallback");
            }
        }

        private void Send(byte[] buffer, Node node)
        {
            NetStatistics.WriteBytes(buffer.Length);
            sendStream.BeginWrite(buffer, 0, buffer.Length, BeginWriteCallback, new object[] { sendStream, node });
        }

        private void BeginWriteCallback(IAsyncResult asyncResult)
        {
            var array = (object[])asyncResult.AsyncState;
            var stream = (Stream)array[0];
            var node = array[1] as Node;
            try
            {
                stream.EndWrite(asyncResult);
            }
            catch (Exception e)
            {
                if (node != null)
                {
                    lock (notSendedBuffer) notSendedBuffer.Add(node);
                }
                LogErrorAndCloseConnection(e, "BeginWriteCallback");
            }
        }

        private void StreamParserOnStreamStart(object sender, Node e, string streamNamespace)
        {
            packetSize = 0;
            var handler = XmppStreamStart;
            if (handler != null) handler(this, new XmppStreamStartEventArgs(Id, e, streamNamespace));
        }

        private void StreamParserOnStreamElement(object sender, Node e)
        {
            packetSize = 0;
            var handler = XmppStreamElement;
            if (handler != null) handler(this, new XmppStreamEventArgs(Id, e));
        }

        private void StreamParserOnStreamEnd(object sender, Node e)
        {
            packetSize = 0;
            OnStreamEnd();
        }

        private void OnStreamEnd()
        {
            if (streamEndFired) return;

            streamEndFired = true;
            var handler = XmppStreamEnd;
            try
            {
                if (handler != null) handler(this, new XmppStreamEndEventArgs(Id, notSendedBuffer));
            }
            finally
            {
                notSendedBuffer.Clear();
            }
        }


        private void LogErrorAndCloseConnection(Exception error, string method)
        {
            if (error is ObjectDisposedException ||
                error.InnerException is ObjectDisposedException ||
                error is IOException)
            {
                //ignore
            }
            else
            {
                log.ErrorFormat("Error {0} connection {1} with {2}: {3}", method, Id, remoteEndPoint, error);
            }
            Close();
        }
    }
}