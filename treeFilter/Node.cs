using System;
using System.Collections.Generic;
using System.Linq;

namespace treeFilter
{
    public class Node
    {
        public Node()
        {
            Parents = new List<Node>();
            Children = new List<Node>();
        }

        public int Id { get; set; }
        public ICollection<Node> Parents { get; set; }
        public ICollection<Node> Children { get; set; }

        public static Node CreateNode(int id, Node parent)
        {
            var n = new Node { Id = id };
            parent.Children.Add(n);
            n.Parents.Add(parent);
            return n;
        }

        public Node FirstOrDefaultDescendant(Func<Node, bool> predicate)
        {
            var stack = new Stack<Node>();
            stack.Push(this);
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                if (predicate(current))
                {
                    return current;
                }
                foreach (var child in current.Children)
                {
                    stack.Push(child);
                }
            }
            return null;
        }

        public Node FirstOrDefaultAncestor(Func<Node, bool> predicate)
        {
            var stack = new Stack<Node>();
            stack.Push(this);
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                if (predicate(current))
                {
                    return current;
                }
                foreach (var parent in current.Parents)
                {
                    stack.Push(parent);
                }
            }
            return null;
        }

        /// <summary>
        /// Partial Bottom Up Breadth first search for nodes which are neither explicitly nor implicitly included
        /// </summary>
        public static Node Filter(Node startNode, int[] includedNodes)
        {
            var stack = new Stack<Node>();
             var queue = new Queue<Node>();
            queue.Enqueue(startNode);

            while (queue.Count != 0)
            {
                var current = queue.Dequeue();
                if (current.IsIncluded(includedNodes))
                {
                    stack.Push(current);
                    foreach (var child in current.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
                else
                {
                    if (current.IsRoot())
                    {
                        return null;
                    }
                    current.RemoveChildReferencesToNode()
                           .RemoveParentReferencesToNode();
                }
            }

            //walk back up from bottom level of included nodes
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                //all nodes in stack are included
                //do they have any excluded parents?
                for (var i = 0; i < current.Parents.Count; i++)
                {
                    var parent = current.Parents.ElementAt(i);
                    if (parent.IsIncluded(includedNodes)) continue;
                    parent.RemoveChildReferencesToNode()
                          .RemoveParentReferencesToNode();
                    i--;
                }
            }
            return startNode;
        }
    }


    public static class Extensions
    {
        /// <summary>
        /// A node with no parents is a root node
        /// </summary>
        public static bool IsRoot(this Node node)
        {
            return !node.Parents.Any();
        }

        public static Node RemoveParentReferencesToNode(this Node node)
        {
            foreach (var child in node.Children)
            {
                child.Parents.Remove(node);
            }
            return node;
        }

        public static Node RemoveChildReferencesToNode(this Node node)
        {
            foreach (var parent in node.Parents)
            {
                parent.Children.Remove(node);
            }
            return node;
        }

        public static bool IsIncluded(this Node node, int[] includedNodes)
        {
            return node.IsExplicitlyIncluded(includedNodes)
            || node.AncestorsAreExplicitlyIncluded(includedNodes)
            || node.DescendantsAreExplicitlyIncluded(includedNodes);
        }

        private static bool IsExplicitlyIncluded(this Node node, IEnumerable<int> explicitlyIncludedNodes)
        {
            return explicitlyIncludedNodes.Contains(node.Id);
        }

        private static bool AncestorsAreExplicitlyIncluded(this Node node, IEnumerable<int> explicitlyIncludedNodes)
        {
            return node.FirstOrDefaultAncestor(n => explicitlyIncludedNodes.Contains(n.Id)) != null;
        }

        private static bool DescendantsAreExplicitlyIncluded(this Node node, IEnumerable<int> explicitlyIncludedNodes)
        {
            return node.FirstOrDefaultDescendant(n => explicitlyIncludedNodes.Contains(n.Id)) != null;
        }
    }
}