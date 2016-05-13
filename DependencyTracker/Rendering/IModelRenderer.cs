namespace DependencyTracker.Rendering
{
  using DependencyTracker.Analysis;

  public interface IModelRenderer
  {
    string Render(ModelGroup model);
  }
}