using System;
using System.Collections.Generic;

namespace treeFilter
{
    /// <summary>
    /// the interface required for this mechanism of conditionally copying a graph
    /// </summary>
    public interface INode
    {
        int Id { get; set; }
        List<Node> Parents { get; }
        List<Node> Children { get; }
        bool IsExplicitlyIncluded { get; set; }
        IEnumerable<Node> DescendantsWhere(Func<Node, bool> predicate);
        Node FirstOrDefaultDescendant(Func<Node, bool> predicate);
        Node FirstOrDefaultAncestor(Func<Node, bool> predicate);
        Node Clone();
        Node CloneBranch();
        Node CloneAncestors(Node clone, Node root);
    }
}