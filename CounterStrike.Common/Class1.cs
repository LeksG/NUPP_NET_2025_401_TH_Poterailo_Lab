using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CounterStrike.Common
{
    // === Делегат для подій ===
    public delegate void KillEventHandler(string message);

    // === Базовий клас Entity ===
    public class Entity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public static int EntitiesCount;

        // Статичний конструктор
        static Entity()
        {
            EntitiesCount = 0;
        }

        // Конструктори
        public Entity() : this("Unknown") { }

        public Entity(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            EntitiesCount++;
        }

        // Статичний метод
        public static int GetEntitiesCount() => EntitiesCount;
    }

    // === Клас Player ===
    public class Player : Entity
    {
        public string Team { get; set; }
        public int Health { get; set; }
        public Weapon EquippedWeapon { get; set; }
        public int Score { get; set; }
        public bool IsOnline { get; set; } = true;

        // === Підключення асинхронного CRUD ===
        private static readonly CrudServiceAsync<Player> _playerService = new("players.json");

        public Player() : base("Unknown") { }

        public Player(string name, string team) : base(name)
        {
            Team = team;
            Health = 100;
            Score = 0;
        }

        // === Генератор випадкового гравця ===
        public static Player CreateRandom()
        {
            string[] names = { "Alex", "Chris", "Jordan", "Niko", "Max", "Ghost", "Blade" };
            string[] teams = { "Terrorists", "Counter-Terrorists" };
            var rand = new Random();

            return new Player(names[rand.Next(names.Length)], teams[rand.Next(teams.Length)])
            {
                Health = rand.Next(50, 100),
                Score = rand.Next(0, 20),
                EquippedWeapon = Weapon.CreateRandom()
            };
        }

        // === CRUD-методи ===

        // 🟢 Створює нового гравця і зберігає його у JSON
        public static async Task<Player> CreateNewAsync()
        {
            var player = CreateRandom();
            await _playerService.CreateAsync(player);
            return player;
        }

        //  Отримати всіх гравців
        public static async Task<IEnumerable<Player>> GetAllAsync() => await _playerService.ReadAllAsync();

        //  Оновити гравця
        public async Task<bool> UpdateAsync() => await _playerService.UpdateAsync(this);

        //  Видалити гравця
        public async Task<bool> RemoveAsync() => await _playerService.RemoveAsync(this);

        public override string ToString()
        {
            return $"{Name} ({Team}) - HP: {Health}, Score: {Score}, Gun: {EquippedWeapon?.Name ?? "None"}";
        }
    }

    // === Клас Weapon ===
    public class Weapon : Entity
    {
        public Weapon() : base("Unknown") { }

        public Weapon(string name, int damage, double price) : base(name)
        {
            Damage = damage;
            Price = price;
            Ammo = 30;
        }

        public int Damage { get; set; }
        public double Price { get; set; }
        public string Type { get; set; } = "Rifle"; // нова властивість
        public int Ammo { get; set; } // нова властивість

        //  Реалізація випадкової зброї
        internal static Weapon CreateRandom()
        {
            var rand = new Random();
            string[] names = { "AK-47", "M4A1", "AWP", "P90", "Deagle", "UMP-45", "Glock" };
            string[] types = { "Rifle", "Sniper", "SMG", "Pistol" };

            return new Weapon(
                names[rand.Next(names.Length)],
                rand.Next(25, 100),
                rand.Next(500, 3500))
            {
                Ammo = rand.Next(10, 120),
                Type = types[rand.Next(types.Length)]
            };
        }

        public override string ToString()
        {
            return $"{Name} (Damage: {Damage}, Price: {Price}$, Ammo: {Ammo}, Type: {Type})";
        }
    }

    // === Клас Round ===
    public class Round
    {
        public static event KillEventHandler OnKill; // статична подія

        public Round(int number)
        {
            Number = number;
            Kills = new List<string>();
            StartedAt = DateTime.Now;
        }

        public int Number { get; set; }
        public List<string> Kills { get; set; }
        public DateTime StartedAt { get; set; } // нова властивість

        // Методи
        public void RegisterKill(Player killer, Player victim, Weapon weapon)
        {
            string log = $"{killer.Name} killed {victim.Name} with {weapon.Name}";
            Kills.Add(log);
            OnKill?.Invoke(log);
        }
    }

    // === Метод розширення ===
    public static class PlayerExtensions
    {
        public static bool IsAlive(this Player player)
        {
            return player.Health > 0;
        }

        public static void Heal(this Player player, int hp)
        {
            player.Health += hp;
            if (player.Health > 100) player.Health = 100;
        }
    }

}