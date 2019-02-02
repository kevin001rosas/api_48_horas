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
    public class clientesController : ApiController
    {
        // GET api/clientes
        public IHttpActionResult Get()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = "SELECT * from lu_clientes where estado=1;";
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // GET api/clientes/5
        public IHttpActionResult Get(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = string.Format("SELECT * from lu_clientes where id='{0}'", id);
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }


        // POST api/clientes
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
            string insert_query = string.Format("INSERT INTO `lu_clientes` " +
            "(`nombres`," +
                "`apellido_paterno`," +
                "`apellido_materno`," +
                "`telefono_local`," +
                "`telefono_celular`," +
                "`calle`," +
                "`numero`," +
                "`delegacion`," +
                "`colonia`," +
                "`fecha_de_nacimiento`," +
                "`id_ciudad`," +
                "`email`," +
                "`id_tipo_de_usuario`," +
                "`id_registrado_por`," +
                "`foto_url`," +
                "`fecha_de_registro`," +
                "`fecha_de_modificacion`) " +
            "VALUES " +
            //Verifica las funciones now() (Parametros 17 y 18), envía un post desde postman llenando estos datos y pon un punto de ruptura aquí para que veas el query. Copia y pega el query en Workbench para debuggearlo. 
            "('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', STR_TO_DATE('{9}', '%Y-%m-%d'), '{10}', '{11}', '{12}', '{13}', '{14}', {15}, {16}); "
            , json["nombres"]
            , json["apellido_paterno"]
            , json["apellido_materno"]
            , json["telefono_local"]
            , json["telefono_celular"]
            , json["calle"]
            , json["numero"]
            , json["delegacion"]
            , json["colonia"]
            , json["fecha_de_nacimiento"]
            , json["id_ciudad"]
            , json["email"]
            , json["id_tipo_de_usuario"]
            , json["id_registrado_por"]
            , json["foto_url"]
            , "now()"
            , "now()");
            //, id);

            //En caso de error, devolverá incorrecto
            tabla_resultado.Rows[0]["id"] = Database.runInsert(insert_query).ToString();
            if (tabla_resultado.Rows[0]["id"].ToString() == "-1")
                return Json("incorrecto");

            //Devolcemos la información de la tabla. 
            return Json(utilidades.convertDataTableToJson(tabla_resultado));
        }

        private bool validacion(JObject json)
        {
            if (!utilidades.validar_token(Request))
                return false;

            if (json["nombres"].ToString().Trim() == "")
                return false;

            return true;
        }

        // PUT api/clientes/5
        public IHttpActionResult Put(int id, [FromBody]Object value)
        {
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_clientes` " +
            "set " +
            "nombres='{0}' " +
            ",apellido_paterno='{1}'" +
            ",apellido_materno='{2}' " +
            ",telefono_local='{3}' " +
            ",telefono_celular='{4}' " +
            ",calle='{5}' " +
            ",numero='{6}' " +
            ",delegacion='{7}' " +
            ",colonia='{8}' " +
            ",fecha_de_nacimiento=STR_TO_DATE('{9}', '%Y-%m-%d') " +
            ",id_ciudad='{10}' " +
            ",email='{11}' " +
            ",id_tipo_de_usuario='{12}' " +
            ",id_registrado_por='{13}' " +
            ",foto_url='{14}' " +
            ",estado_cliente='{15}' " +
            ",fecha_de_modificacion=now() " +
            "where id='{16}'"
            , json["nombres"]
            , json["apellido_paterno"]
            , json["apellido_materno"]
            , json["telefono_local"]
            , json["telefono_celular"]
            , json["calle"]
            , json["numero"]
            , json["delegacion"]
            , json["colonia"]
            , json["fecha_de_nacimiento"]
            , json["id_ciudad"]
            , json["email"]
            , json["id_tipo_de_usuario"]
            , json["id_registrado_por"]
            , json["foto_url"]
            , json["estado_cliente"]
            , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        // DELETE api/clientes/5
        public IHttpActionResult Delete(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_clientes` " +
            "set " +
            "estado=0 " +
            ",fecha_de_modificacion=now() " +
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
