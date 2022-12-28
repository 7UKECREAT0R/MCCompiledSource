using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mc_compiled.MCC.Server
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
        /// Get:<br />
        ///     Returns {path}{name}.{ext} or null if none has been set yet.<br />
        /// <br />
        /// Set:<br />
        ///     Sets file {path}{name}.{ext} and sets fileLocation,<br />
        ///     fileDirectory, and sets application-wide active directory
        /// </summary>
        internal string File
        {
            get => fileLocation;
            set
            {
                this.hasFile = value != null;
                this.fileLocation = value;
                this.fileDirectory = Path.GetDirectoryName(value);

                // set working directory
                Directory.SetCurrentDirectory(this.fileDirectory);
            }
        }
        internal MCCServerProject(MCCServer server)
        {
            this.server = server;
            this.hasFile = false;
            this.fileLocation = null;
            this.fileDirectory = null;
        }


        /// <summary>
        /// Allows the user to choose a location/filename to write their project to.
        /// </summary>
        /// <returns>True if they chose a file, false if they cancelled.</returns>
        internal bool RunSaveFileDialog()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "MCC Source File (.mcc)|*.mcc";
                dialog.AddExtension = true;
                dialog.DefaultExt = "mcc";
                dialog.DereferenceLinks = true;
                dialog.Title = "Save project...";
                dialog.FileName = "web_project.mcc";

                bool selected = dialog.ShowDialog() == DialogResult.OK;

                if (selected)
                {
                    // did choose file
                    this.File = dialog.FileName;
                    return true;
                }

                // didn't choose file
                return false;
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

                bool selected = dialog.ShowDialog() == DialogResult.OK;

                if (selected)
                {
                    // did choose file
                    this.File = dialog.FileName;
                    return true;
                }

                // didn't choose file
                return false;
            }
        }

    }
}
