namespace DependencyTracker.Analysis
{
  public class ModelAggregator
  {
    public ModelSummary GenerateModelSummary(ModelGroup model)
    {
      var result = new ModelSummary { AssemblyRegistry = model.Registry };

      foreach (var registeredAssembly in result.AssemblyRegistry.Values)
      {
        foreach (var assemblyReference in registeredAssembly.Assembly.GetReferencedAssemblies())
        {
          if (!result.AssemblyRegistry.ContainsKey(assemblyReference.ResolveId()))
          {
            result.MissingReferences.Add(assemblyReference);
          }
        }
      }

      return result;
    }
  }
}