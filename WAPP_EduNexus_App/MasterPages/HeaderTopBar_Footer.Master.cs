using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_EduNexus_App
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPage = System.IO.Path.GetFileName(Page.AppRelativeVirtualPath);

            if (currentPage.Equals("BrowseCourse.aspx", StringComparison.OrdinalIgnoreCase))
            {
                navBrowse.Attributes["class"] = "nav-link active";
            }
            else if (currentPage.Equals("MyLearningPage.aspx", StringComparison.OrdinalIgnoreCase))
            {
                navLearning.Attributes["class"] = "nav-link active";
            }
        }
    }
}