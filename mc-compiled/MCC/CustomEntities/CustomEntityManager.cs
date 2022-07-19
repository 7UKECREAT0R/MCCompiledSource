using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.CustomEntities
{
    /// <summary>
    /// Provides a means of managing the creation of a custom entity.
    /// </summary>
    internal abstract class CustomEntityManager : ISelectorProvider
    {
        internal readonly Executor parent;
        internal bool createdEntityFiles;

        internal CustomEntityManager(Executor parent)
        {
            this.parent = parent;
        }
        internal void AddEntityToProject()
        {
            if (createdEntityFiles)
                return;

            parent.AddExtraFiles(CreateEntityFiles());
            createdEntityFiles = true;
            return;
        }

        internal abstract IAddonFile[] CreateEntityFiles();
        public abstract bool HasEntity(string name);
        public abstract bool Search(string name, out Commands.Selectors.Selector selector);
    }
}