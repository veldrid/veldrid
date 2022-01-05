using System;
using System.Collections.Generic;
using Veldrid.Tests;

class Program
{
    static int Main(string[] args)
    {
        List<string> newArgs = new List<string>(args);
        newArgs.InsertRange(0, new[] { "-parallel", "none" });
        newArgs.Insert(0, typeof(BufferTestBase<>).Assembly.Location);
        int returnCode = Xunit.ConsoleClient.Program.Main(newArgs.ToArray());
        Console.WriteLine("Tests finished. Press any key to exit.");
        if (!Console.IsInputRedirected)
        {
            Console.ReadKey(true);
        }
        return returnCode;
    }
}
