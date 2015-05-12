using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI.Skins.Controls;
using System.Collections;
using DotNetNuke.Services.Installer.Log;
using System.Web.Services;
using System.Threading;
using DotNetNuke.Security.Roles;
using System.Text;

namespace Christoc.Modules.CraftAdminModule
{
    public partial class craftManPage : System.Web.UI.Page
    {
        private string _craftID;
        private string _formtype;
        private string _PORTALID;
        private CultureDropDownTypes DisplayType { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                hfControlPath.Value = Request.Form["controlPath"];
                hfstrServerPath.Value = Globals.GetAbsoluteServerPath(Request);
                hfDomainName.Value = Globals.GetDomainName(Request, true);
                
                _craftID = Request.Form["id_craft"];
                _formtype = Request.Form["formType"];
                _PORTALID = Request.Form["id_portal"];
                
                if(PortalSettings.Current.UserInfo.IsSuperUser)
                    hostOptions.Visible = true;
                if(_formtype != null)
                {
                    switch(_formtype)
                    {
                        case "view":
                            if (_craftID != null)
                            {
                                addCategories(false);
                                addCountries(false);
                                addRegions(false);
                                addTempaltes(true);
                                loadCraftManData(_craftID);
                                disableAllFields();
                            }
                            break;
                        case "new":
                                addCategories(true);
                                addCountries(true);
                                addRegions(true);
                                addTempaltes(true);
                                addMunicipalities();
                                footerButtons.Visible = true;
                                buttonAddNew.Visible = true;
                            break;
                        case "edit":
                            if (_craftID != null)
                            {
                                hfCraftManID.Value = _craftID;
                                addCategories(true);
                                addCountries(true);
                                addRegions(true);
                                addTempaltes(true);
                                addMunicipalities();
                                loadCraftManData(_craftID);
                                footerButtons.Visible = true;
                                buttonEditCraftMan.Visible = true;
                                disableNonEditableFields();
                            }
                            break;
                    }
                }
                
            }
            catch (Exception ex)
            {
                AddErrorToLogs(ex.Message.ToString(), 0);
            }
        }

        private void addMunicipalities()
        {
            string rolename = "CraftMan Support";
            RoleController roleCtrl = new RoleController();
            var roleUssers = roleCtrl.GetUsersByRole(0, rolename);
            foreach (var user in roleUssers)
            {
                ListItem userItem = new ListItem();
                userItem.Value = user.UserID.ToString();
                userItem.Text = user.Username;
                dropdownAdministroator.Items.Add(userItem);
            }
        }

