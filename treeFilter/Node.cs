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
        public ICollection<Node> Parents { get; private set; }
        public ICollection<Node> Children { get; private set; }

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
            if (startNode.IsRoot() && !startNode.IsIncluded(includedNodes))
            {
                return null;
            }

            var stack = BreadthFirstBranchExclusion(startNode, includedNodes);
            CheckStackForDanglingExcludedParents(includedNodes, stack);
            return startNode;
        }

        /// <summary>
        /// Breadth first traversal that rooms branches which are not included
        /// </summary>
        private static Stack<Node> BreadthFirstBranchExclusion(Node startNode, int[] includedNodes)
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
                    current.RemoveChildReferencesToNode()
                           .RemoveParentReferencesToNode();
                }
            }
            return stack;
        }

        /// <summary>
        /// Bottom up depth first traversal of included nodes which removes dangling excluded parent references.
        /// Accounts for included nodes which have more than one parent 
        /// where at least one parent is on an excluded branch
        /// </summary>
        private static void CheckStackForDanglingExcludedParents(int[] includedNodes, Stack<Node> stack)
        {
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                //all nodes in stack are included
                //do they have any excluded parents?
                if (current.Parents.Count <= 1) continue;
                for (var i = 0; i < current.Parents.Count; i++)
                {
                    var parent = current.Parents.ElementAt(i);
                    if (parent.IsIncluded(includedNodes)) continue;
                    parent.RemoveChildReferencesToNode()
                          .RemoveParentReferencesToNode();
                    i--;
                }
            }
        }
    }
}