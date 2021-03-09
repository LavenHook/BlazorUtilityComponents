# ComponentFor usage
## Register Template Types
Register the default template resolution mechanism
```cs
services.AddDefaultComponentMapper();
```

### Register your templates:
For a general component:
```cs
services.AddComponentMap<MyCustomDataModelType, MyCustomComponentType>()();
```
This is useful for resolving a component that does not typically change based on any context specifics, such as the parent container.

For a context-type specific component:
```cs
services.AddComponentMap<MyCustomDataModelType, MyCustomComponentType, SpecificParentComponentType>();
```
**NOTE:** the `TContext` (3rd type parameter type) that is provided isn't actually limited to IComponents. It could be any type, as long as that same type is provide when using the `<ComponentFor ... **TContext="SpecificParentComponentType"** >`

For a context-instance specific component:
```cs
services.AddComponentMap<MyCustomDataModelType, MyCustomComponentType, SpecificParentComponentType>(SpecificParentComponentType InstanceOfSpecificParentComponentType);
```
**NOTE:** the InstanceOfSpecificParentComponentType that is provided isn't actually limited to IComponents. It could be any type, as long as that same type is provide when using the `<ComponentFor ... **ContextObject="InstanceOfSpecificParentComponentType"** >`

## Resolve Template
Resolve a general component
```razor
<ComponentFor Model="@MyCustomDataModel" AdditionalAttributes* />
```

Resolve a context-type specific component:
```razor
<ComponentFor TModel="@MyCustomDataModelType" TContext="@SpecificParentComponentType" AdditionalAttributes* />
```

Resolve a context-instance specific component:
```razor
<ComponentFor TModel="@MyCustomDataModelType" ContextObject="@this**" AdditionalAttributes*  />
```

### Template Resolution Notes
*AdditionalAttributes: in order to ensure that the resolved component has the necessary settable parameters, an additional attribute `AssignableFrom` can be provided. For example, to resolve a component for a `Person` model, you may want to create a base abstract IComponent `PersonComponent` that has a `[Parameter] public Person PersonData;`. Such a component can be ensured to return bu using 
```razor
<ComponentFor ... AssignableFrom="new Type[] { typeof(PersonComponent) }" @bind-PersonData="Person" />
```
**The ContextObject that is provided would typically be the parent component, i.e. `@this`, but circumstances may exist where that is not the desired restriction. Any such object could be used for the ContextObject parameter. As such, the default registration resolver will resolve the correct component if that object was provided at registration time.
```razor
services.AddComponentMap<Person, PersonComponent, CustomWackAssType>(MyCustomWackAssTypeInstance)
```

Last, you can also use components that have `[Parameter] RenderFragment ChildComponent;` properties. When this is desired, it would be best practice to follow the notes for the AdditionalAttributes and AssignableFrom concepts.
___
## Customization
This templating mechanism has been build for customization. The main customization point is the I...Resolver types that are registered when `services.AddDefaultComponentMapper()` is called. ComponentFor is injected with `IObjectContextualComponentTypeResolver` (the default being `DefaultComponentTypeResolver`), which implements resolvers for the the AddComponentMap registrations. Future versions of this templating mechanism should probably allow more thorough overriding of this, but for now the only injection that is necessary into ComponentFor is the IObjectContextualComponentTypeResolver. To customize the way that ComponentFor resolves components, override the registered IObjectContextualComponentTypeResolver:
```cs
services.AddScoped<IObjectContextualComponentTypeResolver>(new CustomerComponentResolver());
```
In furutre version I may try to incorporate flexibility to do the same for IComponentTypeResolver and ITypeContextualComponentTypeResolver, but it's not necessary for my current needs.
##### TODO:
Implement:
```cs
services.AddScoped<ITypeContextualComponentTypeResolver>(p => p.GetService<DefaultComponentTypeResolver>());
```
```cs
services.AddScoped<IObjectContextualComponentTypeResolver>(p => p.GetService<DefaultComponentTypeResolver>());
```

## Notes
Examples that I have used:

Where the original component was  
```razor
<PhysicalAddressComponent Title="Physical Address" @bind-Address="ViewModel.PhysicalAddress" />
```
I replaced it with:
`<ComponentFor Title="Physical Address" Model="@ViewModel.PhysicalAddress" ContextObject="@this" @bind-Address="ViewModel.PhysicalAddress"  />`
`<Level1.ComponentFor Title="Physical Address" TModel="@PhysicalAddress" @bind-Address="ViewModel.PhysicalAddress"  />`
`<Level2.ComponentFor Title="Physical Address" TModel="@PhysicalAddress" ContextObject="@this" TContext="ParentComponentType" @bind-Address="ViewModel.PhysicalAddress"  />`
or for an original component 
```razor
<Printing.DocumentPrintManager @bind-PrintManager="pm">
            <ChildComponent Attributes="ChildAttributes" />
          </Printing.DocumentPrintManager>
```
I Replaced it with:
```razor
<ComponentFor @bind-PrintManager="@printManager" Lead="@LeadState.Lead" ContextObject="@this" Model="@printManager" AssignableFrom="@(new  ype[] { typeof(ViewModels.DocumentPrintManager) })">
        <ChildComponent Attributes="ChildAttributes" />
      </ComponentFor>
```