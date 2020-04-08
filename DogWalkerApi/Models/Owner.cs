using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkerApi.Models
{
    public class Owners
    {
        public int Id { get; set; }
        public string DogOwnerName { get; set; }
        public string DogOwnerAddress { get; set; }
        public int NeighborhoodId { get; set; }
        public string Phone { get; set; }
        public Neighborhoods Neighborhood { get; set; }
        public List<Dogs> DogsList { get; set; }
    }
}
