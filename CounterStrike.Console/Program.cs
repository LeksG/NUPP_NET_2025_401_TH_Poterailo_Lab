using System;
using System.Linq;
using CounterStrike.Common;

namespace CounterStrike.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Підписка на подію
            Round.OnKill += (msg) => Console.WriteLine($"[EVENT]: {msg}");

            // CRUD сервіси
            var playerService = new CrudService<Player>();
            var weaponService = new CrudService<Weapon>();

            // Створимо зброю
            var ak = new Weapon("AK-47", 35, 2700);
            var awp = new Weapon("AWP", 90, 4750);

            weaponService.Create(ak);
            weaponService.Create(awp);

            // Створимо гравців
            var p1 = new Player("s1mple", "Terrorists") { EquippedWeapon = ak };
            var p2 = new Player("ZywOo", "Counter-Terrorists") { EquippedWeapon = awp };

            playerService.Create(p1);
            playerService.Create(p2);

            // Виведемо всіх гравців
            Console.WriteLine("Players:");
            foreach (var pl in playerService.ReadAll())
                Console.WriteLine(pl);

            Console.WriteLine("\nWeapons:");
            foreach (var w in weaponService.ReadAll())
                Console.WriteLine(w);

            //Зберігаємо у JSON
            playerService.Save("players.json");
            weaponService.Save("weapons.json");
            Console.WriteLine("\nData saved to JSON files.");

            // Очистимо сервіси
            playerService = new CrudService<Player>();
            weaponService = new CrudService<Weapon>();

            //Завантажуємо з JSON
            playerService.Load("players.json");
            weaponService.Load("weapons.json");
            Console.WriteLine("Data loaded from JSON files.\n");

            Console.WriteLine("Loaded Players:");
            foreach (var pl in playerService.ReadAll())
                Console.WriteLine(pl);

            Console.WriteLine("\nLoaded Weapons:");
            foreach (var w in weaponService.ReadAll())
                Console.WriteLine(w);

            //Запускаємо раунд 
            var round1 = new Round(1);
            var loadedP1 = playerService.ReadAll().First();
            var loadedP2 = playerService.ReadAll().Last();

            loadedP2.TakeDamage(loadedP1.EquippedWeapon.Damage);
            round1.RegisterKill(loadedP1, loadedP2, loadedP1.EquippedWeapon);

            Console.WriteLine($"\nIs {loadedP2.Name} alive? " + (loadedP2.IsAlive() ? "Yes" : "No"));
        }
    }
}
