using System;
using System.Web.Script.Serialization; 
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Net.Mail;

namespace WeApi.Controllers
{
    class utilidades
    {
        public static string convertDataTableToJson(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    //Hacemos el pase del datetime
                    if(col.DataType==typeof(DateTime))
                    {
                        try
                        { childRow.Add(col.ColumnName, DateTime.Parse(row[col].ToString()).ToString("yyyy-MM-dd")); }
                        catch { childRow.Add(col.ColumnName, DateTime.Now.ToString("yyyy-MM-dd")); }
                    }
                        
                    else
                        childRow.Add(col.ColumnName, row[col]);
                     
                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);  
        }

        internal static bool validar_token(System.Net.Http.HttpRequestMessage Request)
        {
            return true; 
            IEnumerable<string> headerValues = Request.Headers.GetValues("token");
            string token = headerValues.FirstOrDefault().ToString();

            IEnumerable<string> headerValues2 = Request.Headers.GetValues("id_usuario");
            string id_usuario = headerValues2.FirstOrDefault().ToString();

            if (token == "kevin")
                return true;
            else
                return false; 

        }

        public static void EnviarCorreo(DataTable mensaje, string direccion)
        {

            leer_accesos_correo();
            if (enviar_correos == "false")
            {
                return;
            }

            MailMessage mail = new MailMessage();
            //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            SmtpClient SmtpServer = new SmtpClient(utilidades.smtp_server);
            mail.From = new MailAddress(utilidades.mail_user);
            mail.To.Add(direccion);
            mail.Subject = "Prueba de mails";
            mail.Body = utilidades.convertDataTableToJson(mensaje);
            mail.IsBodyHtml = true;

            SmtpServer.Port = 587;
            //SmtpServer.Credentials = new System.Net.NetworkCredential("knrosas@gmail.com", "contraseña");
            SmtpServer.Credentials = new System.Net.NetworkCredential(utilidades.mail_user, utilidades.mail_password);
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
            //MessageBox.Show("Correo enviado");
        }

        private static void leer_accesos_correo()
        {
            mail_user = "testingxiksolutions@gmail.com";
            enviar_correos = "true";
            smtp_server = ("smtp.gmail.com");
            mail_password = "Conker1234";
            //throw new NotImplementedException();

        }

        //recuerda que en utilidades tendremos todas las funciones y variables que utilizaremos en el esistema. 
        //Aquí pondemos las variables para controlar la paginación. Siempre hay que poner la cantidad deseada + 1 
        static public int elementos_por_pagina = 6;
        private static string smtp_server;
        private static string enviar_correos;
        private static string mail_user;
        private static string mail_password;

        public static object MessageBox { get; private set; }
    }
}
