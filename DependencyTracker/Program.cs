namespace DependencyTracker
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

  using DependencyTracker.Analysis;
  using DependencyTracker.Rendering;

  using Newtonsoft.Json;

  static class Program
  {
    static void Main(string[] args)
    {
      var input = ReadArgument(args, "i:", @".\input");
      var outputFolderPath = ReadArgument(args, "o:", @".\output");
      var templateFolder = ReadArgument(args, "t:", @".\Templates");
      var outputFolder = !Directory.Exists(outputFolderPath) ? Directory.CreateDirectory(outputFolderPath) : new DirectoryInfo(outputFolderPath);
      
      var namingSettings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string,string>>>(File.ReadAllText(@".\namingConfiguration.json"));
      var runtimesettings = new RuntimeSettings(namingSettings);

      IModelRenderer dgmlRenderer = new DgmlRenderer(new DirectoryInfo(templateFolder));

      var assemblyRepos = AssemblyRepository.GetFromRootFolder(input);

      var fullConfig = new AnalysisConfiguration()
        .GroupBy(descriptor => descriptor.SystemName)
        .GroupBy(descriptor => descriptor.GetCompanyName(runtimesettings))
        .GroupBy(descriptor => descriptor.GetProductName(runtimesettings))
        .GroupBy(descriptor => descriptor.GetAssemblyName(), (descriptor, parent) => descriptor.Assembly.GetName().ResolveId())
        .Configure();

      var fullDgml = dgmlRenderer.Render(fullConfig.Analyze(assemblyRepos));
      WriteToOutputFile(outputFolder, fullDgml, "fullModel.dgml");

      var internalOnlyConfig = new AnalysisConfiguration()
        .Where(descriptor => descriptor.GetAssemblyName().StartsWith("sitecore", StringComparison.OrdinalIgnoreCase))
        .GroupBy(descriptor => descriptor.SystemName)
        .GroupBy(descriptor => descriptor.GetProductName(runtimesettings))
        .GroupBy(descriptor => descriptor.GetAssemblyName(), (descriptor, parent) => descriptor.Assembly.GetName().ResolveId())
        .Configure();

      var internalDgml = dgmlRenderer.Render(internalOnlyConfig.Analyze(assemblyRepos));
      WriteToOutputFile(outputFolder, internalDgml, "internalModel.dgml");

      var suspectConfig = new AnalysisConfiguration()
        .Where(descriptor => descriptor.SuspiciousReferences.Count > 0 || descriptor.SuspiciousReferers.Count > 0)
        .GroupBy(descriptor => descriptor.SystemName)
        .GroupBy(descriptor => descriptor.GetCompanyName(runtimesettings))
        .GroupBy(descriptor => descriptor.GetProductName(runtimesettings))
        .GroupBy(descriptor => descriptor.GetAssemblyName(), (descriptor, parent) => descriptor.Assembly.GetName().ResolveId())
        .Configure();

      var suspectDgml = dgmlRenderer.Render(suspectConfig.Analyze(assemblyRepos));
      WriteToOutputFile(outputFolder, suspectDgml, "suspectModel.dgml");
    }

    private static void WriteToOutputFile(DirectoryInfo outputFolder, string suspectDgml, string fileName)
    {
      using (var dgmlFile = File.CreateText(outputFolder.FullName.TrimEnd('\\') + "\\" + fileName))
      {
        dgmlFile.Write(suspectDgml);
      }
    }

    private static string ReadArgument(string[] args, string index, string defaultValue)
    {
      var fullArgString = args.FirstOrDefault(arg => arg.StartsWith(index));

      if (fullArgString == default(string))
        return defaultValue;

      return fullArgString.Substring(index.Length);
    }
  }
}
