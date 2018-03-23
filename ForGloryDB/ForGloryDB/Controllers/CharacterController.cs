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
    public class CharacterController : Controller
    {
        private ForGloryContext _context;

        public CharacterController(ForGloryContext context)
        {
            _context = context;
        }
        [HttpGet("{username}")]
        public IActionResult GetUserChars(string username)
        {
            if (username == "")
                return BadRequest();
            var data = _context.Character.Where(e => e.Username.Equals(username));
            if (data == null)
                return NotFound();
            if(data.ToArray().Length == 0)
                return NoContent();
            return new ObjectResult(data);
        }
        [HttpPut("{name}/{username}")]
        public IActionResult UpdateChar([FromBody] Character ch,string name, string username)
        {
            if (name == "" || username == "")
                return BadRequest();
            var data = _context.Character.FirstOrDefault(e => e.Name.Equals(name));
            if (!data.Username.Equals(username))
                return BadRequest();
            data.CharacterType = ch.CharacterType;
            data.Name = ch.Name;
            _context.Character.Update(data);
            _context.SaveChanges();
            return Accepted();
            
        }
        [HttpPut("{name}")]
        public IActionResult UpdateCharLocation([FromBody] double[] ch, string name)
        {
            if (name == "")
                return BadRequest();
            var data = _context.Character.FirstOrDefault(e => e.Name.Equals(name));
            if (data != null)
                return NotFound();
            data.X = ch[0];
            data.Y = ch[1];
            data.Z = ch[2];
            _context.Character.Update(data);
            _context.SaveChanges();
            return Accepted();
        }
        [HttpPost]
        public IActionResult CreatCharacter([FromBody] Character chr)
        {
            if (chr.Slot > 2 || chr.X != 0 || chr.Y != 0 || chr.Z != 0)
                return StatusCode(418);
            if(_context.Character.FirstOrDefault(e => e.Name.Equals(chr.Name)) != null || _context.Character.FirstOrDefault(e => e.Username.Equals(chr.Slot))!=null)
                return StatusCode(409);
            _context.Character.Add(chr);
            _context.SaveChanges();
            return Accepted();
        }
        [HttpDelete("{name}")]
        public IActionResult DeleteCharacter(string name)
        {
            if(name == "")
                return BadRequest();
            var data = _context.Character.FirstOrDefault(e => e.Name.Equals(name));
            if(data == null)
                return NotFound();
            _context.Character.Remove(data);
            _context.SaveChanges();
            return Ok();
        }
    }
}