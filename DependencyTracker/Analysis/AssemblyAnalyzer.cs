namespace DependencyTracker.Analysis
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;

  public class AssemblyAnalyzer
  {
    private readonly Stack<AnalysisConfigurationLayer> layers;
    private readonly Func<ModelAssembly, bool> shouldBeIncludedFilter;

    public AssemblyAnalyzer(Stack<AnalysisConfigurationLayer> modelLayers, Func<ModelAssembly, bool> filter)
    {
      this.shouldBeIncludedFilter = filter;
      this.layers = modelLayers;
    }

    public ModelGroup Analyze(IEnumerable<AssemblyRepository> assemblyRepositories)
    {
      var assemblies = new List<ModelAssembly>();
      foreach (var assemblyRepo in assemblyRepositories)
      {
        foreach (var assemblyFile in assemblyRepo.GetAssemblies())
        {
          try
          {
            var loadedAssembly = Assembly.LoadFile(assemblyFile.FullName);
            LoadAssemblyDependencies(loadedAssembly);
            var mappedAssembly = new ModelAssembly(loadedAssembly, assemblyRepo.RootName, (int)assemblyFile.Length / 1024);

            assemblies.Add(mappedAssembly);
          }
          catch (Exception e)
          {
            this.ReportThatAssemblyWasSkipped(assemblyFile.Name, e.Message);
          }
        }
      }

      return this.Analyze(assemblies);
    }

    private static void LoadAssemblyDependencies(Assembly loadedAssembly)
    {
      loadedAssembly.CustomAttributes.Any();
    }

    public ModelGroup Analyze(List<ModelAssembly> assemblies)
    {
      var assemblyRegistry = new Dictionary<string, ModelAssembly>();
      foreach (var assembly in assemblies)
      {
        var assemblyId = assembly.Assembly.GetName().ResolveId();
        if(assemblyRegistry.ContainsKey(assemblyId))
        {
          this.ReportThatAssemblyWasSkipped(assemblyId, "Duplicate detected");
          continue;
        }
        assemblyRegistry.Add(assemblyId, assembly);
      }

      foreach (var sourceAssembly in assemblyRegistry.Values)
      {
        foreach (var reference in sourceAssembly.Assembly.GetReferencedAssemblies())
        {
          var targetId = reference.ResolveId();

          if (assemblyRegistry.ContainsKey(targetId))
          {
            var targetAssembly = assemblyRegistry[targetId];
            if (targetAssembly.Assembly.GetName().Version != reference.Version)
            {
              targetAssembly.SuspiciousReferers.Add(sourceAssembly);
              sourceAssembly.SuspiciousReferences.Add(targetAssembly);
            }
          }
          else
          {
            sourceAssembly.MissingReferences.Add(reference);
          }
        }
      }

      var root = new ModelGroup();

      foreach (var assemblyDescriptor in assemblyRegistry.Values)
      {
        if (this.shouldBeIncludedFilter == null || this.shouldBeIncludedFilter(assemblyDescriptor))
        {
          root.Sprout(assemblyDescriptor, this.layers);
        }
      }

      return root;
    }

    private void ReportThatAssemblyWasSkipped(string assemblyName, string message)
    {
      using (var log = File.AppendText("log.txt"))
      {
        var logLine = string.Format("{2}\tSkipped assembly\t{0}\tdue to\t{1}", assemblyName, message, DateTime.Now.ToShortTimeString());


        Console.WriteLine(logLine);
        log.WriteLine(DateTime.Now.ToShortTimeString() + logLine);
      }
    }
  }
}
