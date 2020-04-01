using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkerApi.Models
{
    public class Walks
    {
        public int Id { get; set; }
        public double Date { get; set; }
        public string Address { get; set; }
        public int Duration { get; set; }
        public int WalkerId { get; set; }
        public int DogId { get; set; }
    }
}
