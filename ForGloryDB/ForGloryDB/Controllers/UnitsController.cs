using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForGloryDB.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ForGloryDB.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class UnitsController : Controller
    {
        private ForGloryContext _context;

        public UnitsController(ForGloryContext context)
        {
            _context = context;
        }

        [HttpGet("inuse/{username}")]
        public IActionResult GetCharUnitsInUse(string username)
        {
            if (username == "")
                return BadRequest();
            var data = _context.Unit.Where(e => e.NameCharacter.Equals(username) && e.InUse==true);
            if (data == null)
                return NotFound();
            if (data.ToArray().Length == 0)
                return NoContent();
            return new ObjectResult(data);
        }
        [HttpGet("{username}")]
        public IActionResult GetCharUnits(string username)
        {
            if (username == "")
                return BadRequest();
            var data = _context.Unit.Where(e => e.NameCharacter.Equals(username));
            if (data == null)
                return NotFound();
            if (data.ToArray().Length == 0)
                return NoContent();
            return new ObjectResult(data);
        }
    }
}