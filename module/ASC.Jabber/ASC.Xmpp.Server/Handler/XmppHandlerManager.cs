﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Streams;
using log4net;
using Stanza = agsXMPP.protocol.Base.Stanza;
using StanzaError = agsXMPP.protocol.client.Error;

namespace ASC.Xmpp.Server.Handler
{
    public class XmppHandlerManager
    {
        private readonly XmppStreamManager streamManager;

        private readonly IXmppSender sender;

        private readonly XmppHandlerContext context;

        private readonly XmppHandlerStorage handlerStorage;

        private readonly XmppStreamValidator validator;

        private readonly XmppXMLSchemaValidator schemaValidator;

        private static readonly ILog log = LogManager.GetLogger(typeof(XmppHandlerManager));

        private const string RECIEVE_FORMAT = "Xmpp stream: connection {0}, namespace {1}\r\n\r\n(C) <<--------------------------------------\r\n{2}\r\n";


        public XmppHandlerManager(XmppStreamManager streamManager, IXmppReceiver receiver, IXmppSender sender, IServiceProvider serviceProvider)
        {
            if (streamManager == null) throw new ArgumentNullException("streamManager");
            if (receiver == null) throw new ArgumentNullException("receiver");
            if (sender == null) throw new ArgumentNullException("sender");
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            this.streamManager = streamManager;
            this.sender = sender;
            this.handlerStorage = new XmppHandlerStorage(serviceProvider);
            this.context = new XmppHandlerContext(serviceProvider);

            this.validator = new XmppStreamValidator();
            this.schemaValidator = new XmppXMLSchemaValidator();

            receiver.XmppStreamStart += XmppStreamStart;
            receiver.XmppStreamElement += XmppStreamElement;
            receiver.XmppStreamEnd += XmppStreamEnd;
        }

        public void AddXmppHandler(Jid address, IXmppHandler handler)
        {
            handlerStorage.AddXmppHandler(address, handler);
        }

        public void RemoveXmppHandler(IXmppHandler handler)
        {
            handlerStorage.RemoveXmppHandler(handler);
        }

