using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding;

namespace mc_compiled.MCC.CustomEntities
{
    /// <summary>
    /// Provides a means of managing the creation of a custom entity.
    /// </summary>
    internal abstract class CustomEntityManager : ISelectorProvider
    {
        private readonly Executor parent;
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
        }

        protected abstract IEnumerable<IAddonFile> CreateEntityFiles();
        public abstract bool HasEntity(string name);
        public abstract bool Search(string name, out Commands.Selectors.Selector selector);
    }
}