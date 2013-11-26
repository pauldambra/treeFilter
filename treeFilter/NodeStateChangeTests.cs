using System.Collections.Generic;

namespace treeFilter
{
    using FluentAssertions;

    using NUnit.Framework;

    [TestFixture]
    public class NodeStateChangeTests
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
        public void CanDeleteNodes()
        {
            _root.FirstOrDefaultDescendant(n => n.Id == 6).IsExplicitlyIncluded = true;
            foreach (var i in new []{0, 3, 8, 9, 10})
            {
                var capturedId = i;
                _root.FirstOrDefaultDescendant(n => n.Id == capturedId).IsImplicitlyIncluded = true;
            }
            _root = Node.BreadthFirstDeletion(_root, new HashSet<int>{0,3,6,8,9,10}, new []{6});
            var removedNodes = new[] { 1, 2, 4, 5, 7 };
            foreach (var id in removedNodes)
            {
                var capturedId = id;
                _root.FirstOrDefaultDescendant(n => n.Id == capturedId).Should().BeNull();
            }
            var keptNodes = new[] { 0, 3, 6, 8, 9, 10 };
            foreach (var id in keptNodes)
            {
                var capturedId = id;
                var match = _root.FirstOrDefaultDescendant(n => n.Id == capturedId);
                match.Should().NotBeNull();
                match.IsExplicitlyIncluded.Should().Be(match.Id == 6);
            }
        }

    }
}
