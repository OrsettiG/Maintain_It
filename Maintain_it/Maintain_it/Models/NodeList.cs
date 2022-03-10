using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Maintain_it.Models
{
    public class NodeList<T> where T : class
    {
        public NodeList()
        {

        }


        public NodeList( T originValue )
        {
            _origin = new Node<T>( originValue );
        }

        public NodeList( Node<T> originNode )
        {
            _origin = originNode;
        }


        #region Read Only
        private Node<T> _origin;
        public Node<T> Origin { get => _origin ??= new Node<T>(); private set => _origin = value; }

        public enum NodeInsertLocationAt { Origin, Front, End, Center }
        #endregion

        #region Methods
        public static NodeList<T> CreateNewListFromValues( IEnumerable<T> values, Expression<Func<T, bool>> sortBy )
        {
            List<T> sortedList = values.OrderBy( x => sortBy ).ToList<T>();

            NodeList<T> newList = new NodeList<T>
            {
                Origin = new Node<T>( sortedList[0] )
            };

            foreach( T next in sortedList )
            {
                _ = newList.AddNewNodeAtEnd( next );
            }

            return newList;
        }

        private Node<T> AddNode( T value, bool atStart = false )
        {
            Node<T> newNode = new Node<T>( value );

            _ = atStart ? Origin.InsertNewNodeAtFront( newNode ) : Origin.InsertNewNodeAtEnd( newNode );

            return newNode;
        }

        private Node<T> AddNode( T value, Node<T> previousNode )
        {
            Node<T> newNode = new Node<T>( value );

            _ = previousNode.InsertNewNodeAfter( newNode );

            return newNode;
        }

        public T AddNewNodeAtEnd( T value )
        {
            return AddNode( value ).GetValue();
        }

        public T AddNewNodeAfter( T value, Node<T> origin )
        {
            return AddNode( value, origin ).GetValue();
        }

        public T AddNewNodeAtStart( T value )
        {
            return AddNode( value, true ).GetValue();
        }

        /// <summary>
        /// Trims the NodeList at the passed in Node. This and all subsequent Nodes in the list will be lost and GC'd.
        /// </summary>
        /// <param name="nodeToTrim">The first node on the portion of the list that will be lost.</param>
        /// <returns>The node right before the trimmed Node</returns>
        public Node<T> Trim()
        {
            Node<T> nodeToTrim = Origin.GetLastNode();
            Node<T> temp = nodeToTrim.GetPreviousNode();
            temp.SetNextToNull();
            nodeToTrim.SetPreviousToNull();

            return temp;
        }
        /// <summary>
        /// Trims the NodeList at the passed in Node. This and all subsequent Nodes in the list will be lost and GC'd.
        /// </summary>
        /// <param name="nodeToTrim">The first node on the portion of the list that will be lost.</param>
        /// <returns>The node right before the trimmed Node</returns>
        public static Node<T> Trim( Node<T> nodeToTrim )
        {
            Node<T> temp = nodeToTrim.GetPreviousNode();
            temp.SetNextToNull();
            nodeToTrim.SetPreviousToNull();

            return temp;
        }

        /// <summary>
        /// Splits the NodeList at the passed in Node.
        /// </summary>
        /// <param name="nodeToSplitOn">The node that will become the new Origin node of the new NodeList</param>
        /// <returns>The new NodeList</returns>
        public static NodeList<T> Split( Node<T> nodeToSplitOn )
        {
            Node<T> temp = nodeToSplitOn.GetPreviousNode();
            temp.SetNextToNull();
            nodeToSplitOn.SetPreviousToNull();

            return new NodeList<T>( nodeToSplitOn );
        }
        #endregion

        public class Node<T> where T : class
        {
            #region Constructors
            public Node()
            {

            }

            public Node( T value )
            {
                Value = value;
            }

            public Node( Node<T> previousNode, T value )
            {
                Value = value;
                PreviousNode = previousNode;

                _ = previousNode.InsertNewNodeAfter( this );
            }

            public Node( T value, Node<T> nextNode )
            {
                Value = value;
                NextNode = nextNode;

                _ = nextNode.InsertNewNodeBefore( this );
            }

            public Node( T value, Node<T> previousNode, Node<T> nextNode )
            {
                Value = value;
                PreviousNode = previousNode;
                NextNode = nextNode;

                _ = previousNode.InsertNewNodeAfter( this );
                _ = nextNode.InsertNewNodeAfter( this );
            }
            #endregion

            private T Value { get; set; }
            private Node<T> NextNode { get; set; } = null;
            private Node<T> PreviousNode { get; set; } = null;
            private Guid guid { get; } = Guid.NewGuid();

            public bool HasValue()
            {
                return Value != null && Value != default;
            }

            public T GetValue()
            {
                return Value ?? default;
            }

            public void SetValue( T value )
            {
                Value = value;
            }

            public bool HasNextNode()
            {
                return NextNode != null;
            }

            public bool HasPreviousNode()
            {
                return PreviousNode != null;
            }

            public Node<T> GetNextNode()
            {
                return NextNode ?? null;
            }

            public bool GetNextNode( out Node<T> nextNode )
            {
                nextNode = NextNode;
                return NextNode != null;
            }

            public void ClearNextNode()
            {
                NextNode = null;
            }

            public Node<T> GetPreviousNode()
            {
                return PreviousNode ?? null;
            }

            public bool GetPreviousNode( out Node<T> previousNode )
            {
                previousNode = PreviousNode;
                return PreviousNode != null;
            }

            public void ClearPreviousNode()
            {
                PreviousNode = null;
            }

            public T GetLastNodeValue()
            {
                return NextNode != null ? NextNode.GetLastNodeValue() : Value;
            }

            public T GetFirstNodeValue()
            {
                return PreviousNode != null ? PreviousNode.GetFirstNodeValue() : Value;
            }

            public Node<T> GetLastNode()
            {
                return NextNode != null ? NextNode.GetLastNode() : this;
            }

            public Node<T> GetFirstNode()
            {
                return PreviousNode != null ? PreviousNode.GetFirstNode() : this;
            }

            public void SetNextToNull()
            {
                NextNode = null;
            }

            public void SetPreviousToNull()
            {
                PreviousNode = null;
            }

            /// <summary>
            /// Inserts the supplied <see cref="Node{T}"/> after the selected node 
            /// </summary>
            /// <param name="node">Node to insert</param>
            /// <returns><see langword="true"/> if Node was succesfully inserted, <see langword="false"/> otherwise</returns>
            public bool InsertNewNodeAfter( Node<T> node )
            {
                Node<T> tempNext = NextNode;
                Node<T> tempPrev = PreviousNode;

                Node<T> newNodeTempNext = node.HasNextNode() ? node.GetNextNode() : null;
                Node<T> newNodeTempPrev = node.HasPreviousNode() ? node.GetPreviousNode() : null;

                NextNode = node;
                node.PreviousNode = this;

                if( tempNext != null )
                {
                    node.NextNode = tempNext;
                }

                if( newNodeTempNext != null )
                {
                    newNodeTempNext.PreviousNode = newNodeTempPrev;
                }

                if( newNodeTempPrev != null )
                {
                    newNodeTempPrev.NextNode = newNodeTempNext;
                }

                return true;
            }

            /// <summary>
            /// Inserts the supplied <see cref="Node{T}"/> before the selected node 
            /// </summary>
            /// <param name="node">Node to insert</param>
            /// <returns><see langword="true"/> if Node was succesfully inserted, <see langword="false"/> otherwise</returns>
            public bool InsertNewNodeBefore( Node<T> node )
            {
                Node<T> tempNext = NextNode;
                Node<T> tempPrev = PreviousNode;

                Node<T> newNodeTempNext = node.HasNextNode() ? node.GetNextNode() : null;
                Node<T> newNodeTempPrev = node.HasPreviousNode() ? node.GetPreviousNode() : null;

                PreviousNode = node;
                node.NextNode = this;
                node.PreviousNode = tempPrev;

                if( tempPrev != null )
                {
                    tempPrev.NextNode = node;
                }

                if( newNodeTempNext != null )
                {
                    newNodeTempNext.PreviousNode = newNodeTempPrev;
                }

                if( newNodeTempPrev != null )
                {
                    newNodeTempPrev.NextNode = newNodeTempNext;
                }

                return true;
            }

            /// <summary>
            /// Inserts a new <see cref="Node{T}"/> at the end of the <see cref="NodeList{T}"/> Sequence
            /// </summary>
            /// <param name="newNode">Node to insert</param>
            /// <returns><see langword="true"/> if succsessful, <see langword="false"/> otherwise</returns>
            public bool InsertNewNodeAtEnd( Node<T> newNode )
            {
                Node<T> LastNode = GetLastNode();
                return LastNode.InsertNewNodeAfter( newNode );
            }

            /// <summary>
            /// Inserts a new <see cref="Node{T}"/> at the front of the <see cref="NodeList{T}"/> Sequence. This replaces the Origin Node with the passed in Node.
            /// </summary>
            /// <param name="newNode">Node to insert</param>
            /// <returns><see langword="true"/> if succsessful, <see langword="false"/> otherwise</returns>
            public bool InsertNewNodeAtFront( Node<T> newNode )
            {
                Node<T> FirstNode = GetFirstNode();
                return FirstNode.InsertNewNodeBefore( newNode );
            }
        }

    }
}