        private void addCategories(bool enabled)
        {
            using(DBConnectionDataContext db = new DBConnectionDataContext())
            {
                if (_craftID != null)
                {
                    var craftUser = (from cr in db._craftin_CraftUsers where cr.ID == int.Parse(_craftID) select cr).FirstOrDefault();
                    var categories = (from cat in db._craftin_CraftManCategories select cat);

                    string disabled = "";
                    if (!enabled)
                        disabled = "disabled";

                    if (categories != null)
                    {
                        StringBuilder sbCategories = new StringBuilder();
                        int i = 1;
                        foreach (var c in categories)
                        {
                            string _checked = "";
                            foreach (var item in craftUser._craftin_CraftManCategoriesLists)
                            {
                                if (item.CategoryID == c.CategoryID)
                                {
                                    _checked = "checked";
                                    break;
                                }
                            }
                            if (i == 1)
                            {
                                sbCategories.AppendLine("<span class=\"categorieItem\"><label for=\"" + c.CategoryName + "\">" + c.CategoryName + "</label><input type=\"checkbox\" data-parsley-required=\"true\" " + disabled + " " + _checked + " data-parsley-mincheck=\"1\" name=\"craftCategories\" id=\"" + c.CategoryName + "\" value=\"" + c.CategoryID.ToString() + "\" /></span><br>");
                                i++;
                            }
                            else
                            {
                                sbCategories.AppendLine("<span class=\"categorieItem\"><label for=\"" + c.CategoryName + "\">" + c.CategoryName + "</label><input type=\"checkbox\" " + disabled + " " + _checked + " name=\"craftCategories\" id=\"" + c.CategoryName + "\" value=\"" + c.CategoryID.ToString() + "\" /></span>");
                            }

                        }
                        craftCategories.Controls.Add(new LiteralControl(sbCategories.ToString()));
                    }
                }
                else
                {
                    var categories = (from cat in db._craftin_CraftManCategories select cat);
                    if (categories != null)
                    {
                        StringBuilder sbCategories = new StringBuilder();
                        int i = 1;
                        foreach (var c in categories)
                        {
                            if (i == 1)
                            {
                                sbCategories.AppendLine("<span class=\"categorieItem\"><label for=\"" + c.CategoryName + "\">" + c.CategoryName + "</label><input type=\"checkbox\" data-parsley-required=\"true\" data-parsley-mincheck=\"1\" name=\"craftCategories\" id=\"" + c.CategoryName + "\" value=\"" + c.CategoryID.ToString() + "\" /></span><br>");
                                i++;
                            }
                            else
                            {
                                sbCategories.AppendLine("<span class=\"categorieItem\"><label for=\"" + c.CategoryName + "\">" + c.CategoryName + "</label><input type=\"checkbox\" name=\"craftCategories\" id=\"" + c.CategoryName + "\" value=\"" + c.CategoryID.ToString() + "\" /></span>");
                            }

                        }
                        craftCategories.Controls.Add(new LiteralControl(sbCategories.ToString()));
                    }
                }
            }
        }

        private void addRegions(bool enabled)
        {
            using (DBConnectionDataContext db = new DBConnectionDataContext())
            {
                if (_craftID != null)
                {
                    var craftUser = (from cr in db._craftin_CraftUsers where cr.ID == int.Parse(_craftID) select cr).FirstOrDefault();
                    var regions = (from reg in db._craftin_CraftManRegions where reg.RegionCountryID == craftUser.CountryID select reg);

                    string disabled = "";
                    if(!enabled) 

                        disabled = "disabled";
                    if (regions != null)
                    {
                        StringBuilder sbRegions = new StringBuilder();
                        int i = 1;
                        foreach (var r in regions)
                        {
                            string _checked = "";
                            foreach (var item in craftUser._craftin_CraftManRegionLists)
                            {
                                if(item.RegionID == r.ID)
                                {
                                    _checked = "checked";
                                    break;
                                }
                            }
                                if (i == 1)
                                {
                                    sbRegions.AppendLine("<span class=\"regionItem\"><label for=\"" + r.RegionName + "\">" + r.RegionName + "</label><input type=\"checkbox\" data-parsley-required=\"true\" " + disabled + " " + _checked + " data-parsley-mincheck=\"1\" name=\"craftRegions\" id=\"" + r.RegionName + "\" value=\"" + r.ID.ToString() + "\" /></span><br>");
                                    i++;
                                }
                                else
                                {
                                    sbRegions.AppendLine("<span class=\"regionItem\"><label for=\"" + r.RegionName + "\">" + r.RegionName + "</label><input type=\"checkbox\" " + disabled + " " + _checked + " name=\"craftRegions\" id=\"" + r.RegionName + "\" value=\"" + r.ID.ToString() + "\" /></span>");
                                }

                        }
                        craftRegions.Controls.Add(new LiteralControl(sbRegions.ToString()));
                    }
                }else
                {
                    var regions = (from reg in db._craftin_CraftManRegions select reg);
                    if (regions != null)
                    {
                        StringBuilder sbRegions = new StringBuilder();
                        int i = 1;
                        foreach (var r in regions)
                        {
                            if (r.RegionCountryID == int.Parse(dropdownCountry.Value))
                            {
                                if (i == 1)
                                {
                                    sbRegions.AppendLine("<span class=\"regionItem\"><label for=\"" + r.RegionName + "\">" + r.RegionName + "</label><input type=\"checkbox\" data-parsley-required=\"true\" data-parsley-mincheck=\"1\" name=\"craftRegions\" id=\"" + r.RegionName + "\" value=\"" + r.ID.ToString() + "\" /></span><br>");
                                    i++;
                                }
                                else
                                {
                                    sbRegions.AppendLine("<span class=\"regionItem\"><label for=\"" + r.RegionName + "\">" + r.RegionName + "</label><input type=\"checkbox\" name=\"craftRegions\" id=\"" + r.RegionName + "\" value=\"" + r.ID.ToString() + "\" /></span>");
                                }
                            }
                            
                        }
                        craftRegions.Controls.Add(new LiteralControl(sbRegions.ToString()));
                        //craftRegions.InnerHtml = sbRegions.ToString();
                    }
                }
                
            }
        }

