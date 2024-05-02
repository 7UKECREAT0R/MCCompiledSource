using System;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.ServerWebSocket
{
    /// <summary>
    /// Implementations of all of the properties that should be handled by the compiler.
    /// </summary>
    internal static class PropertyImplementations
    {
        private static readonly List<PropertyImpl> ALL_PROPERTIES = new List<PropertyImpl>();

        /// <summary>
        /// Calls the <see cref="PropertyImpl"/> for the given property string, if one exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="currentProject"></param>
        internal static void TrySetProperty(string name, string value, MCCServerProject currentProject)
        {
            PropertyImpl find = ALL_PROPERTIES.FirstOrDefault(impl => impl.InvokeName.Equals(name));

            if (find == null)
                return;

            find.Call(value, currentProject);
        }
        /// <summary>
        /// Reset all property implementations and set the passed in project's properties to their defaults.
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

        static PropertyImplementations()
        {
            ALL_PROPERTIES.Add(new PropertyImpl("debug", true,
                (_value, project) =>
                {
                    if (!bool.TryParse(_value, out bool value))
                        return;

                    project.server.debug = value;
                    Program.DEBUG = value;

                    if (Program.DEBUG)
                        Console.WriteLine("Debug enabled by remote client.");
                    else
                        Console.WriteLine("Debug disabled by remote client.");
                }));
            ALL_PROPERTIES.Add(new PropertyImpl("decorate", false,
                (_value, project) =>
                {
                    if (!bool.TryParse(_value, out bool value))
                        return;

                    Program.DECORATE = value;
                }));
            ALL_PROPERTIES.Add(new PropertyImpl("export_all", false,
                (_value, project) =>
                {
                    if (!bool.TryParse(_value, out bool value))
                        return;

                    Program.EXPORT_ALL = value;
                }));
            ALL_PROPERTIES.Add(new PropertyImpl("ignore_manifests", false,
                (_value, project) =>
                {
                    if (!bool.TryParse(_value, out bool value))
                        return;

                    Program.IGNORE_MANIFESTS = value;
                }));
        }
    }
    /// <summary>
    /// An implementation of a compiler-specified property. Includes the action to perform when set, and the default value to become when reset.
    /// </summary>
    internal class PropertyImpl
    {
        private readonly string invokeName;
        private readonly string defaultValue;
        private readonly Action<string, MCCServerProject> invokeAction;

        internal PropertyImpl(string invokeName, object defaultValue, Action<string, MCCServerProject> invokeAction)
        {
            this.invokeName = invokeName;
            this.defaultValue = defaultValue.ToString().ToLower();
            this.invokeAction = invokeAction;
        }

        /// <summary>
        /// Returns the name used to invoke this <see cref="PropertyImpl"/>.
        /// </summary>
        internal string InvokeName { get => this.invokeName; }
        /// <summary>
        /// Returns the default value that this property should be.
        /// </summary>
        internal string DefaultValue { get => this.defaultValue; }
        /// <summary>
        /// Call the action on this <see cref="PropertyImpl"/> with the given value.
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
        /// Resets this PropertyImpl to its default value.
        /// </summary>
        /// <param name="project"></param>
        internal void Reset(MCCServerProject project)
        {
            Call(this.defaultValue, project);
        }
    }
}
