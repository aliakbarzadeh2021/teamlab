using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty.Configuration;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.ApplicationBlock.Facebook;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;

namespace ASC.FederatedLogin.LoginProviders
{
    class FacebookLoginProvider : ILoginProvider
    {
        public LoginProfile ProcessAuthoriztion(HttpContext context)
        {
            var client = new FacebookClient
                             {
                                 ClientIdentifier = KeyStorage.Get("facebookAppID"),
                                 ClientSecret = KeyStorage.Get("facebookAppSecret"),
                             };
            IAuthorizationState authorization = client.ProcessUserAuthorization(new HttpRequestInfo(context.Request));
            if (authorization == null)
            {
                // Kick off authorization request
                var scope = new List<string>()
                                {
                                    "email,user_about_me",
                                };
                client.RequestUserAuthorization(scope, null, null);
            }
            else
            {
                var request = WebRequest.Create("https://graph.facebook.com/me?access_token=" + Uri.EscapeDataString(authorization.AccessToken));
                using (var response = request.GetResponse())
                {
                    if (response != null)
                        using (var responseStream = response.GetResponseStream())
                        {
                            var graph = FacebookGraph.Deserialize(responseStream);
                            var profile = ProfileFromFacebook(graph);
                            return profile;
                        }
                }
            }
            return LoginProfile.FromError(new Exception("Failed to login with facebook"));
        }

        internal static LoginProfile ProfileFromFacebook(FacebookGraph graph)
        {
            var profile = new LoginProfile
                              {
                                  BirthDay = graph.Birthday,
                                  Link = graph.Link.ToString(),
                                  FirstName = graph.FirstName,
                                  LastName = graph.LastName,
                                  Gender = graph.Gender,
                                  DisplayName = graph.FirstName + graph.LastName,
                                  EMail = graph.Email,
                                  Id = graph.Id.ToString(),
                                  TimeZone = graph.Timezone,
                                  Locale = graph.Locale,
                                  Provider = ProviderConstants.Facebook
                              };

            return profile;
        }
    }
}