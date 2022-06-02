﻿<%@ Assembly Name="ASC.Web.UserControls.Wiki" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListFiles.ascx.cs" Inherits="ASC.Web.UserControls.Wiki.UC.ListFiles" %>
<%@ Import Namespace="ASC.Web.UserControls.Wiki.Data" %>
<div class="wikiList">
    <asp:Repeater ID="rptListFiles" runat="Server">
        <ItemTemplate>
            <div>
                <a href="<%#GetFileLink((Container.DataItem as Files).FileName)%>" title="<%#(Container.DataItem as Files).UploadFileName%>">
                    <%#(Container.DataItem as Files).FileName%></a> &nbsp;
                <asp:LinkButton ID="cmdDeleteFile" runat="Server" Text="Del" OnClientClick="javascript:return confirm('Delete?');"
                    OnClick="cmdDeleteFile_Click" CommandName='<%# (Container.DataItem as Files).FileName%>' />
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>