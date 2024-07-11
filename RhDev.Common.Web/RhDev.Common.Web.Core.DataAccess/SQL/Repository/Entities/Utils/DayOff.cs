using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Utils
{    
    public class DayOff : StoreEntity, IDataStoreEntity
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(2048)]
        public string Title { get; set; }
        public DateTime Day { get; set; }
        public bool Repeat { get; set; }
        public override string Identifier => Id.ToString();
    }
}
