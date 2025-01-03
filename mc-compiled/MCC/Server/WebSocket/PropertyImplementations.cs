using System;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.ServerWebSocket;

/// <summary>
///     Implementations of all the properties that should be handled by the compiler.
/// </summary>
internal static class PropertyImplementations
{
    private static readonly List<PropertyImpl> ALL_PROPERTIES = [];

    static PropertyImplementations()
    {
        ALL_PROPERTIES.Add(new PropertyImpl("debug", true,
            (_value, project) =>
            {
                if (!bool.TryParse(_value, out bool value))
                    return;

                project.server.debug = value;
                GlobalContext.Current.debug = value;

                Console.WriteLine(value ? "Debug enabled by remote client." : "Debug disabled by remote client.");
            }));
        ALL_PROPERTIES.Add(new PropertyImpl("decorate", false,
            (_value, _) =>
            {
                if (!bool.TryParse(_value, out bool value))
                    return;

                GlobalContext.Current.decorate = value;
            }));
        ALL_PROPERTIES.Add(new PropertyImpl("export_all", false,
            (_value, _) =>
            {
                if (!bool.TryParse(_value, out bool value))
                    return;

                GlobalContext.Current.exportAll = value;
            }));
        ALL_PROPERTIES.Add(new PropertyImpl("ignore_manifests", false,
            (_value, _) =>
            {
                if (!bool.TryParse(_value, out bool value))
                    return;

                GlobalContext.Current.ignoreManifests = value;
            }));
    }

    /// <summary>
    ///     Calls the <see cref="PropertyImpl" /> for the given property string, if one exists.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="currentProject"></param>
    internal static void TrySetProperty(string name, string value, MCCServerProject currentProject)
    {
        PropertyImpl find = ALL_PROPERTIES.FirstOrDefault(impl => impl.InvokeName.Equals(name));
        find?.Call(value, currentProject);
    }
    /// <summary>
    ///     Reset all property implementations and set the passed in project's properties to their defaults.
    /// </summary>
    /// <param name="currentProject"></param>
    internal static void ResetAll(MCCServerProject currentProject)
    {
        foreach (PropertyImpl property in ALL_PROPERTIES)
        {
            property.Reset(currentProject);
            currentProject.properties[property.InvokeName] = property.DefaultValue;
        }
    }
}

/// <summary>
///     An implementation of a compiler-specified property. Includes the action to perform when set, and the default value
///     to become when reset.
/// </summary>
internal class PropertyImpl
{
    private readonly Action<string, MCCServerProject> invokeAction;

    internal PropertyImpl(string invokeName, object defaultValue, Action<string, MCCServerProject> invokeAction)
    {
        this.InvokeName = invokeName;
        this.DefaultValue = defaultValue.ToString().ToLower();
        this.invokeAction = invokeAction;
    }

    /// <summary>
    ///     Returns the name used to invoke this <see cref="PropertyImpl" />.
    /// </summary>
    internal string InvokeName { get; }
    /// <summary>
    ///     Returns the default value that this property should be.
    /// </summary>
    internal string DefaultValue { get; }
    /// <summary>
    ///     Call the action on this <see cref="PropertyImpl" /> with the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="project"></param>
    /// <exception cref="NullReferenceException"></exception>
    internal void Call(string value, MCCServerProject project)
    {
        if (this.invokeAction == null)
            throw new NullReferenceException("Attempted to call a PropertyImpl without a valid invokeAction.");
        this.invokeAction(value, project);
    }
    /// <summary>
    ///     Resets this PropertyImpl to its default value.
    /// </summary>
    /// <param name="project"></param>
    internal void Reset(MCCServerProject project) { Call(this.DefaultValue, project); }
}