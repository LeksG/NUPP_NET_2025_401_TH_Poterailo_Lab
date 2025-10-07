using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public Player() : base("Unknown") { }

        public Player(string name, string team) : base(name)
        {
            Team = team;
            Health = 100;
            Score = 0;
        }

        public string Team { get; set; }
        public int Health { get; set; }
        public Weapon EquippedWeapon { get; set; }
        public int Score { get; set; } // нова властивість
        public bool IsOnline { get; set; } = true; // нова властивість

        // Методи
        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health < 0) Health = 0;
        }

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

    // === Інтерфейс CRUD ===
    public interface ICrudService<T>
    {
        void Create(T element);
        T Read(Guid id);
        IEnumerable<T> ReadAll();
        void Update(T element);
        void Remove(T element);
    }

    // === Дженерік CRUD з JSON Save/Load ===
    public class CrudService<T> : ICrudService<T> where T : class
    {
        private readonly List<T> _storage = new List<T>();

        // Методи CRUD
        public void Create(T element) => _storage.Add(element);

        public T Read(Guid id)
        {
            return _storage.FirstOrDefault(e =>
            {
                var prop = e.GetType().GetProperty("Id");
                return prop != null && (Guid)prop.GetValue(e) == id;
            });
        }

        public IEnumerable<T> ReadAll() => _storage;

        public void Update(T element)
        {
            var prop = element.GetType().GetProperty("Id");
            if (prop == null) return;

            var id = (Guid)prop.GetValue(element);
            var existing = Read(id);
            if (existing != null)
            {
                _storage.Remove(existing);
                _storage.Add(element);
            }
        }

        public void Remove(T element) => _storage.Remove(element);

        // JSON Save/Load
        public void Save(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            string json = JsonSerializer.Serialize(_storage, options);
            File.WriteAllText(filePath, json);
        }

        public void Load(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var options = new JsonSerializerOptions
            {
                IncludeFields = true
            };

            string json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<List<T>>(json, options);
            if (data != null)
            {
                _storage.Clear();
                _storage.AddRange(data);
            }
        }
    }
}

