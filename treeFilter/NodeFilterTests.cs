using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class NodeFilterTests
    {
        private Node _root;

        [SetUp]
        public void Setup()
        {
            _root = GraphBuilder.BuildExampleGraph();
        }

        [Test]
        public void CanFilterOnSingleMatch()
        {
            var filteredGraph = Node.Filter(_root, new[] {1});
            filteredGraph.Should().NotBeNull();
            filteredGraph.Id.Should().Be(0);
            filteredGraph.Children.Should().HaveCount(1);
            filteredGraph.Children.First().Children.Should().HaveCount(2);
            var five = filteredGraph.Children.First().Children.First(n => n.Id == 5);
            five.Children.Should().HaveCount(1);
            var eight = five.Children.First();
            //eight's parent route to six would allow access to unpermitted nodes. 
            //it should be removed
            eight.Parents.Should().HaveCount(1);
        }

        [Test]
        public void CanFilterOnMultipleMatch()
        {
            var filteredGraph = Node.Filter(_root, new[] { 2, 9 });
            filteredGraph.Should().NotBeNull();
            filteredGraph.Id.Should().Be(0);
            filteredGraph.Children.Should().HaveCount(2);
            var two = filteredGraph.Children.First(n => n.Id == 2);
            two.Children.Should().BeEmpty();

            var three = filteredGraph.Children.First(n => n.Id == 3);
            three.Children.Should().NotBeEmpty().And.HaveCount(1);

            var six = three.Children.Single();
            six.Children.Should().NotBeEmpty().And.HaveCount(1);
            six.Children.Single().Children.Should().NotBeEmpty().And.HaveCount(1);
        }

        [Test]
        public void CanFilterWhenNoMatch()
        {
            var filteredGraph = Node.Filter(_root, new[] { 12 });
            filteredGraph.Should().BeNull();
        }
    }
}
