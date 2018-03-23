using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ForGloryDB.Model;

namespace ForGloryDB.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : Controller
    {
        private ForGloryContext _context;

        public UserController(ForGloryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult LogInUser()
        {
            if (!Request.Headers.ContainsKey("username") || !Request.Headers.ContainsKey("password"))
                return BadRequest();
            if (_context.User.FirstOrDefault(e => e.Password.Equals(Request.Headers["password"]) && e.Username.Equals(Request.Headers["username"])) == null)
                return NotFound();
            return Accepted();
        }
        [HttpGet("{email}")]
        public IActionResult GetUserData(string email)
        {
            if (email == "")
                return BadRequest();
            var user = _context.User.FirstOrDefault(e => e.Email.Equals(email));
            if ( user == null)
                return NotFound();
            return new ObjectResult(user);
        }
        [HttpPost]
        public IActionResult RegisterUser([FromBody] User user)
        {
            if(user == null)
                return BadRequest();
            if (_context.User.FirstOrDefault(e => e.Email == user.Email) != null || _context.User.FirstOrDefault(e => e.Username == user.Username) != null)
                return NotFound();
            _context.User.Add(user);
            _context.SaveChanges();
            return NoContent();
        }
        [HttpDelete("{username}")]
        public IActionResult DeleteUser(string username)
        {
            var data = _context.User.FirstOrDefault(e => e.Username.Equals(username));
            if (data == null)
                return NotFound();
            _context.User.Remove(data);
            _context.SaveChanges();
            return Ok();
        }
    }
}