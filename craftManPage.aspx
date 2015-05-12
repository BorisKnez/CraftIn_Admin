<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="craftManPage.aspx.cs" Inherits="Christoc.Modules.CraftAdminModule.craftManPage" %>
<%@ Register TagPrefix="dnn" TagName="TextEditor" Src="~/controls/TextEditor.ascx"%>
<script src='<%=ResolveUrl("js/parsley.js")%>' type="text/javascript"></script>
<script src='<%=ResolveUrl("js/craftmandetails.js")%>' type="text/javascript"></script>
<script type="text/javascript" src='<%=ResolveUrl("js/jquery-ui.min.js") %>'></script>
<script type="text/javascript" src='<%=ResolveUrl("js/jquery.fileupload.js") %>'></script>
<script type="text/javascript" src='<%=ResolveUrl("js/jquery.iframe-transport.js") %>'></script>
<html>
     <body>
<form id="craftContent" runat="server" onsubmit="return false;">
    <div class="fluid borBox craftManDetails">	
        <div class="gridRow">
	        <div id="UserInfo" class="span4 UserInfo fluid borBox">
                <h2>User info</h2>
                <section class="mw-line">
                    <span class="mw-title">Username*</span>
                    <span class="mw-field">
                        <input type="text" id="txtUsername" name="txtUsername" data-parsley-length="[3, 40]" placeholder="Enter craftman's username" data-parsley-pattern="/^[a-zA-Z0-9]+$/" data-parsley-pattern-message="One word only" runat="server" required>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Password*</span>
                    <span class="mw-field">
                        <input type="text" id="txtPassword" name="txtPassword" data-parsley-minlength="7" placeholder="Enter password" runat="server" required>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Email address*</span>
                    <span class="mw-field">
                        <input type="text" id="txtEmailAddress" name="txtEmailAddress" placeholder="Enter craftman's email" data-parsley-type="email" runat="server" required>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">First name*</span>
                    <span class="mw-field">
                        <input type="text" id="txtFirstName" name="txtFirstName" placeholder="Enter craftman's first name" runat="server" required>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Last name*</span>
                    <span class="mw-field">
                        <input type="text" id="txtLastName" name="txtLastName" placeholder="Enter craftman's last name" runat="server" required>
                    </span>
                </section>
	        </div>

	        <div id="BusinessInfo" class="span4 BusinessInfo">
		        <h2>Business info</h2>
                <section class="mw-line">
                    <span class="mw-title">Business name*</span>
                    <span class="mw-field">
                        <input type="text" id="txtBusinesssName" placeholder="Enter craftman's business name" name="txtBusinesssName" runat="server" required>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Description</span>
                    <span class="mw-field">
                        <%--<dnn:TextEditor ID="Description" runat="server" height="200" />--%>
                        <textarea id="txtDescription" name="txtDescription" placeholder="Enter craftman's description" runat="server"></textarea>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Business email</span>
                    <span class="mw-field">
                        <input type="text" id="txtBusinessEmailAddress" name="txtBusinessEmailAddress" data-parsley-type="email" placeholder="Enter craftman's business email" runat="server">
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Telephone</span>
                    <span class="mw-field">
                        <input type="text" id="txtTelephone" name="txtTelephone" placeholder="Enter craftmans's telephone humber" runat="server">
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Address</span>
                    <span class="mw-field">
                        <input type="text" id="txtAddress" name="txtAddress" placeholder="Enter craftman's address" runat="server">
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">City</span>
                    <span class="mw-field">
                        <input type="text" id="txtCity" name="txtCity" placeholder="Enter craftman's city" runat="server">
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Zip code</span>
                    <span class="mw-field">
                        <input type="text" id="txtZipCode" name="txtZipCode" data-parsley-type="number" placeholder="Enter craftman's zip code" runat="server">
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Country</span>
                    <span class="mw-field">
                        <select id="dropdownCountry" class="dropdownCountry" runat="server"></select>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title"><label for="craftRegions">Regions (1 minimum):</label></span>
                    <span class="mw-field" id="craftRegions" runat="server">
                         <%--<asp:CheckBoxList ID="checkboxListRegion" runat="server" RepeatColumns="2"></asp:CheckBoxList>	--%>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Vat ID</span>
                    <span class="mw-field">
                        <input type="text" id="txtVatID" name="txtVatID" data-parsley-type="number" placeholder="Enter craftman's VAT ID" runat="server">
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title"><label for="craftCategories">Categories (1 minimum):</label></span>
                    <span class="mw-field" id="craftCategories" runat="server">
                        <%--<asp:CheckBoxList id="checkboxListCategories" data-parsley-mincheck="1" runat="server" RepeatColumns="2"></asp:CheckBoxList>--%>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Logo</span>
                    <span class="mw-field">
                        <input type="file" id="LogoUpload" accept="image/*" name="LogoUpload" runat="server"/>
                        <span class="mw-image-look" id="mwimagelook" runat="server"></span>
                    </span>
                </section>
	        </div>

	        <div id="PortalInfo" class="span4 PortalInfo">
		        <h2>Portal info</h2>
		        <section class="mw-line">
                    <span class="mw-title">Portal alias*</span>
                    <span class="mw-field">
                        <input type="text" id="txtPortalAlias" name="txtPortalAlias" placeholder="Enter craftman's portal alias" class="tbReq" runat="server" required>
                    </span>
                </section>
                <section class="mw-line">
                    <span class="mw-title">Template*</span>
                    <span class="mw-field">
                        <asp:DropDownList ID="dropdownTemplate" runat="server"></asp:DropDownList>
                    </span>
                </section>
                <div id="hostOptions" runat="server" visible="false">
                    <h2>Co-administrator</h2>
                    <section class="mw-line">
                        <span class="mw-title">Admin</span>
                        <span class="mw-field">
                            <asp:DropDownList ID="dropdownAdministroator" runat="server"></asp:DropDownList>
                        </span>
                    </section>
                </div>
	        </div>

        </div>
        <div id="footerButtons" runat="server" visible="false" class="gridRow bottomBar">

		       <asp:Button ID="buttonAddNew" CssClass="btnAddNewCraftman" runat="server" Text="Add new craftman" OnClientClick="return validateInputs();" Visible="false"/>
               <asp:Button ID="buttonEditCraftMan" CssClass="btnEditCraftMan" runat="server" Text="Edit craftman" OnClientClick="return validateInputsEdit();" Visible="false"/>
                <img src="/DesktopModules/CraftAdminModule/images/loading-bar.gif" alt="loading" runat="server" class="loadingBar"/>
	    </div>
    </div>
    <asp:HiddenField ID="hfControlPath" runat="server" Value="" />
    <asp:HiddenField ID="hfstrServerPath" runat="server" Value="" />
    <asp:HiddenField ID="hfDomainName" runat="server" Value="" />
    <asp:HiddenField ID="hfCraftManID" runat="server" Value="" />
    <input type="hidden" id="hfImageName" name="hfImageName" runat="server">
</form>
</body>
</html>