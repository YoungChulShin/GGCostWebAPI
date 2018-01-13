using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GGCostWebAPI.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace GGCostWebAPI.Controllers
{
    [Route("api/test")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            GoogleDrive googleDrive = new GoogleDrive();
            googleDrive.InsertFiles ("e:\\db.sqlite");
            System.Threading.Thread.Sleep(5000);
            return new string[] { "Fender", "Gibson" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            System.Threading.Thread.Sleep(6000);
            return $"The value you sent was: {id} ";
        }
    }
}
