﻿
<%@ Assembly Name="ASC.Web.UserControls.Forum"%>
<%@ Assembly Name="ASC.Forum"%>
<%@ Assembly Name="ASC.Web.Core"%>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ThreadCategoryListControl.ascx.cs" Inherits="ASC.Web.UserControls.Forum.ThreadCategoryListControl" %>

<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.UserControls.Forum.Common" %>
<%@ Import Namespace="ASC.Web.UserControls.Forum.Resources" %>


<asp:Repeater ID="_categoryRepeater" runat="server">
    <ItemTemplate>
       <div class="clearFix">
            <div class="headerBase" style="padding:25px 0px 15px 0px;">
                <%#HttpUtility.HtmlEncode((string)Eval("Title"))%>
            </div>
            
            <asp:Repeater ID="_threadRepeater" runat="server">
                <ItemTemplate>                
                    <div class="<%#RenderThreadCSSClass(Container.ItemIndex, (Container.DataItem as ASC.Forum.Thread))%> clearFix borderBase" style="padding:5px 0px; margin-top:-1px; border-right:none; border-left:none;">
                        <table cellpadding="0" cellspacing="0" style="width:100%;">
                            <tr valign="top">
                                <td align="center" style="width:35px; padding:0px 10px;">
                                <%# ((ASC.Forum.Thread)Container.DataItem).IsNew()?                                    
                                    ("<img alt=\"new\"  align='absmiddle' src=\"" + WebImageSupplier.GetAbsoluteWebPath("forum_read.png", _settings.ImageItemID) + "\"/>"):
                                    ("<img alt=\"\" align='absmiddle' src=\"" + WebImageSupplier.GetAbsoluteWebPath("forum_unread.png", _settings.ImageItemID) + "\"/>")
                                %> 
                                </td> 
                                <td style="width:40%; padding-top:8px;">
                                    <div style="padding-right:5px;">
                                        <a class="linkHeaderLight" href="<%#_settings.LinkProvider.TopicList((int)Eval("ID"))%>"><%#HttpUtility.HtmlEncode((string)Eval("Title"))%></a>
                                    </div>
                                    <div class="textMediumDescribe" style="padding:2px 0px;"><%#HttpUtility.HtmlEncode((string)Eval("Description"))%></div>                
                                </td>
                                <td class="headerBase" style="width:12%; padding-top:8px; text-align:center;">
                                    <%#Eval("TopicCount")%>
                                </td>
                                <td class="headerBase" style=" padding-top:8px; text-align:center;">                                    
                                    <%#Eval("PostCount")%>
                                </td>
                                <td style="width:25%; padding-top:8px;">
                                <%#RenderRecentUpdate((Container.DataItem as ASC.Forum.Thread))%>                                
                                </td>          
                            </tr>
                        </table>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
       </div>        
    </ItemTemplate>
</asp:Repeater>
