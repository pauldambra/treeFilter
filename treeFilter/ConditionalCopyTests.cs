using System.Collections.Generic;
using System.Linq;

namespace treeFilter
{
    using FluentAssertions;

    using NUnit.Framework;

    [TestFixture]
    public class ConditionalCopyTests
    {
        private Node _root;
        /// <summary>
        /// Example graph looks like:
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
        [SetUp]
        public void Setup()
        {
            _root = GraphBuilder.BuildExampleGraph();
        }

        [Test]
        public void CanConditionallyCopy()
        {
            var explicitNodes = new List<Node>
                {
                    _root.FirstOrDefaultDescendant(n => n.Id == 5),
                    _root.FirstOrDefaultDescendant(n => n.Id == 6)
                };

            var root = Node.ConditionallyCopyTree(explicitNodes);
            //result should be
            //                     0
            //                   /   \
            //                  1     3
            //                   \    |
            //                    5   6
            //                     \ / \
            //                      8   9
            //                          |
            //                          10
            root.Should().NotBeNull();
            root.Children.Should().HaveCount(2);
            var first = root.Children.Single(n => n.Id == 1);
            first.Should().NotBeNull();
            first.Children.Single(n => n.Id == 5).Should().NotBeNull();
            first.Children.Single(n => n.Id == 5).Children.Single(n => n.Id == 8).Should().NotBeNull();
            
            var three = root.Children.Single(n => n.Id == 3);
            three.Should().NotBeNull();
            

        }
    }
}
