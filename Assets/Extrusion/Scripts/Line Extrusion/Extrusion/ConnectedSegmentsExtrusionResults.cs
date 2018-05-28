using BabyDinoHerd.Extrusion.Line.TextureMapping;
using BabyDinoHerd.Extrusion.Line.Geometry;
using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Extrusion
{
    /// <summary>
    /// Results from connecting extruded contours to each other via equal-u-parameter segments.
    /// </summary>
    public class ConnectedSegmentsExtrusionResults
    {
        public readonly List<ExtrudedContourMonotonicChunk> MonotonicIncreasingChunks;
        public readonly List<ExtrudedContourMonotonicChunk> MonotonicDecreasingChunks;
        public readonly SingleContourSegmentwiseCoverage SegmentwiseCoverageOfSingleContour;

        /// <summary>
        /// Creates an instance of <see cref="ConnectedSegmentsExtrusionResults"/> from a set of extruded contours with altered UV parameters.
        /// </summary>
        /// <param name="extrudedVectorUVAlteredContours">A set of extruded contours with altered UV parameters</param>
        public ConnectedSegmentsExtrusionResults(List<Vector2WithUV[]> extrudedVectorUVAlteredContours)
        {
            List<ExtrudedContourMonotonicChunk> monotonicIncreasingChunks = new List<ExtrudedContourMonotonicChunk>();
            List<ExtrudedContourMonotonicChunk> monotonicDecreasingChunks = new List<ExtrudedContourMonotonicChunk>();

            for (int contourIndex = 0; contourIndex < extrudedVectorUVAlteredContours.Count; contourIndex++)
            {
                var contour = extrudedVectorUVAlteredContours[contourIndex];
                ExtrudedContourMonotonicChunk monotonicIncreasingChunk, monotonicDecreasingChunk;
                bool gotChunks = ConnectedSegmentsUVGeneration.GetExtrudedContourMonotonicChunks(contour, out monotonicIncreasingChunk, out monotonicDecreasingChunk);
                if(gotChunks)
                {
                    monotonicIncreasingChunks.Add(monotonicIncreasingChunk);
                    monotonicDecreasingChunks.Add(monotonicDecreasingChunk);
                }
            }

            SingleContourSegmentwiseCoverage singleContourSegmentwiseCoverage = null;
            if (extrudedVectorUVAlteredContours.Count == 1 && monotonicIncreasingChunks.Count == 1 && monotonicDecreasingChunks.Count == 1)
            {
                singleContourSegmentwiseCoverage = ConnectedSegmentsUVGeneration.GetSingleContourSegmentwiseCoverage(monotonicIncreasingChunks[0], monotonicDecreasingChunks[0]);
            }

            MonotonicIncreasingChunks = monotonicIncreasingChunks;
            MonotonicDecreasingChunks = monotonicDecreasingChunks;
            SegmentwiseCoverageOfSingleContour = singleContourSegmentwiseCoverage;
        }

        
    }
}