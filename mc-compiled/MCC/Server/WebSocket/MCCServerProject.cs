using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace mc_compiled.MCC.ServerWebSocket
{
    /// <summary>
    /// Defines the active project being modified by the server.
    /// </summary>
    internal class MCCServerProject
    {
        public readonly MCCServer server;
        internal bool hasFile;              // if we have a file yet
        internal string fileLocation;       // output file for the project
        internal string fileDirectory;      // working directory for the project

        /// <summary>
        /// The properties for this project.
        /// </summary>
        internal Dictionary<string, string> properties = new Dictionary<string, string>();
        /// <summary>
        /// Get or set by the base64 representation of the project's properties.
        /// </summary>
        internal string PropertiesBase64
        {
            get
            {
                IEnumerable<JProperty> props = this.properties
                    .Select(kv => new JProperty(kv.Key, kv.Value));

                JObject json = new JObject();

                foreach (JProperty prop in props)
                    json.Add(prop);

                return json.ToString(Newtonsoft.Json.Formatting.None).Base64Encode();
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
        /// Sets a project property and invokes any compiler-implemented properties.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        internal void SetProperty(string property, string value)
        {
            this.properties[property] = value;
            PropertyImplementations.TrySetProperty(property, value, this);
        }
        /// <summary>
        /// Resets all of the properties in this project and their implementations.
        /// </summary>
        internal void ResetProperties()
        {
            this.properties.Clear();

            // this method call sets `properties` back to what their defaults should be
            PropertyImplementations.ResetAll(this);
        }
        /// <summary>
        /// Gets and returns a property's current value.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal string GetProperty(string property)
        {
            if(this.properties.TryGetValue(property, out string value))
                return value;
            return null;
        }

        /// <summary>
        /// Get:<br />
        ///     Returns {path}{name}.{ext} or null if none has been set yet.<br />
        /// <br />
        /// Set:<br />
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

                // set working directory
                if(this.fileDirectory != null)
                    Directory.SetCurrentDirectory(this.fileDirectory);
            }
        }
        internal MCCServerProject(MCCServer server)
        {
            this.server = server;
            this.hasFile = false;
            this.fileLocation = null;
            this.fileDirectory = null;

            ResetProperties();
        }


        /// <summary>
        /// Allows the user to choose a location/filename to write their project to.
        /// </summary>
        /// <returns>True if they chose a file, false if they cancelled.</returns>
        internal bool RunSaveFileDialog(string defaultName = "web_project.mcc")
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "MCC Source File (.mcc)|*.mcc";
                dialog.AddExtension = true;
                dialog.DefaultExt = "mcc";
                dialog.DereferenceLinks = true;
                dialog.Title = "Save project...";
                dialog.FileName = defaultName;

                using (Form zSetter = new Form())
                {
                    zSetter.TopMost = true;
                    zSetter.TopLevel = true;
                    zSetter.WindowState = FormWindowState.Minimized;
                    zSetter.Show();
                    
                    bool selected = dialog.ShowDialog(zSetter) == DialogResult.OK;

                    if (!selected)
                        return false;
                    
                    // did choose file
                    this.File = dialog.FileName;
                    return true;
                }
            }
        }
        /// <summary>
        /// Allows the user to choose a location/filename to write their project to.
        /// </summary>
        /// <returns>True if they chose a file, false if they cancelled.</returns>
        internal bool RunLoadFileDialog()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "MCC Source File (.mcc)|*.mcc";
                dialog.CheckPathExists = true;
                dialog.CheckFileExists = true;
                dialog.DereferenceLinks = true;
                dialog.Title = "Load project...";
                dialog.FileName = "web_project.mcc";

                using (Form zSetter = new Form())
                {
                    zSetter.TopMost = true;
                    zSetter.TopLevel = true;
                    zSetter.WindowState = FormWindowState.Minimized;
                    zSetter.Show();
                    
                    bool selected = dialog.ShowDialog(zSetter) == DialogResult.OK;

                    if (!selected)
                        return false;
                    
                    // did choose file
                    this.File = dialog.FileName;
                    return true;
                }
            }
        }

    }
}
