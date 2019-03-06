using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WeApi.Controllers
{
    public class descargasController : ApiController
    {
        // GET api/clientesdescargas        
        [Route("api/descargas/getClientes")]
        public IHttpActionResult getClientes()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            DataTable dt = Database.runSelectQuery("select a.id " +
            ", a.nombres " +
            ", a.apellido_paterno " +
            ", a.apellido_materno " +
            ", a.telefono_local " +
            ", a.telefono_celular " +
            ", b.nombre as ciudad " +
            ", a.delegacion " +
            ", a.colonia " +
            ", a.calle " +
            ", a.numero " +
            ", a.fecha_de_nacimiento " +
            ", a.email " +
            ", concat(d.nombres, ' ' , d.apellido_paterno, ' ' , d.apellido_materno) as registrado_por " +
            ", d.email as email_registrado_por " +
            ", c.nombre as estado " +
            "from lu_clientes a " +
            "left join cf_ciudades b on a.id_ciudad=b.id " +
            "left join cf_estados_de_cliente c on c.id=a.id_estado " +
            "left join lu_usuarios d on d.id=a.id_registrado_por  " +
            "where a.estado=1 " +
            "order by a.nombres asc;  ");
            IWorkbook workbook = GenerateExcelFile(dt);

            var fileName = "exportacion_clientes_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";

            //save the file to server temp folder
            string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), fileName);

            using (var exportData = new MemoryStream())
            {
                //I don't show the detail how to create the Excel, this is not the point of this article,
                //I just use the NPOI for Excel handler
                FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                workbook.Write(file);
                file.Close();
            }


            //Convirtiendo el archivo 
            var dataBytes = File.ReadAllBytes(fullPath);
            //adding bytes to memory stream   
            var dataStream = new MemoryStream(dataBytes);

            

            //Comenzamos la descarga. 
            return new Descarga(dataStream, Request, fileName);

        }

        [Route("api/descargas/getMascotas")]
        public IHttpActionResult getMascotas()
        {
            if (!utilidades.validar_token(Request))
                return Json("incorrecto");

            string select_query = "select  " +
            "a.id " +
            ", a.nombre " +
            ", a.fecha_de_nacimiento " +
            ", a.fecha_de_registro as fecha_de_emision_de_certificado " +
            ", a.fecha_de_modificacion as fecha_de_modificacion_de_certificado " +
            ", a.genero " +
            ", a.esterilizado " +
            ", d.nombre as tipo  " +
            ", b.nombre as raza " +
            ", b.talla talla " +            
            ", concat(c.nombres, ' ', c.apellido_paterno, ' ', c.apellido_materno) as dueño " +
            ", c.email as email_dueño " +
            ", c.telefono_local as telefono_local_dueño " +
            ", c.telefono_celular as telefono_celular_dueño " +
            ", concat(e.nombres, ' ', e.apellido_paterno, ' ', e.apellido_materno) as criador " +
            ", e.email as email_criador " +
            ", f.nombre as zona " +
            ", (CASE   " +
            "		when e.micrositio=1 then 'Sí'   " +
            "		else 'No'   " +
            "	end      " +
            " ) as micrositio    " +
            ", (CASE   " +
            "		when e.c48_horas=1 then 'Sí'   " +
            "		else 'No'   " +
            "	end      " +
            " ) as c48_horas    " +
            "from  " +
            "lu_mascotas a " +
            "left join lu_razas b on b.id=a.id_raza " +
            "left join lu_clientes c on c.id=a.id_cliente " +
            "left join lu_tipos_de_mascota d on b.id_tipo_de_mascota=d.id " +
            "left join lu_usuarios e on e.id=c.id_registrado_por " +
            "left join cf_ciudades f on e.id_ciudad=f.id " +
            "where a.estado=1 " +
            "order by a.nombre asc;"; 

            DataTable dt = Database.runSelectQuery(select_query);
            IWorkbook workbook = GenerateExcelFile(dt);

            var fileName = "exportacion_mascotas_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";

            //save the file to server temp folder
            string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), fileName);

            using (var exportData = new MemoryStream())
            {
                //I don't show the detail how to create the Excel, this is not the point of this article,
                //I just use the NPOI for Excel handler
                FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                workbook.Write(file);
                file.Close();
            }


            //Convirtiendo el archivo 
            var dataBytes = File.ReadAllBytes(fullPath);
            //adding bytes to memory stream   
            var dataStream = new MemoryStream(dataBytes);

            //Comenzamos la descarga. 
            return new Descarga(dataStream, Request, fileName);

        }

        [Route("api/descargas/getUsuarios")]
        public IHttpActionResult getUsuarios()
        {
            try
            {


                if (!utilidades.validar_token(Request))
                    return Json("incorrecto");

                IEnumerable<string> headerValues_id_usuario = Request.Headers.GetValues("id_usuario");
                string id_usuario = headerValues_id_usuario.FirstOrDefault().ToString();

                IEnumerable<string> headerValues_id_tipo_de_usuario = Request.Headers.GetValues("id_tipo_de_usuario");
                string id_tipo_de_usuario = headerValues_id_tipo_de_usuario.FirstOrDefault().ToString();

                IEnumerable<string> headerValues_modo = Request.Headers.GetValues("modo");
                string modo = headerValues_modo.FirstOrDefault().ToString();

                IEnumerable<string> headerValues_id_distribuidor = Request.Headers.GetValues("id_distribuidor");
                string id_distribuidor = headerValues_id_distribuidor.FirstOrDefault().ToString();

                //En caso de ser un distribuidor, solo le mostraremos los usuarios que le corresponden a él.             
                string condicion_distribuidor = id_tipo_de_usuario == "5" ? string.Format(" and id_distribuidor='{0}' ", id_distribuidor) : "";

                //Condición para mostrar solo a los clientes activos. 
                string condicion_administrador = id_tipo_de_usuario != "1" ? " and a.id_estado_de_usuario=1 " : "";

                string condicion_modo = "";
                if (modo == "solicitados")
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
                if ((id_tipo_de_usuario != "1") && (id_tipo_de_usuario != "5"))
                    return Json("Incorrecto"); 

                string select_query = string.Format("select " +
                                "a.id " +
                                ", a.nombres " +
                                ", a.apellido_paterno " +
                                ", a.apellido_materno " +
                                ", a.telefono_local " +
                                ", a.telefono_celular " +
                                ", a.calle " +
                                ", a.numero " +
                                ", a.delegacion " +
                                ", a.colonia " +
                                ", a.fecha_de_nacimiento " +
                                ", b.nombre as zona  " +
                                ", a.email " +
                                ", a.secret as pass " +
                                ", c.nombre as tipo_de_usuario " +
                                ", a.establecimiento " +                                
                                ", (CASE   " +
                                "		when a.micrositio=1 then 'Sí'   " +
                                "		else 'No'   " +
                                "	end      " +
                                " ) as micrositio    " +
                                ", (CASE   " +
                                "		when a.c48_horas=1 then 'Sí'   " +
                                "		else 'No'   " +
                                "	end      " +
                                " ) as c48_horas    " +
                                ", (CASE   " +
                                "		when a.acuerdos_leidos=1 then 'Sí'   " +
                                "		else 'No'   " +
                                "	end      " +
                                " ) as acuerdos_leidos    " +
                                ", d.nombre as marca_recomendada  " +
                                ", count(e.id) as cantidad_de_clientes " +
                                "from lu_usuarios a " +
                                "left join cf_ciudades b on a.id_ciudad=b.id " +
                                "left join cf_tipos_de_usuario c on c.id=a.id_tipo_de_usuario " +
                                "left join cf_marcas d on d.id=a.id_marca " +
                                "left join lu_clientes e on e.id_registrado_por=a.id " +
                                "where a.estado=1 {0} {1} {2} group by a.id " +                                
                                "order by a.nombres;  "
                                , condicion_distribuidor
                                , condicion_modo
                                , condicion_administrador); 
                DataTable dt = Database.runSelectQuery(select_query);
                IWorkbook workbook = GenerateExcelFile(dt);

                var fileName = "exportacion_usuarios_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";

                //save the file to server temp folder
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), fileName);

                using (var exportData = new MemoryStream())
                {
                    //I don't show the detail how to create the Excel, this is not the point of this article,
                    //I just use the NPOI for Excel handler
                    FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                    workbook.Write(file);
                    file.Close();
                }


                //Convirtiendo el archivo 
                var dataBytes = File.ReadAllBytes(fullPath);
                //adding bytes to memory stream   
                var dataStream = new MemoryStream(dataBytes);

                //Comenzamos la descarga. 
                return new Descarga(dataStream, Request, fileName);
            }
            catch(Exception e)
            {
                return Json(e.ToString()); 
            }

        }

        [Route("api/descargas/getKits")]
        public IHttpActionResult getKits()
        {
            try
            {


                if (!utilidades.validar_token(Request))
                    return Json("incorrecto");
                string select_query = string.Format("select  " +
                            " a.id " +
                            ", concat(b.nombres, ' ' , b.apellido_paterno, ' ' , b.apellido_materno) as solicitado_por " +
                            ", a.cantidad  " +
                            ", c.nombre as raza " +
                            ", c.talla as talla " +
                            ", DATE_FORMAT(a.fecha_de_registro, '%d/%m/%Y') AS fecha_de_solicitud " +
                            ", DATE_FORMAT(a.fecha_de_nacimiento_camada, '%d/%m/%Y') AS fecha_de_nacimiento_camada " +
                            ", e.nombre as estado_de_kit " +
                            ", b.email as email_solicitado_por " +  
                            "from  " +
                            "ft_kits a  " +
                            "LEFT JOIN lu_usuarios b on a.id_solicitado_por=b.id " +
                            "LEFT JOIN lu_razas c on c.id=a.id_raza  " +
                            "LEFT JOIN lu_tipos_de_mascota d on d.id=c.id_tipo_de_mascota  " +
                            "LEFT JOIN cf_estados_de_kit e on e.id=a.id_estado_de_kit " +
                             "where a.estado=1 " +
                             "order by a.fecha_de_modificacion desc;  "); 
                DataTable dt = Database.runSelectQuery(select_query);
                IWorkbook workbook = GenerateExcelFile(dt);

                var fileName = "exportacion_kits_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".xls";

                //save the file to server temp folder
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/temp"), fileName);

                using (var exportData = new MemoryStream())
                {
                    //I don't show the detail how to create the Excel, this is not the point of this article,
                    //I just use the NPOI for Excel handler
                    FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                    workbook.Write(file);
                    file.Close();
                }


                //Convirtiendo el archivo 
                var dataBytes = File.ReadAllBytes(fullPath);
                //adding bytes to memory stream   
                var dataStream = new MemoryStream(dataBytes);

                //Comenzamos la descarga. 
                return new Descarga(dataStream, Request, fileName);
            }
            catch (Exception e)
            {
                return Json(e.ToString());
            }

        }


        public IWorkbook GenerateExcelFile(DataTable dt)
        {
            IWorkbook workbook;
            workbook = new HSSFWorkbook();


            ISheet sheet1 = workbook.CreateSheet("Sheet 1");

            //make a header row
            IRow row1 = sheet1.CreateRow(0);

            for (int j = 0; j < dt.Columns.Count; j++)
            {

                ICell cell = row1.CreateCell(j);
                String columnName = dt.Columns[j].ToString();
                cell.SetCellValue(columnName);
            }

            //loops through data
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet1.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {

                    ICell cell = row.CreateCell(j);
                    String columnName = dt.Columns[j].ToString();
                    cell.SetCellValue(dt.Rows[i][columnName].ToString());
                }
            }
            return workbook;
            // Declare one MemoryStream variable for write file in stream  
            /*var stream = new MemoryStream();
            workbook.Write(stream);

            string FilePath = "archivo001";

            //Write to file using file stream  
            FileStream file = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
            stream.WriteTo(file);
            file.Close();
            stream.Close();*/
        }
    }
}