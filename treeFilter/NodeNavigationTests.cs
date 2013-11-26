using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace treeFilter
{
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
    [TestFixture]
    public class NodeNavigationTests
    {
        private Node _root;

        [SetUp]
        public void Setup()
        {
            _root = GraphBuilder.BuildExampleGraph();
        }

        [Test]
        public void CanTestForListOfNodes()
        {
            var matches = _root.DescendantsWhere(n => new[] { 2, 5, 9 }.Contains(n.Id));
            matches.Should().HaveCount(3);
        }

        [Test]
        public void CanBuildGraph()
        {
            _root.Should().NotBeNull();
            _root.Children.Should().HaveCount(3);
        }

        [Test]
        [TestCase(0, 8)]
        [TestCase(1, 7)]
        [TestCase(0, 10)]
        public void CanSearchDescendants(int startId, int targetId)
        {
            var start = _root.FirstOrDefaultDescendant(n => n.Id == startId);
            start.Should().NotBeNull();
            var match = start.FirstOrDefaultDescendant(n => n.Id == targetId);
            match.Should().NotBeNull();
            match.Id.Should().Be(targetId);
        }

        [Test]
        public void CanSearchDescendantsWhenIdDoesNotMatch()
        {
            var match = _root.FirstOrDefaultDescendant(n => n.Id == 11);
            match.Should().BeNull();
        }

        [Test]
        [TestCase(8, 1)]
        [TestCase(8, 3)]
        [TestCase(8, 0)]
        [TestCase(10, 6)]
        [TestCase(7, 1)]
        public void CanSearchAncestors(int startId, int targetId)
        {
            var start = _root.FirstOrDefaultDescendant(n => n.Id == startId);
            start.Should().NotBeNull();
            var match = start.FirstOrDefaultAncestor(n => n.Id == targetId);
            match.Should().NotBeNull();
            match.Id.Should().Be(targetId);
        }

        [Test]
        public void CanSearchAncestorsWhenNoMatch()
        {
            var start = _root.FirstOrDefaultDescendant(n => n.Id == 10);
            start.Should().NotBeNull();
            var match = start.FirstOrDefaultAncestor(n => n.Id == -1);
            match.Should().BeNull();
        }

        [Test]
        public void CanSearchAncestorsWhenNoRoute()
        {
            var start = _root.FirstOrDefaultDescendant(n => n.Id == 10);
            start.Should().NotBeNull();
            var match = start.FirstOrDefaultAncestor(n => n.Id == 2);
            match.Should().BeNull();
        }
    }
}
