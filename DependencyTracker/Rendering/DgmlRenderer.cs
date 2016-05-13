namespace DependencyTracker.Rendering
{
  using System;
  using System.IO;
  using System.Text;

  using DependencyTracker.Analysis;

  public class DgmlRenderer : IModelRenderer
  {
    private readonly DirectoryInfo templateFolder;

    public DgmlRenderer(DirectoryInfo templateFolder)
    {
      this.templateFolder = templateFolder;
    }

    public string Render(ModelGroup model)
    {
      var template = File.OpenText(this.templateFolder.FullName.TrimEnd('\\') + "\\DgmlTemplate.txt").ReadToEnd();

      var nodeBuffer = new StringBuilder();
      this.RenderDgmlNodes(model, nodeBuffer);
      var linkBuffer = new StringBuilder();
      this.RenderDgmlLinks(model, linkBuffer);

      var dgmlFileContents = template
        .Replace("{--NODES--}", nodeBuffer.ToString())
        .Replace("{--GROUP CONTAINER LINKS--}", linkBuffer.ToString())
        .Replace("{--ASSEMBLY REFERENCES--}", GetAssemblyLinks(new ModelAggregator().GenerateModelSummary(model)));

      return dgmlFileContents;
    }

    public void RenderDgmlNodes(ModelGroup model, StringBuilder buffer)
    {
      if (!IsRoot(model))
      {
        if (!IsLeaf(model))
        {
          WriteDgmlGroup(model, buffer);
        }
        else
        {
          WriteDgmlLeaf(model, buffer);
        }
      }

      foreach (var child in model.Children.Values)
      {
        this.RenderDgmlNodes(child, buffer);
      }
    }

    public void RenderDgmlLinks(ModelGroup model, StringBuilder buffer)
    {
      if (!IsRoot(model))
      {
        WriteDgmlContainerLink(model, buffer);
      }

      foreach (var child in model.Children.Values)
      {
        this.RenderDgmlLinks(child, buffer);
      }
    }

    private static void WriteDgmlGroup(ModelGroup model, StringBuilder buffer)
    {
      buffer.AppendFormat(@"
    <Node Id=""{0}"" Label=""{1}"" Group=""Collapsed"" FileGroupSize=""{2}""/>", model.Id, model.Name, model.Size);
    }

    private static void WriteDgmlLeaf(ModelGroup model, StringBuilder buffer)
    {
      buffer.AppendFormat(@"
    <Node Id=""{1}"" Label =""{0}"" IsAssembly=""True"" FileGroupSize=""{2}""/>", model.Name, model.Id, model.Size);
    }

    private static void WriteDgmlContainerLink(ModelGroup model, StringBuilder buffer)
    {
      buffer.AppendFormat(@"
    <Link Source=""{1}"" Target=""{0}"" Category=""Contains"" />", model.Id, model.Parent.Id);
    }

    private static bool IsLeaf(ModelGroup model)
    {
      return model.Children.Count == 0;
    }

    private static bool IsRoot(ModelGroup model)
    {
      return string.IsNullOrEmpty(model.Id);
    }

    private static string GetAssemblyLinks(ModelSummary metadata)
    {
      var result = "";
      foreach (var solutionAssembly in metadata.AssemblyRegistry.Values)
      {
        var solutionAssemblyName = solutionAssembly.Assembly.GetName();

        foreach (var referencedAssembly in solutionAssembly.Assembly.GetReferencedAssemblies())
        {
          if (!metadata.AssemblyRegistry.ContainsKey(referencedAssembly.ResolveId()))
          {
            continue;
          }

          var registeredAssembly = metadata.AssemblyRegistry[referencedAssembly.ResolveId()];
          if (registeredAssembly.Assembly.GetName().Version != referencedAssembly.Version)
          {
            result += String.Format(@"<Link Source=""{0}"" Target=""{1}"" Suspicious=""True"" Label=""{2}"" />", solutionAssemblyName.ResolveId(), referencedAssembly.ResolveId(), referencedAssembly.Version);
          }
          else
          {
            result += String.Format(@"<Link Source=""{0}"" Target=""{1}"" />", solutionAssemblyName.ResolveId(), referencedAssembly.ResolveId());
          }
        }
      }
      return result;
    }
  }
}
