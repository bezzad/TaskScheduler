using Hasin.Taaghche.Probes;
using Microsoft.Owin;
using NLog.Owin.Logging;
using Owin;
using System.Web.Http;

[assembly: OwinStartup(typeof(Startup))]

namespace Hasin.Taaghche.Probes
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            #region Config NLog middleware

            app.UseNLog();

            #endregion

            #region Configure Web API

            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);

            #endregion
        }
    }
}