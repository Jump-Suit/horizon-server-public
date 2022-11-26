using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Database.Models
{
    public class UniverseNewsDTO
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public string News { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime ModifiedDt { get; set; }
    }
}
