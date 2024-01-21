using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A preprocessor variable.
    /// </summary>
    public sealed class PreprocessorVariable : PreprocessorVariableRaw<dynamic>
    {
        public PreprocessorVariable(params dynamic[] items) : base(items) {}
        
        // ReSharper disable once UnusedMember.Local
        private PreprocessorVariable() {}

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
        
        [PublicAPI]
        public void SetAll(params T[] items)
        {
            if (items.Length == 0)
                throw new Exception("Cannot fill preprocessor variable with 0 items.");

            this.items = items;
            Length = items.Length;
            Capacity = items.Length;
        }
        /// <summary>
        /// Sets all the items of the preprocessor variable with the specified items.
        /// </summary>
        /// <typeparam name="T">The type of items.</typeparam>
        /// <param name="other">The PreprocessorVariableRaw object containing the items to set.</param>
        /// <exception cref="Exception">Thrown when the items array is empty.</exception>
        [PublicAPI]
        public void SetAll(PreprocessorVariableRaw<T> other)
        {
            if (other.Length <= Capacity)
                CopyItemsAndSetLength(other);
            else
            {
                items = new T[other.Length];
                Capacity = other.Length;
                CopyItemsAndSetLength(other);
            }
        }
        private void CopyItemsAndSetLength(PreprocessorVariableRaw<T> source)
        {
            Array.Copy(source.items, items, source.Length);
            Length = source.Length;
        }

        /// <summary>
        /// Adds an item to the end of the collection.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Append(T item)
        {
            if (Length == Capacity)
            {
                if (Capacity == 0)
                    Capacity = 1;
                else
                    Capacity *= 2;
                
                Array.Resize(ref items, Capacity);
            }
            items[Length] = item;
            Length++;
        }
        /// <summary>
        /// Adds an item to the start of the collection.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Prepend(T item)
        {
            if (Length == Capacity)
            {
                if (Capacity == 0)
                    Capacity = 1;
                else
                    Capacity *= 2;
                
                Array.Resize(ref items, Capacity);
            }
            
            Array.Copy(items, 0, items, 1, Length);
            items[0] = item;
            Length++;
        }
        
        /// <summary>
        /// Appends a range of items to the existing items in the array.
        /// </summary>
        /// <typeparam name="T">The type of items in the array.</typeparam>
        /// <param name="_items">The range of items to append.</param>
        public void AppendRange(IEnumerable<T> _items)
        {
            T[] items = _items.ToArray();
            int length = items.Length;
            int resultLength = Length + length;

            if (resultLength > Capacity)
            {
                if (Capacity == 0)
                    Capacity = 1;

                while (resultLength > Capacity)
                    Capacity *= 2;
                
                Array.Resize(ref this.items, Capacity);
            }
            
            Array.Copy(items, 0, this.items, Length, length);
            Length = resultLength;
        }
        /// <summary>
        /// Prepends a collection of items to the beginning of the current collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="_items">The collection to prepend to the current collection.</param>
        public void PrependRange(IEnumerable<T> _items)
        {
            T[] items = _items.ToArray();
            int length = items.Length;
            int resultLength = Length + length;

            if (resultLength > Capacity)
            {
                if (Capacity == 0)
                    Capacity = 1;

                while (resultLength > Capacity)
                    Capacity *= 2;
                
                Array.Resize(ref this.items, Capacity);
            }
            
            // move all elements right in this.items 'length'
            Array.Copy(this.items, 0, this.items, length, Length);
            // insert items to prepend
            Array.Copy(items, 0, this.items, 0, length);
            Length = resultLength;
        }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the index is outside the valid range.</exception>
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