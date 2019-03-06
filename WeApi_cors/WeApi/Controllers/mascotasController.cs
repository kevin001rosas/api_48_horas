using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading;

namespace WeApi.Controllers
{
    public class mascotasController : ApiController
    {
        
        // GET api/mascotas
        BackgroundWorker bg_enviar_certificado = new BackgroundWorker();
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

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

            string condicion_administrador = id_tipo_de_usuario != "1" ? string.Format("and d.id_registrado_por='{0}' ", id_usuario) : ""; 

            string query = string.Format("SELECT " + 
                "a.* " +
                ", c.id as id_tipo_de_mascota " +
                ", d.email as email " + 
                ", concat(d.nombres, ' ' , d.apellido_paterno, ' ', d.apellido_materno) as cliente " + 
                "from lu_mascotas a" + 
                " LEFT JOIN lu_razas b on a.id_raza=b.id " +
                " LEFT JOIN lu_tipos_de_mascota c on b.id_tipo_de_mascota=c.id " +
                " LEFT JOIN lu_clientes d on d.id=a.id_cliente " +
                "where a.id='{0}' {1} "
                , id
                , condicion_administrador);
            DataTable tabla = Database.runSelectQuery(query);
            if (tabla == null)
                return Json("incorrecto"); 
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

            if (!utilidades.validar_token(Request))
                return Json("incorrecto");


            IEnumerable<string> headerValues = Request.Headers.GetValues("pagina");
            string string_pagina = headerValues.FirstOrDefault().ToString();
            int pagina = int.Parse(string_pagina);

            IEnumerable<string> headerValues_nombre = Request.Headers.GetValues("nombre");
            string nombre = headerValues_nombre.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_cliente = Request.Headers.GetValues("id_cliente");
            string id_cliente = headerValues_id_cliente.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
            string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();


            string condicion_administrador = id_tipo_de_usuario != "1" ? string.Format(" and d.id_registrado_por='{0}'", id_usuario) : ""; 
            string condicion_cliente = "";

            if ((id_cliente != "") && (id_cliente != "null"))
            {
                condicion_cliente = string.Format(" and a.id_cliente='{0}' ", id_cliente); 
            }



           string query = string.Format("select  " +
            "a.id " +
            ",a.nombre " +
            ",a.genero " +
            ",a.foto_url " +
            ",b.nombre as raza " +
            ", c.nombre as tipo " +
            ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento " +
            ", concat(d.nombres, ' ' , d.apellido_paterno,  ' ' ,d.apellido_materno) as cliente " +
            "from lu_mascotas a  " +
            "left join lu_razas b on a.id_raza=b.id  " +
            "left join lu_tipos_de_mascota c on c.id=b.id_tipo_de_mascota " +
            "left join lu_clientes d on d.id=a.id_cliente " +
            "left join lu_usuarios e on e.id=d.id_registrado_por " +
            "left join cf_ciudades f on f.id=e.id_ciudad " +
            "where a.estado=1 " +
            "and (a.nombre like '%{2}%' " +
            "or e.email like '%{2}%' or e.nombres like '%{2}%' or e.apellido_paterno like '%{2}%' or e.apellido_materno like '%{2}%' " +
            "or f.nombre like '%{2}%')" + 
            "{3} {4}" + 
            "order by a.fecha_de_modificacion desc limit {0} offset {1};  "
                , utilidades.elementos_por_pagina
                , ((pagina - 1) * (utilidades.elementos_por_pagina - 1))
                , nombre
                , condicion_cliente
                , condicion_administrador);



            //OBtenmeos el Datatable con la información 
            DataTable tabla_resultado = Database.runSelectQuery(query);

            //Viste como Debuguiee? Cuando te salga algun errror, copia y pega el query que pones aqupara correlo en el Workbench Y listo :D
            //Convertimos a Json y regresamos los datos. 

            return Json(utilidades.convertDataTableToJson(tabla_resultado));

            //Ya terminamos... Ahora a probar. Pondré un punto de ruptura al inicio de la funciójn pra debuggear. 
            //Te sugiero que hagas lo mismo para las funciones que hagas. Creo que es muy útil. 

        }

