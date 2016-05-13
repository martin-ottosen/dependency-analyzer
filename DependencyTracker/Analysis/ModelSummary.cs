namespace DependencyTracker.Analysis
{
  using System.Collections.Generic;
  using System.Reflection;

  public class ModelSummary
  {
    public Dictionary<string, ModelAssembly> AssemblyRegistry = new Dictionary<string, ModelAssembly>(300);
    public readonly List<AssemblyName> MissingReferences = new List<AssemblyName>();    
  }
}