        private void addCountries(bool enabled)
        {
            using (DBConnectionDataContext db = new DBConnectionDataContext())
            {
                var countries = (from cou in db._craftin_CraftManCountryLists select cou);
                if (countries != null)
                {
                    foreach (var c in countries)
                    {
                        ListItem cb = new ListItem();
                        cb.Text = c.CountryName;
                        cb.Value = c.CountryID.ToString();
                        dropdownCountry.Items.Add(cb);
                    }
                    dropdownCountry.Disabled = !enabled;
                }
            }
        }


        class TemplateDisplayComparer : IComparer<PortalController.PortalTemplateInfo>
        {
            public int Compare(PortalController.PortalTemplateInfo x, PortalController.PortalTemplateInfo y)
            {
                var cultureCompare = String.Compare(x.CultureCode, y.CultureCode, StringComparison.CurrentCulture);
                if (cultureCompare == 0)
                {
                    return String.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
                }

                //put blank cultures last
                if (string.IsNullOrEmpty(x.CultureCode) || string.IsNullOrEmpty(y.CultureCode))
                {
                    cultureCompare *= -1;
                }
                return cultureCompare;
            }
        }

        ListItem CreateListItem(PortalController.PortalTemplateInfo template)
        {
            string text, value;
            if (string.IsNullOrEmpty(template.CultureCode))
            {
                text = template.Name;
                value = Path.GetFileName(template.TemplateFilePath);
            }
            else
            {
                if (DisplayType == 0)
                {
                    string _ViewType = Convert.ToString(DotNetNuke.Services.Personalization.Personalization.GetProfile("LanguageDisplayMode", "ViewType" + _PORTALID));
                    switch (_ViewType)
                    {
                        case "NATIVE":
                            DisplayType = CultureDropDownTypes.NativeName;
                            break;
                        case "ENGLISH":
                            DisplayType = CultureDropDownTypes.EnglishName;
                            break;
                        default:
                            DisplayType = CultureDropDownTypes.DisplayName;
                            break;
                    }
                }

                text = string.Format("{0} - {1}", template.Name, Localization.GetLocaleName(template.CultureCode, DisplayType));
                value = string.Format("{0}|{1}", Path.GetFileName(template.TemplateFilePath), template.CultureCode);
            }

            return new ListItem(text, value);
        }

