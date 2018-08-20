using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ForGloryDB.Model
{
    public class Items
    {
        [Key]
        public int Id { set; get; }
        public int ItemId { get; set; }
        public int Level { get; set; } = 1;
        public string Type { get; set; }
        public bool InUse { get; set; } = true;

        public string NameCharacter { get; set; } = null;
        public int UnitId { get; set; } = -1;

        public Items()
        {

        }

        public Items(int itemId, string type, string ch)
        {
            ItemId = itemId;
            Type = type;
            NameCharacter = ch;
        }
        public Items(int itemId, string type, int unit)
        {
            ItemId = itemId;
            Type = type;
            UnitId = unit;
        }
    }
}
