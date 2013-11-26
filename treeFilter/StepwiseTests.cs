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
    public class StepwiseTests
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
        public void Foo()
        {
            var explicitNodes = new List<Node>();
            var one = _root.FirstOrDefaultDescendant(n => n.Id == 5);
            var another = _root.FirstOrDefaultDescendant(n => n.Id == 6);

            //copy the explicit nodes to a new collection.
            //copy their children
            //traverse just the explicit nodes parents copying the parents 
            //(but not their children across)
            //if the parent has already been copied simply set the relationship
        

        }
    }
}
