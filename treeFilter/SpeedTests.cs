using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace treeFilter
{
    [TestFixture]
    public class SpeedTests
    {

        [Test]
        public void AnalyseConditionalCopySpeed()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var timings = new List<long>
                {
                    RunConditionalCopy(BuildAndMeasureTree(stopwatch), stopwatch),
                    RunConditionalCopy(BuildAndMeasureTree(stopwatch), stopwatch),
                    RunConditionalCopy(BuildAndMeasureTree(stopwatch), stopwatch),
                    RunConditionalCopy(BuildAndMeasureTree(stopwatch), stopwatch),
                    RunConditionalCopy(BuildAndMeasureTree(stopwatch), stopwatch),
                    RunConditionalCopy(BuildAndMeasureTree(stopwatch), stopwatch),
                    RunConditionalCopy(BuildAndMeasureTree(stopwatch), stopwatch),
                    RunConditionalCopy(BuildAndMeasureTree(stopwatch), stopwatch),
                };
            timings.Average().Should().BeLessOrEqualTo(100);
        }

        private static dynamic BuildAndMeasureTree(Stopwatch stopwatch)
        {
            var before = stopwatch.ElapsedMilliseconds;
            //has c9000 nodes at the bottom level
            var idAndTree = GraphBuilder.BuildLargeGraph(30, 3);
            Debug.WriteLine("Took {0} milliseconds to build the graph", stopwatch.ElapsedMilliseconds - before);
            return idAndTree;
        }

        private static long RunConditionalCopy(dynamic idAndTree, Stopwatch stopwatch)
        {
            var rand = new Random();
            var filter = new List<int>
                {
                    rand.Next(0, idAndTree.MaxId),
                    rand.Next(0, idAndTree.MaxId),
                    rand.Next(0, idAndTree.MaxId),
                    rand.Next(0, idAndTree.MaxId)
                };

            Node tree = idAndTree.Tree;
            var includedNodes = tree.DescendantsWhere(n => filter.Contains(n.Id));

            var filterStart = stopwatch.ElapsedMilliseconds;
            Node.ConditionallyCopyTree(includedNodes);
            var filterMark = stopwatch.ElapsedMilliseconds - filterStart;
            Debug.WriteLine("took {0} milliseconds to filter tree", filterMark);
            return filterMark;
        }
    }
}
