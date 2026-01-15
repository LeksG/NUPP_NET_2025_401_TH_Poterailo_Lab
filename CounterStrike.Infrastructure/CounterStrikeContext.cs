using Microsoft.EntityFrameworkCore;
using CounterStrike.Infrastructure.Models;

namespace CounterStrike.Infrastructure
{
    public class CounterStrikeContext : DbContext
    {
        public DbSet<PlayerModel> Players { get; set; }
        public DbSet<WeaponModel> Weapons { get; set; }
        public DbSet<TeamModel> Teams { get; set; }

        public CounterStrikeContext(DbContextOptions<CounterStrikeContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === Table-per-Type ===
            modelBuilder.Entity<PlayerModel>().ToTable("Players");
            modelBuilder.Entity<WeaponModel>().ToTable("Weapons");
            modelBuilder.Entity<TeamModel>().ToTable("Teams");

            // === Один-до-одного: Player ↔ Weapon ===
            modelBuilder.Entity<PlayerModel>()
                .HasOne(p => p.Weapon)
                .WithOne(w => w.Player)
                .HasForeignKey<PlayerModel>(p => p.WeaponId)
                .OnDelete(DeleteBehavior.SetNull);

            // === Один-до-багатьох: Team → Players ===
            modelBuilder.Entity<PlayerModel>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
