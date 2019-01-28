using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WeApi.Models;
using System.Data;

namespace WeApi.Controllers
{
    public class PersonasController : ApiController
    {

        Carro[] carros = new Carro[]{
       
       new Carro{idcarro=1,marca="Ferrari",modelo=2012},
       new Carro{idcarro=2,marca="BMW",modelo=2010},
       new Carro{idcarro=3,marca="Mazda",modelo=2002},
       new Carro{idcarro=4,marca="Nizzan",modelo=2004},
       new Carro{idcarro=5,marca="Renault",modelo=1998}
       
       };

        
        public IHttpActionResult GetAllProductos()
        {
            return Json("kevin"); 
            /*string select_query = "SELECT * from lu_productos;";
            DataTable tabla = Database.runSelectQuery(select_query);
            string json = utilidades.convertDataTableToJson(tabla);
            return Json(json); */
        }


        public IHttpActionResult GetProducto(int papas) {
            var carro = carros.FirstOrDefault((c) => c.idcarro == papas);
            if (carro!=null)
            {
                return Ok(carro);
            }
            else
            {
                return NotFound();
            }
        }



    }
}
