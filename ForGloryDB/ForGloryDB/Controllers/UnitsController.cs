using System;
using System.Collections.Generic;
using System.IO;
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

        public class UnitInfo
        {
            public string type { get; set; }
            public string charname { get; set; }
        }

        public UnitsController(ForGloryContext context)
        {
            _context = context;
        }

        [HttpGet("inuse/{username}")]
        public IActionResult GetCharUnitsInUse(string username)
        {
            if (username == "")
                return BadRequest();
            var data = _context.Unit.Where(e => e.NameCharacter.Equals(username) && e.Slot != -1);
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
        [HttpPost]
        public IActionResult CreateUnit()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEnd().Split("|");
                var player = _context.Character.FirstOrDefault(x => x.Name.Equals(body[1]));
                if(player == null)
                    return NotFound();
                if(player.Gold < int.Parse(body[2]))
                    return StatusCode(409);
                player.Gold -= int.Parse(body[2]);
                Unit unit = new Unit(body[0], body[1]);
                if (unit == null)
                    return NotFound();
                _context.Unit.Add(unit);
                _context.Character.Update(player);
                _context.SaveChanges();
                return new ObjectResult(unit);
            }
        }
        [HttpPut("progress/{username}/{pos}")]
        public IActionResult SetUnitProgress([FromBody] Unit ch, string username, int pos = -1)
        {
            if (username == "" || pos < 0 && pos >= 3)
                return BadRequest();
            var data = _context.Unit.FirstOrDefault(e => e.NameCharacter.Equals(username) && e.Slot == pos);
            if (data == null)
                return NotFound();
            data.Level = ch.Level;
            data.Progress = ch.Progress;
            _context.Unit.Update(data);
            _context.SaveChanges();
            return new ObjectResult(data);
        }
        [HttpPut("inuse/{id}/{pos}")]
        public IActionResult SetUnityInUse(int id = -1, int pos = -2)
        {
            if (id == -1 || pos == -2)
                return BadRequest();
            var data = _context.Unit.FirstOrDefault(e => e.Id == id);
            if (data == null)
                return NotFound();
            data.Slot = pos;
            _context.Unit.Update(data);
            _context.SaveChanges();
            return new ObjectResult(data);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteUnit(int id)
        {
            if (id == -1)
                return BadRequest();
            var data = _context.Unit.FirstOrDefault(e => e.Id == id);
            if (data == null)
                return NotFound();
            _context.Unit.Remove(data);
            _context.SaveChanges();
            return Ok();
        }
    }
}