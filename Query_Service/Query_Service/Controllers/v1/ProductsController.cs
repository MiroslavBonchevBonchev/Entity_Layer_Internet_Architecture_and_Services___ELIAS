using Asp.Versioning;
using Data_Messages;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Query_Service.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    public class ProductsController : ControllerBase
    {
        private readonly IPublishEndpoint _endpoint;
        public ProductsController( IPublishEndpoint endpoint )
        {
            _endpoint = endpoint;
        }

        // GET: api/<ProductsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1 api v1", "value2 api v1" };
        }

        // GET api/<ProductsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value api v1";
        }

        // POST api/<ProductsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ProductsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProductsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            // await
            _endpoint.Publish( new MessageTestForward { Id = new Guid(), SendTime_UTC = DateTime.UtcNow } /* , cancellation token */ );
        }
    }
}
