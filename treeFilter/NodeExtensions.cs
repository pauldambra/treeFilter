using System.Collections.Generic;
using System.Linq;

namespace treeFilter
{
    public static class NodeExtensions
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