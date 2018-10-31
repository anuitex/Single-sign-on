using System;
using System.Linq;
using System.Reflection;

namespace DbUp
{
    class Program
    {
        static int Main(string[] args)
        {
            var connectionString = args.FirstOrDefault() ?? "Server=(local);Database=SingleSignOn;Trusted_Connection=True;MultipleActiveResultSets=true";

            var upgradeEngine = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                //.JournalTo(new NullJournal())
                .Build();

            var req = upgradeEngine.IsUpgradeRequired();

            var result = upgradeEngine.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
                Console.ReadLine();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();


            Console.ReadLine();

            return 0;
        }
    }
}
