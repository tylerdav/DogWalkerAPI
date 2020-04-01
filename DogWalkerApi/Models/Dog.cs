using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkerApi.Models
{
    public class Dogs
    {
        public int Id { get; set; }
        public string DogName { get; set; }
        public int DogOwnerId { get; set; }
        public string Breed { get; set; }
        public string Notes { get; set; }
    }
}

