﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace treeFilter
{
    public class Node
    {
        public override string ToString()
        {
            return string.Format("Node - Id: {0}",Id);
        }

        public Node()
        {
            Parents = new List<Node>();
            Children = new List<Node>();
        }

        public int Id { get; set; }
        public ICollection<Node> Parents { get; private set; }
        public ICollection<Node> Children { get; private set; }

        public bool IsImplicitlyIncluded { get; set; }

        public bool IsExplicitlyIncluded { get; set; }

        public static Node CreateNode(int id, Node parent)
        {
            var n = new Node { Id = id };
            parent.Children.Add(n);
            n.Parents.Add(parent);
            return n;
        }


        public IEnumerable<Node> DescendantsWhere(Func<Node, bool> predicate)
        {
            var stack = new Stack<Node>();
            stack.Push(this);
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                if (predicate(current))
                {
                    yield return current;
                }
                foreach (var child in current.Children)
                {
                    stack.Push(child);
                }
            }
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
        public static Node Filter(Node startNode, IEnumerable<int> includedNodes)
        {
            var explicitlyIncludedNodes = startNode.DescendantsWhere(n => includedNodes.Contains(n.Id)).ToArray();
            
            if (!explicitlyIncludedNodes.Any())
            {
                return null;
            }

            var closures = NodeClosureAnalyser.Analyse(startNode);
            var results = new HashSet<int>();
            foreach (var node in explicitlyIncludedNodes)
            {
                var ancestors = closures.AncestorClosures[node.Id];
                var descendants = closures.DescendantClosures[node.Id];
                results = new HashSet<int>(ancestors.Union(descendants).Union(results));
            }
            return BreadthFirstDeletion(startNode, results, includedNodes.ToArray());
            return null;
        }

        public static Node BreadthFirstDeletion(Node startNode, HashSet<int> allIncludedNodes, 
            int[] explicitlyIncludedNodes)
        {
            var stack = new Stack<Node>();
            stack.Push(startNode);
            while (stack.Count != 0)
            {
                var current = stack.Pop();

                if (allIncludedNodes.Contains(current.Id))
                {
                    MarkNodeInclusionType(explicitlyIncludedNodes, current);
                    RemoveExcludedParents(allIncludedNodes, current.Parents);

                    foreach (var child in current.Children)
                    {
                        stack.Push(child);
                    }
                }
                else
                {
                    RemoveNodeFromTheTree(current);
                }
            }
            return startNode;
        }

        private static void MarkNodeInclusionType(IEnumerable<int> explicitlyIncludedNodes, Node current)
        {
            if (explicitlyIncludedNodes.Contains(current.Id))
            {
                current.IsExplicitlyIncluded = true;
            }
            else
            {
                current.IsImplicitlyIncluded = true;
            }
        }

        private static void RemoveExcludedParents(ICollection<int> allIncludedNodes, ICollection<Node> parents)
        {
            for (var i = 0; i < parents.Count; i++)
            {
                var parent = parents.ElementAt(i);
                if (allIncludedNodes.Contains(parent.Id))
                {
                    continue;
                }
                parents.Remove(parent);
                i--;
            }
        }

        private static void RemoveNodeFromTheTree(Node current)
        {
            for (var i = 0; i < current.Parents.Count; i++)
            {
                var parent = current.Parents.ElementAt(i);
                parent.Children.Remove(current);
            }
        }

        protected bool Equals(Node other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Node)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static Node ConditionallyCopyTree(IEnumerable<Node> explicitlyncludedNodes)
        {
            Node root = null;
            //copy the explicit nodes to a new collection.
            foreach (var exp in explicitlyncludedNodes)
            {
                var newNode = exp.CloneBranch();
                root = exp.CloneAncestors(newNode, root);

            }
            return root;
        }

        public Node Clone()
        {
            return new Node {Id = Id};
        }

        public Node CloneBranch()
        {
            var n = new Node {Id = Id};
            ProcessChildren(this, n);
            return n;
        }

        private static void ProcessChildren(Node templateParent, Node copyParent)
        {
            var stack = new Stack<Node>();
            foreach (var child in templateParent.Children)
            {
                stack.Push(child);
            }
            while (stack.Count != 0)
            {
                var template = stack.Pop();
                var copy = new Node { Id = template.Id };
                copy.Parents.Add(copyParent);
                copyParent.Children.Add(copy);
                ProcessChildren(template, copy);
            }
        }

        public Node CloneAncestors(Node clone, Node root)
        {
            ProcessParents(this, clone, root);
            return clone.FirstOrDefaultAncestor(n => n.Parents.Count == 0);
        }

        private static void ProcessParents(Node template, Node clone, Node root)
        {
            var stack = new Stack<Node>();
            foreach (var parent in template.Parents)
            {
                stack.Push(parent);
            }
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                Node targetParent;
                
                if (root == null)
                {
                    targetParent = new Node {Id = current.Id};
                }
                else
                {
                    targetParent = root.FirstOrDefaultDescendant(n => n.Id == current.Id) ?? new Node { Id = current.Id };
                }
                
                clone.Parents.Add(targetParent);
                targetParent.Children.Add(clone);
                ProcessParents(current, targetParent, root);
            }
        }
    }
}