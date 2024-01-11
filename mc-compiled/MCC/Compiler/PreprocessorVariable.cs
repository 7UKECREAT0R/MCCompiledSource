using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A container holding multiple dynamic values with support for reallocation like a list when needed.
    /// Think of this like a hybrid between a dynamic[] and List[dynamic].
    /// </summary>
    public class PreprocessorVariable
    {
        private dynamic[] items;
        
        [PublicAPI]
        public int Length { get; private set; }
        [PublicAPI]
        public int Capacity { get; private set; }
        
        public PreprocessorVariable(params dynamic[] items)
        {
            if (items.Length == 0)
                throw new Exception("Attempted to create empty PreprocessorVariable.");
            
            this.items = items;
            Length = items.Length;
            Capacity = items.Length;
        }

        /// <summary>
        /// Retrieves the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the index is out of range.</exception>
        public dynamic Get(int index)
        {
            if (index >= Length)
                throw new IndexOutOfRangeException();
            return items[index];
        }
        /// <summary>
        /// Sets the value of an item at a specific index in the array.
        /// </summary>
        /// <param name="index">The index at which to set the item.</param>
        /// <param name="item">The item to be set.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
        public void Set(int index, dynamic item)
        {
            if (index >= Length)
                throw new IndexOutOfRangeException();
            items[index] = item;
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(dynamic item)
        {
            if (Length == Capacity)
            {
                Capacity *= 2;
                Array.Resize(ref items, Capacity);
            }
            items[Length] = item;
            Length++;
        }
        /// <summary>
        /// Removes an element at the specified index from the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when the index is less than zero or greater than or equal to the length of the collection.
        /// </exception>
        public void Remove(int index)
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();
            
            for (int i = index; i < Length - 1; i++)
            {
                items[i] = items[i + 1];
            }
            Length--;
        }
    }
}