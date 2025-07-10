using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinyDialogsNet;

namespace mc_compiled.MCC.ServerWebSocket;

/// <summary>
///     Defines the active project being modified by the server.
/// </summary>
internal class MCCServerProject
{
    /// <summary>
    ///     The properties for this project.
    /// </summary>
    internal readonly Dictionary<string, string> properties = new();
    public readonly MCCServer server;
    internal string fileDirectory; // working directory for the project
    internal string fileLocation; // output file for the project
    internal bool hasFile; // if we have a file yet
    internal MCCServerProject(MCCServer server)
    {
        this.server = server;
        this.hasFile = false;
        this.fileLocation = null;
        this.fileDirectory = null;

        ResetProperties();
    }
    /// <summary>
    ///     Get or set by the base64 representation of the project's properties.
    /// </summary>
    internal string PropertiesBase64
    {
        get
        {
            IEnumerable<JProperty> props = this.properties
                .Select(kv => new JProperty(kv.Key, kv.Value));

            var json = new JObject();

            foreach (JProperty prop in props)
                json.Add(prop);

            return json.ToString(Formatting.None).Base64Encode();
        }

        set
        {
            if (string.IsNullOrEmpty(value))
                return;

            JObject json = JObject.Parse(value.Base64Decode());

            ResetProperties();

            foreach (JProperty property in json.Properties())
            {
                string propertyName = property.Name;
                string propertyValue = property.Value.ToString();
                SetProperty(propertyName, propertyValue);
            }
        }
    }

    /// <summary>
    ///     Get:<br />
    ///     Returns {path}{name}.{ext} or null if none has been set yet.<br />
    ///     <br />
    ///     Set:<br />
    ///     Sets file {path}{name}.{ext} and sets fileLocation,<br />
    ///     fileDirectory, and sets application-wide active directory
    /// </summary>
    internal string File
    {
        get => this.fileLocation;
        set
        {
            this.hasFile = value != null;
            this.fileLocation = value;
            this.fileDirectory = Path.GetDirectoryName(value);

            // set the working directory
            if (this.fileDirectory != null)
                Directory.SetCurrentDirectory(this.fileDirectory);
        }
    }

    /// <summary>
    ///     Sets a project property and invokes any compiler-implemented properties.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="value"></param>
    internal void SetProperty(string property, string value)
    {
        this.properties[property] = value;
        PropertyImplementations.TrySetProperty(property, value, this);
    }
    /// <summary>
    ///     Resets all the properties in this project and their implementations.
    /// </summary>
    internal void ResetProperties()
    {
        this.properties.Clear();

        // this method call sets `properties` back to what their defaults should be
        PropertyImplementations.ResetAll(this);
    }
    /// <summary>
    ///     Gets and returns a property's current value.
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    internal string GetProperty(string property)
    {
        if (this.properties.TryGetValue(property, out string value))
            return value;
        return null;
    }

    /// <summary>
    ///     Allows the user to choose a location/filename to write their project to.
    /// </summary>
    /// <returns>True if they chose a file, false if they canceled.</returns>
    internal bool RunSaveFileDialog(out bool unsupported, string defaultName = "web_project.mcc")
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version.Major < 6)
        {
            unsupported = true;
            return false;
        }

        unsupported = false;

        (bool canceled, string path) = TinyDialogs.SaveFileDialog("Saving code...", defaultName,
            new FileFilter("MCCompiled File (.mcc)", ["*.mcc"])
        );

        if (canceled)
            return false;

        this.File = path;
        return true;
    }
    /// <summary>
    ///     Allows the user to choose a location/filename to write their project to.
    /// </summary>
    /// <returns>True if they chose a file, false if they canceled.</returns>
    internal bool RunLoadFileDialog(out bool unsupported)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version.Major < 6)
        {
            unsupported = true;
            return false;
        }

        unsupported = false;

        (bool canceled, IEnumerable<string> paths) = TinyDialogs.OpenFileDialog("Loading code...", "/", false,
            new FileFilter("MCCompiled File (.mcc)", ["*.mcc"])
        );

        string[] pathsArray = paths as string[] ?? paths.ToArray();

        if (canceled || pathsArray.Length < 1)
            return false;

        this.File = pathsArray[0];
        return true;
    }
}