using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ForGloryDB.Model
{
    public class Character
    {
        [Key]
        public string Name { get; set; }
        public string Username { get; set; }
        public string CharacterType { get; set; }
        public int Slot { get; set; }
        public int Str { get; set; } = 10;
        public int Cons { get; set; } = 10;
        public int Def { get; set; } = 10;

        public int Level { get; set; } = 1;

        public int Progress { get; set; } = 0;

        public int Gold { get; set; } = 0;
    }
}
