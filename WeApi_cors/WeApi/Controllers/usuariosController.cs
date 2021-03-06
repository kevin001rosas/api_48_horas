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

            string query = "";
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

        [Route("api/usuarios/getSearchByPage")]
        public IHttpActionResult getSearchByPage()
        {
            //Recuerda poner siempre la función de validación de token. Ya entró; pero no le mande la página en el header. 
            //Para eso utilizaremos POstman !! :D 
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            //ahora si debe traer la página...
            //Aquí obtendré el valor de la página que me solicitam . 
            IEnumerable<string> headerValues = Request.Headers.GetValues("pagina");
            string string_pagina = headerValues.FirstOrDefault().ToString();
            int pagina = int.Parse(string_pagina);

            IEnumerable<string> headerValues_nombre = Request.Headers.GetValues("nombre");
            string nombre = headerValues_nombre.FirstOrDefault().ToString();


            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            //Recuerda siempre poner la condicio´n del estado. ¿Ok? 
            string query = string.Format("select  " +
            "a.id " +
            ",a.nombres " +
            ",a.apellido_paterno " +
            ",a.apellido_materno " +
            ",a.telefono_local " +
            ",a.telefono_celular " +
            ",a.calle " +
            ",a.numero" +
            ",a.delegacion " +
            ",a.colonia " +
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            ",a.email " +
            ",a.establecimiento " +                       
            ", concat(a.nombres, ' ' , a.apellido_paterno,  ' ' ,a.apellido_materno) as usuario " +
            "from lu_usuarios a  " +
            "where a.estado=1 " +
            "and a.nombres like '%{2}%' " +
            "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            //Viste como Debuguiee? Cuando te salga algun errror, copia y pega el query que pones aqupara correlo en el Workbench Y listo :D
            //Convertimos a Json y regresamos los datos. 

            return Json(utilidades.convertDataTableToJson(tabla_resultado));

            //Ya terminamos... Ahora a probar. Pondré un punto de ruptura al inicio de la funciójn pra debuggear. 
            //Te sugiero que hagas lo mismo para las funciones que hagas. Creo que es muy útil. 

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
                "`codigo_postal`," +
                "`numero`," +
                "`delegacion`," +
                "`colonia`," +
                "`fecha_de_nacimiento`," +
                "`id_ciudad`," +
                "`email`," +                
                "`id_tipo_de_usuario`," +
                "`foto_url`," +
                "`establecimiento`," +
                "`fecha_de_registro`," +
                "`fecha_de_modificacion`) " +
            "VALUES " +
            //Verifica las funciones now() (Parametros 17 y 18), envía un post desde postman llenando estos datos y pon un punto de ruptura aquí para que veas el query. Copia y pega el query en Workbench para debuggearlo. 
            "('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}','{9}', STR_TO_DATE('{10}', '%Y-%m-%d'), '{11}', '{12}', '{13}', '{14}', '{15}', {16}, {17}); "
                , json["nombres"]
                , json["apellido_paterno"]
                , json["apellido_materno"]
                , json["telefono_local"]
                , json["telefono_celular"]
                , json["calle"]
                , json["codigo_postal"]
                , json["numero"]
                , json["delegacion"]
                , json["colonia"]
                , json["fecha_de_nacimiento"]
                , json["id_ciudad"]
                , json["email"]                
                , json["id_tipo_de_usuario"]
                , json["foto_url"]
                , json["establecimiento"]
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
            ",codigo_postal='{6}' " +
            ",numero='{7}' " +
            ",delegacion='{8}' " +
            ",colonia='{9}' " +
            ",fecha_de_nacimiento=STR_TO_DATE('{10}', '%Y-%m-%d') " +
            ",id_ciudad='{11}' " +
            ",email='{12}' " +
            ",id_tipo_de_usuario='{13}' " +
            ",foto_url='{14}' " +
            ",establecimiento='{15}' " +
            ",fecha_de_modificacion=now() " +
            "where id='{16}'"
            , json["nombres"]
            , json["apellido_paterno"]
            , json["apellido_materno"]
            , json["telefono_local"]
            , json["telefono_celular"]
            , json["calle"]
            , json["codigo_postal"]
            , json["numero"]
            , json["delegacion"]
            , json["colonia"]
            , json["fecha_de_nacimiento"]
            , json["id_ciudad"]
            , json["email"]
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
