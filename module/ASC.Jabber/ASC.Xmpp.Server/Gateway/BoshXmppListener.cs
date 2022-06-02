﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using ASC.Xmpp.Common;
using log4net;
using ASC.Xmpp.Server.Utils;

namespace ASC.Xmpp.Server.Gateway
{
    class BoshXmppListener : XmppListenerBase
    {
        private const string DEFAULT_BIND_URL = "http://*:5280/http-poll/";
        private const string DEFAULT_POLICY_URL = "http://*:5280/";
        private const string DEFAULT_API_URL = "http://*:5280/api/";
        private const string DEFAULT_POLICY_FILE = "crossdomain.xml";

        private readonly HttpListener httpListener = new HttpListener();
        private readonly List<IHttpResponder> httpResponders = new List<IHttpResponder>();

        private System.Uri bindUri;
        private System.Uri domainUri;
        private System.Uri apiUri;

        private string policyFile = DEFAULT_POLICY_FILE;
        private string policy = string.Empty;
        private bool policyLoaded = false;
        private long maxPacket = 524288;//512 kb

        private static readonly ILog log = LogManager.GetLogger(typeof(BoshXmppListener));


        public override void Configure(IDictionary<string, string> properties)
        {
            try
            {
                string hostname = Dns.GetHostName().ToLowerInvariant();

                string bindPrefix = properties.ContainsKey("bind") ? properties["bind"] : DEFAULT_BIND_URL;
                bindUri = new System.Uri(bindPrefix.Replace("*", hostname));
                XmppRuntimeInfo.BoshUri = bindUri;

                string policyPrefix = properties.ContainsKey("policy") ? properties["policy"] : DEFAULT_POLICY_URL;
                domainUri = new System.Uri(policyPrefix.Replace("*", hostname));

                string apiPrefix = properties.ContainsKey("api") ? properties["api"] : DEFAULT_API_URL;
                apiUri = new System.Uri(apiPrefix.Replace("*", hostname));

                if (policyPrefix.Contains(".")) policyPrefix = policyPrefix.Substring(0, policyPrefix.LastIndexOf("/"));
                if (!policyPrefix.EndsWith("/")) policyPrefix += "/";
                if (!apiPrefix.EndsWith("/")) apiPrefix += "/";

                httpListener.Prefixes.Add(bindPrefix);
                httpListener.Prefixes.Add(policyPrefix);
                httpListener.Prefixes.Add(apiPrefix);

                log.InfoFormat("Configure listener {0} on {1}", Name, bindPrefix);
                log.InfoFormat("Configure policy {0} on {1}", Name, policyPrefix);
                log.InfoFormat("Configure api {0} on {1}", Name, apiPrefix);

                BoshXmppHelper.CompressResponse = true;
                if (properties.ContainsKey("compress"))
                {
                    BoshXmppHelper.CompressResponse = bool.Parse(properties["compress"]);
                }

                if (properties.ContainsKey("policyFile")) policyFile = properties["policyFile"];
                policyFile = PathUtils.GetAbsolutePath(policyFile);

                if (properties.ContainsKey("maxpacket"))
                {
                    var value = 0L;
                    if (long.TryParse(properties["maxpacket"], out value)) maxPacket = value;
                }

                foreach (var responderType in properties.Where(property => property.Key.StartsWith("responder")).Select(property => Type.GetType(property.Value)).Where(responderType => responderType != null))
                {
                    if (responderType.GetInterfaces().Contains(typeof(IHttpResponder)))
                    {
                        httpResponders.Add((IHttpResponder)Activator.CreateInstance(responderType));
                    }
                }
            }
            catch (Exception e)
            {
                log.DebugFormat("Error configure listener {0}: {1}", Name, e);
                throw;
            }
        }

        protected override void DoStart()
        {
            httpListener.Start();
            BeginGetContext();
        }

        protected override void DoStop()
        {
            httpListener.Stop();
        }

        private void BeginGetContext()
        {
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.BeginGetContext(GetContextCallback, null);
            }
        }

        private void GetContextCallback(IAsyncResult asyncResult)
        {
            HttpListenerContext ctx = null;
            try
            {
                try
                {
                    ctx = httpListener.EndGetContext(asyncResult);
                }
                finally
                {
                    BeginGetContext();
                }

                if (maxPacket < ctx.Request.ContentLength64)
                {
                    BoshXmppHelper.TerminateBoshSession(ctx, "request-too-large");
                    return;
                }

                var url = ctx.Request.Url;
                log.DebugFormat("{0}: Begin process http request {1}", Name, url);

                if (url.AbsolutePath == bindUri.AbsolutePath)
                {
                    var body = BoshXmppHelper.ReadBodyFromRequest(ctx);
                    if (body == null)
                    {
                        BoshXmppHelper.TerminateBoshSession(ctx, "bad-request");
                        return;
                    }

                    var connection = GetXmppConnection(body.Sid) as BoshXmppConnection;

                    if (!string.IsNullOrEmpty(body.Sid) && connection == null)
                    {
                        BoshXmppHelper.TerminateBoshSession(ctx, "item-not-found");
                        return;
                    }

                    if (connection == null)
                    {
                        connection = new BoshXmppConnection();
                        AddNewXmppConnection(connection);
                    }

                    connection.ProcessBody(body, ctx);
                }
                else if ((url.AbsolutePath == domainUri.AbsolutePath || url.AbsolutePath == "/crossdomain.xml") && ctx.Request.HttpMethod == "GET")
                {
                    SendPolicy(ctx);
                }
                else
                {
                    IHttpResponder responder;
                    if (url.AbsolutePath.Length > apiUri.AbsolutePath.Length)
                    {
                        var urlPath = url.AbsolutePath.Substring(apiUri.AbsolutePath.Length).Trim('/');
                        if ((responder = httpResponders.FirstOrDefault(x => x.Path == urlPath)) != null)
                        {
                            //To responder
                            try
                            {
                                var responce = responder.Process(ctx.Request).ToString();
                                BoshXmppHelper.SendAndCloseResponse(ctx, responce);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                BoshXmppHelper.TerminateBoshSession(ctx, "unathorized");
                            }
                            catch (Exception)
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                BoshXmppHelper.TerminateBoshSession(ctx, "server-error");
                            }
                        }
                        else
                        {
                            BoshXmppHelper.TerminateBoshSession(ctx, "bad-request");
                        }
                    }
                    else
                    {
                        BoshXmppHelper.TerminateBoshSession(ctx, "bad-request");
                    }
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                if (ctx != null) BoshXmppHelper.TerminateBoshSession(ctx, "internal-server-error");
                if (Started) log.ErrorFormat("{0}: Error GetContextCallback: {1}", Name, e);
            }
        }

        private void SendPolicy(HttpListenerContext ctx)
        {
            log.DebugFormat("{0}: Send policy.", Name);

            if (!policyLoaded)
            {
                try
                {
                    policy = File.ReadAllText(policyFile);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Can not load policy file: {0}, error: {1}", policyFile, ex);
                }
                policyLoaded = true;
            }
            BoshXmppHelper.SendAndCloseResponse(ctx, policy);
        }
    }
}