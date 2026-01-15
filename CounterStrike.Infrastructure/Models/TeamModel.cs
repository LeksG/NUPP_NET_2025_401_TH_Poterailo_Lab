using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CounterStrike.Infrastructure.Models
{
    [Table("Teams")]
    public class TeamModel : EntityModel
    {
        public string Faction { get; set; }

        // 🔗 Один-до-багатьох: Team → Players
        public ICollection<PlayerModel> Players { get; set; } = new List<PlayerModel>();
    }
}
