﻿using Lucene.Net.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Lucene.Net
{
    public static class Collections
    {
        public static ISet<T> Singleton<T>(T o)
        {
            return ImmutableHashSet.Create(o); // LUCENENET TODO: Immutable != Singleton
        }

        public static IList<T> EmptyList<T>()
        {
            return ImmutableList<T>.Empty; // LUCENENET TODO: Shouldn't be Immutable
        }

        public static IList<T> UnmodifiableList<T>(IEnumerable<T> items)
        {
            return ImmutableList.Create<T>(items.ToArray()); // LUCENENET TODO: Immutable != Unmodifiable
        }

        public static IList<T> UnmodifiableList<T>(List<T> items)
        {
            return items.AsReadOnly();
        }

        public static ISet<T> UnmodifiableSet<T>(IEnumerable<T> items)
        {
            return ImmutableHashSet.Create<T>(items.ToArray()); // LUCENENET TODO: Immutable != Unmodifiable
        }

        public static IDictionary<TKey, TValue> UnmodifiableMap<TKey, TValue>(IDictionary<TKey, TValue> d)
        {
            return new UnmodifiableDictionary<TKey, TValue>(d);
        }

        public static IDictionary<TKey, TValue> SingletonMap<TKey, TValue>(TKey key, TValue value)
        {
            return new Dictionary<TKey, TValue> {{key, value}};
        }

        public static ISet<T> NewSetFromMap<T, S>(IDictionary<T, bool?> map)
        {
            return new SetFromMap<T>(map);
        }

        internal class SetFromMap<T> : ICollection<T>, IEnumerable<T>, IEnumerable, ISet<T>, IReadOnlyCollection<T>
#if FEATURE_SERIALIZABLE
            ,ISerializable, IDeserializationCallback
#endif
        {
            private readonly IDictionary<T, bool?> m; // The backing map
#if FEATURE_SERIALIZABLE
            [NonSerialized]
#endif
            private ICollection<T> s;

            internal SetFromMap(IDictionary<T, bool?> map)
            {
                if (map.Any())
                    throw new ArgumentException("Map is not empty");
                m = map;
                s = map.Keys;
            }

            public void Clear()
            {
                m.Clear();
            }

            public int Count
            {
                get
                {
                    return m.Count;
                }
            }

            // LUCENENET: IsEmpty doesn't exist here

            public bool Contains(T item)
            {
                return m.ContainsKey(item);
            }

            public bool Remove(T item)
            {
                return m.Remove(item);
            }

            public bool Add(T item)
            {
                m.Add(item, true);
                return m.ContainsKey(item);
            }

            void ICollection<T>.Add(T item)
            {
                m.Add(item, true);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return s.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return s.GetEnumerator();
            }

            // LUCENENET: ToArray() is part of LINQ

            public override string ToString()
            {
                return s.ToString();
            }

            public override int GetHashCode()
            {
                return s.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj == this || s.Equals(obj);
            }

            public virtual bool ContainsAll(IEnumerable<T> other)
            {
                // we don't care about order, so sort both sequences before comparing
                return this.OrderBy(x => x).SequenceEqual(other.OrderBy(x => x));
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                m.Keys.CopyTo(array, arrayIndex);
            }


            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public bool SetEquals(IEnumerable<T> other)
            {
                if (other == null)
                {
                    throw new ArgumentNullException("other");
                }
                SetFromMap<T> set = other as SetFromMap<T>;
                if (set != null)
                {
                    if (this.m.Count != set.Count)
                    {
                        return false;
                    }
                    return this.ContainsAll(set);
                }
                ICollection<T> is2 = other as ICollection<T>;
                if (((is2 != null) && (this.m.Count == 0)) && (is2.Count > 0))
                {
                    return false;
                }
                foreach (var item in this)
                {
                    if (!is2.Contains(item))
                    {
                        return false;
                    }
                }
                return true;
            }

#region Not Implemented Members
            public void ExceptWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void IntersectWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsSubsetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsSupersetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool Overlaps(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void SymmetricExceptWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void UnionWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

#if FEATURE_SERIALIZABLE
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException();
            }
#endif

            public void OnDeserialization(object sender)
            {
                throw new NotImplementedException();
            }
#endregion
        }

        public static IComparer<T> ReverseOrder<T>()
        {
            return (IComparer<T>)ReverseComparer.REVERSE_ORDER;
        }

        private class ReverseComparer : IComparer<IComparable>
        {
            internal static readonly ReverseComparer REVERSE_ORDER = new ReverseComparer();


            public int Compare(IComparable c1, IComparable c2)
            {
                return c2.CompareTo(c1);
            }
        }

        public static IComparer<T> ReverseOrder<T>(IComparer<T> cmp)
        {
            if (cmp == null)
                return ReverseOrder<T>();

            if (cmp is ReverseComparer2<T>)
                return ((ReverseComparer2<T>)cmp).cmp;

            return new ReverseComparer2<T>(cmp);
        }

        private class ReverseComparer2<T> : IComparer<T>

        {
            /**
             * The comparer specified in the static factory.  This will never
             * be null, as the static factory returns a ReverseComparer
             * instance if its argument is null.
             *
             * @serial
             */
            internal readonly IComparer<T> cmp;

            public ReverseComparer2(IComparer<T> cmp)
            {
                Debug.Assert(cmp != null);
                this.cmp = cmp;
            }

            public int Compare(T t1, T t2)
            {
                return cmp.Compare(t2, t1);
            }

            public override bool Equals(object o)
            {
                return (o == this) ||
                    (o is ReverseComparer2<T> &&
                     cmp.Equals(((ReverseComparer2<T>)o).cmp));
            }

            public override int GetHashCode()
            {
                return cmp.GetHashCode() ^ int.MinValue;
            }


            public IComparer<T> Reversed()
            {
                return cmp;
            }
        }
    }
}
