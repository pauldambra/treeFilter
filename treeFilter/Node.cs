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
        /// Bottom Up Breadth first search for nodes which are neither explicitly nor implicitly included
        /// </summary>
        public static Node Filter(Node startNode, int[] includedNodes)
        {
            var stack = GetReverseStack(startNode, includedNodes);
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                if (current.IsIncluded(includedNodes)) continue;

                if (current.IsRoot())
                {
                    return null;
                }
                current.RemoveChildReferencesToNode()
                       .RemoveParentReferencesToNode();
            }
            return startNode;
        }

        /// <summary>
        /// Breadth first search to build a stack with children on top 
        /// and root at the bottom
        /// </summary>
        private static Stack<Node> GetReverseStack(Node startNode, int[] includedNodes)
        {
            var queue = new Queue<Node>();
            queue.Enqueue(startNode);

            var stack = new Stack<Node>();

            while (queue.Count != 0)
            {
                var current = queue.Dequeue();
                stack.Push(current);
                foreach (var child in current.Children)
                {
                    queue.Enqueue(child);
                }
            }
            return stack;
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