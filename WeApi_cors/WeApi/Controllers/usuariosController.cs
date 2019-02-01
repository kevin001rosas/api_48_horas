﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WeApi.Controllers
{
    public class usuariosController : ApiController
    {
        // GET api/usuarios
        public IHttpActionResult Get()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = "SELECT * from lu_usuarios where estado=1;";
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // GET api/usuarios/5
        public IHttpActionResult Get(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = string.Format("SELECT * from lu_usuarios where id='{0}'", id);
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }


        // POST api/usuarios
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
            string insert_query = string.Format("INSERT INTO `lu_usuarios` " +
            "(`nombres`," +
                "`apellido_paterno`," +
                "`apellido_materno`," +
                "`telefono_local`," +
                "`telefono_celular`," +
                "`calle`," +
                "`numero`," +
                "`delegacion`," +
                "`fecha_de_nacimiento`," +
                "`id_ciudad`," +
                "`email`," +
                "`estado`," +
                "`id_tipo_de_usuario`," +
                "`foto_url`," +
                "`establecimiento`," +
                "`fecha_de_registro`," +
                "`fecha_de_modificacion`) " +
            "VALUES " +
            "('{0}', '{1}', {2}, {3}, {4}, {5}. {6}. {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}); "
                , json["nombres"]
                , json["apellido_paterno"]
                , json["apellido_materno"]
                , json["telefono_local"]
                , json["telefono_celular"]
                , json["calle"]
                , json["numero"]
                , json["delegacion"]
                , json["fecha_de_nacimiento"]
                , json["id_ciudad"]
                , json["email"]
                , json["estado"]
                , json["id_tipo_de_usuario"]
                , json["foto_url"]
                , json["establecimiento"]
                , "now()"
                , "now()");
            //, id);

            //En caso de error, devolverá incorrecto
            tabla_resultado.Rows[0]["id"] = Database.runInsert(insert_query).ToString();
            if (tabla_resultado.Rows[0]["id"] == "-1")
                return Json("incorrecto"); 

            //Devolcemos la información de la tabla. 
            return Json(utilidades.convertDataTableToJson(tabla_resultado));
        }

        private bool validacion(JObject json)
        {
            if (!utilidades.validar_token(Request))
                return false;

            if (json["nombre"].ToString().Trim() == "")
                return false;

            return true;
        }

        // PUT api/usuarios/5
        public IHttpActionResult Put(int id, [FromBody]Object value)
        {
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_usuarios` " +
            "set " +
            "nombres='{0}' " +
            ",apellido_paterno='{1}'" +
            ",apellido_materno='{2}' " +
            ",telefono_local='{3}' " +
            ",telefono_celular='{4}' " +
            ",calle='{5}' " +
            ",numero='{6}' " +
            ",delegacion='{7}' " +
            ",fecha_de_nacimiento='{8}' " +
            ",id_ciudad='{9}' " +
            ",email='{10}' " +
            ",estado='{11}' " +
            ",id_tipo_de_usuario='{12}' " +
            ",foto_url='{13}' " +
            ",establecimiento='{14}' " +
            ",fecha_de_modificacion=now() " +
            "where id='{15}'"
            , json["nombres"]
            , json["apellido_paterno"]
            , json["apellido_materno"]
            , json["telefono_local"]
            , json["telefono_celular"]
            , json["calle"]
            , json["numero"]
            , json["delegacion"]
            , json["fecha_de_nacimiento"]
            , json["id_ciudad"]
            , json["email"]
            , json["estado"]
            , json["id_tipo_de_usuario"]
            , json["foto_url"]
            , json["establecimiento"]
            , id);






            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        // DELETE api/usuarios/5
        public IHttpActionResult Delete(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto"); 

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_usuarios` " +
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