using System;

namespace CounterStrike.REST.Models
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Health { get; set; }
        public int Score { get; set; }
        public Guid? WeaponId { get; set; }
        public Guid TeamId { get; set; }
    }
}
