using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Maintain_it.Models
{
    public class NodeList<T> where T : class
    {
        #region Constructors
        public NodeList()
        {

        }

        public NodeList( T value )
        {
            Value = value;
        }

        public NodeList( NodeList<T> previousNode, T value )
        {
            Value = value;
            PreviousNode = previousNode;

            _ = previousNode.InsertNewNodeAhead( this );
        }

        public NodeList( T value, NodeList<T> nextNode )
        {
            Value = value;
            NextNode = nextNode;

            _ = nextNode.InsertNewNodeBehind( this );
        }

        public NodeList( T value, NodeList<T> previousNode, NodeList<T> nextNode )
        {
            Value = value;
            PreviousNode = previousNode;
            NextNode = nextNode;

            _ = previousNode.InsertNewNodeAhead( this );
            _ = nextNode.InsertNewNodeAhead( this );
        }
        #endregion

        private T Value { get; set; }
        private NodeList<T> NextNode { get; set; } = null;
        private NodeList<T> PreviousNode { get; set; } = null;

        public bool HasValue()
        {
            return Value != null && Value != default;
        }

        public T GetValue()
        {
            return Value ?? default;
        }

        public bool HasNextNode()
        {
            return NextNode != null;
        }

        public bool HasPreviousNode()
        {
            return PreviousNode != null;
        }

        public NodeList<T> GetNextNode()
        {
            return NextNode ?? null;
        }

        public bool GetNextNode( out NodeList<T> nextNode )
        {
            nextNode = NextNode;
            return NextNode != null;
        }

        public void ClearNextNode()
        {
            NextNode = null;
        }

        public NodeList<T> GetPreviousNode()
        {
            return PreviousNode ?? null;
        }

        public bool GetPreviousNode( out NodeList<T> previousNode )
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

        public NodeList<T> GetLastNode()
        {
            return NextNode != null ? NextNode.GetLastNode() : this;
        }

        public NodeList<T> GetFirstNode()
        {
            return PreviousNode != null ? PreviousNode.GetFirstNode() : this;
        }

        //public bool InsertNewNodeBehind( NodeList<T> nodeToInsert, bool Cascade = true )
        //{

        //    //This makes sure we aren't doing a bunch of work for nothing
        //    if( nodeToInsert == PreviousNode )
        //        return true;

        //    // This makes sure we properly re-reference everything if we are dealing only with the next node
        //    if( nodeToInsert == NextNode )
        //    {
        //        NextNode = nodeToInsert.GetNextNode();
        //        _ = NextNode.InsertNewNodeBehind( this, Cascade: false );
        //    }
        //    // This makes sure we are properly re-referencing everything if the node is coming from somewhere else in the chain (or another chain of the same type for that matter)
        //    else
        //    {
        //        NodeList<T> ntiTempNext = nodeToInsert.GetNextNode();
        //        NodeList<T> ntiTempPrev = nodeToInsert.GetPreviousNode();

        //        if( ntiTempNext != null )
        //        {
        //            _ = ntiTempNext.InsertNewNodeBehind( ntiTempPrev );
        //        }
        //        if( ntiTempPrev != null )
        //        {
        //            _ = ntiTempPrev.InsertNewNodeAhead( ntiTempNext );
        //        }
        //    }

        //    NodeList<T> thisTempPrev = PreviousNode;

        //    if( HasPreviousNode() )
        //    {
        //        _ = thisTempPrev.InsertNewNodeAhead( nodeToInsert );
        //        _ = nodeToInsert.InsertNewNodeBehind( thisTempPrev );
        //    }
        //    else
        //    {
        //        nodeToInsert.ClearPreviousNode();
        //    }

        //    PreviousNode = nodeToInsert;
        //    _ = nodeToInsert.InsertNewNodeAhead( this );

        //    return HasPreviousNode()
        //        ? nodeToInsert.GetNextNode() == this && nodeToInsert.GetPreviousNode() == thisTempPrev &&
        //               PreviousNode == nodeToInsert
        //        : false;
        //}

        //public bool InsertNewNodeAhead( NodeList<T> nodeToInsert, bool Cascade = true )
        //{
        //    //This makes sure we aren't doing a bunch of work for nothing
        //    if( nodeToInsert == NextNode )
        //        return true;

        //    // This makes sure we properly re-reference everything if we are dealing only with the next node
        //    if( nodeToInsert == PreviousNode )
        //    {
        //        PreviousNode = nodeToInsert.GetPreviousNode();
        //        _ = PreviousNode.InsertNewNodeAhead( this, Cascade: false );
        //    }
        //    // This makes sure we are properly re-referencing everything if the node is coming from somewhere else in the chain (or another chain of the same type for that matter)
        //    else
        //    {
        //        NodeList<T> ntiTempNext = nodeToInsert.GetNextNode();
        //        NodeList<T> ntiTempPrev = nodeToInsert.GetPreviousNode();

        //        if( ntiTempNext != null )
        //        {
        //            _ = ntiTempNext.InsertNewNodeBehind( ntiTempPrev );
        //        }
        //        if( ntiTempPrev != null )
        //        {
        //            _ = ntiTempPrev.InsertNewNodeAhead( ntiTempNext );
        //        }
        //    }

        //    NodeList<T> thisTempNext = NextNode;

        //    if( HasNextNode() )
        //    {
        //        _ = thisTempNext.InsertNewNodeBehind( nodeToInsert );
        //        _ = nodeToInsert.InsertNewNodeAhead( thisTempNext );
        //    }
        //    else
        //    {
        //        nodeToInsert.ClearNextNode();
        //    }

        //    NextNode = nodeToInsert;
        //    _ = nodeToInsert.InsertNewNodeBehind( this );

        //    return HasNextNode()
        //        ? nodeToInsert.GetPreviousNode() == this && nodeToInsert.GetNextNode() == thisTempNext &&
        //               NextNode == nodeToInsert
        //        : false;
        //}

        //public bool InsertNewNodeAfter( NodeList<T> newNode )
        //{
        //    if( NextNode != null )
        //    {
        //        NodeList<T> temp = NextNode;
        //        _ = InsertNewNodeAhead( newNode );
        //        _ = newNode.InsertNewNodeBehind( this );
        //        _ = newNode.InsertNewNodeAhead( temp );
        //        _ = temp.InsertNewNodeBehind( newNode );
        //        return true;
        //    }

        //    if( NextNode == null )
        //    {
        //        _ = InsertNewNodeAhead( newNode );
        //        _ = newNode.InsertNewNodeBehind( this );

        //        return true;
        //    }

        //    return false;
        //}

        //public bool InsertNewNodeBefore( NodeList<T> newNode )
        //{
        //    if( PreviousNode != null )
        //    {
        //        NodeList<T> temp = PreviousNode;
        //        _ = InsertNewNodeBehind( newNode );
        //        _ = newNode.InsertNewNodeAhead( this );
        //        _ = newNode.InsertNewNodeBehind( temp );
        //        _ = temp.InsertNewNodeAhead( newNode );
        //        return true;
        //    }

        //    if( PreviousNode == null )
        //    {
        //        _ = InsertNewNodeBehind( newNode );
        //        _ = newNode.InsertNewNodeAhead( this );

        //        return true;
        //    }

        //    return false;
        //}

        public bool InsertNewNodeAhead( NodeList<T> node )
        {
            NodeList<T> tempNext = NextNode;
            NodeList<T> tempPrev = PreviousNode;

            NodeList<T> newNodeTempNext = node.HasNextNode() ? node.GetNextNode() : null;
            NodeList<T> newNodeTempPrev = node.HasPreviousNode() ? node.GetPreviousNode() : null;

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

        public bool InsertNewNodeBehind( NodeList<T> node )
        {
            NodeList<T> tempNext = NextNode;
            NodeList<T> tempPrev = PreviousNode;

            NodeList<T> newNodeTempNext = node.HasNextNode() ? node.GetNextNode() : null;
            NodeList<T> newNodeTempPrev = node.HasPreviousNode() ? node.GetPreviousNode() : null;

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

        public bool InsertNewNodeAtEnd( NodeList<T> newNode )
        {
            NodeList<T> LastNode = GetLastNode();
            return LastNode.InsertNewNodeAhead( newNode );
        }

        public bool InsertNewNodeAtFront( NodeList<T> newNode )
        {
            NodeList<T> FirstNode = GetFirstNode();
            return FirstNode.InsertNewNodeBehind( newNode );
        }
    }
}
