using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkerApi.Models
{
    public class Walkers
    {
        public int Id { get; set; }
        public string WalkerName { get; set; }
        public int NeighborhoodId { get; set; }
        public List<Walks> Walk { get; set; }
    }
}
