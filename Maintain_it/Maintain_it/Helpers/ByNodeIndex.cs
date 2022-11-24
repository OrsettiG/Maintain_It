using System;
using System.Collections.Generic;
using System.Text;
using Maintain_it.Models.Interfaces;
using Maintain_it.Services;

namespace Maintain_it.Helpers
{
    internal class ByNodeIndex<INode, T> : IComparer<INode<T>> where T : IStorableObject, new()
    {
        int xIndex, yIndex;

        public int Compare( INode<T> x, INode<T> y )
        {
            xIndex = x.Index;
            yIndex = y.Index;

            return xIndex.CompareTo( yIndex );
        }
    }
}
