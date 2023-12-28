using System;
using System.IO;
using Steal;
namespace Default{
	public class Program{
		public static void Main(){
			Console.WriteLine(
				"Welcome into Steal (Strictly Typed Entertaiment Algorithmic Language) v0.2 Shell\n" +
				"Enter file(realtive or absolute path) to execute"
			);
			string src;
            while (true)
            {
				string name = Console.ReadLine();
                if (File.Exists(name))
                {
					src = File.ReadAllText(name);
					break;
                }
				else if (Directory.Exists(name))
                {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"{name} is a folder");
					Console.ResetColor();
                }
                else
                {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"File {name} does not exists");
					Console.ResetColor();
				}
            }
			new StealLang(src).Run();
			Console.WriteLine("Press any key to exit ...");
			Console.ReadKey();
		}
	}
}