        private void addTempaltes(bool enabled)
        {
            var templates = PortalController.Instance.GetAvailablePortalTemplates();
            templates = templates.OrderBy(x => x, new TemplateDisplayComparer()).ToList();

            foreach (var template in templates)
            {
                var item = CreateListItem(template);
                ListItem cb = new ListItem();
                cb.Text = item.Text;
                cb.Value = item.Value;
                cb.Enabled = enabled;
                dropdownTemplate.Items.Add(cb);
            }

            SelectADefaultTemplate(templates);

            if (dropdownTemplate.Items.Count == 0)
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "", "PortalMissing", ModuleMessage.ModuleMessageType.RedError);
            }

        }

        void SelectADefaultTemplate(IList<PortalController.PortalTemplateInfo> templates)
        {
            string currentCulture = Thread.CurrentThread.CurrentUICulture.Name;

            var defaultTemplates = templates.Where(x => Path.GetFileNameWithoutExtension(x.TemplateFilePath) == "Default Website").ToList();

            var match = defaultTemplates.FirstOrDefault(x => x.CultureCode == currentCulture);
            if (match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => x.CultureCode.StartsWith(currentCulture.Substring(0, 2)));
            }
            if (match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => String.IsNullOrEmpty(x.CultureCode));
            }
            if (match != null)
            {
                dropdownTemplate.SelectedIndex = templates.IndexOf(match);
            }
        }



        private void loadCraftManData(string _craftID)
        {
            int ID;
            bool result = Int32.TryParse(_craftID, out ID);
            if (result)
            {
                using(DBConnectionDataContext db = new DBConnectionDataContext())
                {
                    var craftman = (from craft in db._craftin_CraftUsers where craft.ID == ID select craft).FirstOrDefault();
                    if(craftman != null)
                    {
                        //user info
                        txtUsername.Value = craftman.Username;
                        txtPassword.Value = craftman.Password;
                        txtEmailAddress.Value = craftman.Email;
                        //txtDisplayName.Value = craftman.DisplayName;
                        txtFirstName.Value = craftman.FirstName;
                        txtLastName.Value = craftman.LastName;
                        //business info
                        txtBusinesssName.Value = craftman.Company;
                        txtDescription.Value = craftman.Description;
                        txtBusinessEmailAddress.Value = craftman.CompanyEmail;
                        txtTelephone.Value = craftman.Phone;
                        txtAddress.Value = craftman.Address;
                        txtCity.Value = craftman.City;
                        txtZipCode.Value = craftman.ZipCode;
                        txtPortalAlias.Value = craftman.PortalName;


                        var admin = (from u in db.Users where u.UserID == craftman.OwnerID select u);
                        if (admin != null)
                        {
                            foreach (var u in admin)
                            {
                                dropdownAdministroator.Items.Add(new ListItem(u.DisplayName, u.UserID.ToString()));
                            }
                        }

                        dropdownCountry.SelectedIndex = -1;
                        dropdownCountry.Items.FindByValue(craftman.CountryID.ToString()).Selected = true;


                        if (craftman.CompanyMainImage != null && craftman.CompanyMainImage.Length > 0)
                        {
                            string picURL = "/Portals/craftman_pictures/" + craftman.CompanyMainImage;
                            hfImageName.Value = craftman.CompanyMainImage;
                            mwimagelook.InnerHtml = "<img style='width: auto; max-height: 170px; display: block;' src='" + picURL + "' alt='image'><input type='button' style='background: none repeat scroll 0px 0px rgb(190, 27, 44); color: white; width: auto; padding: 5px 10px; margin-bottom: 5px;' class='btnRemoveImage' value='Remove'>";
                        }


                    }
                }
            }
        }//end loadCraftManData



        private void disableAllFields()
        {
            txtUsername.Disabled = true;
            txtPassword.Disabled = true;
            txtEmailAddress.Disabled = true;
            txtFirstName.Disabled = true;
            txtLastName.Disabled = true;
            txtBusinesssName.Disabled = true;
            txtDescription.Disabled = true;
            txtBusinessEmailAddress.Disabled = true;
            txtTelephone.Disabled = true;
            txtAddress.Disabled = true;
            txtCity.Disabled = true;
            txtZipCode.Disabled = true;
            txtVatID.Disabled = true;
            LogoUpload.Disabled = true;
            txtPortalAlias.Disabled = true;
            dropdownTemplate.Enabled = false;
            dropdownAdministroator.Enabled = false;
        }
        private void disableNonEditableFields()
        {
            txtUsername.Disabled = true;
            txtPassword.Disabled = true;
            txtPortalAlias.Disabled = true;
            dropdownTemplate.Enabled = false;
        }


        private void AddErrorToLogs(string _msg, int _status)
        {
            EventLogController eventLog = new EventLogController();
            LogInfo logInfo = new LogInfo();
            logInfo.LogPortalID = PortalSettings.Current.PortalId;
            logInfo.LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString();
            logInfo.AddProperty("Message", _msg);
            eventLog.AddLog(logInfo);
            Response.Redirect(Globals.NavigateURL(88) + "?err=" + _status);
        }


    }
}