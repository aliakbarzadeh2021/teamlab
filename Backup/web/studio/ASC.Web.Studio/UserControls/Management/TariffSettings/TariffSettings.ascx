<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TariffSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.TariffSettings" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Users" TagPrefix="ascwc" %>

<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<%@ Import Namespace="ASC.Core.Users" %>
 

<div class="tariffPlanBox">
    
    <%if (_tariff == ASC.Core.Billing.TariffPlan.Premium && !_expired)
   { %>
        <div class="headerBase">
             <%=String.Format(Resources.Resource.PremiumTariffEndDateTitle, " <span class=\"date\">" + _tariffEndDate.ToShortDateString() + "</span>")%>
        </div>
        
    <%}
    else if (_tariff == ASC.Core.Billing.TariffPlan.Premium && _expired)
    { %>
        <div class="headerBase">  
            <%=Resources.Resource.ProlongPremiumTitle%>
        </div>
        <div class="description">
             <%=String.Format(Resources.Resource.TariffDescriptionPremium,"<br/>")%>
        </div>
        <div class="goBtnBox clearFix">
            <a target="_blank" id="expiredBuyButton" href="<%=ASC.Web.Studio.Utility.CommonLinkUtility.ShoppingCardUrl()%>" class="premiumButton">
                <div class="left">
                    <div class="content"><%=Resources.Resource.PremiumStubButton%></div>
                </div>
                <div class="right">&nbsp;</div>
            </a>
        </div>
    <%}
      else if (_tariff == ASC.Core.Billing.TariffPlan.Free)
   {%>
   
    <div class="headerBase">  
            <%=Resources.Resource.GetPremiumTitle%>
        </div>
        <div class="description">
            <%=Resources.Resource.TariffDescriptionFree%>
        </div>
        <div class="goBtnBox clearFix">
            <a target="_blank" id="tariffBuyButton" href="<%=ASC.Web.Studio.Utility.CommonLinkUtility.ShoppingCardUrl()%>" class="premiumButton">
                <div class="left">
                    <div class="content"><%=Resources.Resource.PremiumStubButton%></div>
                </div>
                <div class="right">&nbsp;</div>
            </a>
        </div>

    <%}%>
        
        <asp:Repeater runat="server" ID="_paymentsRepeater">
        <HeaderTemplate>
            <div class="headerBase" id="paymentsTitle">
                <%=Resources.Resource.PaymentsTitle%>
            </div>     
            
            <table class="paymentsTable">
        </HeaderTemplate>
            <ItemTemplate>
                <tr class="borderBase <%#Container.ItemIndex %2 ==0?"tintMedium":""%>">
                    <td>#<%#Eval("CartId")%></td> 
                    <td><%# ((DateTime)Eval("Date")).ToShortDateString() %> <%#((DateTime)Eval("Date")).ToShortTimeString() %></td>
                    <td><%#((decimal)Eval("Price")).ToString("###,##")%> <%#Eval("Currency")%></td>
                    <td><%#Eval("Method")%></td>
                    <td><%#Eval("Name")%></td>
                    <td><%#Eval("Email")%></td>
                </tr>
                
            </ItemTemplate>            
            
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    
   
</div>

<script language="javascript">
    jq(function() {
        jq('#tariffBuyButton').click(function() { EventTracker.Track('buynow_paymentsFromFree') });
        jq('#expiredBuyButton').click(function() { EventTracker.Track('buynow_paymentsExpired') });
    })
    
</script>