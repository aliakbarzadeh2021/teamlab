using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AjaxPro;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Core;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Users
{

	[AjaxNamespace("SubscriptionManager")]
	public partial class UserSubscriptions : System.Web.UI.UserControl
	{
		public static string Location
		{
			get { return "~/UserControls/Users/UserSubscriptions.ascx"; }
		}

		public Guid ProductID { get; set; }


		protected void Page_Load(object sender, EventArgs e)
		{
			AjaxPro.Utility.RegisterTypeForAjax(this.GetType());


		}

        protected bool IsAdmin()
        {
            return CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID);
        }

		#region Init Notify by comboboxes
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			try
			{
				const string notifyByComboboxesScriptKey = "QuickLinksOnLoadJavaScriptBlock";
				if (!Page.ClientScript.IsClientScriptBlockRegistered(notifyByComboboxesScriptKey))
				{
					var notifyByComboboxesScript = "jq(function() { CommonSubscriptionManager.InitNotifyByComboboxes(); } ); ";
					Page.ClientScript.RegisterClientScriptBlock(typeof(string), notifyByComboboxesScriptKey, notifyByComboboxesScript, true);
				}

				//styles
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "studio_usermaker_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/users/usermaker/css/<theme_folder>/usermaker.css") + "\">", false);
				Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "studio_usermaker_textoverflow", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + VirtualPathUtility.ToAbsolute("~/usercontrols/users/usermaker/css/" + WebSkin.DefaultSkin.FolderName + "/usermaker.text-overflow.css") + "\" />", false);
			}
			catch { }
		}
		#endregion

		protected string RenderSubscriptions()
		{
			bool isEmpty;
			if (!ProductID.Equals(Guid.Empty))
				return RenderSubscriptions(ProductID, out isEmpty);

			StringBuilder sb = new StringBuilder();
			bool isFirst = true;
			foreach (var product in WebItemManager.Instance.GetItems(ASC.Web.Core.WebZones.WebZoneType.All).OfType<IProduct>())
			{
				string str = RenderSubscriptions(product.ProductID, out isEmpty);

				sb.Append("<div id='studio_product_subscribeBox_" + product.ProductID + "' class='borderBase tintMedium clearFix' style='border-left:none; border-right:none; margin-top:-1px; padding:10px;'>");
				sb.Append("<div class='headerBase' style='float:left; cursor:pointer;' onclick=\"CommonSubscriptionManager.ToggleProductList('" + product.ProductID + "');\">");
				string logoURL = product.GetIconAbsoluteURL();
				if (!String.IsNullOrEmpty(logoURL))
					sb.Append("<img alt='' style='margin-right:5px;' align='absmiddle' src='" + logoURL + "'/>");
				sb.Append(HttpUtility.HtmlEncode(product.ProductName));
                if(!isEmpty)
				    sb.Append("<img alt='' align='absmiddle' id='studio_subscribeProductState_" + product.ProductID + "' style='margin-left:15px;'  src='" + WebImageSupplier.GetAbsoluteWebPath(isFirst ? "collapse_down_dark.png" : "collapse_right_dark.png") + "'/>");
				sb.Append("</div>");


				//Unsubscribe product
				sb.AppendFormat(@"<div style='float:right; text-align:right; width:110px;'>");
                if (!isEmpty)
	                sb.AppendFormat(@"<a {0} href='javascript:CommonSubscriptionManager.UnsibscribeProduct(""{1}"");'>{2}</a>",
                                        SetupInfo.WorkMode == WorkMode.Promo ? "class='promoAction unsubscribe'" : "class='unsubscribe'",
				                        product.ProductID,
				                        Resources.Resource.UnsubscribeButton);

                sb.AppendFormat("&nbsp;</div>");


				var notifyBy = GetNotifyByMethod(product.ProductID);

				sb.AppendFormat(@"
<div style='float: right;'>
		<select id='NotifyByCombobox_{0}' class='comboBox' style='display: none;' onchange='CommonSubscriptionManager.SetNotifyByMethod(""{0}"", jq(this).val());'>
			<option class='optionItem' value='0'{4}>{1}</option>
			<option class='optionItem' value='1'{5}>{2}</option>
			<option class='optionItem' value='2'{6}>{3}</option>
		</select>
</div>", product.ProductID,
			Resources.Resource.NotifyByEmail, Resources.Resource.NotifyByTMTalk, Resources.Resource.NotifyByEmailAndTMTalk,
			0 == notifyBy ? " selected='selected'" : string.Empty,
			1 == notifyBy ? " selected='selected'" : string.Empty,
			2 == notifyBy ? " selected='selected'" : string.Empty);
								

				sb.Append("</div>");
				sb.Append("<div id=\"studio_product_subscriptions_" + product.ProductID + "\" style=\"padding-left:40px; " + (isFirst ? "" : "display:none;") + "\">");
				sb.Append(str);
				sb.Append("</div>");
				isFirst = false;
			}

			return sb.ToString();
		}

		private class GroupItem
		{
			public Guid ID { get; set; }
			public string Name { get; set; }
			public string ImageURL { get; set; }
			public GroupByType Type { get; private set; }

			public List<SubscriptionType> SubscriptionTypes { get; private set; }
			public List<SubscriptionObject> SubscriptionObjects { get; private set; }

			public GroupItem(IModule module, List<SubscriptionType> subscriptionTypes, List<SubscriptionObject> subscriptionObjects)
			{
				this.ID = module.ModuleID;
				this.Name = module.ModuleName;
				this.ImageURL = module.GetIconAbsoluteURL();
				this.SubscriptionTypes = subscriptionTypes;
				this.SubscriptionObjects = subscriptionObjects;
				Type = GroupByType.Modules;

			}

			public GroupItem(SubscriptionGroup group, List<SubscriptionType> subscriptionTypes, List<SubscriptionObject> subscriptionObjects)
			{
				this.ID = group.ID;
				this.Name = group.Name;
				this.SubscriptionTypes = subscriptionTypes;
				this.SubscriptionObjects = subscriptionObjects;
				Type = GroupByType.Groups;
			}

		}

		private string RenderSubscriptions(Guid productID, out bool isEmpty)
		{
			isEmpty = true;
			IProduct product = ProductManager.Instance[productID];
			if (product == null || product.Context == null || product.Context.SubscriptionManager == null)
				return "";

			List<GroupItem> groupItems = new List<GroupItem>();
			var productSubscriptionManager = product.Context.SubscriptionManager;
			if (productSubscriptionManager.GroupByType == GroupByType.Modules)
			{
				foreach (var item in WebItemManager.Instance.GetSubItems(product.ProductID))
				{
					if ((item is IModule) == false)
						continue;

					var module = item as IModule;

					ISubscriptionManager subscriptionManager = null;
					var moduleContext = ProductManager.Instance.GetModuleContext(module.ModuleID);

					if (moduleContext != null)
						subscriptionManager = moduleContext.SubscriptionManager;

					if (subscriptionManager == null)
						continue;

					var subscriptionTypes = subscriptionManager.GetSubscriptionTypes();
					if (subscriptionTypes == null || subscriptionTypes.Count == 0)
						continue;
                    else
                        subscriptionTypes = subscriptionTypes.FindAll(type => (type.IsEmptySubscriptionType != null && !type.IsEmptySubscriptionType(product.ProductID, module.ModuleID, type.ID)));

                    if (subscriptionTypes == null || subscriptionTypes.Count == 0)
                        continue;

					groupItems.Add(new GroupItem(module, subscriptionTypes, null));
					isEmpty = false;
				}

			}
			else if (productSubscriptionManager.GroupByType == GroupByType.Groups)
			{

				IProductSubscriptionManager subscriptionManager = null;
				if (product.Context != null)
					subscriptionManager = product.Context.SubscriptionManager;

				if (subscriptionManager == null)
					return "";

				var subscriptionTypes = subscriptionManager.GetSubscriptionTypes();
				if (subscriptionTypes == null || subscriptionTypes.Count == 0)
					return "";

				List<SubscriptionGroup> groups = subscriptionManager.GetSubscriptionGroups();
				if (groups != null)
				{
					foreach (var gr in groups)
					{
                        var sTypes = subscriptionTypes.FindAll(type => (type.IsEmptySubscriptionType != null && !type.IsEmptySubscriptionType(product.ProductID, gr.ID, type.ID)));
                        if (sTypes == null || sTypes.Count == 0)
                            continue;

						groupItems.Add(new GroupItem(gr, subscriptionTypes, null));
						isEmpty = false;
					}
				}
			}

			if (!isEmpty)
				return RenderGroupItemSubscriptions(product, groupItems);

			return "";
		}

		private string RenderGroupItemSubscriptions(IProduct product, List<GroupItem> groupItems)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var groupItem in groupItems)
			{
				var subscriptionTypes = groupItem.SubscriptionTypes;

				if (subscriptionTypes != null)
					subscriptionTypes = subscriptionTypes.FindAll(type => (type.IsEmptySubscriptionType != null && !type.IsEmptySubscriptionType(product.ProductID, groupItem.ID, type.ID)));

				if (subscriptionTypes == null || subscriptionTypes.Count == 0)
					continue;

				sb.Append("<div id='studio_module_subscribeBox_" + product.ProductID + "_" + groupItem.ID + "' class='borderBase clearFix' style='border-left:none; border-right:none; margin-top:-1px; padding:10px;'>");

				sb.Append("<div class='headerBaseMedium' style='float:left; cursor:pointer;' onclick=\"CommonSubscriptionManager.ToggleModuleList('" + product.ProductID + "','" + groupItem.ID + "');\">");

				if (!String.IsNullOrEmpty(groupItem.ImageURL))
					sb.Append("<img alt='' style='margin-right:5px;' align='absmiddle' src='" + groupItem.ImageURL + "'/>");
				sb.Append(HttpUtility.HtmlEncode(groupItem.Name));
				sb.Append("<img alt='' align='absmiddle' id='studio_subscribeModuleState_" + product.ProductID + "_" + groupItem.ID + "' style='margin-left:15px;'  src='" + WebImageSupplier.GetAbsoluteWebPath("collapse_down_dark.png") + "'/>");
				sb.Append("</div>");

				//sb.Append("<div style='float:right; text-align:right; padding-top:5px; width:29%'><a href=\"javascript:CommonSubscriptionManager.UnsibscribeModule('" + _product.ProductID + "','" + module.ModuleID + "');\">" + Resources.Resource.UnsubscribeButton + "</a></div>");

				sb.Append("</div>");

				sb.Append("<div id='studio_module_subscriptions_" + product.ProductID + "_" + groupItem.ID + "'>");

				#region suscription types
				foreach (var type in subscriptionTypes)
				{
					sb.Append("<div id='studio_subscribeType_" + product.ProductID + "_" + groupItem.ID + "_" + type.ID + "' class=\"tintMedium borderBase\" style='border-top:none; border-left:none; border-right:none; padding:10px 10px 10px 30px;'>");

					sb.Append("<div class='clearFix'>");
					if (!type.Single)
					{
						sb.Append("<div style='float:left; cursor:pointer;' onclick=\"CommonSubscriptionManager.ToggleSubscriptionList('" + product.ProductID + "','" + groupItem.ID + "','" + type.ID + "');\">");
						sb.Append(HttpUtility.HtmlEncode(type.Name));
						sb.Append("<img alt='' align='absmiddle' id='studio_subscriptionsState_" + product.ProductID + "_" + groupItem.ID + "_" + type.ID + "' style='margin-left:15px;'  src='" + WebImageSupplier.GetAbsoluteWebPath("collapse_right_light.png") + "'/>");
						sb.Append("</div>");
					}
					else
					{
						sb.Append("<div style='float:left;'>");
						sb.Append(HttpUtility.HtmlEncode(type.Name));
						sb.Append("</div>");
					}

					sb.Append("<div style='float:right; text-align:right; width:110px;'><a " + (SetupInfo.WorkMode == WorkMode.Promo ? "class=\"promoAction\"" : "") + " href=\"javascript:CommonSubscriptionManager.UnsibscribeType('" + product.ProductID + "','" + groupItem.ID + "','" + type.ID + "');\">" + Resources.Resource.UnsubscribeButton + "</a></div>");

					sb.Append("</div>");

					sb.Append("</div>");
				}
				#endregion

				sb.Append("</div>");
			}
			return sb.ToString();
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public string RenderGroupItemSubscriptions(Guid productID, Guid moduleID, Guid subscriptionTypeID)
		{
			SubscriptionType type = null;

			IProduct product = ProductManager.Instance[productID];
			if (product == null || product.Context == null || product.Context.SubscriptionManager == null)
				return string.Empty;

			ISubscriptionManager subscriptionManager = null;

			var productSubscriptionManager = product.Context.SubscriptionManager;
			if (productSubscriptionManager.GroupByType == GroupByType.Modules)
			{
				var module = ProductManager.Instance.GetModuleByID(moduleID);
				var moduleContext = ProductManager.Instance.GetModuleContext(module.ModuleID);
				if (moduleContext != null)
					subscriptionManager = moduleContext.SubscriptionManager;
			}
			else if (productSubscriptionManager.GroupByType == GroupByType.Groups)
			{
				if (product.Context != null)
					subscriptionManager = product.Context.SubscriptionManager;

				if (subscriptionManager == null)
					return string.Empty;
			}

			var subscriptionTypes = subscriptionManager.GetSubscriptionTypes();
			if (subscriptionTypes != null && subscriptionTypes.Count != 0)
			{
				try
				{
					type = (from s in subscriptionTypes
							where s.ID.Equals(subscriptionTypeID)
							select s).Single<SubscriptionType>();
				}
				catch
				{
					return string.Empty;
				}
			}


			#region render subscription objects

			var sb = new StringBuilder();

			if (type.Single)
				return String.Empty;

			if (type.IsEmptySubscriptionType != null && type.IsEmptySubscriptionType(productID, moduleID, subscriptionTypeID))
				return string.Empty;


			if (type.GetSubscriptionObjects == null)
				return string.Empty;

			var typeObjects = type.GetSubscriptionObjects(productID, moduleID, subscriptionTypeID);
			if (typeObjects == null || typeObjects.Count == 0)
				return string.Empty;

			sb.Append("<div style='padding-top:15px;' id='studio_subscriptions_" + product.ProductID + "_" + moduleID + "_" + type.ID + "'>");

			typeObjects.Sort((s1, s2) => String.Compare(s1.Name, s2.Name, true));
			foreach (var subscription in typeObjects)
			{
				sb.Append("<div id='studio_subscribeItem_" + product.ProductID + "_" + moduleID + "_" + subscription.SubscriptionType.ID + "_" + subscription.ID + "' class='clearFix' style='margin-bottom:5px;'>");

				sb.Append("<div style='float:left;'>");
				sb.Append("<input id='studio_subscribeItemChecker_" + product.ProductID + "_" + moduleID + "_" + subscription.SubscriptionType.ID + "_" + subscription.ID + "' value='" + subscription.ID + "' type='checkbox'/>");
				sb.Append("</div>");

                sb.Append("<div style='float:left; margin-left:15px; width:70%; overflow: hidden;'>");
                if (!String.IsNullOrEmpty(subscription.URL))
                    sb.Append("<a href='" + subscription.URL + "' title='" + HttpUtility.HtmlEncode(subscription.Name) + "'>" + HttpUtility.HtmlEncode(subscription.Name) + "</a>");
                else
                    sb.Append(HttpUtility.HtmlEncode(subscription.Name));

				sb.Append("</div>");
				//sb.Append("<div style='float:right; text-align:right; width:26%'><a class='linkAction' href=\"javascript:CommonSubscriptionManager.UnsibscribeObject('" + _product.ProductID + "','" + module.ModuleID + "','" + subscription.SubscriptionType.ID + "','" + subscription.ID + "');\">" + Resources.Resource.UnsubscribeButton + "</a></div>");
				sb.Append("</div>");

			}

			sb.Append("<div class='clearFix' style='margin-top:15px;'>");
			sb.Append("<a " + (SetupInfo.WorkMode == WorkMode.Promo ? "class=\"promoAction\"" : "") + " href=\"javascript:CommonSubscriptionManager.UnsibscribeObjects('" + product.ProductID + "','" + moduleID + "','" + type.ID + "');\" style='float:left;' class='baseLinkButton'>" + Resources.Resource.UnsubscribeButton + "</a>");
			sb.Append("</div>");

			sb.Append("</div>");

			#endregion

			return sb.ToString();
		}

        #region what's new 

        protected string RenderWhatsNewSubscriptionState()
        {
            return RenderWhatsNewSubscriptionState(StudioNotifyService.Instance.IsSubscribeToWhatsNew(SecurityContext.CurrentAccount.ID));
        }
        protected string RenderWhatsNewSubscriptionState(bool isSubscribe)
        {
            if (isSubscribe)
                return "<a " + (SetupInfo.WorkMode == WorkMode.Promo ? "class=\"promoAction\"" : "") + " href=\"javascript:CommonSubscriptionManager.SubscribeToWhatsNew();\">" + Resources.Resource.UnsubscribeButton + "</a>";
            else
                return "<a " + (SetupInfo.WorkMode == WorkMode.Promo ? "class=\"promoAction\"" : "") + " href=\"javascript:CommonSubscriptionManager.SubscribeToWhatsNew();\">" + Resources.Resource.SubscribeButton + "</a>";

        }

        protected string RenderWhatsNewNotifyByCombobox()
        {
            var subscriptionManager = StudioSubscriptionManager.Instance;

            var notifyBy = ConvertToNotifyByValue(subscriptionManager, Constants.ActionSendWhatsNew);

            return string.Format(@"
<select id='NotifyByCombobox_WhatsNew' class='comboBox' style='display: none;' onchange='CommonSubscriptionManager.SetWhatsNewNotifyByMethod(jq(this).val());'>
	<option class='optionItem' value='0'{3}>{0}</option>
	<option class='optionItem' value='1'{4}>{1}</option>
	<option class='optionItem' value='2'{5}>{2}</option>
</select>",
        Resources.Resource.NotifyByEmail, Resources.Resource.NotifyByTMTalk, Resources.Resource.NotifyByEmailAndTMTalk,
        0 == notifyBy ? " selected='selected'" : string.Empty,
        1 == notifyBy ? " selected='selected'" : string.Empty,
        2 == notifyBy ? " selected='selected'" : string.Empty);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SetWhatsNewNotifyByMethod(int notifyBy)
        {
            try
            {
                AjaxResponse resp = new AjaxResponse();
                IList<string> notifyByList = ConvertToNotifyByList(notifyBy);
                SetNotifyBySubsriptionTypes(notifyByList, StudioSubscriptionManager.Instance, Constants.ActionSendWhatsNew);
                return resp;
            }
            catch
            {
                return null;
            }
        }
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeToWhatsNew()
        {
            AjaxResponse resp = new AjaxResponse();
            resp.rs1 = "0";
            try
            {
                bool isSubscribe = StudioNotifyService.Instance.IsSubscribeToWhatsNew(SecurityContext.CurrentAccount.ID);

                StudioNotifyService.Instance.SubscribeToWhatsNew(SecurityContext.CurrentAccount.ID, !isSubscribe);
                resp.rs2 = RenderWhatsNewSubscriptionState(!isSubscribe);

                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs2 = e.Message.HtmlEncode();
            }

            return resp;

        }
        #endregion

        #region admin notifies

        protected string RenderAdminNotifySubscriptionState()
        {
            return RenderAdminNotifySubscriptionState(StudioNotifyService.Instance.IsSubscribeToAdminNotify(SecurityContext.CurrentAccount.ID));
        }
        protected string RenderAdminNotifySubscriptionState(bool isSubscribe)
        {
            if (isSubscribe)
                return "<a " + (SetupInfo.WorkMode == WorkMode.Promo ? "class=\"promoAction\"" : "") + " href=\"javascript:CommonSubscriptionManager.SubscribeToAdminNotify();\">" + Resources.Resource.UnsubscribeButton + "</a>";
            else
                return "<a " + (SetupInfo.WorkMode == WorkMode.Promo ? "class=\"promoAction\"" : "") + " href=\"javascript:CommonSubscriptionManager.SubscribeToAdminNotify();\">" + Resources.Resource.SubscribeButton + "</a>";

        }

        protected string RenderAdminNotifyNotifyByCombobox()
        {
            var subscriptionManager = StudioSubscriptionManager.Instance;

            var notifyBy = ConvertToNotifyByValue(subscriptionManager, Constants.ActionAdminNotify);

            return string.Format(@"
<select id='NotifyByCombobox_AdminNotify' class='comboBox' style='display: none;' onchange='CommonSubscriptionManager.SetAdminNotifyNotifyByMethod(jq(this).val());'>
	<option class='optionItem' value='0'{3}>{0}</option>
	<option class='optionItem' value='1'{4}>{1}</option>
	<option class='optionItem' value='2'{5}>{2}</option>
</select>",
        Resources.Resource.NotifyByEmail, Resources.Resource.NotifyByTMTalk, Resources.Resource.NotifyByEmailAndTMTalk,
        0 == notifyBy ? " selected='selected'" : string.Empty,
        1 == notifyBy ? " selected='selected'" : string.Empty,
        2 == notifyBy ? " selected='selected'" : string.Empty);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SetAdminNotifyNotifyByMethod(int notifyBy)
        {
            try
            {
                AjaxResponse resp = new AjaxResponse();
                IList<string> notifyByList = ConvertToNotifyByList(notifyBy);
                SetNotifyBySubsriptionTypes(notifyByList, StudioSubscriptionManager.Instance, Constants.ActionAdminNotify);
                return resp;
            }
            catch
            {
                return null;
            }
        }
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeToAdminNotify()
        {
            AjaxResponse resp = new AjaxResponse();
            resp.rs1 = "0";
            try
            {
                bool isSubscribe = StudioNotifyService.Instance.IsSubscribeToAdminNotify(SecurityContext.CurrentAccount.ID);

                StudioNotifyService.Instance.SubscribeToAdminNotify(SecurityContext.CurrentAccount.ID, !isSubscribe);
                resp.rs2 = RenderAdminNotifySubscriptionState(!isSubscribe);

                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs2 = e.Message.HtmlEncode();
            }

            return resp;

        }
        #endregion


		private IRecipient GetCurrentRecipient()
		{
			return new DirectRecipient(SecurityContext.CurrentAccount.ID.ToString(), SecurityContext.CurrentAccount.Name);
		}

		

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse UnsubscribeObject(Guid productID, Guid moduleID, Guid subscriptionTypeID, string objID)
		{
			AjaxResponse resp = new AjaxResponse();
			resp.rs2 = productID.ToString();
			resp.rs3 = moduleID.ToString();
			resp.rs4 = subscriptionTypeID.ToString();
			resp.rs5 = objID;

			try
			{
				ISubscriptionManager subscriptionManager = null;
				var psm = ProductManager.Instance[productID].Context.SubscriptionManager;
				if (psm.GroupByType == GroupByType.Groups)
					subscriptionManager = psm;

				else if (psm.GroupByType == GroupByType.Modules)
					subscriptionManager = ProductManager.Instance.GetModuleContext(moduleID).SubscriptionManager;

				if (subscriptionManager != null)
				{
					var types = subscriptionManager.GetSubscriptionTypes();
					var type = types.Find(t => t.ID.Equals(subscriptionTypeID));
					subscriptionManager.SubscriptionProvider.UnSubscribe(type.NotifyAction, objID, GetCurrentRecipient());
				}
				resp.rs1 = "1";
			}
			catch (Exception e)
			{
				resp.rs1 = "0";
				resp.rs6 = "<div class='errorBox'>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
			}

			return resp;
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse UnsubscribeObjects(Guid productID, Guid moduleID, Guid subscriptionTypeID, List<string> objIDs)
		{
			AjaxResponse resp = new AjaxResponse();
			resp.rs2 = productID.ToString();
			resp.rs3 = moduleID.ToString();
			resp.rs4 = subscriptionTypeID.ToString();


			try
			{
				ISubscriptionManager subscriptionManager = null;
				var psm = ProductManager.Instance[productID].Context.SubscriptionManager;
				if (psm.GroupByType == GroupByType.Groups)
					subscriptionManager = psm;

				else if (psm.GroupByType == GroupByType.Modules)
					subscriptionManager = ProductManager.Instance.GetModuleContext(moduleID).SubscriptionManager;

				if (subscriptionManager != null)
				{
					var types = subscriptionManager.GetSubscriptionTypes();
					var type = types.Find(t => t.ID.Equals(subscriptionTypeID));

					foreach (var objID in objIDs)
					{
						resp.rs5 += objID + ",";
						subscriptionManager.SubscriptionProvider.UnSubscribe(type.NotifyAction, objID, GetCurrentRecipient());
					}

					resp.rs5 = resp.rs5.TrimEnd(',');
				}
				resp.rs1 = "1";
			}
			catch (Exception e)
			{
				resp.rs1 = "0";
				resp.rs6 = "<div class='errorBox'>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
			}
			return resp;
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse UnsubscribeType(Guid productID, Guid moduleID, Guid subscriptionTypeID)
		{
			AjaxResponse resp = new AjaxResponse();
			resp.rs2 = productID.ToString();
			resp.rs3 = moduleID.ToString();
			resp.rs4 = subscriptionTypeID.ToString();
			try
			{
				ISubscriptionManager subscriptionManager = null;
				var psm = ProductManager.Instance[productID].Context.SubscriptionManager;
				if (psm.GroupByType == GroupByType.Groups)
				{
					subscriptionManager = psm;
					if (subscriptionManager != null)
					{
						var objs = subscriptionManager.GetSubscriptionObjects();
						objs.RemoveAll(o => !o.SubscriptionGroup.ID.Equals(moduleID) || !o.SubscriptionType.ID.Equals(subscriptionTypeID));

						foreach (var o in objs)
						{
							subscriptionManager.SubscriptionProvider.UnSubscribe(o.SubscriptionType.NotifyAction, o.ID, GetCurrentRecipient());
						}
					}
				}

				else if (psm.GroupByType == GroupByType.Modules)
				{
					subscriptionManager = ProductManager.Instance.GetModuleContext(moduleID).SubscriptionManager;
					if (subscriptionManager != null)
					{
						var subscriptionType = subscriptionManager.GetSubscriptionTypes().Find(st => st.ID.Equals(subscriptionTypeID));
						subscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, GetCurrentRecipient());
					}
				}
				resp.rs1 = "1";
			}
			catch (Exception e)
			{
				resp.rs1 = "0";
				resp.rs5 = "<div class='errorBox'>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
			}
			return resp;
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse UnsubscribeModule(Guid productID, Guid moduleID)
		{
			AjaxResponse resp = new AjaxResponse();
			resp.rs2 = productID.ToString();
			resp.rs3 = moduleID.ToString();
			try
			{
				ISubscriptionManager subscriptionManager = null;
				var moduleContext = ProductManager.Instance.GetModuleContext(moduleID);

				if (moduleContext != null)
					subscriptionManager = moduleContext.SubscriptionManager;

				if (subscriptionManager != null)
				{
					foreach (var subscriptionType in subscriptionManager.GetSubscriptionTypes())
						subscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, GetCurrentRecipient());
				}
				resp.rs1 = "1";
			}
			catch (Exception e)
			{
				resp.rs1 = "0";
				resp.rs4 = "<div class='errorBox'>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
			}
			return resp;
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse UnsubscribeProduct(Guid productID)
		{
			AjaxResponse resp = new AjaxResponse();
			resp.rs2 = productID.ToString();
			try
			{
				ISubscriptionManager subscriptionManager = null;
				List<SubscriptionType> subscriptionTypes = new List<SubscriptionType>();
				var product = ProductManager.Instance[productID];
				var productSubscriptionManager = ProductManager.Instance[productID].Context.SubscriptionManager;
				if (productSubscriptionManager.GroupByType == GroupByType.Modules)
				{
					foreach (var item in WebItemManager.Instance.GetSubItems(product.ProductID))
					{
						if ((item is IModule) == false)
							continue;
						var module = item as IModule;
						var moduleContext = ProductManager.Instance.GetModuleContext(module.ModuleID);

						if (moduleContext != null)
						{
							subscriptionManager = moduleContext.SubscriptionManager;
							if (subscriptionManager != null)
							{
								foreach (var subscriptionType in subscriptionManager.GetSubscriptionTypes())
									subscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, GetCurrentRecipient());
							}
						}
					}
				}
				else if (productSubscriptionManager.GroupByType == GroupByType.Groups)
				{
					if (product.Context != null)
						subscriptionManager = product.Context.SubscriptionManager;

					if (subscriptionManager != null)
					{
						foreach (var subscriptionType in subscriptionManager.GetSubscriptionTypes())
							subscriptionManager.SubscriptionProvider.UnSubscribe(subscriptionType.NotifyAction, GetCurrentRecipient());
					}
				}
				resp.rs1 = "1";
			}
			catch (Exception e)
			{
				resp.rs1 = "0";
				resp.rs4 = "<div class='errorBox'>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
			}
			return resp;
		}


		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse SetNotifyByMethod(Guid productID, int notifyBy)
		{
			AjaxResponse resp = new AjaxResponse();
			resp.rs2 = productID.ToString();
			try
			{
				IList<string> notifyByList = ConvertToNotifyByList(notifyBy);

				ISubscriptionManager subscriptionManager = null;

				List<SubscriptionType> subscriptionTypes = new List<SubscriptionType>();

				var product = ProductManager.Instance[productID];

				var productSubscriptionManager = ProductManager.Instance[productID].Context.SubscriptionManager;
				if (productSubscriptionManager.GroupByType == GroupByType.Modules)
				{
					foreach (var item in WebItemManager.Instance.GetSubItems(product.ProductID))
					{
						if ((item is IModule) == false)
							continue;
						var module = item as IModule;
						var moduleContext = ProductManager.Instance.GetModuleContext(module.ModuleID);

						if (moduleContext != null)
						{
							subscriptionManager = moduleContext.SubscriptionManager;
							if (subscriptionManager != null)
							{
								SetNotifyBySubsriptionTypes(notifyByList, subscriptionManager);
							}
						}
					}
				}
				else if (productSubscriptionManager.GroupByType == GroupByType.Groups)
				{
					if (product.Context != null)
						subscriptionManager = product.Context.SubscriptionManager;

					SetNotifyBySubsriptionTypes(notifyByList, subscriptionManager);
				}
				resp.rs1 = "1";
			}
			catch (Exception e)
			{
				resp.rs1 = "0";
				resp.rs6 = "<div class='errorBox'>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
			}

			return null;
		}

		private static IList<string> ConvertToNotifyByList(int notifyBy)
		{
			IList<string> notifyByList = new List<string>();
			switch (notifyBy)
			{
				case 0:
					notifyByList.Add(ASC.Core.Configuration.Constants.NotifyEMailSenderSysName);
					break;
				case 1:
					notifyByList.Add(ASC.Core.Configuration.Constants.NotifyMessengerSenderSysName);
					break;
				case 2:
					notifyByList.Add(ASC.Core.Configuration.Constants.NotifyEMailSenderSysName);
					notifyByList.Add(ASC.Core.Configuration.Constants.NotifyMessengerSenderSysName);
					break;
			}
			return notifyByList;
		}


		private void SetNotifyBySubsriptionTypes(IList<string> notifyByList, ISubscriptionManager subscriptionManager)
		{
			var subscriptionTypes = subscriptionManager.GetSubscriptionTypes();
			if (subscriptionTypes != null)
			{
                foreach (var type in subscriptionTypes)
                    SetNotifyBySubsriptionTypes(notifyByList, subscriptionManager, type.NotifyAction);
			}
		}
        private void SetNotifyBySubsriptionTypes(IList<string> notifyByList, ISubscriptionManager subscriptionManager,INotifyAction action)
        {
           subscriptionManager
               .SubscriptionProvider
               .UpdateSubscriptionMethod(
                        action, 
                        GetCurrentRecipient(), 
                        notifyByList.ToArray());
        }

		private int GetNotifyByMethod(Guid productID)
		{
			ISubscriptionManager subscriptionManager = null;

			List<SubscriptionType> subscriptionTypes = new List<SubscriptionType>();

			var product = ProductManager.Instance[productID];

			var productSubscriptionManager = ProductManager.Instance[productID].Context.SubscriptionManager;
            if (productSubscriptionManager == null)
                return 0;

			if (productSubscriptionManager.GroupByType == GroupByType.Modules)
			{
				foreach (var item in WebItemManager.Instance.GetSubItems(product.ProductID))
				{
					if ((item is IModule) == false)
						continue;
					var module = item as IModule;
					var moduleContext = ProductManager.Instance.GetModuleContext(module.ModuleID);

					if (moduleContext == null)
					{
						continue;
					}

					subscriptionManager = moduleContext.SubscriptionManager;

					if (subscriptionManager == null)
					{
						continue;
					}

					subscriptionTypes = subscriptionManager.GetSubscriptionTypes();
					foreach (var s in subscriptionTypes)
					{
						return ConvertToNotifyByValue(subscriptionManager, s);
					}
				}
			}
			else if (productSubscriptionManager.GroupByType == GroupByType.Groups)
			{
				if (product.Context != null)
					subscriptionManager = product.Context.SubscriptionManager;
				subscriptionTypes = subscriptionManager.GetSubscriptionTypes();
				foreach (var s in subscriptionTypes)
				{
					return ConvertToNotifyByValue(subscriptionManager, s);
				}
			}
			return 0;
		}

		private int ConvertToNotifyByValue(ISubscriptionManager subscriptionManager, SubscriptionType s)
		{
            return ConvertToNotifyByValue(subscriptionManager, s.NotifyAction);
		}
        
        private int ConvertToNotifyByValue(ISubscriptionManager subscriptionManager, INotifyAction action)
        {
            var notifyByArray = subscriptionManager.SubscriptionProvider.GetSubscriptionMethod(action, GetCurrentRecipient());
            if (notifyByArray.Length == 1)
            {
                if (notifyByArray.Contains(ASC.Core.Configuration.Constants.NotifyEMailSenderSysName))
                {
                    return 0;
                }
                if (notifyByArray.Contains(ASC.Core.Configuration.Constants.NotifyMessengerSenderSysName))
                {
                    return 1;
                }
            }
            if (notifyByArray.Length == 2)
            {
                if (notifyByArray.Contains(ASC.Core.Configuration.Constants.NotifyEMailSenderSysName) && notifyByArray.Contains(ASC.Core.Configuration.Constants.NotifyMessengerSenderSysName))
                {
                    return 2;
                }
            }
            return 0;
        }


		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse SaveNotifySenders(Guid productID, Guid moduleID, Guid subscriptionTypeID, string senders)
		{
			AjaxResponse resp = new AjaxResponse();
			resp.rs2 = productID.ToString();
			resp.rs3 = moduleID.ToString();
			resp.rs4 = subscriptionTypeID.ToString();

			try
			{
				var senderNames = senders.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);

				ISubscriptionManager subscriptionManager = null;
				var moduleContext = ProductManager.Instance.GetModuleContext(moduleID);

				if (moduleContext != null)
					subscriptionManager = moduleContext.SubscriptionManager;

				if (subscriptionManager != null)
				{
					var subscriptionType = subscriptionManager.GetSubscriptionTypes().Find(st => st.ID.Equals(subscriptionTypeID));
					subscriptionManager.SubscriptionProvider.UpdateSubscriptionMethod(subscriptionType.NotifyAction, GetCurrentRecipient(), senderNames);
				}

				resp.rs1 = "1";
			}
			catch (Exception e)
			{
				resp.rs1 = "0";
				resp.rs2 = "<div class='errorBox'>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
			}
			return resp;
		}

	}
}