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
        public int Level { get; set; }
        public string Type { get; set; }
        public string NameCharacter { get; set; }
        public bool InUse { get; set; }
    }
}
