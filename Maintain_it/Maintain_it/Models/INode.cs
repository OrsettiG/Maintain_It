using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Helpers;
using Maintain_it.Services;

using SQLite;

namespace Maintain_it.Models
{
    public interface INode<T> : IEquatable<T>, IComparable<T> where T : IStorableObject, new()
    {
        public int Id { get; set; }

        [NotNull]
        public int Index { get; set; }

        public int NextNodeId { get; set; }

        public int PreviousNodeId { get; set; }
    }


    public class NodeList<T> : IList<INode<T>>, IStorableObject where T : IStorableObject, new()
    {
        public NodeList()
        {
            CreatedOn = DateTime.UtcNow;
            Name = $"{typeof( T )} NodeList";
            Type = typeof( T );
        }

        public int Id { get; set; }
#nullable enable
        public string? Name { get; set; }
#nullable disable

        public DateTime CreatedOn { get; set; }
        public Type Type { get; }

        List<INode<T>> _nodes;
        List<INode<T>> nodes { get => _nodes ??= new List<INode<T>>(); }

        [NotNull]
        public int FrontNodeId { get => nodes.First().Id; }
        [NotNull]
        public int BackNodeId { get => nodes.Last().Id; }

        public int Count => nodes.Count();
        public bool IsReadOnly => IsReadOnly;
        public INode<T> this[int index] { get => nodes[index]; set => AddNode( value, index ); }

        public IOrderedEnumerable<T> GetNodeList
        {
            get => nodes.OrderBy( x => x.Index ) as IOrderedEnumerable<T>;
        }

        public IOrderedEnumerable<INode<T>> GetINodeTList
        {
            get => nodes.OrderBy( x => x.Index );
        }

        public SortedSet<T> GetSortedNodeSet
        {
            get => nodes.OrderBy( x => x.Index ) as SortedSet<T>;
        }

        public SortedSet<INode<T>> GetSortedINodeTSet
        {
            get => nodes.OrderBy( x => x.Index ) as SortedSet<INode<T>>;
        }

        /// <summary>
        /// Adds the passed in node to the end of the NodeList and updates all the 
        /// </summary>
        public void AddNode( INode<T> node )
        {
            if( !Contains( node ) )
            {
                node.Index = nodes.Count() + 1;
                nodes.Add( node );
            }
        }

        /// <summary>
        /// Adds the node at the desired index, pushing all other nodes forward. 
        /// <para>I.E. given the following nodelist : n1, n2, n3, n4, n5, n6 inserting a new node (nn) into the list and specifying an index of 3 would result in a final list of:
        /// n1, n2, nn, n3, n4, n5, n6</para>
        /// </summary>
        public void AddNode( INode<T> node, int index )
        {
            if( !Contains( node ) )
            {
                if( index > nodes.Count() )
                {
                    AddNode( node );
                }
                else
                {

                    List<INode<T>> subList = nodes.Where( x => x.Index >= index) as List<INode<T>>;

                    foreach( INode<T> n in subList )
                    {
                        n.Index++;
                    }

                    node.Index = index;
                    nodes.Add( node );
                }
            }
        }

        public int IndexOf( INode<T> item )
        {
            return item.Index;
        }

        public void Insert( int index, INode<T> item )
        {
            AddNode( item, index );
        }

        public void RemoveAt( int index )
        {
            INode<T> node = nodes.Where(x => x.Index == index).SingleOrDefault();
            _ = Remove( node );
        }

        public void Add( INode<T> item )
        {
            AddNode( item );
        }

        public void Clear()
        {
            nodes.Clear();
        }

        public bool Contains( INode<T> item )
        {
            foreach( INode<T> n in nodes )
            {
                if( item.Id == n.Id )
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo( INode<T>[] array, int arrayIndex )
        {
            nodes.CopyTo( array, arrayIndex );
        }

        public bool Remove( INode<T> item )
        {
            if( Contains( item ) && nodes.Remove( item ))
            {
                int i = item.Index;
                List<INode<T>> subList = nodes.Where( x => x.Index > i ) as List<INode<T>>;

                foreach( INode<T> n in subList )
                {
                    n.Index--;
                }

                return true;
            }

            return false;
        }

        public IEnumerator<INode<T>> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
    }
}
