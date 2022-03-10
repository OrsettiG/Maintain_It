using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Maintain_it.Models
{
    //public class Node<T> where T : class
    //{
    //    #region Constructors
    //    public Node()
    //    {

    //    }

    //    public Node( T value )
    //    {
    //        Value = value;
    //    }

    //    public Node( Node<T> previousNode, T value )
    //    {
    //        Value = value;
    //        PreviousNode = previousNode;

    //        _ = previousNode.InsertNewNodeAhead( this );
    //    }

    //    public Node( T value, Node<T> nextNode )
    //    {
    //        Value = value;
    //        NextNode = nextNode;

    //        _ = nextNode.InsertNewNodeBehind( this );
    //    }

    //    public Node( T value, Node<T> previousNode, Node<T> nextNode )
    //    {
    //        Value = value;
    //        PreviousNode = previousNode;
    //        NextNode = nextNode;

    //        _ = previousNode.InsertNewNodeAhead( this );
    //        _ = nextNode.InsertNewNodeAhead( this );
    //    }
    //    #endregion

    //    private T Value { get; set; }
    //    private Node<T> NextNode { get; set; } = null;
    //    private Node<T> PreviousNode { get; set; } = null;

    //    public bool HasValue()
    //    {
    //        return Value != null && Value != default;
    //    }

    //    public T GetValue()
    //    {
    //        return Value ?? default;
    //    }

    //    public void SetValue( T value )
    //    {
    //        Value = value;
    //    }

    //    public bool HasNextNode()
    //    {
    //        return NextNode != null;
    //    }

    //    public bool HasPreviousNode()
    //    {
    //        return PreviousNode != null;
    //    }

    //    public Node<T> GetNextNode()
    //    {
    //        return NextNode ?? null;
    //    }

    //    public bool GetNextNode( out Node<T> nextNode )
    //    {
    //        nextNode = NextNode;
    //        return NextNode != null;
    //    }

    //    public void ClearNextNode()
    //    {
    //        NextNode = null;
    //    }

    //    public Node<T> GetPreviousNode()
    //    {
    //        return PreviousNode ?? null;
    //    }

    //    public bool GetPreviousNode( out Node<T> previousNode )
    //    {
    //        previousNode = PreviousNode;
    //        return PreviousNode != null;
    //    }

    //    public void ClearPreviousNode()
    //    {
    //        PreviousNode = null;
    //    }

    //    public T GetLastNodeValue()
    //    {
    //        return NextNode != null ? NextNode.GetLastNodeValue() : Value;
    //    }

    //    public T GetFirstNodeValue()
    //    {
    //        return PreviousNode != null ? PreviousNode.GetFirstNodeValue() : Value;
    //    }

    //    public Node<T> GetLastNode()
    //    {
    //        return NextNode != null ? NextNode.GetLastNode() : this;
    //    }

    //    public Node<T> GetFirstNode()
    //    {
    //        return PreviousNode != null ? PreviousNode.GetFirstNode() : this;
    //    }

    //    public void SetNextToNull()
    //    {
    //        NextNode = null;
    //    }
        
    //    public void SetPreviousToNull()
    //    {
    //        PreviousNode = null;
    //    }

    //    public bool InsertNewNodeAhead( Node<T> node )
    //    {
    //        Node<T> tempNext = NextNode;
    //        Node<T> tempPrev = PreviousNode;

    //        Node<T> newNodeTempNext = node.HasNextNode() ? node.GetNextNode() : null;
    //        Node<T> newNodeTempPrev = node.HasPreviousNode() ? node.GetPreviousNode() : null;

    //        NextNode = node;
    //        node.PreviousNode = this;

    //        if( tempNext != null )
    //        {
    //            node.NextNode = tempNext;
    //        }

    //        if( newNodeTempNext != null )
    //        {
    //            newNodeTempNext.PreviousNode = newNodeTempPrev;
    //        }

    //        if( newNodeTempPrev != null )
    //        {
    //            newNodeTempPrev.NextNode = newNodeTempNext;
    //        }

    //        return true;
    //    }

    //    public bool InsertNewNodeBehind( Node<T> node )
    //    {
    //        Node<T> tempNext = NextNode;
    //        Node<T> tempPrev = PreviousNode;

    //        Node<T> newNodeTempNext = node.HasNextNode() ? node.GetNextNode() : null;
    //        Node<T> newNodeTempPrev = node.HasPreviousNode() ? node.GetPreviousNode() : null;

    //        PreviousNode = node;
    //        node.NextNode = this;
    //        node.PreviousNode = tempPrev;

    //        if( tempPrev != null )
    //        {
    //            tempPrev.NextNode = node;
    //        }

    //        if( newNodeTempNext != null )
    //        {
    //            newNodeTempNext.PreviousNode = newNodeTempPrev;
    //        }

    //        if( newNodeTempPrev != null )
    //        {
    //            newNodeTempPrev.NextNode = newNodeTempNext;
    //        }

    //        return true;
    //    }

    //    public bool InsertNewNodeAtEnd( Node<T> newNode )
    //    {
    //        Node<T> LastNode = GetLastNode();
    //        return LastNode.InsertNewNodeAhead( newNode );
    //    }

    //    public bool InsertNewNodeAtFront( Node<T> newNode )
    //    {
    //        Node<T> FirstNode = GetFirstNode();
    //        return FirstNode.InsertNewNodeBehind( newNode );
    //    }
    //}
}
