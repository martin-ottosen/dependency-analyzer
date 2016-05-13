namespace DependencyTracker.Analysis
{
  using System.Collections.Generic;
  using System.Reflection;

  public class ModelAssembly
  {
    public string SystemName { get; private set; }
    public int FileSize { get; private set; }
    public Assembly Assembly { get; private set; }
    public readonly List<AssemblyName> MissingReferences = new List<AssemblyName>();
    public readonly List<ModelAssembly> SuspiciousReferences = new List<ModelAssembly>();
    public readonly List<ModelAssembly> SuspiciousReferers = new List<ModelAssembly>();

    public ModelAssembly(Assembly assembly, string systemName, int fileSize)
    {
      this.Assembly = assembly;
      this.SystemName = systemName;
      this.FileSize = fileSize;
    }
    public string GetAssemblyName()
    {
      var assemblyName = this.Assembly.GetName();
      return assemblyName.Name + ".dll " + assemblyName.Version;
    }
  }
}
