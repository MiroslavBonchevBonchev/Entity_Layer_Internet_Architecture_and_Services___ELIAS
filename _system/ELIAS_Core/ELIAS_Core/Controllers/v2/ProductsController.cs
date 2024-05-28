using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ELIAS_Core.Controllers
{
   [Route( "api/[controller]" )]
   [ApiController]
   [Authorize]
   [ApiVersion( "2.0" )]
   public class ProductsController : ControllerBase
   {
      // GET: api/<ProductsController>
      [HttpGet]
      public IEnumerable<string> Get()
      {
         return new string[] { "value1 api v2", "value2 api v2" };
      }

      // GET api/<ProductsController>/5
      [HttpGet( "{id}" )]
      public string Get( int id )
      {
         return "value api v2";
      }

      // POST api/<ProductsController>
      [HttpPost]
      public void Post( [FromBody] string value )
      {
      }

      // PUT api/<ProductsController>/5
      [HttpPut( "{id}" )]
      public void Put( int id, [FromBody] string value )
      {
      }

      // DELETE api/<ProductsController>/5
      [HttpDelete( "{id}" )]
      public void Delete( int id )
      {
      }
   }
}
