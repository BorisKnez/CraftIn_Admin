using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

namespace Christoc.Modules.CraftAdminModule
{
    /// <summary>
    /// Summary description for CraftManWS
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class CraftManWS : System.Web.Services.WebService
    {
        private UserInfo user = UserController.GetUserByName(PortalSettings.Current.PortalId, HttpContext.Current.User.Identity.Name);

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string getRegionsForCountry(int country)
        {
            using (DBConnectionDataContext db = new DBConnectionDataContext())
            {
                StringBuilder sbRegions = new StringBuilder();
                var regions = (from reg in db._craftin_CraftManRegions where reg.RegionCountryID == country select reg);
                if (regions != null)
                {
                        int i = 1;
                        foreach (var r in regions)
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
                return sbRegions.ToString();
            }
        }


        PortalController.PortalTemplateInfo LoadPortalTemplateInfoForSelectedItem(string value)
        {
            var values = value.Split('|');
            return PortalController.Instance.GetPortalTemplate(Path.Combine(TestableGlobals.Instance.HostMapPath, values[0]), values.Length > 1 ? values[1] : null);
        }

        //@strServer - fizična pot na disku
        //@domain - domena lokalno naprimer  craftin
        //Return statuses:
        //1 - All ok
        //2 - Misssing data
        //3 - Error - View event viewer for details


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<craftMessage> CreateNewPortal(string strServer, string domain, string strPortalName, string strPassword, string strEmail
            , string strFirstName, string strLastName, string strUserName, string strBusinessName, string strBusinessDescription, string strBusinessEmail
            , string strBusinessTelephone, string strBusinessAddress, string strBusinessCity, string strBusinessZipCode, string strBusinessVatID, string strCountryID
            , string strBusinessRegions, string strBusinessCategories, string selectedTemplate, string strBusinessMainImage)
        {
            List<craftMessage> craftReturn = new List<craftMessage>();
            craftMessage craftMessage = new craftMessage();


            if(strServer == null || strServer.Length < 1 || domain == null || domain.Length < 1 || strPortalName == null || strPortalName.Length < 1 || 
                strPassword == null || strPassword.Length < 1 || strEmail == null || strEmail.Length < 1 || strFirstName == null || strFirstName.Length < 1 ||
                strLastName == null || strLastName.Length < 1 || strUserName == null || strUserName.Length < 1 || strCountryID == null || strCountryID.Length < 1 ||
                selectedTemplate == null || selectedTemplate.Length < 1)
            {
                craftMessage.Status = 2;
                craftMessage.Message = "Missing data";
                craftReturn.Add(craftMessage);
                return craftReturn;
            }

            try
            {
                PortalController.PortalTemplateInfo template = LoadPortalTemplateInfoForSelectedItem(selectedTemplate);
                bool blnChild;
                string strPortalAlias = string.Empty;
                string strChildPath = string.Empty;

                var objPortalController = new PortalController();

                //check template validity
                var messages = new ArrayList();
                /* string schemaFilename = Server.MapPath(string.Concat(AppRelativeTemplateSourceDirectory, "portal.template.xsd"));
                 string xmlFilename = template.TemplateFilePath;
                 var xval = new PortalTemplateValidator();
                 if (!xval.Validate(xmlFilename, schemaFilename))
                 {
                     DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "", "InvalidTemplate" + Path.GetFileName(template.TemplateFilePath), ModuleMessage.ModuleMessageType.RedError);
                     messages.AddRange(xval.Errors);
                     //lstResults.Visible = true;
                     //lstResults.DataSource = messages;
                     //lstResults.DataBind();
                     //validationPanel.Visible = true;
                     return;
                 }*/

                //Validate Portal Name
                if (!Globals.IsHostTab(PortalSettings.Current.ActiveTab.TabID))
                {
                    blnChild = true;
                    strPortalAlias = strPortalName;
                }
                else
                {
                    blnChild = true;
                    //strPortalAlias = blnChild ? PortalController.GetPortalFolder(txtPortalAlias.Text) : txtPortalAlias.Text;
                }

                ModuleMessage.ModuleMessageType messageType = ModuleMessage.ModuleMessageType.RedError;
                if (!PortalAliasController.ValidateAlias(strPortalAlias, blnChild))
                {
                    craftMessage.Status = 3;
                    craftMessage.Message = "Invalid portal alias";
                    craftReturn.Add(craftMessage);
                    return craftReturn;
                }

                //check whether have conflict between tab path and portal alias.
                var checkTabPath = string.Format("//{0}", strPortalAlias);
                if (TabController.GetTabByTabPath(PortalSettings.Current.PortalId, checkTabPath, string.Empty) != Null.NullInteger)
                {
                    craftMessage.Status = 3;
                    craftMessage.Message = "Duplicate tab path and portal alias";
                    craftReturn.Add(craftMessage);
                    return craftReturn;
                }

                string strServerPath = strServer;
                //Set Portal Alias for Child Portals
                    if (blnChild)
                    {
                        strChildPath = strServerPath + strPortalAlias;

                        if (Directory.Exists(strChildPath))
                        {
                            craftMessage.Status = 3;
                            craftMessage.Message = "Child portal with that name already exists. Please change portal alias.";
                            craftReturn.Add(craftMessage);
                            return craftReturn;
                        }
                        else
                        {
                            if (!Globals.IsHostTab(PortalSettings.Current.ActiveTab.TabID))
                            {
                                strPortalAlias = domain + "/" + strPortalAlias;
                            }
                            else
                            {
                                strPortalAlias = strPortalName;
                            }
                        }
                    }
                //Get Home Directory
                //string homeDir = txtHomeDirectory.Text != @"Portals/[PortalID]" ? txtHomeDirectory.Text : "";

                string homeDir = "";

                //Validate Home Folder
                if (!string.IsNullOrEmpty(homeDir))
                {
                    if (string.IsNullOrEmpty(String.Format("{0}\\{1}\\", Globals.ApplicationMapPath, homeDir).Replace("/", "\\")))
                    {
                        craftMessage.Status = 3;
                        craftMessage.Message = "Invalid home folder.";
                        craftReturn.Add(craftMessage);
                        return craftReturn;
                    }
                    if (homeDir.Contains("admin") || homeDir.Contains("DesktopModules") || homeDir.ToLowerInvariant() == "portals/")
                    {
                        craftMessage.Status = 3;
                        craftMessage.Message = "Home folder error. Portal alias includes forbidden words like admin/desktopmodules/portals. Please change portal alias";
                        craftReturn.Add(craftMessage);
                        return craftReturn;
                    }
                }

                //Validate Portal Alias
                if (!string.IsNullOrEmpty(strPortalAlias))
                {
                    PortalAliasInfo portalAlias = PortalAliasController.Instance.GetPortalAlias(strPortalAlias.ToLower());
                    if (portalAlias != null)
                    {
                        craftMessage.Status = 3;
                        craftMessage.Message = "Portal aslias with that name already exist. Please change it.";
                        craftReturn.Add(craftMessage);
                        return craftReturn;
                    }
                }
                //check if username for new user exist

                using (DBConnectionDataContext db = new DBConnectionDataContext())
                {

                    if (db.Users.Any(u => u.Username == strUserName))
                    {
                        craftMessage.Status = 3;
                        craftMessage.Message = "Username already taken. Please change it and try again.";
                        craftReturn.Add(craftMessage);
                        return craftReturn;
                    }
                }

                //Create Portal
                //if (String.IsNullOrEmpty(message))
                //{
                    UserInfo adminUserForNewPortal = new UserInfo();
                    int intPortalId;  //new portal ID
                    try
                    {
                        intPortalId = objPortalController.CreatePortal(strPortalName,
                                                                   user.UserID,   //prenesemo občino/hosta kot admina
                                                                   strPortalName,
                                                                   "", //keywords
                                                                   template,
                                                                   homeDir,
                                                                   strPortalAlias,
                                                                   strServerPath,
                                                                   strChildPath,
                                                                   blnChild);
                        //Clears the cache
                        DotNetNuke.Common.Utilities.DataCache.ClearPortalCache(PortalSettings.Current.PortalId, false);
                    }
                    catch (Exception ex)
                    {
                        intPortalId = Null.NullInteger;
                        craftMessage.Status = 3;
                        craftMessage.Message = ex.Message.ToString();
                        craftReturn.Add(craftMessage);
                        return craftReturn;

                    }
                    if (intPortalId != -1)
                    {
                        adminUserForNewPortal = new UserInfo
                        {
                            FirstName = strFirstName,
                            LastName = strLastName,
                            Username = strUserName,
                            DisplayName = strFirstName + " " + strLastName,
                            Email = strEmail,
                            IsSuperUser = false,
                            PortalID = intPortalId,
                            Membership =
                            {
                                Approved = true,
                                Password = strPassword,
                                PasswordQuestion = "",
                                PasswordAnswer = ""
                            },
                            Profile =
                            {
                                FirstName = strFirstName,
                                LastName = strLastName
                            }
                        };

                        UserCreateStatus userCreatstatus = UserController.CreateUser(ref adminUserForNewPortal);
                        if (userCreatstatus == UserCreateStatus.Success)
                        {
                            //ga prenesemo še na glavni portal
                            using (DBConnectionDataContext db = new DBConnectionDataContext())
                            {
                                int maxUserID = db.UserPortals.Max(u => u.UserPortalId);
                                UserPortal dodPortal = new UserPortal()
                                {
                                    UserId = adminUserForNewPortal.UserID,
                                    PortalId = 0,
                                    UserPortalId = maxUserID + 1,
                                    CreatedDate = DateTime.Now,
                                    Authorised = true,
                                    IsDeleted = false,
                                    RefreshRoles = false,
                                    VanityUrl = ""
                                };
                                try
                                {
                                    var portal0Exist = (from zapis in db.UserPortals where zapis.PortalId == 0 select zapis.PortalId).FirstOrDefault();
                                    if (portal0Exist == 0)
                                    {
                                        db.UserPortals.InsertOnSubmit(dodPortal);
                                        db.SubmitChanges();
                                    }
                                }
                                catch (Exception e)
                                {
                                    EventLogController eventLog = new EventLogController();
                                    LogInfo logInfo = new LogInfo();
                                    logInfo.LogPortalID = PortalSettings.Current.PortalId;
                                    logInfo.LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString();
                                    logInfo.AddProperty("Message", e.Message);
                                    eventLog.AddLog(logInfo);
                                    craftMessage.Status = 3;
                                    craftMessage.Message = "Error adding new craft user to main portal";
                                    craftReturn.Add(craftMessage);
                                    return craftReturn;
                                }
                            }


                            if (addRoleToUser(adminUserForNewPortal, "Administrators", DateTime.MaxValue, intPortalId) == false)
                            {
                                craftMessage.Status = 3;
                                craftMessage.Message = "Error adding role to new user id:" + adminUserForNewPortal.UserID + " on new portal id" + intPortalId;
                                craftReturn.Add(craftMessage);
                                return craftReturn;
                            }
                        }


                        //Dodamo nov zapis v tabelo _Craftuser
                        using (DBConnectionDataContext db = new DBConnectionDataContext())
                        {
                            _craftin_CraftUser craftUser = new _craftin_CraftUser()
                            {
                                Username = strUserName,
                                Email = strEmail,
                                Password = strPassword,
                                FirstName = strFirstName,
                                LastName = strLastName,
                                DisplayName = strFirstName + " " + strLastName,
                                Company = strBusinessName,
                                Description = strBusinessDescription,
                                CompanyEmail = strBusinessEmail,
                                Phone = strBusinessTelephone,
                                Address = strBusinessAddress,
                                City = strBusinessCity,
                                ZipCode = strBusinessZipCode,
                                VatID = strBusinessVatID,
                                CountryID = int.Parse(strCountryID),
                                PortalAlias = strPortalAlias,
                                CreationDate = DateTime.Now,
                                PortalID = intPortalId,
                                PortalName = strPortalName,
                                OwnerID = user.UserID, //current user/občina
                                DefaultLang = "en",
                                DateInsert = DateTime.Now,
                                DateUpdate = DateTime.Now,
                                UserInsert = user.Username,
                                UserUpdate = user.Username,
                                DNNUserID = adminUserForNewPortal.UserID,
                                CompanyMainImage = strBusinessMainImage
                            };
                            try
                            {
                                db._craftin_CraftUsers.InsertOnSubmit(craftUser);
                                db.SubmitChanges();
                            }
                            catch (Exception e)
                            {
                                craftMessage.Status = 3;
                                craftMessage.Message = "Error adding new craft user "+e.Message.ToString();
                                craftReturn.Add(craftMessage);
                                return craftReturn;
                            }
                            //dodamo regije k craftUserju
                            string[] splitedRegions = strBusinessRegions.Split(',');
                            foreach (var reg in splitedRegions)
                            {
                                int regNumber;
                                bool result = Int32.TryParse(reg, out regNumber);
                                if(result)
                                {
                                    _craftin_CraftManRegionList reglistitem = new _craftin_CraftManRegionList()
                                    {
                                        CraftUserID = craftUser.ID,
                                        RegionID = regNumber
                                    };
                                    try
                                    {
                                        db._craftin_CraftManRegionLists.InsertOnSubmit(reglistitem);
                                        db.SubmitChanges();
                                    }
                                    catch (Exception e)
                                    {
                                        AddErrorToLogs(e.Message.ToString());
                                    }
                                }
                            }
                            //dodamo kategorije k craftUserju
                            string[] splitedCategories = strBusinessCategories.Split(',');
                            foreach (var cat in splitedCategories)
                            {
                                int catNumber;
                                bool result = Int32.TryParse(cat, out catNumber);
                                if (result)
                                {
                                    _craftin_CraftManCategoriesList catlistitem = new _craftin_CraftManCategoriesList()
                                    {
                                        CraftUserID = craftUser.ID,
                                        CategoryID = catNumber
                                    };
                                    try
                                    {
                                        db._craftin_CraftManCategoriesLists.InsertOnSubmit(catlistitem);
                                        db.SubmitChanges();
                                    }
                                    catch (Exception e)
                                    {
                                        AddErrorToLogs(e.Message.ToString());
                                    }
                                }
                            }




                        }


                        //Create a Portal Settings object for the new Portal
                        PortalInfo objPortal = objPortalController.GetPortal(intPortalId);
                        var newSettings = new PortalSettings { PortalAlias = new PortalAliasInfo { HTTPAlias = strPortalAlias }, PortalId = intPortalId, DefaultLanguage = objPortal.DefaultLanguage };
                        string webUrl = Globals.AddHTTP(strPortalAlias);
                        try
                        {
                            /*if (!Globals.IsHostTab(PortalSettings.Current.ActiveTab.TabID))
                            {
                                message = Mail.SendMail(PortalSettings.Current.Email,
                                                           txtEmail.Text,
                                                           PortalSettings.Email + ";" + Host.HostEmail,
                                                           Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_SUBJECT", adminUser),
                                                           Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_BODY", adminUser),
                                                           "",
                                                           "",
                                                           "",
                                                           "",
                                                           "",
                                                           "");
                            }
                            else
                            {
                                message = Mail.SendMail(Host.HostEmail,
                                                           txtEmail.Text,
                                                           Host.HostEmail,
                                                           Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_SUBJECT", adminUser),
                                                           Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_BODY", adminUser),
                                                           "",
                                                           "",
                                                           "",
                                                           "",
                                                           "",
                                                           "");
                            }*/
                        }
                        catch (Exception exc)
                        {
                            //Logger.Error(exc);

                            //string closePopUpStr = (PortalSettings.Current.EnablePopUps) ? "onclick=\"return " + UrlUtils.ClosePopUp(true, webUrl, true) + "\"" : "";
                            craftMessage.Status = 3;
                            craftMessage.Message = string.Format("UnknownSendMail.Error", webUrl, exc.Message.ToString());
                            craftReturn.Add(craftMessage);
                            return craftReturn;
                        }

                        var objEventLog = new EventLogController();
                        objEventLog.AddLog(objPortalController.GetPortal(intPortalId), PortalSettings.Current, adminUserForNewPortal.UserID, "", EventLogController.EventLogType.PORTAL_CREATED);

                        // mark default language as published if content localization is enabled
                        bool ContentLocalizationEnabled = PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", PortalSettings.Current.PortalId, false);
                        if (ContentLocalizationEnabled)
                        {
                            LocaleController lc = new LocaleController();
                            lc.PublishLanguage(intPortalId, objPortal.DefaultLanguage, true);
                        }

                        if (webUrl != null && webUrl.Length > 0)
                        {
                            craftMessage.Status = 1;
                            craftMessage.Message = webUrl;
                            craftReturn.Add(craftMessage);
                            return craftReturn;
                        }
                        else
                        {
                            craftMessage.Status = 1;
                            craftMessage.Message = webUrl;
                            craftReturn.Add(craftMessage);
                            return craftReturn;
                        }

                    }
                //}end if

                craftMessage.Status = 1;
                craftMessage.Message = "Somthing went wrong";
                craftReturn.Add(craftMessage);
                return craftReturn;
            }
            catch (Exception exc) //Module failed to load
            {
                craftMessage.Status = 3;
                craftMessage.Message = "Outer try/catch block" + exc.Message.ToString();
                craftReturn.Add(craftMessage);
                return craftReturn;
            }
        }

        private bool addRoleToUser(UserInfo user, string roleName, DateTime expiry, int newPortalID)
        {
	        bool rc = false;
	        var roleCtl = new RoleController();
            RoleInfo newRole = roleCtl.GetRoleByName(newPortalID, roleName);
	        if (newRole != null && user != null)
	        {
		        rc = user.IsInRole(roleName);
                roleCtl.AddUserRole(newPortalID, user.UserID, newRole.RoleID, DateTime.MinValue, expiry);
		        // Refresh user and check if role was added
                user = UserController.GetUserById(newPortalID, user.UserID);
		        rc = user.IsInRole(roleName);
	        }
            //dodamo še v privzete role na novem portalu
            var arrRolesPortalNew = roleCtl.GetRoles(newPortalID);
            foreach (RoleInfo r in arrRolesPortalNew)
            {
                if (r.AutoAssignment == true)
                {
                    try
                    {
                        roleCtl.AddUserRole(newPortalID, user.UserID, r.RoleID, DateTime.Now, DateTime.Now.AddYears(2000));
                    }
                    catch (Exception ex)
                    {
                        AddErrorToLogs(ex.Message.ToString());
                        return false;
                    }
                }
            }
            //dodamo še v privzete role na glavnem portalu
            var arrRolesPortal0 = roleCtl.GetRoles(0);
            foreach (RoleInfo r in arrRolesPortal0)
            {
                if (r.AutoAssignment == true)
                {
                    try
                    {
                        roleCtl.AddUserRole(0, user.UserID, r.RoleID, DateTime.Now, DateTime.Now.AddYears(2000));
                    }
                    catch (Exception ex)
                    {
                        AddErrorToLogs(ex.Message.ToString());
                        return false;
                    }
                }
            }
	        return rc;
        }



        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<craftMessage> EditCraftman(int craftManID, string strEmail, string strFirstName, string strLastName, string strBusinessName, string strBusinessDescription, string strBusinessEmail
            , string strBusinessTelephone, string strBusinessAddress, string strBusinessCity, string strBusinessZipCode, string strBusinessVatID, int strCountryID
            , string strBusinessRegions, string strBusinessCategories, string strBusinessMainImage)
        {
            List<craftMessage> craftReturn = new List<craftMessage>();
            craftMessage craftMessage = new craftMessage();


            if (strEmail == null || strEmail.Length < 1 || strFirstName == null || strFirstName.Length < 1 ||
                strLastName == null || strLastName.Length < 1 || strCountryID < 1 || craftManID < 1)
            {
                craftMessage.Status = 2;
                craftMessage.Message = "Missing data";
                craftReturn.Add(craftMessage);
                return craftReturn;
            }


            bool countryChange = false;


            try
            {
                    //Dodamo nov zapis v tabelo _Craftuser
                    using (DBConnectionDataContext db = new DBConnectionDataContext())
                    {
                        try
                        {
                            _craftin_CraftUser craftUpdateUser = (from cu in db._craftin_CraftUsers where cu.ID == craftManID select cu).FirstOrDefault();

                            if (craftUpdateUser.CountryID != strCountryID)
                            {
                                countryChange = true;
                            }

                            if (craftUpdateUser != null)
                            {
                                craftUpdateUser.Email = strEmail;
                                craftUpdateUser.FirstName = strFirstName;
                                craftUpdateUser.LastName = strLastName;
                                craftUpdateUser.DisplayName = strFirstName + " " + strLastName;
                                craftUpdateUser.Company = strBusinessName;
                                craftUpdateUser.Description = strBusinessDescription;
                                craftUpdateUser.CompanyEmail = strBusinessEmail;
                                craftUpdateUser.Phone = strBusinessTelephone;
                                craftUpdateUser.Address = strBusinessAddress;
                                craftUpdateUser.City = strBusinessCity;
                                craftUpdateUser.ZipCode = strBusinessZipCode;
                                craftUpdateUser.VatID = strBusinessVatID;
                                craftUpdateUser.CountryID = strCountryID;
                                craftUpdateUser.DateUpdate = DateTime.Now;
                                craftUpdateUser.UserUpdate = user.Username;
                                craftUpdateUser.CompanyMainImage = strBusinessMainImage;
                            }
                            db.SubmitChanges();

                            ////////////////////////kategorije//////////////////////////////
                            string[] splitedCategories = strBusinessCategories.Split(',');

                            //Odstranimo kategorije ki jih ni več
                            foreach (var existing in craftUpdateUser._craftin_CraftManCategoriesLists)
                            {
                                if (splitedCategories.Contains(existing.CategoryID.ToString()) == false)
                                {
                                    //zbrišemo iz baze
                                    db._craftin_CraftManCategoriesLists.DeleteOnSubmit(existing);
                                }
                            }
                            try
                            {
                                db.SubmitChanges();
                            }
                            catch (Exception e)
                            {
                                AddErrorToLogs(e.Message.ToString());
                            }

                            foreach (var cat in splitedCategories)
                            {
                                int catNumber;
                                bool result = Int32.TryParse(cat, out catNumber);
                                if (result)
                                {
                                    //ne obstaja zapis ga moramo dodati
                                    if (db._craftin_CraftManCategoriesLists.Any(u => u.CraftUserID == craftManID && u.CategoryID == catNumber) == false)
                                    {
                                        _craftin_CraftManCategoriesList catlistitem = new _craftin_CraftManCategoriesList()
                                        {
                                            CraftUserID = craftManID,
                                            CategoryID = catNumber
                                        };
                                        try
                                        {
                                            db._craftin_CraftManCategoriesLists.InsertOnSubmit(catlistitem);
                                            db.SubmitChanges();
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorToLogs(e.Message.ToString());
                                        }
                                    }
                                }
                            }
                            ////////////////////////end edit kategorije//////////////////////////////
                            ////////////////////////edit regije /////////////////////////////////////
                            string[] splitedRegions = strBusinessRegions.Split(',');
                            if(countryChange)
                            {
                                //deletiramo vse obstoječe regije
                                foreach (var existing in craftUpdateUser._craftin_CraftManRegionLists)
                                {
                                    db._craftin_CraftManRegionLists.DeleteOnSubmit(existing);
                                }
                                try
                                {
                                    db.SubmitChanges();
                                }
                                catch (Exception e)
                                {
                                    AddErrorToLogs(e.Message.ToString());
                                }
                                //insertamo nove, ki jih dobimo
                                foreach (var reg in splitedRegions)
                                {
                                    int regNumber;
                                    bool result = Int32.TryParse(reg, out regNumber);
                                    if (result)
                                    {
                                        _craftin_CraftManRegionList reglistitem = new _craftin_CraftManRegionList()
                                        {
                                            CraftUserID = craftManID,
                                            RegionID = regNumber
                                        };
                                        try
                                        {
                                            db._craftin_CraftManRegionLists.InsertOnSubmit(reglistitem);
                                            db.SubmitChanges();
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorToLogs(e.Message.ToString());
                                        }
                                    }
                                }
                            }else
                            {
                                //Odstranimo regije ki jih ni več
                                foreach (var existing in craftUpdateUser._craftin_CraftManRegionLists)
                                {
                                    if (splitedRegions.Contains(existing.ID.ToString()) == false)
                                    {
                                        db._craftin_CraftManRegionLists.DeleteOnSubmit(existing);
                                    }
                                }
                                try
                                {
                                    db.SubmitChanges();
                                }
                                catch (Exception e)
                                {
                                    AddErrorToLogs(e.Message.ToString());
                                }

                                foreach (var reg in splitedRegions)
                                {
                                    int regNumber;
                                    bool result = Int32.TryParse(reg, out regNumber);
                                    if (result)
                                    {
                                        //ne obstaja zapis ga moramo dodati
                                        if (db._craftin_CraftManRegionLists.Any(u => u.CraftUserID == craftManID && u.RegionID == regNumber) == false)
                                        {
                                            _craftin_CraftManRegionList reglistitem = new _craftin_CraftManRegionList()
                                            {
                                                CraftUserID = craftManID,
                                                RegionID = regNumber
                                            };
                                            try
                                            {
                                                db._craftin_CraftManRegionLists.InsertOnSubmit(reglistitem);
                                                db.SubmitChanges();
                                            }
                                            catch (Exception e)
                                            {
                                                AddErrorToLogs(e.Message.ToString());
                                            }
                                        }
                                    }
                                }
                            }
                            ////////////////////////end edit regije /////////////////////////////////////

                            craftMessage.Status = 1;
                            craftMessage.Message = "OK";
                            craftReturn.Add(craftMessage);
                            return craftReturn;
                        }
                        catch (Exception e)
                        {
                            craftMessage.Status = 3;
                            craftMessage.Message = "Error editing craft user " + e.Message.ToString();
                            craftReturn.Add(craftMessage);
                            return craftReturn;
                        }
                    }//end using
            }
            catch (Exception exc) //Module failed to load
            {
                craftMessage.Status = 3;
                craftMessage.Message = "Outer try/catch block" + exc.Message.ToString();
                craftReturn.Add(craftMessage);
                return craftReturn;
            }
            craftMessage.Status = 3;
            craftMessage.Message = "Somthing went wrong. Check event viewer.";
            craftReturn.Add(craftMessage);
            return craftReturn;
        }



        [WebMethod]
        public string regenerateTable(int categoryID, int RegionID, int CountryID)
        {
            StringBuilder sb = new StringBuilder();
            using (DBConnectionDataContext db = new DBConnectionDataContext())
            {
                var craftmen = default(IQueryable<_craftin_CraftUser>);
                if (categoryID.ToString().Length > 0)
                {
                    if (categoryID == -1)
                    {
                        craftmen = (from craft in db._craftin_CraftUsers select craft);
                    }
                    else
                    {
                        craftmen = (from craft in db._craftin_CraftUsers where craft._craftin_CraftManCategoriesLists.Any(u => u.CategoryID == categoryID) select craft);
                    }
                    if(RegionID.ToString().Length > 0)
                    {
                        if (RegionID > 0)
                        {
                            craftmen = (from craft in craftmen where craft._craftin_CraftManRegionLists.Any(u => u.RegionID == RegionID) select craft);
                        }
                    }
                    if (CountryID.ToString().Length > 0)
                    {
                        if (CountryID > 0)
                        {
                            craftmen = (from craft in craftmen where craft._craftin_CraftManCountryList.CountryID == CountryID select craft);
                        }
                    }
                }
                if (craftmen != null)
                {
                    foreach (var cr in craftmen)
                    {
                        sb.AppendLine("<tr><input type=\"hidden\" id=\"hfCraftManID" + cr.ID + "\" value=\"" + cr.ID + "\" />");
                        sb.AppendLine("     <td data-th=\"Business name\">" + cr.Company + "</td>");
                        StringBuilder categoriesString = new StringBuilder();
                        if (cr._craftin_CraftManCategoriesLists != null)
                        {
                            foreach (var c in cr._craftin_CraftManCategoriesLists)
                            {
                                categoriesString.Append(c._craftin_CraftManCategory.CategoryName).Append(", ");
                            }
                        }
                        if (categoriesString.Length > 0)
                        {
                            categoriesString.Length = categoriesString.Length - 2;
                            sb.AppendLine("     <td data-th=\"Categories\">" + categoriesString.ToString() + "</td>");
                        }
                        else
                        {
                            sb.AppendLine("     <td data-th=\"Categories\">No categories found</td>");
                        }

                        StringBuilder regionString = new StringBuilder();
                        if (cr._craftin_CraftManRegionLists != null)
                        {
                            foreach (var r in cr._craftin_CraftManRegionLists)
                            {
                                regionString.Append(r._craftin_CraftManRegion.RegionName).Append(", ");
                            }
                        }
                        if (regionString.Length > 0)
                        {
                            regionString.Length = regionString.Length - 2;
                            sb.AppendLine("     <td data-th=\"Region\">" + regionString.ToString() + "</td>");
                        }
                        else
                        {
                            sb.AppendLine("     <td data-th=\"Region\">No regions found</td>");
                        }
                        var countryName = (from c in db._craftin_CraftManCountryLists where c.CountryID == cr.CountryID select c.CountryName).FirstOrDefault();
                        sb.AppendLine("     <td data-th=\"Country\">" + countryName + "</td>");
                        sb.AppendLine("     <td data-th=\"Email\">" + cr.Email + "</td>");
                        sb.AppendLine("     <td data-th=\"User\">" + cr.Username + "</td>");
                        sb.AppendLine("<td data-th=\"Portal\"><a href=\"" + Globals.AddHTTP(cr.PortalAlias) + "\" target=\"_blank\"><i class=\"icon-globe-1\"></i></a></td>");
                        var userstatus = (from status in db.UserPortals where status.PortalId == cr.PortalID select status.Authorised).FirstOrDefault();
                        if (userstatus == true)
                        {
                            sb.AppendLine("     <td data-th=\"Status\"><i class=\"icon-flag status-published\"></i></td>");
                        }
                        else
                        {
                            sb.AppendLine("     <td data-th=\"Status\"><i class=\"icon-flag status-unpublished\"></i></td>");
                        }

                        sb.AppendLine("     <td data-th=\"Controls\" class=\"craftControls\">");
                        sb.AppendLine("         <a href=\"javascript:;\" class=\"viewCurrentCraftMan\" title=\"View craftman details\"><i class=\"icon-eye-2\"></i></a>");
                        sb.AppendLine("         <a href=\"javascript:;\" class=\"editCurrentCraftMan\" title=\"Edit craftman details\"><i class=\"icon-edit\"></i></a>");
                        sb.AppendLine("     </td>");
                        sb.AppendLine("</tr>");
                    }
                }
            }
            return sb.ToString();
        }


        [WebMethod]
        public string updateCraftManPicture()
        {
            string fajl = string.Empty;
            try
            {
                HttpFileCollection hfc = HttpContext.Current.Request.Files;
                for (int i = 0; i < hfc.Count; i++)
                {
                    HttpPostedFile hpf = hfc[i];
                    fajl = Path.GetFileName(hpf.FileName);
                    if (hpf.ContentLength > 0)
                    {
                        if (hpf.ContentLength < (10 * 1024 * 1024))
                        {
                            string hashFilename = Guid.NewGuid().ToString().Replace("-", "") + "-" + fajl.Replace(" ", "_");
                            Directory.CreateDirectory(Server.MapPath("/Portals/craftman_pictures/"));
                            hpf.SaveAs(Server.MapPath("/Portals/craftman_pictures/") + hashFilename);
                            return hashFilename;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
                //AddErrorToLogs(ex.Message.ToString(), 0);
            }
            return "ERROR";
        }



        private void AddErrorToLogs(string _msg)
        {
            EventLogController eventLog = new EventLogController();
            LogInfo logInfo = new LogInfo();
            logInfo.LogPortalID = 0;
            logInfo.LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString();
            logInfo.AddProperty("Message", _msg);
            eventLog.AddLog(logInfo);
        }




    }
}
