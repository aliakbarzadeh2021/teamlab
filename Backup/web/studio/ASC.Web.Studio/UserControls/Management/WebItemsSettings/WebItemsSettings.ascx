<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebItemsSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.WebItemsSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>

<%--<div class="headerBase borderBase wisettingsTitle">
     <%=Resources.Resource.StudioProductModulesViewSettings%>
</div>--%>

<div id="studio_productModulesSettingsInfo">
</div>
    
<div id="studio_productModulesSettingsBox">

    <div>
    <%=Resources.Resource.ModuleProductSettingsDescription%>
    </div>

    <div class="clearFix" style="padding: 3px 0px;">
        <asp:Repeater runat="server" ID="_itemsRepeater">
            <ItemTemplate>
                <div class="headerBaseMedium" style="margin:20px 0px 10px;">
                    <input style="margin-right: 10px;" id="studio_poptDisabled_<%#Eval("ID")%>" value="<%#((bool)Eval("Disabled"))?"1" : "0"%>"
                        <%#((bool)Eval("Disabled"))?"" : "checked=checked"%> type="checkbox" onchange="WebItemsSettingsManager.ClickOnProduct('<%#Eval("ID")%>');" />
                    <label for="studio_poptDisabled_<%#Eval("ID")%>"><%#HttpUtility.HtmlEncode((string)Eval("Name"))%></label>
                </div>
                
                <div name="<%#Eval("ID")%>" id="studio_poptBox_<%#Eval("ID")%>" style="padding-left:25px;">
                    <asp:Repeater runat="server" ID="_subItemsRepeater">
                        <ItemTemplate>
                            <div class="borderBase<%#(bool)Eval("SortDisabled")?"": " sort"%>" id="studio_mopt_<%#Eval("ID")%>" name="<%#Eval("ID")%>" style="margin-bottom:3px;">
                                <table cellpadding="0" cellspacing="0" style="width:100%;">
                                    <tr valign="top">
                                        <td id="studio_moptHandle_<%#Eval("ID")%>" class="<%#(bool)Eval("SortDisabled")?"tintMedium": "moveHoverBackground"%> borderBase" align="center" style="width:18px; border-top:none;border-left:none; border-bottom:none; padding-right:1px;">&nbsp;</td>
                                        <td style="width:20px; padding:3px;">
                                            <input id="studio_moptDisabled_<%#Eval("ID")%>" value="<%#((bool)Eval("Disabled"))?"1" : "0"%>"
                                                onchange="WebItemsSettingsManager.ClickOnModule(this);" <%#((bool)Eval("Disabled"))?"" : "checked=checked"%>
                                                type="checkbox" <%#((bool)Eval("DisplayedAlways"))?"style='display:none;'" : ""%> />
                                        </td>
                                        <td style="padding:3px;">
                                            <div class="headerBaseSmall"><label for="studio_moptDisabled_<%#Eval("ID")%>"><%#HttpUtility.HtmlEncode((string)Eval("Name"))%></label></div>
                                        </td>
                                     </tr>
                                </table>
                            </div>                        
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

            </ItemTemplate>
        </asp:Repeater>       
    </div>
    <div class="clearFix" style="margin-top: 20px;">
        <a id="saveItemsSettingsBtn" class="baseLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo?" promoAction":"") %>" href="javascript:void(0);"><%=Resources.Resource.SaveButton %></a>
    </div>
</div>
