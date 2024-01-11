using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A preprocessor variable.
    /// </summary>
    public class PreprocessorVariable : PreprocessorVariableRaw<dynamic>
    {
        public PreprocessorVariable(params dynamic[] items) : base(items) {}

        /// <summary>
        /// Creates a shallow copy of this preprocessor variable, copying the internal array but not the items.
        /// </summary>
        public PreprocessorVariable Clone()
        {
            dynamic[] clonedItems = new dynamic[Length];
            Array.Copy(items, clonedItems, Length);
            return new PreprocessorVariable(clonedItems);
        }
    }
    
    /// <summary>
    /// A container holding multiple T values with support for reallocation like a list when needed.
    /// Think of this like a hybrid between a T[] and List[T].
    /// </summary>
    public class PreprocessorVariableRaw<T> : IEnumerable<T>
    {
        protected T[] items;
        
        [PublicAPI]
        public int Length { get; private set; }
        [PublicAPI]
        public int Capacity { get; private set; }
        
        public PreprocessorVariableRaw(params T[] items)
        {
            if (items.Length == 0)
                throw new Exception("Cannot create empty PreprocessorVariable.");
            
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
        private T Get(int index)
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
        private void Set(int index, T item)
        {
            if (index >= Length)
                throw new IndexOutOfRangeException();
            items[index] = item;
        }
        /// <summary>
        /// Sets all the items of the preprocessor variable with the specified items.
        /// </summary>
        /// <typeparam name="T">The type of items.</typeparam>
        /// <param name="items">The items to set in the preprocessor variable.</param>
        /// <exception cref="Exception">Thrown when the items array is empty.</exception>
        public void SetAll(params T[] items)
        {
            if (items.Length == 0)
                throw new Exception("Cannot fill preprocessor variable with 0 items.");

            this.items = items;
            Length = items.Length;
            Capacity = items.Length;
        }
        
        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(T item)
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
        
        public T this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return items[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}