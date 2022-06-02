﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Core;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Common.WhatsNew
{
    //Ну гребаный контрол, сейчас мы с тобой побеседуем...
    [DefaultProperty("Title")]
    [ToolboxData("<{0}:FeedControl runat=server></{0}:FeedControl>")]
    public class FeedControl : Control
    {
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Title { get; set; }

        [DefaultValue("")]
        [Localizable(false)]
        public string ProductId { get; set; }

        [DefaultValue("")]
        [Localizable(false)]
        public string ContainerId { get; set; }

        [DefaultValue("")]
        [Localizable(true)]
        public string ModuleId { get; set; }

        [DefaultValue(false)]
        [Localizable(false)]
        public bool ContentOnly { get; set; }

        [DefaultValue(true)]
        [Localizable(false)]
        public bool AutoFill { get; set; }

        public FeedControl()
        {
            ContentOnly = false;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (AutoFill)
            {
                if (string.IsNullOrEmpty(ProductId))
                {
                    ProductId = CommonLinkUtility.GetProductID().ToString("D");
                }
                if (string.IsNullOrEmpty(ModuleId))
                {
                    IProduct product;
                    IModule module;
                    CommonLinkUtility.GetLocationByRequest(HttpContext.Current.Request, out product, out module);
                    if (module != null)
                    {
                        ModuleId = module.ModuleID.ToString("D");
                    }
                }
            }

            writer.Write(Services.WhatsNew.feed.RenderRssMeta(
                        Title, 
                        ProductId, 
                        ModuleId,
                        ContentOnly? new int?(UserActivityConstants.ContentActionType): null, 
                        ContainerId));
        }
    }
}
