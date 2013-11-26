using System;
using System.Collections.Generic;
using System.Linq;

namespace treeFilter
{
    using System.Threading.Tasks;

    public class Node
    {
        private bool? _isImplicitlyIncluded;

        public Node()
        {
            Parents = new List<Node>();
            Children = new List<Node>();
        }

        public int Id { get; set; }
        public ICollection<Node> Parents { get; private set; }
        public ICollection<Node> Children { get; private set; }

        private bool IsImplicitlyIncluded
        {
            get
            {
                if (_isImplicitlyIncluded.HasValue)
                {
                    return _isImplicitlyIncluded.Value;
                }
                //otherwise test parents
                Parallel.ForEach(
                    Parents,
                    (parent, parallelLoopState) =>
                        {
                            if (parent.AnyAncestor(n => n.IsExplicitlyIncluded))
                            {
                                _isImplicitlyIncluded = true;
                                parallelLoopState.Stop();
                            }
                        });
                //recheck state
                if (_isImplicitlyIncluded.HasValue)
                {
                    return _isImplicitlyIncluded.Value;
                }
                //otherwise test children
                Parallel.ForEach(
                    Parents,
                    (parent, parallelLoopState) =>
                        {
                            if (parent.AnyDescendant(n => n.IsExplicitlyIncluded))
                            {
                                _isImplicitlyIncluded = true;
                                parallelLoopState.Stop();
                            }
                        });
                //final state
                return _isImplicitlyIncluded.HasValue && _isImplicitlyIncluded.Value;
            }
            set
            {
                _isImplicitlyIncluded = value;
            }
        }

        public bool IsExplicitlyIncluded { get; set; }

        private bool IsExcluded
        {
            get
            {
                return !(IsImplicitlyIncluded || IsExplicitlyIncluded);
            }
        }

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

        public bool AnyAncestor(Func<Node, bool> predicate)
        {
            var stack = new Stack<Node>();
            stack.Push(this);
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                if (predicate(current))
                {
                    return true;
                }
                foreach (var parent in current.Parents)
                {
                    stack.Push(parent);
                }
            }
            return false;
        }

        public bool AnyDescendant(Func<Node, bool> predicate)
        {
            var stack = new Stack<Node>();
            stack.Push(this);
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                if (predicate(current))
                {
                    return true;
                }
                foreach (var parent in current.Parents)
                {
                    stack.Push(parent);
                }
            }
            return false;
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
            CheckStackForDanglingExcludedParents(stack);
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

                if (current.IsExplicitlyIncluded || current.IsExplicitlyIncluded(includedNodes))
                {
                    current.IsExplicitlyIncluded = true;
                    AddToStackAndQueue(stack, current, queue);
                    continue;
                }
                
                if(current.IsImplicitlyIncluded || current.IsImplicitlyIncluded(includedNodes))
                {
                    current.IsImplicitlyIncluded = true;
                    AddToStackAndQueue(stack, current, queue);
                    continue;
                }

                current.RemoveChildReferencesToNode()
                       .RemoveParentReferencesToNode();
            }
            return stack;
        }

        private static void AddToStackAndQueue(Stack<Node> stack, Node current, Queue<Node> queue)
        {
            stack.Push(current);
            foreach (var child in current.Children)
            {
                queue.Enqueue(child);
            }
        }

        /// <summary>
        /// Bottom up depth first traversal of included nodes which removes dangling excluded parent references.
        /// Accounts for included nodes which have more than one parent 
        /// where at least one parent is on an excluded branch
        /// </summary>
        private static void CheckStackForDanglingExcludedParents(Stack<Node> stack)
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
                    if (!parent.IsExcluded) continue;
                    parent.RemoveChildReferencesToNode()
                          .RemoveParentReferencesToNode();
                    i--;
                }
            }
        }
    }
}