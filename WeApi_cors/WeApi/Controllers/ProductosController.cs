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
    public class ProductosController : ApiController
    {
        // GET api/productos
        public IHttpActionResult Get()
        {
            string query = "SELECT * from lu_productos limit 1000000;";
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // GET api/productos/5
        public IHttpActionResult Get(int id)
        {
            string query = string.Format("SELECT * from lu_productos where id_producto='{0}'", id);
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // POST api/productos
        /*public void Post([FromBody]string value)
        {
        }*/

        public void Post([FromBody]Object value)
        {
            JObject json = JObject.Parse(value.ToString());

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE lu_productos set clave='{0}' where id_producto='{1}'"
                , json["clave"]); 
                //, id);

            //Contestamos con el id del nuevo registro. 
            Database.runQuery(update_query);
        }

        // PUT api/productos/5
        public void Put(int id, [FromBody]Object value)
        {
            JObject json = JObject.Parse(value.ToString());

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE lu_productos set clave='{0}' where id_producto='{1}'"
                , json["clave"]
                , id);

            Database.runQuery(update_query);
        }

        // DELETE api/productos/5
        public void Delete(int id)
        {
        }
    }
}
