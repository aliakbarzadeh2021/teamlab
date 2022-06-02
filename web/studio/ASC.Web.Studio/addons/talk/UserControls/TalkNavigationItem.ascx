<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TalkNavigationItem.ascx.cs" Inherits="ASC.Web.Talk.UserControls.TalkNavigationItem" %>

<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Talk.Addon" %>

<%@ Register Assembly="ASC.Web.Core" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<style type="text/css">
  .studioTopNavigationPanel .systemSection .itemBox.talk span.link{background:url("<%=WebImageSupplier.GetAbsoluteWebPath("messages_user.png")%>") left 1px no-repeat;}
  .studioTopNavigationPanel .systemSection .itemBox.talk span.link span.count-container{display:none;}
  .studioTopNavigationPanel .systemSection .itemBox.talk span.link.message-blink{background-image:url("<%=WebImageSupplier.GetAbsoluteWebPath("ico_tm-talk-incoming.gif")%>");}
  .studioTopNavigationPanel .systemSection .itemBox.talk span.link.message-blink span.count-container{display:inline;}
</style>

<script type="text/javascript" src="<%=WebPath.GetPath("addons/talk/js/talk.navigationitem.js")%>"></script>

<script type="text/javascript">
  ASC.Controls.JabberClient.init('<%=GetUserName()%>', '<%=GetJabberClientPath()%>', '<%=GetOpenContactHandler()%>');
  ASC.Controls.NavigationItem.init('<%=GetUnreadMessagesHandler()%>', '<%=GetUpdateInterval()%>');
</script>

<li class="itemBox talk" style="float: right;">
  <span id="talkMsgLabel" class="link" onclick="ASC.Controls.JabberClient.open()">
    <span><%=GetMessageStr()%><span class="count-container">&nbsp;(<span id="talkMsgCount">0</span>)</span></span>
  </span>
</li>
