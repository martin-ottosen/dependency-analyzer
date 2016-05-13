namespace DependencyTracker.Analysis
{
  using System.Reflection;

  public static class AssemblyNameExtensions
  {
    public static string ResolveId(this AssemblyName assemblyName)
    {
      return assemblyName.Name;
    }
  }
}