        public void ProcessStreamStart(Node node, string ns, XmppStream xmppStream)
        {
            try
            {
                var stream = node as Stream;
                if (stream == null)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.BadFormat);
                    return;
                }
                if (!stream.HasTo)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.ImproperAddressing);//TODO: Return something more correct^)
                    return;
                }
                if (!stream.To.IsServer)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.ImproperAddressing);
                    return;
                }

                var handlers = handlerStorage.GetStreamStartHandlers(stream.To);
                if (handlers.Count == 0)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.HostUnknown);
                    return;
                }

                var handler = handlers.Find(h => h.Namespace == ns);
                if (handler == null)
                {
                    sender.SendToAndClose(xmppStream, XmppStreamError.BadNamespacePrefix);
                    return;
                }

                xmppStream.Namespace = ns;
                xmppStream.Domain = stream.To.Server;
                xmppStream.Language = stream.Language;

                handler.StreamStartHandle(xmppStream, stream, context);
            }
            catch (Exception ex)
            {
                ProcessException(ex, node, xmppStream);
            }
        }

        public void ProcessStreamElement(Node node, XmppStream stream)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (stream == null) throw new ArgumentNullException("stream");

            try
            {
                schemaValidator.ValidateNode(node, stream, context);

                var address = new Jid(stream.Domain);

                foreach (var handler in handlerStorage.GetStreamHandlers(address, node.GetType()))
                {
                    handler.ElementHandle(stream, (Element)node.Clone(), context);
                }

                if (node is Stanza)
                {
                    var stanza = (Stanza)node;

                    if (!validator.ValidateStanza(stanza, stream, context)) return;

                    if (stanza.HasTo) address = stanza.To;

                    var handlres = handlerStorage.GetStanzaHandlers(address, GetStanzaType(stanza));
                    if (handlres.Count == 0)
                    {
                        if (stanza is IQ)
                        {
                            var iq = (IQ)stanza;
                            if ((iq.Type == IqType.error || iq.Type == IqType.result) && iq.HasTo && iq.To.HasUser)
                            {
                                //result and error retranslate to user
                                var session = context.SessionManager.GetSession(iq.To);
                                if (session != null)
                                {
                                    sender.SendTo(session, iq);
                                    return;
                                }
                            }
                            //result and error ignored by server
                        }
                        sender.SendTo(stream, XmppStanzaError.ToServiceUnavailable(stanza));
                        log.DebugFormat("Stanza handler not found for address '{0}'", address);
                        return;
                    }

                    bool iqHandled = true;
                    Stopwatch stopwatch = null;

                    foreach (var handler in handlres)
                    {
                        if (log.IsDebugEnabled)
                        {
                            stopwatch = Stopwatch.StartNew();
                        }

                        if (stanza is IQ)
                        {
                            var answer = handler.HandleIQ(stream, (IQ)stanza.Clone(), context);
                            if (answer != null)
                            {
                                sender.SendTo(stream, answer);
                                iqHandled = answer.Id == stanza.Id;
                            }
                        }
                        else if (stanza is Message)
                        {
                            handler.HandleMessage(stream, (Message)stanza.Clone(), context);
                        }
                        else if (stanza is Presence)
                        {
                            handler.HandlePresence(stream, (Presence)stanza.Clone(), context);
                        }
                        else
                        {
                            sender.SendTo(stream, XmppStanzaError.ToNotAcceptable(stanza));
                            return;
                        }

                        if (log.IsDebugEnabled)
                        {
                            stopwatch.Stop();
                            log.DebugFormat("Process stanza handler '{1}' on address '{0}', time: {2}ms", address, handler.GetType().FullName, stopwatch.Elapsed.TotalMilliseconds);
                        }
                    }
                    if (!iqHandled)
                    {
                        sender.SendTo(stream, XmppStanzaError.ToServiceUnavailable(stanza));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessException(ex, node, stream);
            }
        }

        public void ProcessStreamEnd(ICollection<Node> notSendedBuffer, XmppStream stream, string connectionId)
        {
            if (notSendedBuffer == null) throw new ArgumentNullException("notSendedBuffer");
            if (stream != null)
            {
                foreach (var session in context.SessionManager.GetStreamSessions(stream.Id))
                {
                    context.SessionManager.CloseSession(session.Jid);
                }

                foreach (var handler in handlerStorage.GetStreamHandlers(stream.Domain))
                {
                    try
                    {
                        handler.StreamEndHandle(stream, notSendedBuffer, context);
                    }
                    catch (Exception) { }
                }
                streamManager.RemoveStream(stream.ConnectionId);
            }
            var connection = sender.GetXmppConnection(connectionId);
            if (connection != null) connection.Close();
        }


        private void XmppStreamStart(object sender, XmppStreamStartEventArgs e)
        {
            try
            {
                log.InfoFormat(RECIEVE_FORMAT, e.ConnectionId, e.Namespace, e.Node.ToString(Formatting.Indented));

                var xmppStream = streamManager.GetOrCreateNewStream(e.ConnectionId);
                ProcessStreamStart(e.Node, e.Namespace, xmppStream);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error process stream start: {0}", ex);
            }
        }

        private void XmppStreamElement(object sender, XmppStreamEventArgs e)
        {
            try
            {
                log.InfoFormat(RECIEVE_FORMAT, e.ConnectionId, string.Empty, e.Node.ToString(Formatting.Indented));

                var stream = streamManager.GetStream(e.ConnectionId);
                if (stream == null) return;
                ProcessStreamElement(e.Node, stream);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error process stream element: {0}", ex);
            }
        }

        private void XmppStreamEnd(object sender, XmppStreamEndEventArgs e)
        {
            try
            {
                log.InfoFormat("Xmpp stream end: connection {0}, not sended elements count {1}", e.ConnectionId, e.NotSendedBuffer.Count);

                var stream = streamManager.GetStream(e.ConnectionId);
                ProcessStreamEnd(e.NotSendedBuffer, stream, e.ConnectionId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error process stream end: {0}", ex);
            }
        }

        private Type GetStanzaType(Stanza stanza)
        {
            var iq = stanza as IQ;
            if (iq == null) return stanza.GetType();

            Element iqInfo = iq.Query ?? (iq.Bind ?? (iq.Vcard ?? ((Element)iq.Session ?? (Element)iq.Blocklist)));
            if (iqInfo != null) return iqInfo.GetType();

            foreach (var child in iq.ChildNodes)
            {
                if (child is Element) return child.GetType();
            }
            return stanza.GetType();
        }

        private void ProcessException(Exception ex, Node node, XmppStream stream)
        {
            if (ex is JabberException)
            {
                var je = (JabberException)ex;
                var error = je.ToElement();

                if (je.StreamError)
                {
                    ((agsXMPP.protocol.Error)error).Text = je.Message;
                    sender.SendTo(stream, error);
                }
                else
                {
                    if (node is Stanza && error is StanzaError)
                    {
                        sender.SendTo(stream, XmppStanzaError.ToErrorStanza((Stanza)node, (StanzaError)error));
                    }
                    else
                    {
                        var streamError = XmppStreamError.InternalServerError;
                        streamError.Text = "Stanza error in stream.";
                        sender.SendToAndClose(stream, streamError);
                    }
                }

                if (je.CloseStream) sender.CloseStream(stream);
            }
            else
            {
                log.Error("InternalServerError", ex);
                var error = XmppStreamError.InternalServerError;
                error.Text = ex.Message;
                sender.SendToAndClose(stream, error);
            }
        }
    }
}