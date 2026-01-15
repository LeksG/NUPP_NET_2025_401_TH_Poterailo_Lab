using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CounterStrike.Infrastructure;
using CounterStrike.Infrastructure.Models;

namespace CounterStrike.ConsoleApp
{
    class Program
    {
        static async Task Main()
        {
            // === Налаштування кирилиці та локалі ===
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("uk-UA");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("uk-UA");
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            // === DI та DbContext ===
            var services = new ServiceCollection();
            services.AddDbContext<CounterStrikeContext>(options =>
                options.UseSqlite("Data Source=counterstrike.db"));
            var provider = services.BuildServiceProvider();
            using var context = provider.GetRequiredService<CounterStrikeContext>();

            // === Репозиторії та CRUD сервіси ===
            var teamRepo = new Repository<TeamModel>(context);
            var weaponRepo = new Repository<WeaponModel>(context);
            var playerRepo = new Repository<PlayerModel>(context);

            var teamService = new CrudServiceAsync<TeamModel>(teamRepo);
            var weaponService = new CrudServiceAsync<WeaponModel>(weaponRepo);
            var playerService = new CrudServiceAsync<PlayerModel>(playerRepo);

            // === Створення бази ===
            context.Database.EnsureCreated();

            // === Команди ===
            if (!(await teamService.ReadAllAsync()).Any())
            {
                await teamService.CreateAsync(new TeamModel { Name = "Tерористи", Faction = "Червоні" });
                await teamService.CreateAsync(new TeamModel { Name = "Контр-терористи", Faction = "Сині" });
            }
            var teams = (await teamService.ReadAllAsync()).ToList();

            // === Генерація зброї та гравців ===
            string[] weaponNames = { "АК-47", "М4А1", "AWP", "P90", "Deagle", "UMP-45", "Glock" };
            string[] playerNames = { "Алекс", "Кріс", "Джордан", "Ніко", "Макс", "Гост", "Блейд" };
            var rnd = new Random();

            for (int i = 0; i < 50; i++)  // 50 гравців для прикладу
            {
                // Створюємо зброю
                var weapon = new WeaponModel
                {
                    Name = weaponNames[rnd.Next(weaponNames.Length)],
                    Damage = rnd.Next(25, 100),
                    Price = rnd.Next(500, 3500)
                };
                await weaponService.CreateAsync(weapon);

                // Створюємо гравця
                var player = new PlayerModel
                {
                    Name = playerNames[rnd.Next(playerNames.Length)],
                    Health = rnd.Next(50, 100),
                    Score = rnd.Next(0, 20),
                    WeaponId = weapon.Id,
                    TeamId = teams[rnd.Next(teams.Count)].Id
                };
                await playerService.CreateAsync(player);
            }

            // === Завантаження гравців та зброї ===
            var players = (await playerService.ReadAllAsync()).ToList();
            var weapons = players.Where(p => p.Weapon != null).Select(p => p.Weapon!).ToList();

           

            // === Вивід статистики ===
            Console.WriteLine("\n=== Після гри: статистика гравців ===");
            Console.WriteLine($"Гравців: {players.Count}");
            Console.WriteLine($"Health -> Min: {players.Min(p => p.Health)}, Max: {players.Max(p => p.Health)}, Avg: {players.Average(p => p.Health):F2}");
            Console.WriteLine($"Score  -> Min: {players.Min(p => p.Score)}, Max: {players.Max(p => p.Score)}, Avg: {players.Average(p => p.Score):F2}");

            Console.WriteLine("\n=== Статистика зброї ===");
            Console.WriteLine($"Damage -> Min: {weapons.Min(w => w.Damage)}, Max: {weapons.Max(w => w.Damage)}, Avg: {weapons.Average(w => w.Damage):F2}");
            Console.WriteLine($"Price  -> Min: {weapons.Min(w => w.Price):F2}, Max: {weapons.Max(w => w.Price):F2}, Avg: {weapons.Average(w => w.Price):F2}");

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();

            Console.WriteLine("\n=== Вміст бази ===");
            var dbPlayers = await playerService.ReadAllAsync();
            foreach (var p in dbPlayers)
            {
                Console.WriteLine($"{p.Name} | HP: {p.Health} | Score: {p.Score} | Team: {teams.First(t => t.Id == p.TeamId).Name}");
            }

        }
    }
}
