namespace DependencyTracker.Analysis
{
  using System;
  using System.Collections.Generic;

  public class AnalysisConfiguration
  {
    public readonly Stack<AnalysisConfigurationLayer> modelLayers = new Stack<AnalysisConfigurationLayer>();

    public AssemblyAnalyzer Configure()
    {
      return new AssemblyAnalyzer(InvertStack(this.modelLayers), this.Filter);
    }

    private static Stack<T> InvertStack<T>(Stack<T> stack)
    {
      return new Stack<T>(stack);
    }

    private static void AddLayerToConfig(Func<ModelAssembly, string> name, Func<ModelAssembly, ModelGroup, string> id, AnalysisConfiguration config)
    {
      AnalysisConfigurationLayer layer;
      if (id == null)
      {
        layer = new AnalysisConfigurationLayer(name);
      }
      else
      {
        layer = new AnalysisConfigurationLayer(name, id);
      }

      config.modelLayers.Push(layer);
    }

    public AnalysisConfiguration GroupBy(Func<ModelAssembly, string> name)
    {
      AddLayerToConfig(name, null, this);
      return this;
    }
    
    public AnalysisConfiguration GroupBy(Func<ModelAssembly, string> name, Func<ModelAssembly, ModelGroup, string> id)
    {
      AddLayerToConfig(name, id, this);
      return this;
    }
    public AnalysisConfiguration Where(Func<ModelAssembly, bool> filter)
    {
      this.Filter = filter;
      return this;
    }

    public Func<ModelAssembly, bool> Filter { get; set; }
  }
}