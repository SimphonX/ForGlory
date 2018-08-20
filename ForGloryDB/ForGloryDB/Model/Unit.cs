using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ForGloryDB.Model
{
    public class Unit
    {
        
        [Key]
        public int Id { get; set; }
        public int Level { get; set; } = 1;
        public string Type { get; set; }
        public string NameCharacter { get; set; }
        public int Slot { get; set; } = -1;
        public int Progress { get; set; } = 0;
        public Unit()
        {
        }
        public Unit(string type, string namecharacter)
        {
            Type = type;
            NameCharacter = namecharacter;
        }
    }
}
