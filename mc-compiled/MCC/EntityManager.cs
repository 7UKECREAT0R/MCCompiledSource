using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.CustomEntities;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Manages advanced spawned entities for this project. Allows searching and selecting.
    /// </summary>
    public class EntityManager
    {
        private readonly List<ISelectorProvider> allProviders;
        internal readonly DummyManager dummies;
        internal readonly ExploderManager exploders;

        public EntityManager(Executor executor)
        {
            this.dummies = new DummyManager(executor);
            this.exploders = new ExploderManager(executor);

            this.allProviders = new List<ISelectorProvider>()
            {
                this.dummies, this.exploders
            };
        }
        /// <summary>
        /// Returns if any managed entities have this name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasEntity(string name)
        {
            return this.allProviders.Any(provider =>
                provider.HasEntity(name));
        }
        /// <summary>
        /// Search for a managed entity by name and return its selector.
        /// </summary>
        /// <param name="name">The name of the entity to search for.</param>
        /// <param name="selector">The selector for this entity.</param>
        /// <returns>If the entity was found and "selector" was set.</returns>
        public bool Search(string name, out Commands.Selectors.Selector selector)
        {
            foreach (ISelectorProvider provider in this.allProviders)
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
        /// <summary>
        /// Returns if a managed entity exists with a name.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns></returns>
        bool HasEntity(string name);
        /// <summary>
        /// Search and output a selector based on a managed entity name. Returns true if a match was found.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        bool Search(string name, out Commands.Selectors.Selector selector);
        
    }
}
