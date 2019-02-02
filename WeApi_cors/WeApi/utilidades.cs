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

        //recuerda que en utilidades tendremos todas las funciones y variables que utilizaremos en el esistema. 
        //Aquí pondemos las variables para controlar la paginación. Siempre hay que poner la cantidad deseada + 1 
        static public int elementos_por_pagina = 6; 
    }
}
