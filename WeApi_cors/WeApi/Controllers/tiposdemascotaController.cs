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
    public class tiposdemascotaController : ApiController
    {
        // GET api/tiposdemascota
        
        public IHttpActionResult Get()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");
            string query = "SELECT * from lu_tipos_de_mascota where estado=1;";
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // GET api/tiposdemascota/GetCustomKevin
        [Route("api/tiposdemascota/GetCustomKevin")]
        public IHttpActionResult GetCustomKevin()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");
            string query = "SELECT * from lu_tipos_de_mascota where estado=1;";
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // GET api/tiposdemascota/GetCustomKevinConId/1        
        [Route("api/tiposdemascota/GetCustomKevinConId/{id}")]
        public IHttpActionResult GetCustomKevinConId(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");
            string query = "SELECT * from lu_tipos_de_mascota where estado=1;";
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }



        // GET api/tiposdemascota/5
        public IHttpActionResult Get(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");
            //Obtenemos los tipos de mascota. 
            string query = string.Format("SELECT a.nombre as nombre_tipo_de_mascota " +
                ",b.nombre as raza  " +
                ",b.id as id_raza  " +
                "FROM lu_tipos_de_mascota a " +
                "LEFT JOIN lu_razas b on b.id_tipo_de_mascota=a.id " +
                "WHERE a.id='{0}' order by b.nombre; "
                , id);
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // POST api/tiposdemascota
        public IHttpActionResult Post([FromBody]Object value)
        {
            //Generamos el Datatable para devolver el resultado. 
            DataTable tabla_resultado = new DataTable();
            tabla_resultado.Columns.Add("id");
            tabla_resultado.Rows.Add();
            tabla_resultado.Rows[0]["id"] = "-1";
            
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string insert_query = string.Format("INSERT INTO `lu_tipos_de_mascota` "  +
            "(`nombre`) " +
            "VALUES "  +
            "('{0}'); " 
            , json["nombre"]);
            //, id);

            //Contestamos con el id del nuevo registro. 
            tabla_resultado.Rows[0]["id"] = Database.runInsert(insert_query).ToString();
            if (tabla_resultado.Rows[0]["id"].ToString()== "-1")
                return Json("incorrecto"); 

            //Devolvera en el resultado -1 en caso de una inserción errónea. 
            return Json(utilidades.convertDataTableToJson(tabla_resultado));          
        }

        private bool validacion(JObject json)
        {
            if (!utilidades.validar_token(Request))
                return false;

            if(json["nombre"].ToString().Trim()=="")
                return false;

            return true; 
        }

        // PUT api/tiposdemascota/5
        public IHttpActionResult Put(int id, [FromBody]Object value)
        {
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_tipos_de_mascota` " +
            "set " + 
            "nombre='{0}' " + 
            "where id='{1}'" 
            , json["nombre"]
            , id);

            //Contestamos con el id del nuevo registro.
            if(Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        // POST api/tiposdemascota/kevin/5
        [Route("api/tiposdemascota/kevin/{id}")]
        public IHttpActionResult kevin(int id, [FromBody]Object value)
        {
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_tipos_de_mascota` " +
            "set " +
            "nombre='{0}' " +
            "where id='{1}'"
            , json["nombre"]
            , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        // DELETE api/tiposdemascota/5
        public IHttpActionResult Delete(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto"); 

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_tipos_de_mascota` " +
            "set " +
            "estao=0 " +
            "where id='{0}'"            
            , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }
    }
}
