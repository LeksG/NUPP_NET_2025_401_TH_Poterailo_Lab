using System;
using System.ComponentModel.DataAnnotations;

namespace CounterStrike.Infrastructure.Models
{
    public abstract class EntityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
