using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace treeFilter
{
    [TestFixture]
    public class SpeedTests
    {
        [Test]
        public void FilterLargeGraphSpeed()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var timings = new List<long>
                {
                    RunFilter(BuildAndMeasureTree(stopwatch), stopwatch),
                    RunFilter(BuildAndMeasureTree(stopwatch), stopwatch),
                };
            timings.Average().Should().BeLessOrEqualTo(100);
        }

        [Test]
        public void AnalyseClosureSpeed()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var timings = new List<long>
                {
                    RunAnalyse(BuildAndMeasureTree(stopwatch), stopwatch),
                    RunAnalyse(BuildAndMeasureTree(stopwatch), stopwatch),
                };
            timings.Average().Should().BeLessOrEqualTo(100);
        }

        private static dynamic BuildAndMeasureTree(Stopwatch stopwatch)
        {
            var before = stopwatch.ElapsedMilliseconds;
            //has c9000 nodes at the bottom level
            var idAndTree = GraphBuilder.BuildLargeGraph(5, 30);
            Debug.WriteLine("Took {0} milliseconds to build the graph", stopwatch.ElapsedMilliseconds - before);
            return idAndTree;
        }

        private static long RunAnalyse(dynamic idAndTree, Stopwatch stopwatch)
        {
            Node tree = idAndTree.Tree;
            var filterStart = stopwatch.ElapsedMilliseconds;
            NodeClosureAnalyser.Analyse(tree);
            var filterMark = stopwatch.ElapsedMilliseconds - filterStart;
            Debug.WriteLine("took {0} milliseconds to filter tree", filterMark);
            return filterMark;
        }

        private static long RunFilter(dynamic idAndTree, Stopwatch stopwatch)
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
            var filterStart = stopwatch.ElapsedMilliseconds;
            Node.Filter(tree, filter.ToArray());
            var filterMark = stopwatch.ElapsedMilliseconds - filterStart;
            Debug.WriteLine("took {0} milliseconds to filter tree", filterMark);
            return filterMark;
        }
    }
}