        [Route("api/mascotas/getMascotasByCliente/{id}")]
        public IHttpActionResult getMascotasByCliente(int id)
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
                "from lu_mascotas a " +
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
                "`esterilizado`," +
                "`id_marca`) " +
            "VALUES " +
            "('{0}', STR_TO_DATE('{1}', '%Y-%m-%d'), '{2}', '{3}', '{4}', '{5}', '{6}'); "
            , json["nombre"]
            , json["fecha_de_nacimiento"]
            , json["genero"]
            , json["id_raza"]
            , json["id_cliente"]            
            , json["esterilizado"]
            , json["id_marca"]); 

            
            //, id);
            
            //En caso de error, devolverá incorrecto
            tabla_resultado.Rows[0]["id"] = Database.runInsert(insert_query).ToString();
            if (tabla_resultado.Rows[0]["id"].ToString() == "-1")
                return Json("incorrecto");
            
            //Preparamos el BackGroundWorker para generar el certificado y enviarlo. 
            List<object> arguments = new List<object>();
            //Mandamoe el id de mascota como argumento. 
            arguments.Add(tabla_resultado.Rows[0]["id"]); 
            bg_enviar_certificado.DoWork += new DoWorkEventHandler(enviar_certificado);
            bg_enviar_certificado.RunWorkerAsync(arguments); 
            

            //Devolcemos la información de la tabla. 
            return Json(utilidades.convertDataTableToJson(tabla_resultado));
        }

        [Route("api/mascotas/getEnviarCertificado")]
        public IHttpActionResult getEnviarCertificado()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            IEnumerable<string> headerValues_id_mascota = Request.Headers.GetValues("id_mascota");
            string id_mascota = headerValues_id_mascota.FirstOrDefault().ToString();

            IEnumerable<string> headerValues_email = Request.Headers.GetValues("email");
            string email = headerValues_email.FirstOrDefault().ToString();

            List<object> arguments = new List<object>();

            arguments.Add(id_mascota);
            arguments.Add(email);
            bg_enviar_certificado.DoWork += new DoWorkEventHandler(enviar_certificado);
            bg_enviar_certificado.RunWorkerAsync(arguments);

            //utilidades.enviar_correo("HOLA", email, "Certificado de Mascota");

            return Json("correcto"); 
        }

        [Route("api/mascotas/getImprimirCertificado")]
        public IHttpActionResult getImprimirCertificado()
        {
            try
            {
                if (!utilidades.validar_token(Request))
                    return Json("incorrecto");

                IEnumerable<string> headerValues_id_mascota = Request.Headers.GetValues("id_mascota");
                string id_mascota = headerValues_id_mascota.FirstOrDefault().ToString();

                //Si el id de mascota no es un número, no hacemos nada. 
                int id_temporal;
                if (!int.TryParse(id_mascota.ToString(), out id_temporal))
                    return Json("incorrecto");


                //Obtenemos todos los datos del certificado. 
                string select_certificado = string.Format("select  " +
    "a.nombre as nombre_mascota " +
    ", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento_mascota " +
    ", a.genero as genero " +
    ", b.nombre as raza  " +
    ", b.talla as talla  " +
    ", c.nombre as tipo_de_mascota " +
    ", concat(d.nombres, ' ' , d.apellido_paterno,  ' ' ,d.apellido_materno) as nombre_completo_cliente " +
    ", d.nombres as nombre_cliente " +
    ", concat(d.calle, ' ', d.numero, ', Col. ' , d.colonia, ', Del. ', d.delegacion , ', ', h.nombre, '.') as direccion_cliente " +
    ", d.telefono_local as telefono_local_cliente " +
    ", d.telefono_celular as telefono_celular_cliente " +
    ", d.email as email_cliente  " +
    ", e.establecimiento as establecimiento  " +
    ", e.telefono_local as telefono_local_establecimiento " +
    ", e.telefono_local as telefono_celular_establecimiento " +
    ", concat(e.calle, ' ', e.numero, ', Col. ' , e.colonia, ', Del. ', e.delegacion , ', ', i.nombre, '.') as direccion_establecimiento " +
    ", f.foto_url as foto_marca " +
    ", f.color_marca as color_marca " +
    ", g.certificado_de as certificado_de " +
    " from  " +
    "lu_mascotas a  " +
    "LEFT JOIN lu_razas b on a.id_raza=b.id " +
    "LEFT JOIN lu_tipos_de_mascota c on b.id_tipo_de_mascota=c.id " +
    "LEFT JOIN lu_clientes d on a.id_cliente=d.id " +
    "LEFT JOIN lu_usuarios e on d.id_registrado_por=e.id " +
    "LEFT JOIN cf_marcas f on a.id_marca=f.id " +
    "LEFT JOIN cf_tipos_de_usuario g on e.id_tipo_de_usuario=g.id " +
    "LEFT JOIN cf_ciudades h on h.id=d.id_ciudad " +
    "LEFT JOIN cf_ciudades i on i.id=e.id_ciudad " +
    "where a.id='{0}'; "
    , id_mascota);

                DataTable tabla_certificado = Database.runSelectQuery(select_certificado);

                //Generamos el PDF.
                string contenido_html = "";

                contenido_html = string.Format("<!doctype html>" +
    "<html lang='es' style='background-color: {2};height: 100%;'>" +
    "   <head>      " +
    "      <meta charset='utf-8'>                  " +
    "      <link href='https://fonts.googleapis.com/css?family=Work+Sans' rel='stylesheet'>" +
    "      <link rel='stylesheet' href='{0}'>" +
    "   </head>" +
    "   <body style='background-color: {2};'>      " +
    "     <div style='padding-top: 100px;padding-left: 50px;padding-right: 50px;'>           " +
    "        <div style='background-color: white;border-radius: 50px;padding:40px;'>" +
    "            <div style='text-align:right;'>" +
    "              <img src='{1}' style='height: 50px; width:auto; '>" +
    "            </div>" +
    "            <br>" +
    "            <div class='letra_encabezado_naranja' style='color:{2}!important;text-align:right;font-size:30px;'>" +
    "              Certificado de {3}              " +
    "            </div>" +
    "            <br>" +
    "            <div class='letra_terminos_y_condiciones' style='font-size:22px;'>" +
    "              Estimado(a) <b>{4}</b>: " +
    "            </div>" +
    "            <br>" +
    "            <div class='letra_terminos_y_condiciones'>" +
    "              A continuación se muestran los datos de certificación de su mascota.               " +
    "            </div>" +
    "            <br>" +
    "            <br>" +
    "            <br>" +
    "            <div class='letra_encabezado_naranja' style='color:{2}!important; font-size: 22px;'>Información del Establecimiento</div>" +
    "            <br>" +
    "            <div class='letra_terminos_y_condiciones'>" +
    "              <div style='width:90%;display: inline-block; '>" +
    "                <b>Dirección:</b> {7}<br><br>" +
    "               </div> " +
    "            </div> " +
    "            <div class='letra_terminos_y_condiciones'>" +
    "              <div style='width:90%;display: inline-block; '>" +
    "                <b>Establecimiento:</b> {5} <br><br>" +
    "               </div> " +
    "            </div> " +
    "            <div class='letra_terminos_y_condiciones'>" +
    "              <div style='width:55%;display: inline-block; '>" +
    "                <b>Teléfono:</b> {6} <br>" +
    "              </div>" +
    "              <div style='width:40%; display: inline-block;text-align: left;'>" +
    "                <b>Celular:</b> {8} <br>                " +
    "              </div>                            " +
    "            </div> " +
    "            <br>" +
    "            <br>" +
    "            <div class='letra_encabezado_naranja' style='color:{2}!important; font-size: 22px;'>Información de Mascota</div>" +
    "            <br>" +
    "            <div class='letra_terminos_y_condiciones' >" +
    "              <div style='width:55%;display: inline-block; '>" +
    "                <b>Fecha de Nacimiento:</b> {9} <br><br>" +
    "                <b>Nombre:</b> {10} <br><br>" +
    "                <b>Género:</b> {11} <br>" +
    "              </div>" +
    "              <div style='width:40%; display: inline-block;text-align: left;'>" +
    "                <b>Raza:</b> {12} <br><br>" +
    "                <b>Talla:</b> {13} <br><br>" +
    "                <b>Tipo:</b> {14} <br>" +
    "              </div>                            " +
    "            </div>" +
    "            <br>" +
    "            <br>" +
    "            <div class='letra_encabezado_naranja' style='color:{2}!important;font-size: 22px;'>Información del Dueño</div>" +
    "            <br>" +
    "            <div class='letra_terminos_y_condiciones'>" +
    "              <div style='width:90%;display: inline-block; '>" +
    "                <b>Nombre Completo:</b> {15} <br><br>" +
    "               </div> " +
    "            </div> " +
    "            <div class='letra_terminos_y_condiciones'>" +
    "              <div style='width:90%;display: inline-block; '>" +
    "                <b>Dirección:</b> {16} <br><br>" +
    "               </div> " +
    "            </div> " +
    "            <div class='letra_terminos_y_condiciones'>" +
    "              <div style='width:55%;display: inline-block; '>" +
    "                <b>Teléfono:</b> {17} <br><br>" +
    "                <b>Celular:</b> {18} <br>" +
    "              </div>" +
    "              <div style='width:40%; display: inline-block;text-align: left;'>" +
    "                <b>Correo Electrónico:</b> {19} <br>" +
    "              </div>                            " +
    "            </div>" +
    "            <br>" +
    "            <br>" +
    "            <br>" +
    "            <div class='letra_terminos_y_condiciones' style='font-size: 12px;'>" +
    "              Royal Canin México S.A de C.V, con domicilio en Calle Lago Zúrich #245 Piso 12 Int. 04, utilizará sus datos personales recabados para:" +
    "Informar y recomendar adecuadamente sobre nuestros productos, puntos de venta, precios públicos, promociones e información de interés," +
    "informar sobre nuevos servicios y productos disponibles, asesoría nutricional y seguimiento durante las diferentes etapas de vida de las" +
    "mascotas. Comercialización, cobranza y surtido de productos. Para mayor información acerca del tratamiento y de los derechos que puede hacer" +
    "vales, usted puede acceder al aviso de privacidad integral a través del siguiente link (http://www.royalcanin.com.mx/pie-de-pagina/aviso-deprivacidad)" +
    "y también en el servicio de atención a clientes 01 800 024 7764." +
    "            </div>" +
    "        </div>" +
    "      </div>" +
    "    </body>" +
    "</html>"
    , "http://micrositioroyalcanin.com.mx/css/estilos.css"
    //, "http://localhost/48_horas/css/estilos.css"
    , tabla_certificado.Rows[0]["foto_marca"].ToString()
    , tabla_certificado.Rows[0]["color_marca"].ToString()
    , tabla_certificado.Rows[0]["certificado_de"].ToString()
    , tabla_certificado.Rows[0]["nombre_cliente"].ToString()
    , tabla_certificado.Rows[0]["establecimiento"].ToString()
    , tabla_certificado.Rows[0]["telefono_local_establecimiento"].ToString()
    , tabla_certificado.Rows[0]["direccion_establecimiento"].ToString()
    , tabla_certificado.Rows[0]["telefono_celular_establecimiento"].ToString()
    , tabla_certificado.Rows[0]["fecha_de_nacimiento_mascota"].ToString()
    , tabla_certificado.Rows[0]["nombre_mascota"].ToString()
    , tabla_certificado.Rows[0]["genero"].ToString()
    , tabla_certificado.Rows[0]["raza"].ToString()
    , tabla_certificado.Rows[0]["talla"].ToString()
    , tabla_certificado.Rows[0]["tipo_de_mascota"].ToString()
    , tabla_certificado.Rows[0]["nombre_completo_cliente"].ToString()
    , tabla_certificado.Rows[0]["direccion_cliente"].ToString()
    , tabla_certificado.Rows[0]["telefono_local_cliente"].ToString()
    , tabla_certificado.Rows[0]["telefono_celular_cliente"].ToString()
    , tabla_certificado.Rows[0]["email_cliente"].ToString());


                //Generamos el archivo PDF desde el HTML. 
                string filename = string.Format("certificado_{0}_{1}.pdf"
                    , id_mascota
                    , DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                utilidades.generar_pdf_con_html(contenido_html, filename);

                //Obtenmos el Base URL de descarga del PDF. 
                string baseUrl = "http://" + Request.Headers.Host + "/temp/" + filename;

                //Regresamos el URL donde se encuentra el PDF. 
                return Json(baseUrl);
            }
            catch(Exception ex)
            {
                return Json(ex.ToString()); 
            }
        }


        private void enviar_certificado(object sender, DoWorkEventArgs e)
        {

            ThreadStart threadStarter = () => { enviar_certificado_thread(sender, e); };
            Thread thread = new Thread(threadStarter);
            thread.Start();

        }

        private void enviar_certificado_thread(object sender, DoWorkEventArgs e)
        {
            List<object> argumentos = e.Argument as List<object>;
            string id_mascota = (string)argumentos[0];
            string email = "";
            try { email = (string)argumentos[1]; }
            catch { email = ""; }

            //Obtenemos todos los datos del certificado. 
            string select_certificado = string.Format("select  " +
"a.nombre as nombre_mascota " +
", DATE_FORMAT(a.fecha_de_nacimiento, '%d/%m/%Y') AS fecha_de_nacimiento_mascota " +
", a.genero as genero " +
", b.nombre as raza  " +
", b.talla as talla  " +
", c.nombre as tipo_de_mascota " +
", concat(d.nombres, ' ' , d.apellido_paterno,  ' ' ,d.apellido_materno) as nombre_completo_cliente " +
", d.nombres as nombre_cliente " +
", concat(d.calle, ' ', d.numero, ', Col. ' , d.colonia, ', Del. ', d.delegacion , ', ', h.nombre, '.') as direccion_cliente " +
", d.telefono_local as telefono_local_cliente " +
", d.telefono_celular as telefono_celular_cliente " +
", d.email as email_cliente  " +
", e.establecimiento as establecimiento  " +
", e.telefono_local as telefono_local_establecimiento " +
", e.telefono_local as telefono_celular_establecimiento " +
", concat(e.calle, ' ', e.numero, ', Col. ' , e.colonia, ', Del. ', e.delegacion , ', ', i.nombre, '.') as direccion_establecimiento " +
", f.foto_url as foto_marca " +
", f.color_marca as color_marca " +
", g.certificado_de as certificado_de " +
" from  " +
"lu_mascotas a  " +
"LEFT JOIN lu_razas b on a.id_raza=b.id " +
"LEFT JOIN lu_tipos_de_mascota c on b.id_tipo_de_mascota=c.id " +
"LEFT JOIN lu_clientes d on a.id_cliente=d.id " +
"LEFT JOIN lu_usuarios e on d.id_registrado_por=e.id " +
"LEFT JOIN cf_marcas f on a.id_marca=f.id " +
"LEFT JOIN cf_tipos_de_usuario g on e.id_tipo_de_usuario=g.id " +
"LEFT JOIN cf_ciudades h on h.id=d.id_ciudad " +
"LEFT JOIN cf_ciudades i on i.id=e.id_ciudad " +
"where a.id='{0}'; "
, id_mascota);

            DataTable tabla_certificado = Database.runSelectQuery(select_certificado);

            //Generamos el PDF.
            string contenido_html = "";

            contenido_html = string.Format("<!doctype html>" +
"<html lang='es' style='background-color: {2};height: 100%;'>" +
"   <head>      " +
"      <meta charset='utf-8'>                  " +
"      <link href='https://fonts.googleapis.com/css?family=Work+Sans' rel='stylesheet'>" +
"      <link rel='stylesheet' href='{0}'>" +
"   </head>" +
"   <body style='background-color: {2};'>      " +
"     <div style='padding-top: 100px;padding-left: 50px;padding-right: 50px;'>           " +
"        <div style='background-color: white;border-radius: 50px;padding:40px;'>" +
"            <div style='text-align:right;'>" +
"              <img src='{1}' style='height: 50px; width:auto; '>" +
"            </div>" +
"            <br>" +
"            <div class='letra_encabezado_naranja' style='color:{2}!important;text-align:right;font-size:30px;'>" +
"              Certificado de {3}              " +
"            </div>" +
"            <br>" +
"            <div class='letra_terminos_y_condiciones' style='font-size:22px;'>" +
"              Estimado(a) <b>{4}</b>: " +
"            </div>" +
"            <br>" +
"            <div class='letra_terminos_y_condiciones'>" +
"              A continuación se muestran los datos de certificación de su mascota.               " +
"            </div>" +
"            <br>" +
"            <br>" +
"            <br>" +
"            <div class='letra_encabezado_naranja' style='color:{2}!important; font-size: 22px;'>Información del Establecimiento</div>" +
"            <br>" +
"            <div class='letra_terminos_y_condiciones'>" +
"              <div style='width:90%;display: inline-block; '>" +
"                <b>Dirección:</b> {7}<br><br>" +
"               </div> " +
"            </div> " +
"            <div class='letra_terminos_y_condiciones'>" +
"              <div style='width:90%;display: inline-block; '>" +
"                <b>Establecimiento:</b> {5} <br><br>" +
"               </div> " +
"            </div> " +
"            <div class='letra_terminos_y_condiciones'>" +
"              <div style='width:55%;display: inline-block; '>" +
"                <b>Teléfono:</b> {6} <br>" +
"              </div>" +
"              <div style='width:40%; display: inline-block;text-align: left;'>" +
"                <b>Celular:</b> {8} <br>                " +
"              </div>                            " +
"            </div> " +
"            <br>" +
"            <br>" +
"            <div class='letra_encabezado_naranja' style='color:{2}!important; font-size: 22px;'>Información de Mascota</div>" +
"            <br>" +
"            <div class='letra_terminos_y_condiciones' >" +
"              <div style='width:55%;display: inline-block; '>" +
"                <b>Fecha de Nacimiento:</b> {9} <br><br>" +
"                <b>Nombre:</b> {10} <br><br>" +
"                <b>Género:</b> {11} <br>" +
"              </div>" +
"              <div style='width:40%; display: inline-block;text-align: left;'>" +
"                <b>Raza:</b> {12} <br><br>" +
"                <b>Talla:</b> {13} <br><br>" +
"                <b>Tipo:</b> {14} <br>" +
"              </div>                            " +
"            </div>" +
"            <br>" +
"            <br>" +
"            <div class='letra_encabezado_naranja' style='color:{2}!important;font-size: 22px;'>Información del Dueño</div>" +
"            <br>" +
"            <div class='letra_terminos_y_condiciones'>" +
"              <div style='width:90%;display: inline-block; '>" +
"                <b>Nombre Completo:</b> {15} <br><br>" +
"               </div> " +
"            </div> " +
"            <div class='letra_terminos_y_condiciones'>" +
"              <div style='width:90%;display: inline-block; '>" +
"                <b>Dirección:</b> {16} <br><br>" +
"               </div> " +
"            </div> " +
"            <div class='letra_terminos_y_condiciones'>" +
"              <div style='width:55%;display: inline-block; '>" +
"                <b>Teléfono:</b> {17} <br><br>" +
"                <b>Celular:</b> {18} <br>" +
"              </div>" +
"              <div style='width:40%; display: inline-block;text-align: left;'>" +
"                <b>Correo Electrónico:</b> {19} <br>" +
"              </div>                            " +
"            </div>" +
"            <br>" +
"            <br>" +
"            <br>" +
"            <div class='letra_terminos_y_condiciones' style='font-size: 12px;'>" +
"              Royal Canin México S.A de C.V, con domicilio en Calle Lago Zúrich #245 Piso 12 Int. 04, utilizará sus datos personales recabados para:" +
"Informar y recomendar adecuadamente sobre nuestros productos, puntos de venta, precios públicos, promociones e información de interés," +
"informar sobre nuevos servicios y productos disponibles, asesoría nutricional y seguimiento durante las diferentes etapas de vida de las" +
"mascotas. Comercialización, cobranza y surtido de productos. Para mayor información acerca del tratamiento y de los derechos que puede hacer" +
"vales, usted puede acceder al aviso de privacidad integral a través del siguiente link (http://www.royalcanin.com.mx/pie-de-pagina/aviso-deprivacidad)" +
"y también en el servicio de atención a clientes 01 800 024 7764." +
"            </div>" +
"        </div>" +
"      </div>" +
"    </body>" +
"</html>"
                , "http://micrositioroyalcanin.com.mx/css/estilos.css"
                //, "http://localhost/48_horas/css/estilos.css"
, tabla_certificado.Rows[0]["foto_marca"].ToString()
, tabla_certificado.Rows[0]["color_marca"].ToString()
, tabla_certificado.Rows[0]["certificado_de"].ToString()
, tabla_certificado.Rows[0]["nombre_cliente"].ToString()
, tabla_certificado.Rows[0]["establecimiento"].ToString()
, tabla_certificado.Rows[0]["telefono_local_establecimiento"].ToString()
, tabla_certificado.Rows[0]["direccion_establecimiento"].ToString()
, tabla_certificado.Rows[0]["telefono_celular_establecimiento"].ToString()
, tabla_certificado.Rows[0]["fecha_de_nacimiento_mascota"].ToString()
, tabla_certificado.Rows[0]["nombre_mascota"].ToString()
, tabla_certificado.Rows[0]["genero"].ToString()
, tabla_certificado.Rows[0]["raza"].ToString()
, tabla_certificado.Rows[0]["talla"].ToString()
, tabla_certificado.Rows[0]["tipo_de_mascota"].ToString()
, tabla_certificado.Rows[0]["nombre_completo_cliente"].ToString()
, tabla_certificado.Rows[0]["direccion_cliente"].ToString()
, tabla_certificado.Rows[0]["telefono_local_cliente"].ToString()
, tabla_certificado.Rows[0]["telefono_celular_cliente"].ToString()
, tabla_certificado.Rows[0]["email_cliente"].ToString());


            //Generamos el archivo PDF desde el HTML. 
            string filename = string.Format("certificado_{0}_{1}.pdf"
                , id_mascota
                , DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            utilidades.generar_pdf_con_html(contenido_html, filename);

            //Obtenmos el Base URL de descarga del PDF. 
            string baseUrl = "http://" + Request.Headers.Host + "/temp/" + filename;

            string mensaje_html = string.Format(
"   <div style='background-color: {2};'>" +
"      <div>" +
"         <div style='padding-top:80px;padding-bottom:80px;padding-left:10vw;padding-right:10vw;'>" +
"           <div></div>" +
"            <div style='background-color: white;border-radius: 50px;padding-top:10vh;padding-bottom:10vh;padding-right:5vw;padding-left:5vw;'>" +
"              <div>" +
"                <div style='color: {2};font-size: 25px;font-weight: 700;text-align: center;'>Certificado de Mascota</div><br>" +
"                <div style='font-size: 20px; text-align: justify;'>" +
"                  <div>" +
"                    <div style='text-align: right;font-weight: bold;'>Estimado(a) {0}:</div><br>" +
"                    <div>A continuación encontrarás el certificado de tu mascota.</div><br><br>" +
"                    <div><b>Da click en el siguiente enlace para descargar:</b><br> <a href='{1}'>{1}</a></div><br>" +
"                    <div style='text-align: center; font-weight: bold;'>¡Te deseamos un excelente día!</div><br>" +
"                    <div style='text-align: right;'>" +
//"                    <img src='{3}' style='margin-bottom:0px;height: auto; width:30vw;max-width:250px; '>" +
"                    </div>                    " +
"                    <div style='text-align: right; font-weight: bold;margin-top: 0px;'>Atención a Clientes " +
"                    </div>" +
"                  </div>" +
"                </div>" +
"                <div></div><br>" +
"              </div>" +
"            </div>" +
"          </div>" +
"        </div>" +
"      </div>"
, tabla_certificado.Rows[0]["nombre_cliente"].ToString()
, baseUrl
, tabla_certificado.Rows[0]["color_marca"].ToString()
, tabla_certificado.Rows[0]["foto_marca"].ToString());

            //string.Format("Descarga: <a href='{0}' target='_blank'>Click</a>", baseUrl);
            email = (email == "") ? tabla_certificado.Rows[0]["email_cliente"].ToString() : email;
            //Enviamos un correo electrónico al dueño. 
            utilidades.enviar_correo(mensaje_html, email, "Certificado de Mascota");
        }


        private bool validacion(JObject json)
        {
            if (!utilidades.validar_token(Request))
                return false;
            return true;
        }

        // PUT api/mascotas/5
        public IHttpActionResult Post(int id, [FromBody]Object value)
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
           ",esterilizado='{5}' " +
           ",id_marca='{6}' " +
           "where id='{7}'"
           , json["nombre"]
           , json["fecha_de_nacimiento"]
           , json["genero"]
           , json["id_raza"]
           , json["id_cliente"]           
           , json["esterilizado"]
           , json["id_marca"]
           , id);





            //Contestamos con el id del nuevo registro.
            if (Database.runQuery(update_query))
                return Json("correcto");
            else
                return Json("incorrecto");
        }

        [Route("api/mascotas/uploadImage/{id}")]

        public IHttpActionResult uploadImage(int id, [FromBody]Object value)
        {
            //Tomar en cuenta que las fechas vienen en el formato YYYY-MM-dd
            JObject json = JObject.Parse(value.ToString());


            if (!validacion(json))
                return Json("incorrecto");

            string filename = string.Format("{0}.jpg", id); 
            utilidades.guardar_imagen(json["foto_url"].ToString(), "mascotas", filename);

            string foto_url =  "http://" + Request.Headers.Host + "/temp/mascotas/" + filename;
            foto_url += "?fecha=" + DateTime.Now.ToString("ddMMyyyy_HHmmss"); 

            //Actualizamos el campo de foto_url de la mascota.             
            string update_query = string.Format("UPDATE `lu_mascotas` " +
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
