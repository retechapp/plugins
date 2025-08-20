namespace Retech.Simulator;

public static class Program
{
  public static void Main()
  {
    Logger.DisableUnityLogger();

    _ = new Retech();

    Console.ReadKey();
  }
}
