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
    public class razasController : ApiController
    {        
        // GET api/razas
        public IHttpActionResult Get()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = "SELECT * from lu_razas where estado=1;";
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        // GET api/razas/5
        public IHttpActionResult Get(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = string.Format("SELECT * from lu_razas where id='{0}'", id);
            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }


        // POST api/razas
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
            string insert_query = string.Format("INSERT INTO `lu_razas` " +
            "(`nombre`, `id_tipo_de_mascota`, `talla`) " +
            "VALUES " +
            "('{0}', '{1}', '{2}'); "
            , json["nombre"]
            , json["id_tipo_de_mascota"]
            , json["talla"]);
            //, id);

            //En caso de error, devolverá incorrecto
            tabla_resultado.Rows[0]["id"] = Database.runInsert(insert_query).ToString();

            //Relacionamos la nueva raza con los usarios para el manejo de inventario de mascotas. 
            string insert_inventario = string.Format("INSERT IGNORE INTO `lu_existencias_de_mascotas` (`id_usuario`, `id_raza`)   " +
                                                        "SELECT a.id as id_usuario, b.id as id_raza  " +
                                                        "from  " +
                                                        "lu_usuarios a  " +
                                                        "LEFT JOIN lu_razas b on 1 " +
                                                        "where b.id='{0}';  ",
                                                        tabla_resultado.Rows[0]["id"]);
            Database.runQuery(insert_inventario); 
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

        // PUT api/razas/5
        public IHttpActionResult Post(int id, [FromBody]Object value)
        {
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_razas` " +
            "set " +
            "nombre='{0}' " +
            ", talla='{1}' " +
            "where id='{2}'"
            , json["nombre"]
            , json["talla"]
            , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        // DELETE api/razas/5
        public IHttpActionResult Delete(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto"); 

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_razas` " +
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

        //Acabo de crear la función. Falta poner la ruta...
        //Con esta línea se agrega la ruta.
        [Route("api/razas/getByPage")]
        public IHttpActionResult getSearchByPage()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            IEnumerable<string> headerValues = Request.Headers.GetValues("pagina");
            string string_pagina = headerValues.FirstOrDefault().ToString();
            int pagina = int.Parse(string_pagina);

            IEnumerable<string> headerValues_nombre = Request.Headers.GetValues("nombre");
            string nombre = headerValues_nombre.FirstOrDefault().ToString();

            string query = string.Format("select  " +
                "a.id " +
                ",a.nombre " +
                ", b.nombre as tipo " +
                "from lu_razas a  " +
                "left join lu_tipos_de_mascota b on a.id=b.id " +
                "where a.nombre like '%{2}%' " +
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
    }
}
