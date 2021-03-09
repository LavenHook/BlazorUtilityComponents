using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace METSv2.Web.BlazorComponents.BlazorComponents
{
  //SEE: https://github.com/dotnet/aspnetcore/issues/17502#issuecomment-560872404
  //the conversation suggests there may be some problems with this, but .net 5 by using the IComponentFactory as seen
  //in https://github.com/dotnet/aspnetcore/issues/7962 and other adjascent threads
  public class DynamicComponent<T> : ComponentBase where T : IComponent
  {
    [Inject]
    protected T Component { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; }

    private IEnumerable<KeyValuePair<string, object>> TAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
      if (Component is T)
      {
        int index = -1;
        builder.OpenComponent(++index, Component.GetType());
        if (AdditionalAttributes?.Any() ?? false)
        {
          builder.AddMultipleAttributes(++index, AdditionalAttributes);
        }
        builder.CloseComponent();
      }
    }

    protected override void OnParametersSet()
    {
      base.OnParametersSet();
      //this.Attributes = this.GetType()
      //  .GetProperties()
      //  .Where(w => w.DeclaringType != typeof(DynamicComponent<T>) && w.GetCustomAttributes(typeof(ParameterAttribute), true)?.Length > 0)
      //  .Select(s => new KeyValuePair<string, object>(s.Name, s.GetValue(this)))
      //  .ToList();
      this.TAttributes = this.GetType()
        .GetProperties()
        .Where(w => w.DeclaringType != typeof(DynamicComponent<T>) && w.GetCustomAttributes(typeof(ParameterAttribute), true)?.Length > 0)
        .ToDictionary(s => s.Name, s => s.GetValue(this));
    }
  }
}
