<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserStatistics.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Users.UserStatistics" %>
<a name="statistic"></a>

<div class="headerBase borderBase" style="padding: 0px 0px 5px 15px; border-top:none; border-right:none; border-left:none;">
 <%=Resources.Resource.Statistic %>
 </div>
<div class="clearFix" style="padding:15px 15px 0 15px;">
    <%=RenderUserStatistic()%>
</div>


