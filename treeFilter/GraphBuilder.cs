using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace treeFilter
{
    public static class GraphBuilder
    {
        private static readonly Random Random = new Random();

        /// <summary>
        ///                     0
        ///                   / | \
        ///                  1  2  3
        ///                 / \    |
        ///                4   5   6
        ///               /     \ / \
        ///              7       8   9
        ///                          |
        ///                          10
        /// </summary>
        public static Node BuildExampleGraph()
        {
            var root = new Node {Id = 0};
            
            var one = Node.CreateNode(1, root);
            Node.CreateNode(2, root);
            var three = Node.CreateNode(3, root);
            
            var four = Node.CreateNode(4, one);
            var five = Node.CreateNode(5, one);
            var six = Node.CreateNode(6, three);

            Node.CreateNode(7, four);
            var eight = Node.CreateNode(8, five);
            eight.Parents.Add(six);
            six.Children.Add(eight);

            var nine = Node.CreateNode(9, six);
            Node.CreateNode(10, nine);

            return root;
        }

        public static dynamic BuildLargeGraph(int depth, int levelMaxSiblings)
        {
            
            var id = 0;
            var builtLevel = 0;

            var root = new Node {Id = id++};
            var queue = new Queue<Node>(100000000);
            queue.Enqueue(root);

            while (builtLevel < depth)
            {
                var nextQueue = new Queue<Node>();
                while (queue.Count != 0)
                {
                    var node = queue.Dequeue();
                    var max = Random.Next(1, levelMaxSiblings);
                    for (var siblingCount = 0; siblingCount < max; siblingCount++)
                    {
                        nextQueue.Enqueue(Node.CreateNode(id++, node));
                    }                
                }
                queue = nextQueue;
                builtLevel++;
            }

            return new {MaxId = id, Tree = root};
        }
    }
}
