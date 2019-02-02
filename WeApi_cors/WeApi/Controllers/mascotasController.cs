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
    public class mascotasController : ApiController
    {
        // GET api/mascotas
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
            "from lu_mascotas a  " +
            "left join lu_razas b on a.id_raza=b.id  " +
            "left join lu_tipos_de_mascota c on c.id=b.id_tipo_de_mascota " +
            "left join lu_clientes d on d.id=a.id_cliente " +
            "where a.estado=1 order by a.fecha_de_modificacion desc;  "; 
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // GET api/mascotas/5
        public IHttpActionResult Get(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = string.Format("SELECT " + 
                "a.* " +
                ", c.id as id_tipo_de_mascota " +
                ", d.email as email " + 
                ", concat(d.nombres, ' ' , d.apellido_paterno, ' ', d.apellido_materno) as cliente " + 
                "from lu_mascotas a" + 
                " LEFT JOIN lu_razas b on a.id_raza=b.id " +
                " LEFT JOIN lu_tipos_de_mascota c on b.id_tipo_de_mascota=c.id " +
                " LEFT JOIN lu_clientes d on d.id=a.id_cliente " +
                "where a.id='{0}'", id);
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        [Route("api/mascotas/getByPage")]
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
            "from lu_mascotas a  " +
            "left join lu_razas b on a.id_raza=b.id  " +
            "left join lu_tipos_de_mascota c on c.id=b.id_tipo_de_mascota " +
            "left join lu_clientes d on d.id=a.id_cliente " +
            "where a.estado=1 order by a.fecha_de_modificacion desc limit {0} offset {1};  "                
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina-1)));

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            //Viste como Debuguiee? Cuando te salga algun errror, copia y pega el query que pones aqupara correlo en el Workbench Y listo :D
            //Convertimos a Json y regresamos los datos. 

            return Json(utilidades.convertDataTableToJson(tabla_resultado));

            //Ya terminamos... Ahora a probar. Pondré un punto de ruptura al inicio de la funciójn pra debuggear. 
            //Te sugiero que hagas lo mismo para las funciones que hagas. Creo que es muy útil. 

        }

        [Route("api/mascotas/getSearchByPage")]
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
            ",a.nombre " +
            ",a.genero " +
            ",b.nombre as raza " +
            ",a.foto_url " +
            ", c.nombre as tipo " +
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            ", concat(d.nombres, ' ' , d.apellido_paterno,  ' ' ,d.apellido_materno) as cliente " +
            "from lu_mascotas a  " +
            "left join lu_razas b on a.id_raza=b.id  " +
            "left join lu_tipos_de_mascota c on c.id=b.id_tipo_de_mascota " +
            "left join lu_clientes d on d.id=a.id_cliente " +
            "where a.estado=1 " +
            "and a.nombre like '%{2}%' " + 
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



        // POST api/mascotas
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
            string insert_query = string.Format("INSERT INTO `lu_mascotas` " +
            "(`nombre`," +
                "`fecha_de_nacimiento`," +
                "`genero`," +
                "`id_raza`," +
                "`id_cliente`," +
                "`foto_url`," +
                "`fecha_de_registro`," +
                "`fecha_de_modificacion`) " +
            "VALUES " +
            "('{0}', STR_TO_DATE('{1}', '%Y-%m-%d'), '{2}', '{3}', '{4}', '{5}', {6}, {7}); "
            , json["nombre"]
            , json["fecha_de_nacimiento"]
            , json["genero"]
            , json["id_raza"]
            , json["id_cliente"]
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

            if (json["nombre"].ToString().Trim() == "")
                return false;

            return true;
        }

        // PUT api/mascotas/5
        public IHttpActionResult Put(int id, [FromBody]Object value)
        {
            //Tomar en cuenta que las fechas vienen en el formato YYYY-MM-dd
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_mascotas` " +
           "set " +
           "nombre='{0}' " +
           ",fecha_de_nacimiento=STR_TO_DATE('{1}', '%Y-%m-%d')" +
           ",genero='{2}' " +
           ",id_raza='{3}' " +
           ",id_cliente='{4}' " +
           ",foto_url='{5}' " +
           ",fecha_de_modificacion=now() " +
           "where id='{6}'"
           , json["nombre"]
           , json["fecha_de_nacimiento"]
           , json["genero"]
           , json["id_raza"]
           , json["id_cliente"]
           , json["foto_url"]
           , id);





            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        // DELETE api/mascotas/5
        public IHttpActionResult Delete(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_mascotas` " +
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
