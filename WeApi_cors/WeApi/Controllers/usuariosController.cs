using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WeApi.Controllers
{
    public class usuariosController : ApiController
    {
        BackgroundWorker bg_enviar_contraseña = new BackgroundWorker(); 
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

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_distribuidor = Request.Headers.GetValues("id_distribuidor");
            string id_distribuidor = headerValues_id_distribuidor.FirstOrDefault().ToString();

            if((id_tipo_de_usuario != "1" && id_tipo_de_usuario != "5")
                && (id.ToString()!=id_usuario))
            {
                return Json("incorrecto"); 
            }

            //En caso de ser un distribuidor, solo le mostraremos los usuarios que le corresponden a él.             
            string condicion_distribuidor = id_tipo_de_usuario == "5" ? string.Format(" and a.id_distribuidor='{0}' ", id_distribuidor) : "";

            string query = string.Format("SELECT  " +
            "a.* " + 
            ", concat(a.nombres, ' ' , a.apellido_paterno, ' ', a.apellido_materno) as usuario  " + 
            ", a.email  " + 
            ", x.nombre as tipo_de_usuario   " + 
            ", a.telefono_celular " + 
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento_format  " + 
            ", a.foto_url " +
            ", e.id as id_tipo_de_mascota_1 " +
            ", f.id as id_tipo_de_mascota_2 " +
            ", g.id as id_tipo_de_mascota_3 " +             
            "from lu_usuarios a  " + 
            "LEFT JOIN cf_tipos_de_usuario x on x.id=a.id_tipo_de_usuario   " +

            "LEFT JOIN lu_razas b on b.id=a.id_raza_1 " +
            "LEFT JOIN lu_razas c on c.id=a.id_raza_2 " +
            "LEFT JOIN lu_razas d on d.id=a.id_raza_3 " +

            "LEFT JOIN lu_tipos_de_mascota e on b.id_tipo_de_mascota=e.id " +
            "LEFT JOIN lu_tipos_de_mascota f on c.id_tipo_de_mascota=f.id " +
            "LEFT JOIN lu_tipos_de_mascota g on d.id_tipo_de_mascota=g.id " + 

            "where a.id='{0}' {1};  " 
            , id
            , condicion_distribuidor);
            DataTable tabla = Database.runSelectQuery(query);

            if (tabla == null)
                return Json("incorrecto"); 
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        [Route("api/usuarios/getUserLevel/{id}")]
        public IHttpActionResult getUserLevel(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = string.Format("select  " + 
            "count(*) as cuenta_clientes  " + 
            ", (CASE  " + 
            "        when count(*)>=(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1)  " + 
            "        and count(*)<(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 1)  " + 
            "        then (select nombre from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1) " + 
            "         " + 
            "        when count(*)>=(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 1)  " + 
            "        and count(*)<(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 2)  " + 
            "        then (select nombre from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 1) " + 
            "         " + 
            "        when count(*)>=(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 2)          " + 
            "        then (select nombre from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 2)         " + 
            "	end      " + 
            " ) as nombre  " + 
            " , (CASE  " + 
            "        when count(*)>=(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1)  " + 
            "        and count(*)<(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 1)  " + 
            "        then (select foto_url from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1) " + 
            "         " + 
            "        when count(*)>=(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 1)  " + 
            "        and count(*)<(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 2)  " +
            "        then (select foto_url from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 1) " + 
            "         " + 
            "        when count(*)>=(select cantidad_de_clientes from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 2)          " +
            "        then (select foto_url from cf_niveles_de_usuario order by cantidad_de_clientes asc limit 1 offset 2)         " + 
            "	end      " +
            " ) as foto_url  " + 
            "from lu_clientes a " + 
            "	LEFT JOIN lu_usuarios b on a.id_registrado_por=b.id " + 
            "    where b.id='{0}';   " , id);

            DataTable tabla = Database.runSelectQuery(query);
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        [Route("api/usuarios/getUserClients/{id}")]
        public IHttpActionResult getUserClients(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = string.Format("select  " + 
                                        "a.id " + 
                                        ", concat(a.nombres, ' ' , a.apellido_paterno,  ' ' ,a.apellido_materno) as cliente " + 
                                        ", a.email  " + 
                                        ", a.telefono_celular  " +                                        
                                        ", DATE_FORMAT(a.fecha_de_registro, '%d/%m/%Y') AS fecha_de_registro  " +
                                        ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento  " + 
                                        "from lu_clientes a " + 
                                        "where a.id_registrado_por='{0}' limit 10;  " , id);

            DataTable tabla = Database.runSelectQuery(query);
            if (tabla == null)
                return Json("vacio"); 
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        [Route("api/usuarios/getUserClientsCount/{id}")]
        public IHttpActionResult getUserClientsCount(int id)
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string query = string.Format("select  " + 
            "concat('Clientes ', a.nombre, '(s)') as nombre " + 
            ", count(b.id) as cuenta from cf_estados_de_cliente a " + 
            "left join lu_clientes b on a.id=b.id_estado and b.id_registrado_por='{0}' " + 
            "group by a.id; " , id); 

            DataTable tabla = Database.runSelectQuery(query);            
            return Json(utilidades.convertDataTableToJson(tabla));
        }

        [Route("api/usuarios/getSearchByEmail")]
        public IHttpActionResult getSearchByEmail()
        {
            /*if (!utilidades.validar_token(Request))
                return Json("incorrecto");*/

            IEnumerable<string> headerValues = Request.Headers.GetValues("email");
            string email = headerValues.FirstOrDefault().ToString();

            string query = string.Format("select  " +
            "a.id " +
            ", concat(a.nombres, ' ' , a.apellido_paterno,  ' ' ,a.apellido_materno) as cliente " +
            "from lu_usuarios a " +
            "where a.estado=1 " +
            "and a.email='{0}' "
                , email);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            if (tabla_resultado == null)
                return Json("incorrecto");

            return Json(utilidades.convertDataTableToJson(tabla_resultado));
        }


        [Route("api/usuarios/getVerificarSecret")]
        public IHttpActionResult getVerificarSecret()
        {
            /*if (!utilidades.validar_token(Request))
                return Json("incorrecto");*/

            IEnumerable<string> headerValues = Request.Headers.GetValues("secret");
            string secret = headerValues.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            string query = string.Format("select  " +
            "a.id " +            
            "from lu_usuarios a " +
            "where a.id={0} and binary secret='{1}' " 
                , id_usuario
                , secret );

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            if (tabla_resultado == null)
                return Json("incorrecto");

            return Json("correcto");
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

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_modo = Request.Headers.GetValues("modo");
            string modo = headerValues_modo.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_distribuidor = Request.Headers.GetValues("id_distribuidor");
            string id_distribuidor = headerValues_id_distribuidor.FirstOrDefault().ToString();

            //En caso de ser un distribuidor, solo le mostraremos los usuarios que le corresponden a él.             
            string condicion_distribuidor = id_tipo_de_usuario == "5" ? string.Format(" and a.id_distribuidor='{0}' ", id_distribuidor) : "";

            //Condición para mostrar solo a los clientes activos. 
            string condicion_administrador = id_tipo_de_usuario != "1" ? " and a.id_estado_de_usuario=1 " : "";


            string condicion_modo = ""; 
            if(modo == "solicitados")
            {
                condicion_modo = " and a.id_estado_de_usuario=3 ";
                //Quitamos la condicion de administrador. 
                condicion_administrador = ""; 
            }
            else
            {
                condicion_modo = " and a.id_estado_de_usuario<>3 ";
            }

            //En caso de ser criador, no le regresaremos nada. 
            if((id_tipo_de_usuario!="1")&&(id_tipo_de_usuario!="5"))
                return Json("Incorrecto"); 

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
            ",a.foto_url " +
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            ",a.email " +
            ",a.establecimiento " +
            ", concat(a.nombres, ' ' , a.apellido_paterno,  ' ' ,a.apellido_materno) as usuario " +
            ",b.nombre as ciudad " +
            ",c.nombre as tipo_de_usuario " +
            ", d.nombre as distribuidor " +
            "from lu_usuarios a  " +
            "left join cf_ciudades b on a.id_ciudad = b.id " +
            "left join cf_tipos_de_usuario c on c.id = a.id_tipo_de_usuario " +
            "left join cf_distribuidores d on d.id=a.id_distribuidor " + 
            "where a.estado=1 " +
            "{3} {4} {5} " +
            "group by a.id " +
            "HAVING usuario like '%{2}%' " + //Buscamos por usuario
            "OR a.email like '%{2}%' " + 
            "OR b.nombre like '%{2}%' " +   //Zona
            "OR d.nombre like '%{2}%' " + //Distribuidor                         
            "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre
                , condicion_distribuidor
                , condicion_modo
                , condicion_administrador);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            //Viste como Debuguiee? Cuando te salga algun errror, copia y pega el query que pones aqupara correlo en el Workbench Y listo :D
            //Convertimos a Json y regresamos los datos. 

            return Json(utilidades.convertDataTableToJson(tabla_resultado));

            //Ya terminamos... Ahora a probar. Pondré un punto de ruptura al inicio de la funciójn pra debuggear. 
            //Te sugiero que hagas lo mismo para las funciones que hagas. Creo que es muy útil. 

        }

        [Route("api/usuarios/getUserPetsByPage")]
        public IHttpActionResult getUserPetsByPage()
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

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario_busqueda");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();


            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            //Recuerda siempre poner la condicio´n del estado. ¿Ok? 
            string query = string.Format("select  " +
            "a.id " +
            ", a.cantidad " +
            ", b.nombre as raza " +
            ", b.talla as talla " +
            ", c.nombre as tipo_de_mascota " +
            " from lu_existencias_de_mascotas a  " +
            " left join lu_razas b on b.id=a.id_raza " +
            " left join lu_tipos_de_mascota c on b.id_tipo_de_mascota=c.id " +
            "where a.id_usuario='{0}' " +  
            "and (b.nombre like '%{1}%' " +
            "or c.nombre like '%{1}%' " +
            "or b.talla like '%{1}%') " +
            "group by a.id " +
            "order by a.cantidad desc, b.nombre asc limit {2} offset {3};  "
                , id_usuario
                , nombre
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

        [Route("api/usuarios/getIniciarSesion")]
        public IHttpActionResult getIniciarSesion()
        {
            //Obtenemos el nombre de usuario y contraseña. 
            IEnumerable<string> headerValues_secret = Request.Headers.GetValues("secret");
            string secret = headerValues_secret.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_email = Request.Headers.GetValues("email");
            string email = headerValues_email.FirstOrDefault().ToString();

            //
            string query = string.Format("select  " +
                                        "a.* " + 
                                        ", a.id as id_usuario " +
                                        ", a.id_tipo_de_usuario as id_tipo_de_usuario" +
                                        ", '' as token " +
                                        ", concat(a.nombres) as nombres_de_usuario " +
                                        ", b.nombre as tipo_de_usuario " + 
                                        ", a.acuerdos_leidos as acuerdos_leidos  " +
                                        ", '' as datos_completos " +
                                        ", c.nombre as distribuidor " + 
                                        "from lu_usuarios a " +
                                        "LEFT JOIN cf_tipos_de_usuario b on b.id=a.id_tipo_de_usuario " + 
                                        "LEFT JOIN cf_distribuidores c on c.id=a.id_distribuidor " +                                         
                                        "where  " + 
                                        "email='{0}'  " + 
                                        "and binary secret='{1}' and id_estado_de_usuario=1; " //Solo traermos a los usuarios activos. 
                                         , email
                                         , secret);

            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            if (tabla_resultado == null)
                return Json("vacio"); 

            
            //Generamos el Token y lo guardamos.
            string token = utilidades.generar_token(tabla_resultado.Rows[0]["id_usuario"].ToString()
                , email);
            tabla_resultado.Rows[0]["token"] = token;

            if (tabla_resultado.Rows[0]["nombres"].ToString() == "" ||
                tabla_resultado.Rows[0]["apellido_paterno"].ToString() == "" ||
                tabla_resultado.Rows[0]["apellido_materno"].ToString() == "" ||
                tabla_resultado.Rows[0]["telefono_local"].ToString() == "" ||
                tabla_resultado.Rows[0]["telefono_celular"].ToString() == "" ||
                tabla_resultado.Rows[0]["email"].ToString() == "" ||
                tabla_resultado.Rows[0]["establecimiento"].ToString() == "" ||
                tabla_resultado.Rows[0]["delegacion"].ToString() == "" ||
                tabla_resultado.Rows[0]["colonia"].ToString() == "" ||
                tabla_resultado.Rows[0]["calle"].ToString() == "" ||
                tabla_resultado.Rows[0]["numero"].ToString() == "")
                tabla_resultado.Rows[0]["datos_completos"] = "0"; 
            else
                tabla_resultado.Rows[0]["datos_completos"] = "1"; 

            return Json(utilidades.convertDataTableToJson(tabla_resultado));

            

        }

        [Route("api/usuarios/getSecret")]
        public IHttpActionResult getSecret()
        {
            try
            {

                //Incrementamos la cantidad de solicitudes. 
                utilidades.solicitudes_de_restablecimiento_de_secret++;
                utilidades.solicitudes_de_restablecimiento_de_secret = 0;
                if (utilidades.solicitudes_de_restablecimiento_de_secret >= 10)
                {
                    return Json("Se ha excedido la cantidad máxima de solicitudes de contraseña. Por favor, contacte a un administrador para restablecerla.");
                }

                //Obtenemos el correo electrónico. 
                IEnumerable<string> headerValues_email = Request.Headers.GetValues("email");
                string email = headerValues_email.FirstOrDefault().ToString();

                string select_query = string.Format("select nombres, secret from lu_usuarios where email='{0}';"
                     , email);
                //Obtenemos los accesos. 
                DataTable tabla_accesos = Database.runSelectQuery(select_query);

                if (tabla_accesos == null)
                    return Json("No se encontró un usuario con el correo especificado.");

                string mensaje = string.Format("La contraseña para el sitio es: '{0}'"
                    , tabla_accesos.Rows[0]["secret"].ToString());


                mensaje = string.Format("<!doctype html>" +
"<html lang='es'>" +
"   <head>      " +
"      <meta charset='utf-8'>      " +
"   </head>" +
"   <body style='background-color: #E2001A;'>" +
"      <div class='container-fluid'>" +
"         <div style='padding-top:80px;padding-bottom:80px;padding-left:10vw;padding-right:10vw;'>" +
"           <div class=''></div>" +
"            <div class='' style='background-color: white;border-radius: 50px;padding-top:10vh;padding-bottom:10vh;padding-right:5vw;padding-left:5vw;'>" +
"              <div class=''>" +
"                <div style='color: #E2001A;font-size: 25px;font-weight: 700;text-align: center;'>Restablecimiento de Contraseña</div><br>" +
"                <div style='font-size: 20px; text-align: justify;'>" +
"                  <div class=''>" +
"                    <div class='' style='text-align: right;font-weight: bold;'>Estimado(a) {0}</div><br>:" +
"                    <div class=''>Recibe saludos del equipo de Royal Canin. A continuación encontrarás los accesos para entrar al micrositio del sistema.</div><br>" +
"                    <div><b>Usuario:</b><br> {1}</div><br>" +
"                    <div><b>Contraseña:</b><br> {2}</div><br>" +
"                    <div class=''><b>Dirección del sitio:</b><br> http://micrositioroyalcanin.com.mx/</div><br>" +
"                    <div class='' style='text-align: center; font-weight: bold;'>Quedamos al pendiente de ti</div><br>" +
"                    <div class='' style='text-align: right;'>" +
"                    <img src='http://micrositioroyalcanin.com.mx/images/royal_canin_logo_02.jpg' style='margin-bottom:0px;height: auto; width:30vw;max-width:250px; '>" +
"                    </div>                    " +
"                    <div class=' col-12' style='text-align: right; font-weight: bold;margin-top: 0px;'>Atención a Clientes " +
"                    </div>" +
"                  </div>" +
"                </div>" +
"                <div class=' col-12'></div><br>" +
"              </div>" +
"            </div>" +
"          </div>" +
"        </div>" +
"      </body>" +
"    </html>"
    , tabla_accesos.Rows[0]["nombres"].ToString()
    , email
    , tabla_accesos.Rows[0]["secret"].ToString());



                List<object> arguments = new List<object>();
                //Mandamoe el id de mascota como argumento. 
                arguments.Add(mensaje); 
                arguments.Add(email);
                arguments.Add("Restablecimiento de contraseña");
                bg_enviar_contraseña.DoWork += new DoWorkEventHandler(enviar_correo);
                bg_enviar_contraseña.RunWorkerAsync(arguments); 

                return Json("Se han enviado instrucciones al correo especificado.");
                //return Json("Se han enviado instrucciones al correo especificado."); 
            }
            catch(Exception ex)
            {
                return Json(ex.ToString()); 
            }
        }

        [Route("api/usuarios/uploadImage/{id}")]

        public IHttpActionResult uploadImage(int id, [FromBody]Object value)
        {
            try
            {
                //Tomar en cuenta que las fechas vienen en el formato YYYY-MM-dd
                JObject json = JObject.Parse(value.ToString());


                if (!validacion(json))
                    return Json("incorrecto");

                string filename = string.Format("{0}.jpg", id);
                utilidades.guardar_imagen(json["foto_url"].ToString(), "usuarios", filename);

                string foto_url = "http://" + Request.Headers.Host + "/temp/usuarios/" + filename;

                foto_url += "?fecha=" + DateTime.Now.ToString("ddMMyyyy_HHmmss");

                //Actualizamos el campo de foto_url de la mascota.             
                string update_query = string.Format("UPDATE `lu_usuarios` " +
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
            catch(Exception ex)
            {
                return Json(ex.ToString()); 
            }
        }

        private void enviar_correo(object sender, DoWorkEventArgs e)
        {
            List<object> argumentos = e.Argument as List<object>;
            string mensaje = (string)argumentos[0];            
            string email = (string)argumentos[1];
            string asunto = (string)argumentos[2];

            utilidades.enviar_correo(mensaje, email, "Restablecimiento de Contraseña");
            
        }

        [Route("api/usuarios/getUserTotalClientsPerState/{id}")]
        public IHttpActionResult getUserTotalClients(int id)
        {
            //Recuerda poner siempre la función de validación de token. Ya entró; pero no le mande la página en el header. 
            //Para eso utilizaremos POstman !! :D 
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            //Utilizaré la variable estatica (global) de la clase de utilidades y el número de la página que me solicitan. 
            //Recuerda siempre poner la condicio´n del estado. ¿Ok? 
            string query = string.Format("select  " +
                "count(b.id) as cuenta  " +
                ", concat(a.nombre, '(s)') as estado  " +
                "from cf_estados_de_cliente a " +
                "left join lu_clientes b on a.id=b.id_estado and b.id_registrado_por='{0}' " +
                "GROUP BY a.id;  "
                , id); 

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
                "`micrositio`," +
                "`c48_horas`," +
                "`id_marca`," +
                "`id_distribuidor`," +                
                "`id_raza_1`," +
                "`id_raza_2`," +
                "`id_raza_3`," +
                "`cantidad_raza_1`," +
                "`cantidad_raza_2`," +
                "`cantidad_raza_3`," +
                "`sitio_web`," +
                "`camadas_al_anio`," +
                "`id_estado_de_usuario`) " +
            "VALUES " +
            //Verifica las funciones now() (Parametros 17 y 18), envía un post desde postman llenando estos datos y pon un punto de ruptura aquí para que veas el query. Copia y pega el query en Workbench para debuggearlo. 
            "('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}','{9}', STR_TO_DATE('{10}', '%Y-%m-%d'), '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}', '{22}', '{23}', '{24}', '{25}', '{26}', '{27}', '{28}'); "
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
                , "foto_url"
                , json["establecimiento"]
                , json["micrositio"]
                , json["c48_horas"]
                , json["id_marca"]
                , json["id_distribuidor"]
                , json["id_raza_1"]
                , json["id_raza_2"]
                , json["id_raza_3"]

                , json["cantidad_raza_1"]
                , json["cantidad_raza_2"]
                , json["cantidad_raza_3"]
                , json["sitio_web"]
                , json["camadas_al_anio"]                
                , json["id_estado_de_usuario"]);
            //, id);

            //En caso de error, devolverá incorrecto
            tabla_resultado.Rows[0]["id"] = Database.runInsert(insert_query).ToString();
            /*
            string insert_inventario = string.Format("INSERT IGNORE INTO `lu_existencias_de_mascotas` (`id_usuario`, `id_raza`)   " +
                                                        "SELECT a.id as id_usuario, b.id as id_raza  " +
                                                        "from  " +
                                                        "lu_usuarios a  " +
                                                        "LEFT JOIN lu_razas b on 1 " +
                                                        "where a.id='{0}';  ",
                                                        tabla_resultado.Rows[0]["id"]);

            //Relacionamos este nuevo usuario con todas las razas para el manejo de su inventario. 
            Database.runQuery(insert_inventario); */
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

        // PUT api/usuarios/5
        public IHttpActionResult Post(int id, [FromBody]Object value)
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
            ", micrositio='{16}' " + 
            ", c48_horas='{17}' " + 
            ", id_marca='{18}' " +
            ", id_distribuidor='{19}' " +

            ", id_raza_1='{20}' " +
            ", id_raza_2='{21}' " +
            ", id_raza_3='{22}' " +

            ", cantidad_raza_1='{23}' " +
            ", cantidad_raza_2='{24}' " +
            ", cantidad_raza_3='{25}' " +
            ",  id_estado_de_usuario='{26}' " +
            ",  sitio_web='{27}' " +
            ",  camadas_al_anio='{28}' " +
            "where id='{29}'"
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
            , json["micrositio"]
            , json["c48_horas"]
            , json["id_marca"]
            , json["id_distribuidor"]

            , json["id_raza_1"]
            , json["id_raza_2"]
            , json["id_raza_3"]

            , json["cantidad_raza_1"]
            , json["cantidad_raza_2"]
            , json["cantidad_raza_3"]
            , json["id_estado_de_usuario"]
            , json["sitio_web"]
            , json["camadas_al_anio"]
            , id);






            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }


        // POST api/tiposdemascota/kevin/5
        [Route("api/usuarios/updateSecret/{id}")]
        public IHttpActionResult updateSecret(int id, [FromBody]Object value)
        {
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Primero verificamos que la contraseña anterior sea correcta. 
            string query = string.Format("select  " +
            "a.id " +
            "from lu_usuarios a " +
            "where a.id={0} and binary secret='{1}' "
                , id
                , json["secret_anterior"]);

            DataTable secret_correcta = Database.runSelectQuery(query);
            if (secret_correcta == null)
                return Json("La contraseña anterior no es válida"); 

            
            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_usuarios` " +
            "set " +
            "secret='{0}' " +            
            "where id='{1}'"
            , json["secret_nueva"]
            , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        // POST api/tiposdemascota/kevin/5
        [Route("api/usuarios/preregistro")]
        public IHttpActionResult preregistro([FromBody]Object value)
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
                "`fecha_de_nacimiento`," +
                "`id_ciudad`," +
                "`email`," +
                "`id_tipo_de_usuario`," +
                "`id_distribuidor`," +                
                "`id_estado_de_usuario`," +
                "`secret`) " +
            "VALUES " +
            //Verifica las funciones now() (Parametros 17 y 18), envía un post desde postman llenando estos datos y pon un punto de ruptura aquí para que veas el query. Copia y pega el query en Workbench para debuggearlo. 
            "('{0}', '{1}', '{2}', '{3}', '{4}', STR_TO_DATE('{5}', '%Y-%m-%d'), '{6}', '{7}', '{8}', '{9}', '{10}', '{11}');"
                , json["nombres"]
                , json["apellido_paterno"]
                , json["apellido_materno"]
                , json["telefono_local"]
                , json["telefono_celular"]                
                , json["fecha_de_nacimiento"]
                , json["id_ciudad"]
                , json["email"]                
                , json["id_tipo_de_usuario"]
                , json["id_distribuidor"]
                , "3"
                , json["secret"]); //Se le pone el 3 que significa solicitado. 
            //, id);

            //En caso de error, devolverá incorrecto
            tabla_resultado.Rows[0]["id"] = Database.runInsert(insert_query).ToString();
            /*
            string insert_inventario = string.Format("INSERT IGNORE INTO `lu_existencias_de_mascotas` (`id_usuario`, `id_raza`)   " +
                                                        "SELECT a.id as id_usuario, b.id as id_raza  " +
                                                        "from  " +
                                                        "lu_usuarios a  " +
                                                        "LEFT JOIN lu_razas b on 1 " +
                                                        "where a.id='{0}';  ",
                                                        tabla_resultado.Rows[0]["id"]);

            //Relacionamos este nuevo usuario con todas las razas para el manejo de su inventario. 
            Database.runQuery(insert_inventario); */
            if (tabla_resultado.Rows[0]["id"].ToString() == "-1")
                return Json("incorrecto"); 

            //Devolcemos la información de la tabla. 
            return Json(utilidades.convertDataTableToJson(tabla_resultado));
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

        [Route("api/usuarios/getAceptarTerminos")]
        public IHttpActionResult getAceptarTerminos()
        {
            if(!utilidades.validar_token(Request))
                return Json("incorrecto");

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_usuarios` " +
            "set " +
            "acuerdos_leidos=1 " +
            "where id='{0}'"
            , id_usuario);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        [Route("api/usuarios/postactualizarexistenciademascota/{id}")]
        public IHttpActionResult postactualizarexistenciademascota(int id, [FromBody]Object value)
        {
            JObject json = JObject.Parse(value.ToString());

            if (!validacion(json))
                return Json("incorrecto");

            //Actualizamos los datos con un update query. 
            string update_query = string.Format("UPDATE `lu_existencias_de_mascotas` " +
            "set " +
            "cantidad='{0}' " +
            "where id='{1}'"
            , json["cantidad"]
            , id);

            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }
    }
}
