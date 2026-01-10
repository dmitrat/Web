using OutWit.Web.Framework.Components.Content;

namespace OutWit.Web.Framework.Services;

/// <summary>
/// Registry mapping component type names to Blazor component types.
/// Add new components here to make them available for embedding in content.
/// </summary>
public class ComponentRegistry
{
    #region Fields

    private readonly Dictionary<string, Type> m_components = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Constructors

    public ComponentRegistry()
    {
        // Register built-in components
        Register("YouTube", typeof(YouTube));
        Register("FloatingImage", typeof(FloatingImage));
        
        // Add more components here:
        // Register("PowerPoint", typeof(PowerPoint));
        // Register("Model3D", typeof(Model3D));
    }

    #endregion

    #region Functions

    /// <summary>
    /// Register a component type.
    /// </summary>
    public void Register(string typeName, Type componentType)
    {
        m_components[typeName] = componentType;
    }

    /// <summary>
    /// Get the Blazor component type for a given type name.
    /// </summary>
    public Type? GetComponentType(string typeName)
    {
        return m_components.GetValueOrDefault(typeName);
    }

    /// <summary>
    /// Check if a component type is registered.
    /// </summary>
    public bool IsRegistered(string typeName)
    {
        return m_components.ContainsKey(typeName);
    }

    /// <summary>
    /// Get all registered component type names.
    /// </summary>
    public IEnumerable<string> GetRegisteredTypes()
    {
        return m_components.Keys;
    }

    #endregion
}
