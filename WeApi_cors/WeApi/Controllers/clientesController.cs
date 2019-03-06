using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
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

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            string condicion_administrador = id_tipo_de_usuario != "1" ? string.Format(" and a.id_registrado_por='{0}' ", id_usuario) : ""; 

            string query = string.Format("SELECT " +
            "a.* " +
            ", concat(a.nombres, ' ' , a.apellido_paterno,  ' ' ,a.apellido_materno) as cliente " +
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento_format   " +
            ", concat(a.calle, ' ', a.numero, ', Col. ' , a.colonia, ', Del. ', a.delegacion , ', ', b.nombre, '.') as direccion " + 
            "from lu_clientes a " + 
            "left join cf_ciudades b on a.id_ciudad=b.id " + 
            "where a.id='{0}' {1}"
            , id
            , condicion_administrador);

            DataTable tabla = Database.runSelectQuery(query);
            if (tabla == null)
                return Json("incorrecto"); 
            return Json(utilidades.convertDataTableToJson(tabla));
        }


        [Route("api/clientes/uploadImage/{id}")]

        public IHttpActionResult uploadImage(int id, [FromBody]Object value)
        {
            //Tomar en cuenta que las fechas vienen en el formato YYYY-MM-dd
            JObject json = JObject.Parse(value.ToString());


            if (!validacion(json))
                return Json("incorrecto");

            string filename = string.Format("{0}.jpg", id);
            utilidades.guardar_imagen(json["foto_url"].ToString(), "clientes", filename);

            string foto_url = "http://" + Request.Headers.Host + "/temp/clientes/" + filename;
            foto_url += "?fecha=" + DateTime.Now.ToString("ddMMyyyy_HHmmss"); 

            //Actualizamos el campo de foto_url de la mascota.             
            string update_query = string.Format("UPDATE `lu_clientes` " +
           "set " +
           "foto_url='{0}' " +
           "where id='{1}'"
           , foto_url
           , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
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
                "`codigo_postal`," +
                "`numero`," +
                "`delegacion`," +
                "`colonia`," +
                "`fecha_de_nacimiento`," +
                "`id_ciudad`," +
                "`email`," +                
                "`id_registrado_por`," +
                "`foto_url`," +
                "`fecha_de_registro`," +
                "`fecha_de_modificacion`) " +
            "VALUES " +
            //Verifica las funciones now() (Parametros 17 y 18), envía un post desde postman llenando estos datos y pon un punto de ruptura aquí para que veas el query. Copia y pega el query en Workbench para debuggearlo. 
            "('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}','{9}',  STR_TO_DATE('{10}', '%Y-%m-%d'), '{11}', '{12}', '{13}', '{14}', {15}, {16}); "
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
            , json["id_registrado_por"]
            , "foto_url"
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

            return true;
        }

        // PUT api/clientes/5
        public IHttpActionResult Post(int id, [FromBody]Object value)
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
            ",codigo_postal='{6}' " +
            ",numero='{7}' " +
            ",delegacion='{8}' " +
            ",colonia='{9}' " +
            ",fecha_de_nacimiento=STR_TO_DATE('{10}', '%Y-%m-%d') " +
            ",id_ciudad='{11}' " +
            ",email='{12}' " +            
            ",id_registrado_por='{13}' " +
            ",foto_url='{14}' " +
            ",id_estado='{15}' " +
            ",comentarios_de_invalidacion='{16}' " +
            "where id='{17}'"
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
            , json["id_registrado_por"]
            , json["foto_url"]
            , json["id_estado"]
            , json["comentarios_de_invalidacion"]
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

        [Route("api/clientes/getUserPets/{id}")]
        public IHttpActionResult getUserPets(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            string condicion_administrador = id_tipo_de_usuario != "1" ? string.Format(" and d.id_registrado_por='{0}' ", id_usuario) : ""; 


            string query = string.Format("select  " +
                                        "a.id " +
                                        ", a.nombre " +
                                        ", b.nombre as raza " +
                                        ", c.nombre as tipo  " +
                                        ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento   " +
                                        "from lu_mascotas a  " +
                                        "LEFT JOIN lu_razas b on a.id_raza=b.id " +
                                        "LEFT JOIN lu_tipos_de_mascota c on c.id=b.id_tipo_de_mascota " +
                                        "LEFT JOIN lu_clientes d on d.id=a.id_cliente " +
                                        "where a.id_cliente='{0}' {1} limit 10;  "
                                        , id
                                        , condicion_administrador);

            DataTable tabla = Database.runSelectQuery(query);
            if(tabla==null)                
                return Json("sin_mascotas");
            
            return Json(utilidades.convertDataTableToJson(tabla));
        }




        [Route("api/clientes/getSearchByPage")]
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

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();


            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            string condicion_usuarios = "";

            //Condición para traer solo los datos de los clietnes que le corresponden. 
            condicion_usuarios = id_tipo_de_usuario.ToString() != "1" ? string.Format(" and a.id_registrado_por='{0}' ", id_usuario) : ""; 

            //Finalmente utilizaré la variable para traer la página que me solicitan. 

            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            //Recuerda siempre poner la condicio´n del estado. ¿Ok? 
            /*string query = string.Format("select  " +
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
            ",a.foto_url " +
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            ",a.email " +            
            ", concat(a.nombres, ' ' , a.apellido_paterno,  ' ' ,a.apellido_materno) as cliente " +
            ", concat(g.nombres, ' ' , g.apellido_paterno,  ' ' ,g.apellido_materno) as criador " +
            ", g.email as email_criador " +
            ",b.nombre as ciudad " +
            ", count(d.id) as mascotas " +            
            "from lu_clientes a  " +
            "left join cf_ciudades b on a.id_ciudad = b.id " +            
            "left join lu_mascotas d on d.id_cliente=a.id " +
            "left join lu_razas e on e.id=d.id_raza " +
            "left join lu_tipos_de_mascota f on f.id=e.id_tipo_de_mascota " +
            "left join lu_usuarios g on g.id=a.id_registrado_por " +
            "where a.estado=1 " +            
            "{3} " +            
            "group by a.id " +
            " HAVING cliente like '%{2}%' " + 
            "OR criador like '%{2}%' " +
            "OR a.email like '%{2}%' " +
            "OR b.nombre like '%{2}%' " +
            "OR g.email like '%{2}%' " +             
            "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre
                , condicion_usuarios);*/

            string query = string.Format("select  " +
            "a.id " +
            ",a.nombres " +
            ",a.apellido_paterno " +
            ",a.apellido_materno " +
            ",a.telefono_local " +
            ",a.telefono_celular " +
            ",a.calle " +
            ",a.numero" +
            ",a.foto_url " +
            ",a.delegacion " +
            ",a.colonia " +
            ",a.foto_url " +
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            ",a.email " +
            ", concat(a.nombres, ' ' , a.apellido_paterno,  ' ' ,a.apellido_materno) as cliente " +
            ", concat(g.nombres, ' ' , g.apellido_paterno,  ' ' ,g.apellido_materno) as criador " +
            ", g.email as email_criador " +
            ",b.nombre as ciudad " +
            "from lu_clientes a  " +
            "left join cf_ciudades b on a.id_ciudad = b.id " +            
            "left join lu_usuarios g on g.id=a.id_registrado_por " +
            "where a.estado=1 " +
            "{3} " +
            "group by a.id " +
            " HAVING cliente like '%{2}%' " +
            "OR  criador like '%{2}%' " +
            "OR a.email like '%{2}%' " +
            "OR b.nombre like '%{2}%' " +
            "OR g.email like '%{2}%' " +
            "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre
                , condicion_usuarios);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            //Viste como Debuguiee? Cuando te salga algun errror, copia y pega el query que pones aqupara correlo en el Workbench Y listo :D
            //Convertimos a Json y regresamos los datos. 

            return Json(utilidades.convertDataTableToJson(tabla_resultado));

            //Ya terminamos... Ahora a probar. Pondré un punto de ruptura al inicio de la funciójn pra debuggear. 
            //Te sugiero que hagas lo mismo para las funciones que hagas. Creo que es muy útil. 

        }

        [Route("api/clientes/getSearchByEmail")]
        public IHttpActionResult getSearchByEmail()
        {
            /*if (!utilidades.validar_token(Request))
                return Json("incorrecto");*/

            IEnumerable<string> headerValues = Request.Headers.GetValues("email");
            string email = headerValues.FirstOrDefault().ToString();

            string query = string.Format("select  " +
            "a.id " +
            ", concat(a.nombres, ' ' , a.apellido_paterno,  ' ' ,a.apellido_materno) as cliente " +
            "from lu_clientes a " +
            "where a.estado=1 " +
            "and a.email='{0}' "                             
                , email); 

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            if (tabla_resultado == null)
                return Json("incorrecto"); 

            return Json(utilidades.convertDataTableToJson(tabla_resultado));
        }
    }
}
