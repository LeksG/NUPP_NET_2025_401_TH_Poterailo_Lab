using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using CounterStrike.Common;

namespace CounterStrike
{
    class Program
    {
        // === Примітиви синхронізації ===
        private static readonly object _lockObj = new();           //  для lock
        private static readonly SemaphoreSlim _semaphore = new(10); //  максимум 10 потоків одночасно
        private static readonly AutoResetEvent _autoReset = new(true); //  сигнал для поетапного виконання

        static async Task Main(string[] args)
        {
            // 🔤 Українська локаль і UTF-8
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("uk-UA");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("uk-UA");
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== Імітація гри Counter-Strike ===");

            int count = 1000;
            List<Player> players = new();

            Console.WriteLine($"▶ Генерація {count} гравців...");

            // === Паралельне створення гравців ===
            var tasks = Enumerable.Range(0, count).Select(i => Task.Run(async () =>
            {
                // Використання SemaphoreSlim для контролю кількості потоків
                await _semaphore.WaitAsync();
                try
                {
                    var player = await Player.CreateNewAsync();

                    //  Використання AutoResetEvent для покрокового доступу
                    _autoReset.WaitOne();

                    // Використання lock — безпечно додаємо у колекцію
                    lock (_lockObj)
                    {
                        players.Add(player);
                    }

                    // Повертаємо сигнал, щоб інші потоки могли продовжити
                    _autoReset.Set();
                }
                finally
                {
                    _semaphore.Release();
                }
            })).ToArray();

            await Task.WhenAll(tasks);

            Console.WriteLine($"✅ Створено {players.Count} гравців!\n");

            // === Аналіз даних ===
            var minHealth = players.Min(p => p.Health);
            var maxHealth = players.Max(p => p.Health);
            var avgHealth = players.Average(p => p.Health);

            var minScore = players.Min(p => p.Score);
            var maxScore = players.Max(p => p.Score);
            var avgScore = players.Average(p => p.Score);

            Console.WriteLine("=== Статистика гравців ===");
            Console.WriteLine($"Health -> Min: {minHealth}, Max: {maxHealth}, Avg: {avgHealth:F2}");
            Console.WriteLine($"Score  -> Min: {minScore}, Max: {maxScore}, Avg: {avgScore:F2}");

            // === Аналіз зброї ===
            var weapons = players
                .Where(p => p.EquippedWeapon != null)
                .Select(p => p.EquippedWeapon)
                .ToList();

            if (weapons.Count > 0)
            {
                var minDamage = weapons.Min(w => w.Damage);
                var maxDamage = weapons.Max(w => w.Damage);
                var avgDamage = weapons.Average(w => w.Damage);

                var minPrice = weapons.Min(w => w.Price);
                var maxPrice = weapons.Max(w => w.Price);
                var avgPrice = weapons.Average(w => w.Price);

                Console.WriteLine("\n=== Статистика зброї ===");
                Console.WriteLine($"Damage -> Min: {minDamage}, Max: {maxDamage}, Avg: {avgDamage:F2}");
                Console.WriteLine($"Price  -> Min: {minPrice:F2}, Max: {maxPrice:F2}, Avg: {avgPrice:F2}");
            }

            // === Збереження у файл ===
            string outputPath = "generated_players.json";
            string json = System.Text.Json.JsonSerializer.Serialize(players, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(outputPath, json);

            Console.WriteLine($"\n💾 Дані збережено у файл: {Path.GetFullPath(outputPath)}");
            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
