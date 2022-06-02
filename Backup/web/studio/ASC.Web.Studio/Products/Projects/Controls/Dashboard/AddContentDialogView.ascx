<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddContentDialogView.ascx.cs" Inherits="ASC.Web.Projects.Controls.Dashboard.AddContentDialogView" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<div id="studio_AddContentDialog" style="display:none;">
  <ascwc:Container runat="server" ID="AddContentContainer">
    <Header>
       "Add content"
    </Header>
    <Body>
      <div id="studio_AddContentContent">
        <asp:Repeater runat="server" ID="ContentTypesRepeater">
          <HeaderTemplate>
            <ul class="types">
          </HeaderTemplate>
          <ItemTemplate>
            <li class="type even">
              <a class="item" href="<%#(String)Eval("Link")%>">
                <img class="icon" src="<%#(String)Eval("Icon")%>" alt="" />
                <span class="label"><%#HttpUtility.HtmlEncode((String)Eval("Label"))%></span>
              </a>
            </li>
          </ItemTemplate>
          <FooterTemplate>
            </ul>
          </FooterTemplate>
        </asp:Repeater>
        
        <div class="pm-h-line"><!– –></div>

        <div class="additionMenu">        
          <a class="button grayLinkButton" href="javascript:void(0);" onclick=" jq.unblockUI();"><%=Resources.Resource.CancelButton %></a>
        </div>
      </div>
    </Body>
  </ascwc:Container>
</div>