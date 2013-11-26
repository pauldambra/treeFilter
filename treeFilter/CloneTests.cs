using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace treeFilter
{
    [TestFixture]
    public class CloneTests
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
        public void CanCloneBranch()
        {
            var clone = _root.FirstOrDefaultDescendant(n => n.Id == 6).CloneBranch();
            clone.Parents.Count.Should().Be(0);
            clone.Children.Select(c => c.Id).Should().BeEquivalentTo(new[] {8, 9});
            clone.Children.Single(n => n.Id == 8).Children.Should().HaveCount(0);
            clone.Children.Single(n => n.Id == 8).Parents.Should().HaveCount(1);
            clone.Children.Single(n => n.Id == 9).Children.Should().HaveCount(1);
        }

        [Test]
        public void CanCloneNode()
        {
            var clone = _root.FirstOrDefaultDescendant(n => n.Id == 6).Clone();
            clone.Parents.Count.Should().Be(0);
            clone.Children.Should().HaveCount(0);
            clone.Id.Should().Be(6);
        }

        [Test]
        public void CanCloneAncestors()
        {
            var template = _root.FirstOrDefaultDescendant(n => n.Id == 6); 
            var clone = _root.FirstOrDefaultDescendant(n => n.Id == 6).Clone();
            var root = template.CloneAncestors(clone, new Node {Id = 0});
            clone.Parents.First().Id.Should().Be(3);
            clone.Parents.First().Children.Should().HaveCount(1);
            clone.Parents.First().Parents.Should().HaveCount(1);
            clone.Parents.First().Parents.First().Id.Should().Be(0);
            clone.Parents.First().Parents.First().Parents.Should().HaveCount(0);
            clone.Parents.First().Parents.First().Should().BeSameAs(root);
        }

        [Test]
        public void CanCloneAncestorsIntoOneBranch()
        {
            var template = _root.FirstOrDefaultDescendant(n => n.Id == 6);
            var secondTemplate = _root.FirstOrDefaultDescendant(n => n.Id == 4);
            var clone = _root.FirstOrDefaultDescendant(n => n.Id == 6).Clone();
            var second = _root.FirstOrDefaultDescendant(n => n.Id == 4).Clone();

            var root = template.CloneAncestors(clone, new Node {Id = 0});
            secondTemplate.CloneAncestors(second, root);

            var threeParent = clone.Parents.First();
            threeParent.Id.Should().Be(3);
            var oneParent = second.Parents.First();
            oneParent.Id.Should().Be(1);

            oneParent.Parents.First().Should().BeSameAs(threeParent.Parents.First());
        }
    }
}
