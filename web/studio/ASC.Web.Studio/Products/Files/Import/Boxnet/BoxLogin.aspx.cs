using System;
using System.Net;
using System.Web;
using System.Xml.Linq;
using ASC.Thrdparty;

namespace ASC.Web.Files.Import.Boxnet
{
    public partial class BoxLogin : OAuthBase
    {
        public static string Location { get { return String.Concat(ASC.Web.Files.Classes.PathProvider.BaseAbsolutePath, "import/boxnet/boxlogin.aspx"); } }

        private const string source = "box.net";

        private const string InteractiveLoginRedirect = "https://www.box.net/api/1.0/auth/{0}";

        private string AuthToken
        {
            get { return TokenHolder.GetToken("box.net_auth_token"); }
            set { TokenHolder.AddToken("box.net_auth_token", value); }
        }

        private string AuthTicket
        {
            get { return TokenHolder.GetToken("box.net_auth_ticket"); }
            set { TokenHolder.AddToken("box.net_auth_ticket", value); }
        }

        protected string LoginUrl { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                if (string.IsNullOrEmpty(Request["auth_token"]))
                {
                    try
                    {
                        //We are not authorized. get ticket and redirect
                        var url = "https://www.box.net/api/1.0/rest?action=get_ticket&api_key=" + ImportConfiguration.BoxNetApiKey;
                        var ticketResponce = new WebClient().DownloadString(url);
                        var response = XDocument.Parse(ticketResponce).Element("response");

                        if (response.Element("status").Value != "get_ticket_ok")
                        {
                            throw new InvalidOperationException("can't retrieve ticket " + response.Element("status").Value);
                        }

                        AuthTicket = response.Element("ticket").Value;
                        var loginRedir = string.Format(InteractiveLoginRedirect, AuthTicket);
                        //TODO:Move to settings
                        var frameCallback = new Uri(ImportConfiguration.BoxNetIFrameAddress, UriKind.Absolute);
                        LoginUrl = string.Format("{0}?origin={1}&go={2}",frameCallback,
                            HttpUtility.UrlEncode(new Uri(Request.Url,"/").ToString()),
                            HttpUtility.UrlEncode(loginRedir));
                    }
                    catch (Exception ex)
                    {
                        //Something goes wrong
                        SubmitError(ex.Message, source);
                    }
                }
                else
                {
                    //We got token
                    AuthToken = Request["auth_token"];
                    //Now we can callback somewhere
                    SubmitToken(AuthToken, source);
                }
            }
            else
            {
                SubmitToken(AuthToken, source);
            }
        }
    }
}
