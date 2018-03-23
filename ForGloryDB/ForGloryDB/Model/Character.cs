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
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public double Z { get; set; } = 0;
    }
}
