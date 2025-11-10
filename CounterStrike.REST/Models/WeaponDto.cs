using System;

namespace CounterStrike.REST.Models
{
    public class WeaponDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Damage { get; set; }
        public double Price { get; set; }
        public string Type { get; set; }
    }
}
