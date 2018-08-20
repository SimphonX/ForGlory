using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForGloryDB.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ForGloryDB.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        public class ItemUpgrade
        {
            public int code { get; set; }
            public int gold { get; set; }
        }
        private ForGloryContext _context;

        public ItemsController(ForGloryContext context)
        {
            _context = context;
        }

        [HttpGet("char/{username}")]
        public IActionResult GetCharacterItems(string username)
        {
            if (username == "")
                return BadRequest();
            var data = _context.Items.Where(x => x.NameCharacter.Equals(username));
            if (data == null)
                return NotFound();
            return new ObjectResult(data);
        }

        [HttpPut("{name}")]
        public IActionResult UpgradeItem([FromBody]ItemUpgrade item, string name)
        {
            if (name == "" || item.code == -1 || item.gold <= 0)
                return BadRequest();
            var items = _context.Items.FirstOrDefault(x => x.NameCharacter.Equals(name) && x.ItemId == item.code);
            var player = _context.Character.FirstOrDefault(x => x.Name.Equals(name));
            if (items == null || player == null)
                return NotFound();
            items.Level++;
            player.Gold -= item.gold;
            _context.Character.Update(player);
            _context.Items.Update(items);
            _context.SaveChanges();
            string json = "UPGRADE|" + player.Name + "|" + items.ItemId + "|" + player.Gold;
            return new ObjectResult(json);
        }
    }
}