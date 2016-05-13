namespace DependencyTracker.Analysis
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

  public class AssemblyRepository
  {
    private const string SearchPattern = "*.dll";

    private readonly DirectoryInfo rootDirectory;
    public readonly string RootName;

    public AssemblyRepository(string path)
    {
      this.rootDirectory = new DirectoryInfo(path);
      this.RootName = this.rootDirectory.Name;
    }

    public IEnumerable<FileInfo> GetAssemblies()
    {
      return this.rootDirectory.GetFiles(SearchPattern, SearchOption.AllDirectories);
    }

    public static List<AssemblyRepository> GetFromRootFolder(string input)
    {
      return new DirectoryInfo(input).GetDirectories().Select(folder => new AssemblyRepository(folder.FullName)).ToList();
    }
  }
}