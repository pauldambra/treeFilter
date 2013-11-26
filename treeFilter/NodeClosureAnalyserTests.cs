using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace treeFilter
{
    using FluentAssertions;

    using NUnit.Framework;

    [TestFixture]
    public class NodeClosureAnalyserTests
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
        public void CanBuildClosureDictionarys()
        {
            var results = NodeClosureAnalyser.Analyse(_root);
            results.Should().NotBeNull();
            results.AncestorClosures.Should().NotBeNull();

            results.AncestorClosures[7].Should().HaveCount(4);

            results.AncestorClosures[8].Should().HaveCount(6).And.BeEquivalentTo(new[] { 8, 5, 6, 1, 3, 0 });
        }
    }
}
