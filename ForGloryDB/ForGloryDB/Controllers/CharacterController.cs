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
        public class Stats
        {
            public int STR { get; set; } = 0;
            public int CONS { get; set; } = 0;
            public int DEF { get; set; } = 0;
        }
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
        [HttpPut]
        public IActionResult UpdateChar([FromBody] Character ch)
        {
            if (ch == null)
                return BadRequest();
            var data = _context.Character.FirstOrDefault(e => e.Name.Equals(ch.Name));
            if (data == null)
                return NotFound();
            if (!data.Username.Equals(ch.Username))
                return BadRequest();
            data.CharacterType = ch.CharacterType;
            data.Name = ch.Name;
            _context.Character.Update(data);
            _context.SaveChanges();
            return Accepted();
            
        }
        [HttpPut("{name}")]
        public IActionResult UpdateCharStats([FromBody] Stats stats, string name)
        {
            if (name == "")
                return BadRequest();
            var data = _context.Character.FirstOrDefault(e => e.Name.Equals(name));
            if (data == null)
                return NotFound();
            if (data.Str+stats.STR + data.Cons+stats.CONS + data.Def + stats.DEF > 30 + 2 * data.Level)
                return StatusCode(409);
            data.Str += stats.STR;
            data.Cons += stats.CONS;
            data.Def += stats.DEF;
            _context.Character.Update(data);
            _context.SaveChanges();
            string msg = "UPDATESTATS|" + data.Str + "|" + data.Cons + "|" + data.Def;
            return new ObjectResult(msg);
        }
        [HttpPut("progress/{name}")]
        public IActionResult UpdateCharProgress([FromBody] Character ch, string name)
        {
            if (name == "")
                return BadRequest();
            var data = _context.Character.FirstOrDefault(e => e.Name.Equals(name));
            if (data == null)
                return NotFound();
            data.Progress = ch.Progress;
            data.Level = ch.Level;
            data.Gold += ch.Gold;
            _context.Character.Update(data);
            _context.SaveChanges();
            return new ObjectResult(data);
        }



        [HttpDelete("reset/{name}")]
        public IActionResult ResetStats(string name)
        {
            if (name == "")
                return BadRequest();
            var data = _context.Character.FirstOrDefault(e => e.Name.Equals(name));
            if (data != null)
                return NotFound();
            data.Str = 10;
            data.Cons = 10;
            data.Def = 10;
            _context.Character.Update(data);
            _context.SaveChanges();
            return Accepted();
        }
        [HttpPost]
        public IActionResult CreatCharacter([FromBody] Character chr)
        {
            if (chr.Slot > 2 || chr.Str != 10 || chr.Cons != 10 || chr.Def != 10)
                return StatusCode(418);
            if(_context.Character.FirstOrDefault(e => e.Name.Equals(chr.Name)) != null || _context.Character.FirstOrDefault(e => e.Slot.Equals(chr.Slot) && e.Username.Equals(chr.Name)) != null)
                return StatusCode(409);
            _context.Character.Add(chr);
            switch(chr.CharacterType)
            {
                case "generic_knight":
                    _context.Items.Add(new Items(100, chr.CharacterType, chr.Name));
                    _context.Items.Add(new Items(800, "Sheald", chr.Name));
                    break;
                case "generic_swordsman":
                    _context.Items.Add(new Items(200, chr.CharacterType, chr.Name));
                    break;
                case "generic_two_hands_swordsman":
                    _context.Items.Add(new Items(300, chr.CharacterType, chr.Name));
                    break;
            }
            _context.Items.Add(new Items(400, "Armour", chr.Name));
            _context.Items.Add(new Items(500, "Boots", chr.Name));
            _context.Items.Add(new Items(600, "Gloves", chr.Name));
            _context.Items.Add(new Items(700, "Helmet", chr.Name));
            
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
            foreach(Unit unit in _context.Unit.Where(x => x.NameCharacter == data.Name))
            {
                _context.Items.RemoveRange(_context.Items.Where(x => x.UnitId == unit.Id));
                _context.Unit.Remove(unit);
            }
            _context.Items.RemoveRange(_context.Items.Where(x => x.NameCharacter == data.Name));
            _context.Character.Remove(data);
            _context.SaveChanges();
            return Ok();
        }
    }
}