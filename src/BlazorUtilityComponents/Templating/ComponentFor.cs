using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace METSv2.Web.BlazorComponents.BlazorComponents.Templating
{
  /// <summary>
  /// Provides mapping for the DI container to resolve what IComponent-Type should be used to display a model of ModelType
  /// </summary>
  internal interface IComponentMap
  {
    Type ModelType { get; }
    Type ComponentType { get; }
  }

  /// <summary>
  /// Provides mapping for the DI container to resolce what IComponent-Type should be used to display a model of ModelType when the parent-ContextType IComponentType is provided
  /// </summary>
  internal interface IContextualComponentMap : IComponentMap
  {
    Type ContextType { get; }
  }

  /// <summary>
  /// Provides mapping for the DI container to resolce what IComponent-Type should be used to display a model of ModelType when the parent-Object IComponentType is provided
  /// </summary>
  internal interface IObjectComponentMap : IContextualComponentMap
  {
    object ContextObject { get; }
  }

  /// <summary>
  /// Provides standardized methods for the ComponentFor Components to be able to resolve the injected template for the given Model/Type
  /// </summary>
  public interface IComponentTypeResolver
  {
    Type ResolveComponentTypeFor(Type Type, params Type[] AssignableFrom);

    Type ResolveComponentTypeFor(object Model, params Type[] AssignableFrom);

    Type ResolveComponentTypeFor<TModel>(TModel Model = default, params Type[] AssignableFrom);
  }

  /// <summary>
  /// Provides standardized methods for the ComponentFor Components to be able to resolve the injected template for the given Model/Type & ContextType combination
  /// </summary>
  public interface ITypeContextualComponentTypeResolver : IComponentTypeResolver
  {
    Type ResolveComponentTypeForContext(Type ContextType, Type ModelType, params Type[] AssignableFrom);

    Type ResolveComponentTypeForContext<TContext, TModel>(TModel Model = default, params Type[] AssignableFrom);
  }

  /// <summary>
  /// Provides standardized methods for the ComponentFor Components to be able to resolve the injected template for the given Model/Type & Context combination
  /// </summary>
  public interface IObjectContextualComponentTypeResolver : ITypeContextualComponentTypeResolver
  {
    Type ResolveComponentTypeForObject(object Context, object Model, params Type[] AssignableFrom);

    Type ResolveComponentTypeForObject<TContext, TModel>(TContext context, TModel Model = default, params Type[] AssignableFrom);
  }

  /// <summary>
  /// A default implementation of IComponentTypeResolver, ITypeContextualComponentTypeResolver,
  /// IObjectContextualComponentTypeResolver which can be used in tandem for complex template resolution
  /// </summary>
  internal class DefaultComponentTypeResolver : IComponentTypeResolver, ITypeContextualComponentTypeResolver, IObjectContextualComponentTypeResolver
  {
    public IEnumerable<IComponentMap> ComponentMaps { get; }
    public IEnumerable<IContextualComponentMap> ContextualComponentMaps { get; }
    public IEnumerable<IObjectComponentMap> ObjectComponentMaps { get; }

    public DefaultComponentTypeResolver(IEnumerable<IComponentMap> ComponentMaps, IEnumerable<IContextualComponentMap> ContextualComponentMaps, IEnumerable<IObjectComponentMap> ObjectComponentMaps)
    {
      this.ComponentMaps = ComponentMaps;
      this.ContextualComponentMaps = ContextualComponentMaps;
      this.ObjectComponentMaps = ObjectComponentMaps;
    }

    public Type ResolveComponentTypeFor(Type ModelType, params Type[] AssignableFrom)
    {
      Type[] assignableFrom = new Type[] { typeof(IComponent) };
      if (AssignableFrom?.Any() ?? false)
      {
        assignableFrom = AssignableFrom.Union(assignableFrom).Distinct().ToArray();
      }
      return ModelType is Type ? ComponentMaps?.LastOrDefault(m => m.ModelType.IsAssignableFrom(ModelType) && assignableFrom.All(t => t.IsAssignableFrom(m.ComponentType)))?.ComponentType : null;
    }

    public Type ResolveComponentTypeFor(object Model, params Type[] AssignableFrom)
    {
      return Model is null ? null : ResolveComponentTypeFor(Model.GetType(), AssignableFrom);
    }

    public Type ResolveComponentTypeFor<TModel>(TModel Model = default, params Type[] AssignableFrom)
    {
      var modelType = Model?.GetType() ?? typeof(TModel);
      return ResolveComponentTypeFor(modelType, AssignableFrom);
    }

    public Type ResolveComponentTypeForContext(Type ContextType, Type ModelType, params Type[] AssignableFrom)
    {
      Type result = null;
      Type[] assignableFrom = new Type[] { typeof(IComponent) };
      if (AssignableFrom?.Any() ?? false)
      {
        assignableFrom = AssignableFrom.Union(assignableFrom).Distinct().ToArray();
      }
      if (ContextType is Type && ModelType is Type)
      {
        var componentMap = ContextualComponentMaps?.Where(m => m.ContextType == ContextType).LastOrDefault(m => m.ModelType == ModelType && assignableFrom.All(t => t.IsAssignableFrom(m.ComponentType)));
        if (componentMap is IContextualComponentMap)
        {
          result = componentMap.ComponentType;
        }
      }
      if (result is null)
      {
        result = ResolveComponentTypeFor(ModelType, AssignableFrom);
      }
      return result;
    }

    public Type ResolveComponentTypeForContext<TContext, TModel>(TModel Model = default, params Type[] AssignableFrom)
    {
      return ResolveComponentTypeForContext(typeof(TContext), Model?.GetType() ?? typeof(TModel), AssignableFrom);
    }

    public Type ResolveComponentTypeForObject(object Context, object Model, params Type[] AssignableFrom)
    {
      Type result = null;
      Type[] assignableFrom = new Type[] { typeof(IComponent) };
      if (AssignableFrom?.Any() ?? false)
      {
        assignableFrom = AssignableFrom.Union(assignableFrom).Distinct().ToArray();
      }
      if (Context is object)
      {
        if (Model is object)
        {
          var componentMap = ObjectComponentMaps?.LastOrDefault(m => object.Equals(m, Context) && m.ModelType == Model.GetType() && assignableFrom.All(t => t.IsAssignableFrom(m.ComponentType)));
          if (componentMap is IContextualComponentMap)
          {
            result = componentMap.ComponentType;
          }
        }
        if (result is null)
        {
          result = ResolveComponentTypeForContext(Context.GetType(), Model.GetType(), AssignableFrom);
        }
      }
      if (result is null)
      {
        result = ResolveComponentTypeFor(Model, AssignableFrom);
      }
      return result;
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="Context"></param>
    /// <param name="Model"></param>
    /// <param name="AssignableFrom"></param>
    /// <returns></returns>
    /// <remarks>untested - use with caution</remarks>
    public Type ResolveComponentTypeForObject<TContext, TModel>(TContext Context, TModel Model = default, params Type[] AssignableFrom)
    {
      {
        Type result = null;
        Type[] assignableFrom = new Type[] { typeof(IComponent) };
        if (AssignableFrom?.Any() ?? false)
        {
          assignableFrom = AssignableFrom.Union(assignableFrom).Distinct().ToArray();
        }
        if (Context is object)
        {
          if (Model is object)
          {
            var componentMap = ObjectComponentMaps?.LastOrDefault(m => object.Equals(m, Context) && m.ModelType == Model.GetType() && assignableFrom.All(t => t.IsAssignableFrom(m.ComponentType)));
            if (componentMap is IContextualComponentMap)
            {
              result = componentMap.ComponentType;
            }
            if (result is null)
            {
              componentMap = ObjectComponentMaps?.LastOrDefault(m => object.Equals(m, Context) && m.ModelType == typeof(TModel) && assignableFrom.All(t => t.IsAssignableFrom(m.ComponentType)));
              if (componentMap is IContextualComponentMap)
              {
                result = componentMap.ComponentType;
              }
            }
          }
          if (result is null)
          {
            result = ResolveComponentTypeForContext(Context.GetType(), typeof(TModel), AssignableFrom);
          }
        }
        if (result is null)
        {
          result = ResolveComponentTypeFor(Model, AssignableFrom);
        }
        return result;
      }
    }
  }

  /// <summary>
  /// A default implementation of IComponentMap
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TComponent"></typeparam>
  internal class DefaultComponentMap<TModel, TComponent> : IComponentMap where TComponent : IComponent
  {
    public Type ModelType => typeof(TModel);

    public Type ComponentType => typeof(TComponent);
  }

  /// <summary>
  /// A default implementation of IContextualComponentMap
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TComponent"></typeparam>
  internal class TypeContextualComponentMap<TContext, TModel, TComponent> : DefaultComponentMap<TModel, TComponent>, IContextualComponentMap where TComponent : IComponent
  {
    public virtual Type ContextType => typeof(TContext);
  }

  /// <summary>
  /// A default implementation of IObjectComponentMap
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TComponent"></typeparam>
  internal class ObjectContextualComponentMap<TContext, TModel, TComponent> : TypeContextualComponentMap<TContext, TModel, TComponent>, IObjectComponentMap where TComponent : IComponent
  {
    public virtual TContext Context { get; private set; }
    public override Type ContextType => Context?.GetType() ?? base.ContextType;

    public object ContextObject => Context;

    public ObjectContextualComponentMap(TContext Context)
    {
      this.Context = Context;
    }
  }

  /// <summary>
  /// An incomplete mock implementation for demonstrating the flexibility of the templating system in that templates
  /// *could* be resolved based on the user, for instance, if a user's preferences are tracked, or to check if a user
  /// has the necessary permissions to view a full SSN or CC number, etc.
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TComponent"></typeparam>
  /// <remarks>this is partly here just to show that you *could* make a component that changes based on user (or other context-based scenarios - maybe keep a user preference for whether they prefer a 'dark' theme, as an example, or for viewing a list of items as a grid or traditional list)</remarks>
  internal class UserContextualComponentMap<TContext, TModel, TComponent> : TypeContextualComponentMap<TContext, TModel, TComponent>, IContextualComponentMap where TComponent : IComponent
  {
    public UserContextualComponentMap(System.Security.Principal.IIdentity identity)
    {
      Identity = identity;
    }

    public IIdentity Identity { get; }
  }

  /// <summary>
  /// A default implementation of IComponentMap, used only internally for the default DI registration mechansims (in the IServiceCollectionExtension class)
  /// </summary>
  internal record DefaultComponentMap(Type ModelType, Type ComponentType) : IComponentMap
  {
  }

  /// <summary>
  /// A default implementation of IContextualComponentMap, used only internally for the default DI registration mechansims (in the IServiceCollectionExtension class
  /// </summary>
  internal record DefaultContextualComponentMap(Type ModelType, Type ComponentType, Type ContextType) : DefaultComponentMap(ModelType, ComponentType), IContextualComponentMap
  {
  }

  /// <summary>
  /// A default implementation of IObjectComponentMap, used only internally for the default DI registration mechansims (in the IServiceCollectionExtension class
  /// </summary>
  internal record DefaultObjectComponentMap : DefaultContextualComponentMap, IObjectComponentMap
  {
    public object ContextObject { get; private set; }

    public DefaultObjectComponentMap(object ContextObject, Type ComponentType, Type ContextType) : base(ContextObject.GetType(), ComponentType, ContextType)
    {
      this.ContextObject = ContextObject;
    }
  }

  /// <summary>
  /// Standardized extenstion methods for registering Model-based, Context-based, blazor component templates into the DI container
  /// </summary>
  public static class IServiceCollectionExtension
  {
    public static IServiceCollection AddDefaultComponentMapper(this IServiceCollection services)
    {
      services.AddScoped<DefaultComponentTypeResolver>();
      services.AddScoped<IComponentTypeResolver>(p => p.GetService<DefaultComponentTypeResolver>());
      services.AddScoped<ITypeContextualComponentTypeResolver>(p => p.GetService<DefaultComponentTypeResolver>());
      services.AddScoped<IObjectContextualComponentTypeResolver>(p => p.GetService<DefaultComponentTypeResolver>());
      return services;
    }

    public static IServiceCollection AddComponentMap<TModel, TComponent>(this IServiceCollection services) where TComponent : IComponent
    {
      services.AddScoped<IComponentMap, DefaultComponentMap<TModel, TComponent>>();
      return services;
    }

    public static IServiceCollection AddComponentMap<TModel, TComponent, TContext>(this IServiceCollection services) where TComponent : IComponent
    {
      services.AddScoped<IContextualComponentMap>(p => new DefaultContextualComponentMap(typeof(TModel), typeof(TComponent), typeof(TContext)));
      return services;
    }

    public static IServiceCollection AddComponentMap<TContext, TModel, TComponent>(this IServiceCollection services, TContext Context) where TComponent : IComponent
    {
      services.AddScoped<IObjectComponentMap>(p => new DefaultObjectComponentMap(Context, typeof(TComponent), typeof(TContext)));
      return services;
    }

    //TODO: add AddComponentMap(this IServiceCollection services, Action<???> configure)
  }

  /// <summary>
  /// A base template-resolution component with common functionality for building the component that is resolved for a given model or context
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  public abstract class BaseComponentFor<TModel> : ComponentBase
  {
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; }

    private Type resolvedType { get; set; }

    public BaseComponentFor()
    {
      AdditionalAttributes = new Dictionary<string, object>();
    }

    protected abstract Type ResolveComponent();

    protected override void OnParametersSet()
    {
      base.OnParametersSet();
      if (resolvedType is null)
      {
        resolvedType = ResolveComponent();
      }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
      if (resolvedType is Type)
      {
        builder.OpenComponent(0, resolvedType);
        if (AdditionalAttributes.Any())
        {
          builder.AddMultipleAttributes(1, AdditionalAttributes);
        }
        builder.CloseComponent();
      }
    }
  }

  /// <summary>
  /// A standard non-generic template-resolution component used to get the template that has been registered for the provided model and context
  /// </summary>
  public class ComponentFor : ComponentBase
  {
    //TODO: Inject IEnumerable<ComponentResolver> and search all so that more flexible Custom resolvers can used when necessary
    [Inject]
    protected IComponentTypeResolver TypeResolver { get; set; }

    [Inject]
    protected ITypeContextualComponentTypeResolver TypeTypeResolver { get; set; }

    [Inject]
    protected IObjectContextualComponentTypeResolver ObjectTypeResolver { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; }

    [Parameter]
    public object ContextObject { get; set; }

    [Parameter]
    public object Model { get; set; }

    [Parameter]
    public Type[] AssignableFrom { get; set; }

    private Type resolvedType { get; set; }

    public ComponentFor()
    {
      AdditionalAttributes = new Dictionary<string, object>();
    }

    protected Type ResolveComponent()
    {
      Type result = null;
      //Don't try to resolve the template multiple times if the injected resolvers are the same object that just implements the different interfaces, like the DefaultComponentTypeResolver
      if (ContextObject is object)
      {
        result = ObjectTypeResolver.ResolveComponentTypeForObject(ContextObject, Model ?? default, AssignableFrom);
      }
      if (result is null && TypeTypeResolver != ObjectTypeResolver)
      {
        result = TypeTypeResolver.ResolveComponentTypeFor(Model ?? default);
      }
      if (result is null && TypeResolver != ObjectTypeResolver && TypeResolver != TypeTypeResolver)
      {
        result = TypeResolver.ResolveComponentTypeFor(Model ?? default);
      }
      return result;
    }

    protected override void OnParametersSet()
    {
      base.OnParametersSet();
      if (resolvedType is null)
      {
        resolvedType = ResolveComponent();
      }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
      if (resolvedType is Type)
      {
        builder.OpenComponent(0, resolvedType);
        if (AdditionalAttributes.Any())
        {
          builder.AddMultipleAttributes(1, AdditionalAttributes);
        }
        builder.CloseComponent();
      }
    }
  }

  /// <summary>
  /// Blazor components will not allow for generic overrides of components, so each generic Component must be in it's own namespace
  /// </summary>
  namespace Level1
  {
    /// <summary>
    /// A standard generic template-resolution component used to get the template that has been registered for the provided model and context
    /// </summary>
    /// <remarks>This is currently untested because our project does not have/require the infrastructure that this component would call for</remarks>
    public class ComponentFor<TModel> : BaseComponentFor<TModel>
    {
      [Inject]
      protected IComponentTypeResolver TypeResolver { get; set; }

      [Parameter]
      public TModel Model { get; set; }

      [Parameter]
      public Type[] AssignableFrom { get; set; }

      protected override Type ResolveComponent()
      {
        return TypeResolver.ResolveComponentTypeFor<TModel>(Model ?? default, AssignableFrom);
      }
    }
  }

  /// <summary>
  /// Blazor components will not allow for generic overrides of components, so each generic Component must be in it's own namespace
  /// </summary>
  namespace Level2
  {
    /// <summary>
    /// A standard generic template-resolution component used to get the template that has been registered for the provided model and context
    /// </summary>
    /// <remarks>This is currently untested because our project does not have/require the infrastructure that this component would call for</remarks>
    public class ComponentFor<TContext, TModel> : Level1.ComponentFor<TModel>
    {
      [Inject]
      private ITypeContextualComponentTypeResolver TypeTypeResolver { get; set; }

      [Inject]
      private IObjectContextualComponentTypeResolver ObjectTypeResolver { get; set; }

      [Parameter]
      public TContext ContextObject { get; set; }

      protected override Type ResolveComponent()
      {
        Type result = null;
        if (ContextObject is TContext)
        {
          result = ObjectTypeResolver.ResolveComponentTypeForObject<TContext, TModel>(ContextObject, Model ?? default, AssignableFrom);
        }
        if (result is null && TypeTypeResolver != ObjectTypeResolver)
        {
          result = TypeTypeResolver.ResolveComponentTypeForContext<TContext, TModel>(Model ?? default, AssignableFrom);
        }
        if (result is null && TypeResolver != ObjectTypeResolver && TypeResolver != TypeTypeResolver)
        {
          result = base.ResolveComponent(); //already uses the model if provided
        }
        return result;
      }
    }
  }
}
