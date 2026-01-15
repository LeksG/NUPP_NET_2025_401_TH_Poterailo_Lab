using System.ComponentModel.DataAnnotations.Schema;

namespace CounterStrike.Infrastructure.Models
{
    [Table("Weapons")]
    public class WeaponModel : EntityModel
    {
        public int Damage { get; set; }
        public double Price { get; set; }
        public string Type { get; set; } = "Rifle";

        // 🔗 Один-до-одного: Weapon ↔ Player
        public PlayerModel? Player { get; set; }
    }
}
