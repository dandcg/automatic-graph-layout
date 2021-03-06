/*
Microsoft Automatic Graph Layout,MSAGL 

Copyright (c) Microsoft Corporation

All rights reserved. 

MIT License 

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
""Software""), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;

using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Layout.ProximityOverlapRemoval.MST;
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Msagl.Routing;
using Microsoft.Msagl.Routing.Rectilinear;

namespace Microsoft.Msagl.Prototype.Ranking {
    /// <summary>
    /// Ranking layout for directed graphs.
    /// </summary>
    public class RankingLayout : AlgorithmBase {
        GeometryGraph graph;

        private RankingLayoutSettings settings;

        /// <summary>
        /// Constructs the ranking layout algorithm.
        /// </summary>
        public RankingLayout(RankingLayoutSettings settings, GeometryGraph geometryGraph)
        {
            this.settings = settings;
            this.graph = geometryGraph;
        }

        private void SetNodePositionsAndMovedBoundaries() {
            
            int pivotNumber = Math.Min(graph.Nodes.Count,settings.PivotNumber);
            double scaleX = settings.ScaleX;
            double scaleY = settings.ScaleY;
          
            int[] pivotArray = new int[pivotNumber];
            PivotDistances pivotDistances = new PivotDistances(graph, false, pivotArray);
            pivotDistances.Run();
            double[][] c = pivotDistances.Result;
            double[] x, y;
            MultidimensionalScaling.LandmarkClassicalScaling(c, out x, out y, pivotArray);

            Standardize(x);
            double[] p = Centrality.PageRank(graph, .85, false);
            // double[] q = Centrality.PageRank(graph, .85, true);
            Standardize(p);
            // Standardize(q);

            int index = 0;
            foreach (Node node in graph.Nodes) {
                node.Center = new Point((int) (x[index]*scaleX), (int) (Math.Sqrt(p[index])*scaleY));
                index++;
            }

            OverlapRemoval.RemoveOverlaps(graph, settings.NodeSeparation);
        }

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void RunInternal() {
            SetNodePositionsAndMovedBoundaries();
            StraightLineEdges.SetStraightLineEdgesWithUnderlyingPolylines(graph);
            SetGraphBoundingBox();
        }

        private void SetGraphBoundingBox() {
            graph.BoundingBox = graph.PumpTheBoxToTheGraphWithMargins();
        }

        /// <summary>
        /// Scales and translates a vector so that all values are exactly between 0 and 1.
        /// </summary>
        /// <param name="x">Vector to be standardized.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        static void Standardize(double[] x)
        {
            double min = Double.PositiveInfinity;
            double max = Double.NegativeInfinity;
            for (int i = 0; i < x.Length; i++)
            {
                max = Math.Max(max, x[i]);
                min = Math.Min(min, x[i]);
            }
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = (x[i] - min) / (max - min);
            }
        }
    }
}
