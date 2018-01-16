using System.Collections.Generic;
using System.Web.Http;

namespace Hasin.Taaghche.Probes.Controller.v1
{
    [RoutePrefix("v1/monitor")]
    public class MonitorController : ApiController
    {
        // GET api/values 
        [HttpGet]
        [Route("sitemap")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [Route("sitemap/{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values 
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5 
        public void Delete(int id)
        {
        }
    }
}
