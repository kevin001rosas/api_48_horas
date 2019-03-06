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
    public class kitsController : ApiController
    {
        // GET api/kits
        public IHttpActionResult Get()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = "select  " +
            "a.id " +
            ",a.nombre " +
            ",a.genero " +
            ",a.foto_url " +
            ", c.nombre as tipo " +
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            ", concat(d.nombres, ' ' , d.apellido_paterno,  ' ' ,d.apellido_materno) as cliente " +
            "from ft_kits a  " +
            "left join lu_razas b on a.id_raza=b.id  " +
            "left join lu_tipos_de_mascota c on c.id=b.id_tipo_de_mascota " +
            "left join lu_clientes d on d.id=a.id_cliente " +
            "where a.estado=1 order by a.fecha_de_modificacion desc;  ";
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // GET api/kits/5
        public IHttpActionResult Get(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = string.Format("SELECT " +
                "a.* " +
                ", c.id as id_tipo_de_mascota " +
                ", c.nombre as tipo_de_mascota " +
                ", concat(b.nombre, b.talla) as raza " +
                ", d.email as email " +
                ", e.nombre as estado_de_kit " +
                ", f.nombre as tipo_de_kit " +
                ", concat(d.nombres, ' ' , d.apellido_paterno, ' ', d.apellido_materno) as solicitado_por " +
                "from ft_kits a" +
                " LEFT JOIN lu_razas b on a.id_raza=b.id " +
                " LEFT JOIN lu_tipos_de_mascota c on b.id_tipo_de_mascota=c.id " +
                " LEFT JOIN lu_usuarios d on d.id=a.id_solicitado_por " +
                " LEFT JOIN cf_estados_de_kit e on e.id=a.id_estado_de_kit " +
                " LEFT JOIN cf_tipos_de_kit f on f.id=a.id_tipo_de_kit " +
                "where a.id='{0}'", id);
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        [Route("api/kits/getByPage")]
        public IHttpActionResult getByPage()
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


            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            //Recuerda siempre poner la condicio´n del estado. ¿Ok? 
            string query = string.Format("select  " +
            "a.id " +
            ",a.nombre " +
            ",a.genero " +
            ",a.foto_url " +
            ", c.nombre as tipo " +
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            ", concat(d.nombres, ' ' , d.apellido_paterno,  ' ' ,d.apellido_materno) as cliente " +
            "from ft_kits a  " +
            "left join lu_razas b on a.id_raza=b.id  " +
            "left join lu_tipos_de_mascota c on c.id=b.id_tipo_de_mascota " +
            "left join lu_clientes d on d.id=a.id_cliente " +
            "where a.estado=1 order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1)));

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            //Viste como Debuguiee? Cuando te salga algun errror, copia y pega el query que pones aqupara correlo en el Workbench Y listo :D
            //Convertimos a Json y regresamos los datos. 

            return Json(utilidades.convertDataTableToJson(tabla_resultado));

            //Ya terminamos... Ahora a probar. Pondré un punto de ruptura al inicio de la funciójn pra debuggear. 
            //Te sugiero que hagas lo mismo para las funciones que hagas. Creo que es muy útil. 

        }

        [Route("api/kits/getSearchByPage")]
        public IHttpActionResult getSearchByPage()
        {

            if (!utilidades.validar_token(Request))
                return Json("incorrecto");


            IEnumerable<string> headerValues = Request.Headers.GetValues("pagina");
            string string_pagina = headerValues.FirstOrDefault().ToString();
            int pagina = int.Parse(string_pagina);

            IEnumerable<string> headerValues_nombre = Request.Headers.GetValues("nombre");
            string nombre = headerValues_nombre.FirstOrDefault().ToString();

            //Id usuario
            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            //Obtenemos el id de usuario. 
            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            string condicion_usuario = "";
            //En caso de ser usuario Admin, mostrarmos todo; 
            //en caso contrario, mostramos solo los kits que le corresponden. 
            if (id_tipo_de_usuario.ToString() !="1")
            {
                condicion_usuario = string.Format(" and a.id_solicitado_por='{0}' ", id_usuario);
            }

            string query = string.Format("select  " + 
            "a.id " + 
            ", concat(b.nombres, ' ' , b.apellido_paterno, ' ' , b.apellido_materno) as solicitado_por " + 
            ", a.cantidad  " + 
            ", c.nombre as raza " + 
            ", c.talla as talla " + 
            ", DATE_FORMAT(a.fecha_de_registro, '%d/%m/%Y') AS fecha_de_solicitud " + 
            ", DATE_FORMAT(a.fecha_de_nacimiento_camada, '%d/%m/%Y') AS fecha_de_nacimiento_camada " +
            ", e.nombre as estado_de_kit " + 
            "from  " + 
            "ft_kits a  " + 
            "LEFT JOIN lu_usuarios b on a.id_solicitado_por=b.id " + 
            "LEFT JOIN lu_razas c on c.id=a.id_raza  " + 
            "LEFT JOIN lu_tipos_de_mascota d on d.id=c.id_tipo_de_mascota  " +
            " LEFT JOIN cf_estados_de_kit e on e.id=a.id_estado_de_kit " +
             "where a.estado=1 {3}" +             
             "HAVING solicitado_por like '%{2}%' " + 
             "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                 , utilidades.elementos_por_pagina
                 , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                 , nombre
                 , condicion_usuario);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            //Viste como Debuguiee? Cuando te salga algun errror, copia y pega el query que pones aqupara correlo en el Workbench Y listo :D
            //Convertimos a Json y regresamos los datos. 
            if(tabla_resultado==null)
                return Json("sin_kits"); 
            else
                return Json(utilidades.convertDataTableToJson(tabla_resultado));

            //Ya terminamos... Ahora a probar. Pondré un punto de ruptura al inicio de la funciójn pra debuggear. 
            //Te sugiero que hagas lo mismo para las funciones que hagas. Creo que es muy útil. 

        }

        [Route("api/kits/getkitsByCliente/{id}")]
        public IHttpActionResult getkitsByCliente(int id)
        {
            //Recuerda poner siempre la función de validación de token. Ya entró; pero no le mande la página en el header. 
            //Para eso utilizaremos POstman !! :D 
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            //Recuerda siempre poner la condicio´n del estado. ¿Ok? 
            string query = string.Format("select  " +
                "a.id " +
                ", a.nombre " +
                ", a.genero " +
                ", b.nombre as raza " +
                ", c.nombre as tipo " +
                "from ft_kits a " +
                "LEFT JOIN lu_razas b on a.id_raza=b.id " +
                "LEFT JOIN lu_tipos_de_mascota c on c.id=b.id_tipo_de_mascota " +
                "where a.id_cliente='{0}';  "
                , id);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            //Viste como Debuguiee? Cuando te salga algun errror, copia y pega el query que pones aqupara correlo en el Workbench Y listo :D
            //Convertimos a Json y regresamos los datos. 

            return Json(utilidades.convertDataTableToJson(tabla_resultado));

            //Ya terminamos... Ahora a probar. Pondré un punto de ruptura al inicio de la funciójn pra debuggear. 
            //Te sugiero que hagas lo mismo para las funciones que hagas. Creo que es muy útil. 

        }

        // POST api/kits
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
            string insert_query = string.Format("INSERT INTO `db_royal_canin`.`ft_kits` " +
            "(`cantidad`, " +
            "`fecha_de_nacimiento_camada`, " +
            "`id_raza`, " +
            "`id_tipo_de_kit`, " +
            "`comentarios`, " +
            "`id_solicitado_por`) " +
            "VALUES " +
            "('{0}', " +
            " STR_TO_DATE('{1}', '%Y-%m-%d'), " +
            "'{2}', " +
            "'{3}', " +
            "'{4}', " +
            "'{5}'); "
            , json["cantidad"]
            , json["fecha_de_nacimiento_camada"]
            , json["id_raza"]
            , json["id_tipo_de_kit"]
            , json["comentarios"]
            , json["id_solicitado_por"]);            

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

        // PUT api/kits/5
        public IHttpActionResult Post(int id, [FromBody]Object value)
        {
            //Tomar en cuenta que las fechas vienen en el formato YYYY-MM-dd
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `db_royal_canin`.`ft_kits` " + 
            "SET " +             
            "`cantidad` = '{0}', " +
            "`fecha_de_nacimiento_camada` = STR_TO_DATE('{1}', '%Y-%m-%d'), " + 
            "`id_raza` = '{2}', " +
            "`id_tipo_de_kit` = '{3}', " + 
            "`comentarios` = '{4}', " + 
            "`id_solicitado_por` = '{5}', " +
            "`id_estado_de_kit` = '{6}' " +             
            "WHERE `id` = '{7}'; "
           , json["cantidad"]
           , json["fecha_de_nacimiento_camada"]
           , json["id_raza"]
           , json["id_tipo_de_kit"]
           , json["comentarios"]
           , json["id_solicitado_por"]
           , json["id_estado_de_kit"]               
           , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        // DELETE api/kits/5
        public IHttpActionResult Delete(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `ft_kits` " +
            "set " +
            "estado=0 " +
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
