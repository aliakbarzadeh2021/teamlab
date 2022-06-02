﻿using System;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty.Configuration;
using ASC.Thrdparty.TokenManagers;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.OAuth;

namespace ASC.FederatedLogin.LoginProviders
{
    class LinkedInLoginProvider : ILoginProvider
    {
        private static InMemoryTokenManager ShortTermUserSessionTokenManager
        {
            get
            {
                var store = HttpContext.Current.Session;
                var tokenManager = (InMemoryTokenManager)store["linkedInShortTermManager"];
                if (tokenManager == null)
                {
                    string consumerKey = KeyStorage.Get("linkedInKey");
                    string consumerSecret = KeyStorage.Get("linkedInSecret");
                    tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
                    store["linkedInShortTermManager"] = tokenManager;
                }
                return tokenManager;
            }
        }

        private static WebConsumer _signInConsumer = null;
        private static readonly object SignInConsumerInitLock = new object();

        private static WebConsumer SignIn
        {
            get
            {
                if (_signInConsumer == null)
                {
                    lock (SignInConsumerInitLock)
                    {
                        if (_signInConsumer == null)
                        {
                            _signInConsumer = new WebConsumer(LinkedInConsumer.ServiceDescription, ShortTermUserSessionTokenManager);
                        }
                    }
                }

                return _signInConsumer;
            }
        }
        //<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        //<person>
        //  <id>FQJIJg-u80</id>
        //  <first-name>alexander</first-name>
        //  <last-name>rusanov</last-name>
        //</person>

        public LoginProfile ProcessAuthoriztion(HttpContext context)
        {
            var token = context.Request["oauth_token"];
            if (string.IsNullOrEmpty(token))
            {
                LinkedInConsumer.RequestAuthorization(SignIn);
            }
            else
            {

                var accessTokenResponse = SignIn.ProcessUserAuthorization();
                if (accessTokenResponse != null)
                {
                    //All ok. request info
                    var responce = LinkedInConsumer.GetProfile(SignIn, accessTokenResponse.AccessToken);
                    var document = XDocument.Parse(responce).CreateNavigator();
                    return new LoginProfile()
                               {
                                   Id = document.SelectNodeValue("//id"),
                                   FirstName = document.SelectNodeValue("//first-name"),
                                   LastName = document.SelectNodeValue("//last-name"),
                                   Avatar = document.SelectNodeValue("//picture-url"),
                                   Provider = ProviderConstants.LinkedIn
                               };
                }
                return LoginProfile.FromError(new Exception("Login failed"));
            }
            return null;
        }
    }
}