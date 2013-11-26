using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace treeFilter
{
    public static class NodeClosureAnalyser
    {
        public static NodeClosures Analyse(Node root)
        {
            var results = new NodeClosures
                              {
                                  AncestorClosures = new Dictionary<int, HashSet<int>>(),
                                  DescendantClosures = new Dictionary<int, HashSet<int>>()
                              };
            //walk the tree and build the closures lists
            var stack = new Stack<Node>();
            stack.Push(root);
            
            var route = new Stack<Node>();

            while (stack.Count != 0)
            {
                var current = stack.Pop();
                
                while (route.Any() && !route.Peek().Children.Any(n => n.Id == current.Id))
                {
                    route.Pop();
                }
                route.Push(current);

                AddNodeRelationships(results, current, route);
                foreach (var child in current.Children)
                {
                    stack.Push(child);
                }
            }
            return results;
        }

        private static void AddNodeRelationships(NodeClosures results, Node current, IEnumerable<Node> route)
        {
            foreach (var node in route)
            {
                results.AncestorClosures.GetOrCreateValuesList(current.Id).Add(node.Id);
                results.DescendantClosures.GetOrCreateValuesList(node.Id).Add(current.Id);
            }
        }
    }

    public struct NodeClosures
    {
        public Dictionary<int, HashSet<int>> AncestorClosures { get; set; }
        public Dictionary<int, HashSet<int>> DescendantClosures { get; set; }
    }

    public static class DictionaryExtension
    {
        public static HashSet<int> GetOrCreateValuesList(
            this IDictionary<int, HashSet<int>> dictionary, int key)
        {
            HashSet<int> ret;
            if (!dictionary.TryGetValue(key, out ret))
            {
                ret = new HashSet<int>(new List<int>());
                dictionary[key] = ret;
            }
            return ret;
        }
    }
}
