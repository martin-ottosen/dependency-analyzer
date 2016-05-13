namespace DependencyTracker.Analysis
{
  using System;

  public class AnalysisConfigurationLayer
  {
    public AnalysisConfigurationLayer(Func<ModelAssembly, string> name, Func<ModelAssembly, ModelGroup, string> id)
    {
      this.ResolveId = id;
      this.ResolveName = name;
    }

    public AnalysisConfigurationLayer(Func<ModelAssembly, string> name)
    {
      this.ResolveName = name;
      this.ResolveId = (descriptor, parent) => parent == null ? this.ResolveName(descriptor) : parent.Id + "/" + this.ResolveName(descriptor);
    }

    public readonly Func<ModelAssembly, ModelGroup, string> ResolveId;
    public readonly Func<ModelAssembly, string> ResolveName;
  }
}