using System.Web;
using System.Web.Optimization;

namespace Hutch
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/custom").Include(
                "~/Scripts/custom/Dropdown.js",
                "~/Scripts/custom/Sidebar.js"));

            bundles.Add(new ScriptBundle("~/bundles/Auth").Include(
                "~/Scripts/Auth/SignupAlert.js",
                "~/Scripts/Auth/LoginAlert.js"));

            bundles.Add(new ScriptBundle("~/bundles/admin").Include(
                "~/Scripts/admin/AdminSidebar.js",
                "~/Scripts/admin/ImagePreview.js",
                "~/Scripts/admin/Alert.js"));

            bundles.Add(new ScriptBundle("~/bundles/Product").Include(
                "~/Scripts/Product/FilterBar.js"));

            bundles.Add(new ScriptBundle("~/bundles/Nav").Include(
               "~/Scripts/Nav/AutoSuggestion.js",
               "~/Scripts/Nav/NavUpper.js"));

            bundles.Add(new ScriptBundle("~/bundles/Wishlist").Include(
               "~/Scripts/Wishlist/HandleWishlist.js",
               "~/Scripts/Wishlist/HandleSingleWishlist.js"));

        }
    }
}
