/*
 *  Copyright (C) 2018 - 2025 iSnackyCracky, NETertainer
 *
 *  This file is part of KeePassRDP.
 *
 *  KeePassRDP is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  KeePassRDP is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with KeePassRDP.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace KeePassRDP
{
    /// <summary>
    /// Represents <see cref="ISet{T}"/> which doesn't allow for items addition.
    /// </summary>
    /// <typeparam name="T">
    /// Type of items in the set.
    /// </typeparam>
    public interface IReadOnlySet<T> : ICollection<T>, ISet<T>
    {
    }

    /// <summary>
    /// Wrapper for an <see cref="ISet{T}"/> which allows lookup only.
    /// </summary>
    /// <typeparam name="T">
    /// Type of items in the set.
    /// </typeparam>
    public class ReadOnlySet<T> : IReadOnlySet<T>
    {
        /// <inheritdoc/>
        public int Count { get { return _set.Count; } }

        /// <inheritdoc/>
        public bool IsReadOnly { get { return true; } }

        private readonly ISet<T> _set;

        /// <summary>
        /// Creates new wrapper instance for given <see cref="ISet{T}"/>.
        /// </summary>
        public ReadOnlySet(ISet<T> set)
        {
            _set = set;
        }

        /// <inheritdoc/>
        public bool Contains(T i)
        {
            return _set.Contains(i);
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        bool ISet<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        /// <inheritdoc/>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        /// <inheritdoc/>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        /// <inheritdoc/>
        public bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        /// <inheritdoc/>
        public bool SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }
    }
}