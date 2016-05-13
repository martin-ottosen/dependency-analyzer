namespace DependencyTracker.Analysis
{
  using System.Collections.Generic;

  public class ModelGroup
  {
    public ModelGroup Parent { get; private set; }

    public string Name { get; private set; }
    public string Id { get; private set; }
    public int Size { get; set; }

    public Dictionary<string, ModelGroup> Children { get; private set; }
    public Dictionary<string, ModelAssembly> Registry { get; private set; }

    public ModelGroup()
    {
      this.Registry = new Dictionary<string, ModelAssembly>();
      this.Children = new Dictionary<string, ModelGroup>();
    }

    public ModelGroup(ModelAssembly assemblyWrapper, ModelGroup parent, AnalysisConfigurationLayer layer)
    {
      this.Children = new Dictionary<string, ModelGroup>();
      this.Registry = new Dictionary<string, ModelAssembly>();

      this.Parent = parent;

      this.Name = layer.ResolveName(assemblyWrapper);
      this.Id = layer.ResolveId(assemblyWrapper, parent);
    }

    public void Sprout(ModelAssembly assembly, Stack<AnalysisConfigurationLayer> layers)
    {
      var modelLayer = layers.Pop();
      
      this.Registry.Add(assembly.Assembly.GetName().ResolveId(), assembly);
      this.Size += assembly.FileSize;

      ModelGroup branch;

      var branchId = modelLayer.ResolveId(assembly, this.Parent);
      if (!this.Children.ContainsKey(branchId))
      {
        this.Children[branchId] = branch = new ModelGroup(assembly, this, modelLayer);
      }
      else
      {
        branch = this.Children[branchId];
      }

      if (layers.Count > 0)
      {
        branch.Sprout(assembly, layers);
      }
      else
      {
        branch.Size = assembly.FileSize;
      }

      layers.Push(modelLayer);
    }
  }
}