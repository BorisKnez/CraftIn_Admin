/*
' Copyright (c) 2015  Christoc.com
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Services.Localization;
using System.Text;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using System.Web.UI.WebControls;

namespace Christoc.Modules.CraftAdminModule
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content
    /// 
    /// Typically your view control would be used to display content or functionality in your module.
    /// 
    /// View may be the only control you have in your project depending on the complexity of your module
    /// 
    /// Because the control inherits from CraftAdminModuleModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : CraftAdminModuleModuleBase, IActionable
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                hfPortalID.Value = PortalId.ToString();
                hfControlPath.Value = ControlPath;

                //fill th options
                fillTHData();

                if (UserId > 0)
                {
                    craftMenListTR.InnerHtml = generateCraftMenTable();
                }
                else { bottomBar.Visible = false; }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }



        private string generateCraftMenTable()
        {
            StringBuilder sb = new StringBuilder();
            using(DBConnectionDataContext db = new DBConnectionDataContext())
            {
                var allCategories = (from cat in db._craftin_CraftManCategories select cat);
                var allRegions = (from reg in db._craftin_CraftManRegions select reg);
                var allCountries = (from cou in db._craftin_CraftManCountryLists select cou);
         
                var craftmen = (from craft in db._craftin_CraftUsers select craft);
                if(craftmen != null)
                {
                    foreach(var cr in craftmen)
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
                        }else
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
                        if(userstatus == true)
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


        private void fillTHData()
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
                        admincountry.Items.Add(cb);
                    }
                }
                var regions = (from reg in db._craftin_CraftManRegions select reg);
                if (regions != null)
                {
                    foreach (var r in regions)
                    {
                        ListItem cb = new ListItem();
                        cb.Text = r.RegionName;
                        cb.Value = r.ID.ToString();
                        adminregion.Items.Add(cb);
                    }
                }
                var categories = (from cat in db._craftin_CraftManCategories select cat);
                if (categories != null)
                {
                    foreach (var ca in categories)
                    {
                        ListItem cb = new ListItem();
                        cb.Text = ca.CategoryName;
                        cb.Value = ca.CategoryID.ToString();
                        admincategory.Items.Add(cb);
                    }
                }



            }
        }



        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection
                    {
                        {
                            GetNextActionID(), Localization.GetString("EditModule", LocalResourceFile), "", "", "",
                            EditUrl(), false, SecurityAccessLevel.Edit, true, false
                        }
                    };
                return actions;
            }
        }





    }
}