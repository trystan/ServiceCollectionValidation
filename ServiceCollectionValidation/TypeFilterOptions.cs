using System.Reflection;

namespace ServiceCollectionValidation;

public class TypeFilterOptions
{
    /// <summary>
    /// What assemblies to search through.
    /// Defaults to <c>null</c>. When <c>null</c>, <c>System.AppDomain.CurrentDomain.GetAssemblies()</c> is used.
    /// </summary>
    public IEnumerable<Assembly>? Assemblies { get; set; }

    /// <summary>
    /// A filter function to apply to all potential types.
    /// </summary>
    /// <remarks>
    /// A common use case is to ensure the fully qualified type starts with the types in this repo to avoid including types from other dependencies.
    /// </remarks>
    /// <example>
    /// TypeFilter = (t) => t.FullyQualifiedName.StartsWith("MyCompany.MyProject");
    /// </example>
    public Func<Type, bool>? TypeFilter { get; set; }
}
