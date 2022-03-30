using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Manages advanced spawned entities for this project. Allows searching and selecting.
    /// </summary>
    public class EntityManager
    {
        internal readonly List<ISelectorProvider> allProviders;
        public readonly NullManager nulls;

        public EntityManager(Executor parent)
        {
            nulls = new NullManager(parent);

            allProviders = new List<ISelectorProvider>()
            {
                nulls
            };
        }
        /// <summary>
        /// Returns if any managed entities have this name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasEntity(string name)
        {
            int hash = name.GetHashCode();

            return allProviders.Any(provider =>
                provider.HasEntity(hash));
        }
        /// <summary>
        /// Search for a managed entity by name and return its selector.
        /// </summary>
        /// <param name="name">The name of the entity to search for.</param>
        /// <param name="selector">The selector for this entity.</param>
        /// <returns>If the entity was found and "selector" was set.</returns>
        public bool Search(string name, out Commands.Selector selector)
        {
            foreach (ISelectorProvider provider in allProviders)
            {
                if (provider.Search(name, out selector))
                    return true;
            }

            selector = null;
            return false;
        }
    }

    /// <summary>
    /// Provides selectors in exchange for entity names.
    /// </summary>
    internal interface ISelectorProvider
    {
        bool HasEntity(int hash);
        bool Search(string name, out Commands.Selector selector);
    }
}
