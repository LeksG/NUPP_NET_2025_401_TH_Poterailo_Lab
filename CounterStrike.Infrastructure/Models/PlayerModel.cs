using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CounterStrike.Infrastructure.Models
{
    [Table("Players")]
    public class PlayerModel : EntityModel
    {
        public int Health { get; set; }
        public int Score { get; set; }

        // 🔗 Один-до-одного: Player ↔ Weapon
        public Guid? WeaponId { get; set; }
        public WeaponModel? Weapon { get; set; }

        // 🔗 Один-до-багатьох: Player → Team
        public Guid TeamId { get; set; }
        public TeamModel Team { get; set; }
    }
}
