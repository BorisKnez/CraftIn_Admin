<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Christoc.Modules.CraftAdminModule.View" %>
<script src='<%=ResolveUrl("js/jquery.colorbox-min.js")%>' type="text/javascript"></script>
<script src='<%=ResolveUrl("js/craftmenlist.js")%>' type="text/javascript"></script>
<table cellspacing="0" cellpadding="0" class="rwd-table table-craftmen">
    <tbody>
        <tr>
            <th><i class="icon-down-open"></i> Busines name <i class="icon-up-open"></i></th>
            <th>
                <select class="select select-admincategory" ID="admincategory" runat="server">
                  <option value="-1">Select category(All)</option>
                </select>
            </th>
            <th>
               <select class="select select-adminregion" ID="adminregion" runat="server">
                  <option value="-1">Select region(All)</option>
              </select>
         	</th>
            <th>
            <select class="select select-admincountry" ID="admincountry" runat="server">
              <option value="-1">Select country(All)</option>
          	</select>
          </th>
            <th><i class="icon-down-open"></i> Email <i class="icon-up-open"></i></th>
            <th><i class="icon-down-open"></i> User <i class="icon-up-open"></i></th>
            <th><i class="icon-down-open"></i> Portal <i class="icon-up-open"></i></th>
            <th>Status</th>
            <th>Controls</th>
        </tr>
    </tbody>
    <tbody class="craftMenListTR" id="craftMenListTR" runat="server"></tbody>
</table>
<div class="bottomBar" id="bottomBar" runat="server">
    <a class="button button-addnewCraftman" href="javascript:void(0)" title="Add new craftman"><em class="icon-plus-squared"></em>Add new craftman</a>
</div>

<asp:HiddenField ID="hfControlPath" runat="server" Value="" />
<asp:HiddenField ID="hfPortalID" runat="server"/>
<asp:HiddenField ID="hfMessageFromCB" runat="server"/>