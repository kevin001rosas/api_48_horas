using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace WeApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {   
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            return; 
            /*
            // Preflight request comes with HttpMethod OPTIONS                            
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS,HEAD");                
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");                
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "*");

                if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
                {
                    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "PUT");
                    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Header", "accept, content-type"); //
                    HttpContext.Current.Response.End();
                }*/
                
        }
    }
}
