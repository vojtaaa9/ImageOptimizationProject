using NetVips;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Routing;

namespace ImageOptimization
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private uint _handlerId;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            _handlerId = Log.SetLogHandler("VIPS", NetVips.Enums.LogLevelFlags.All, (domain, level, message) =>
            {
                Debug.WriteLine("Domain: '{0}' Level: {1}", domain, level);
                Debug.WriteLine("Message: {0}", message);
            });
        }
    }
}
