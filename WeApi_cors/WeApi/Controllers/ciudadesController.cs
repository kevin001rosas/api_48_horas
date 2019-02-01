using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WeApi.Controllers
{
    public class ciudadesController : ApiController
    {
        // GET api/ciudades
        public IHttpActionResult Get()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = "SELECT id, nombre from cf_ciudades where estado=1 order by nombre;";
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // GET api/ciudades/5
        public IHttpActionResult Get(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = string.Format("SELECT * from cf_ciudades where id='{0}'", id);
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

    }
}