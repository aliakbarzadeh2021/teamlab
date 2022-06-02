<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserSubscriptions.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.UserSubscriptions" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<a name="subscriptions"></a>


<div style="float: right; margin-top: -21px; margin-right: 125px;">
	<%=Resources.Resource.NotifyBy%>
</div>

<div class="borderBase clearFix" style="border-left:none; border-right:none; padding:10px;">
   <div class="headerBase" style="float:left;">
       <img alt="" style="margin-right:5px;" align="absmiddle" src="<%=WebImageSupplier.GetAbsoluteWebPath("lastadded_widget.png")%>"/>
       <%=Resources.Resource.WhatsNewSubscriptionName%>                
   </div>
    <div id="studio_newSubscriptionButton" style="float:right; text-align:right; padding-top:5px; width:110px">
        <%=RenderWhatsNewSubscriptionState()%>
    </div>
    
    <%--What's new Notify by Combobox--%>
    <div style="float: right;">
		<%=RenderWhatsNewNotifyByCombobox() %>
    </div>
</div>
            
<% if (IsAdmin())
   {%>
    <div class="borderBase clearFix" style="border-top:none; border-left:none; border-right:none; padding:10px;">
       <%--name & log for admin notify settings--%>
       <div class="headerBase" style="float:left;">
           <img alt="" style="margin-right:5px;" align="absmiddle" src="<%=WebImageSupplier.GetAbsoluteWebPath("btn_settings.png")%>"/>
           <%=Resources.Resource.AdministratorNotifySenderTypeName%>                
       </div>
        <%--unsubscribe link--%>    
        <div id="studio_adminSubscriptionButton" style="float:right; text-align:right; padding-top:5px; width:110px">
            <%=RenderAdminNotifySubscriptionState()%>
        </div>
        
        <%--Admin Notify Notify By Combobox--%>
        <div style="float: right;">
		    <%=RenderAdminNotifyNotifyByCombobox()%>
        </div>
    </div>
<% }%>

<div id="studio_notifySenders" class="clearFix">
    <%=RenderSubscriptions()%>
</div>
