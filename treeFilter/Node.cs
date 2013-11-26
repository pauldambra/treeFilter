using System;
using System.Collections.Generic;

namespace treeFilter
{
    public class Node : INode
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
        public List<Node> Parents { get; private set; }
        public List<Node> Children { get; private set; }

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

        private bool Equals(Node other)
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
            if (obj.GetType() != GetType())
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
                newNode.IsExplicitlyIncluded = true;
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