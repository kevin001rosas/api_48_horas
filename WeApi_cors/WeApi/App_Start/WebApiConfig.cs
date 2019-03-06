using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WeApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //Inicializamos el timer para evitar ataques de recuperación de contraseñas.
            WeApi.Controllers.utilidades.timer.Elapsed += WeApi.Controllers.utilidades.OnTimedEvent;
            WeApi.Controllers.utilidades.timer.Enabled = true;
            WeApi.Controllers.utilidades.timer.AutoReset = true;

            IronPdf.License.LicenseKey = "IRONPDF-888052B015-310873-2B8CE5-56C1D9273C-1CD0E4F7-UEx08D76A427A9B7D8-PROJECT.LICENSE.1.DEVELOPER.SUPPORTED.UNTIL.25.FEB.2020";

            //WeApi.Controllers.utilidades.base_url = "http://http://apiRoyalCaninProduccion.xik.mx/"; 

            // Web API configuration and services
            //EnableCorsAttribute cors = new EnableCorsAttribute("*", "*", "GET,POST,PUT,DELETE", "Content-Disposition");            
            EnableCorsAttribute cors = new EnableCorsAttribute("*", "*", "*", "Content-Disposition");            
            
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();
            
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


        }

    }
}
