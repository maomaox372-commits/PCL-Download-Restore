// Test-only host for the REAL InstallerEngine from the built production assembly.
// This intentionally tests the engine separately from the interactive Main gate.
using System;
using System.Reflection;

internal static class ProductionEngineProbe
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3) throw new ArgumentException("Usage: probe <installer.exe> <command> <isolated-target>");
            Assembly assembly = Assembly.LoadFrom(args[0]);
            Type type = assembly.GetType("InstallerEngine", true);
            object engine = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { null }, null);
            string method;
            object[] inputs;
            if (args[1] == "--install" || args[1] == "--install-lock") { method = "Install"; inputs = new object[] { args[2], args[1] == "--install-lock" }; }
            else if (args[1] == "--restore") { method = "Restore"; inputs = new object[] { args[2] }; }
            else if (args[1] == "--inspect") { method = "Inspect"; inputs = new object[] { args[2] }; }
            else throw new ArgumentException("Unsupported test command");
            object result = type.GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(engine, inputs);
            Console.WriteLine(result); return 0;
        }
        catch (Exception ex)
        {
            while (ex is TargetInvocationException && ex.InnerException != null) ex = ex.InnerException;
            Console.Error.WriteLine(ex.Message); return 1;
        }
    }
}
