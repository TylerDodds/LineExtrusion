using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Interfaces;
using BabyDinoHerd.Extrusion.Line.Segmentation;
using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Extrusion
{
    public class SegmentedLineExtrusionFromIntersection
    {
        /// <summary>
        /// Return <see cref="LineExtrusionResults"/> containing contiguous lines of extruded points.
        /// </summary>
        /// <param name="originalLine">The line points to be extruded.</param>
        /// <param name="lineExtrusionConfiguration">The extrusion configuration.</param>
        public static LineExtrusionResults ExtrudeSegmentedLineBothDirections(ILineSegmentation originalLine, LineExtrusionConfiguration lineExtrusionConfiguration)
        {
            var originalLinePointsUV = LineSegmentation.GetLinePointsUV(originalLine);

            LineExtrusionResults results;

            if (originalLinePointsUV.Count >= 2)
            {
                var extrudedSubspaceTotal = SegmentedLineExtrusionFromSegments.GetExtrudedSubspaceAllSides(originalLinePointsUV, lineExtrusionConfiguration.ExtrusionAmount);

                results = GetExtrudedLines(originalLinePointsUV, extrudedSubspaceTotal, lineExtrusionConfiguration);
            }
            else
            {
                results = LineExtrusionResults.Empty;
            }

            return results;
        }

        /// <summary>
        /// Extract a list of <see cref="Vector2WithUV[]"/> representing contiguous lines of extruded points.
        /// </summary>
        /// <param name="linePoints">The line from which extruded points came from.</param>
        /// <param name="extrudedPoints">The extruded points.</param>
        /// <param name="lineExtrusionConfiguration">The extrusion configuration.</param>
        private static LineExtrusionResults GetExtrudedLines(IList<LinePointUV> linePoints, IList<ExtrudedPointUV> extrudedPoints, LineExtrusionConfiguration lineExtrusionConfiguration)
        {
            SegmentwiseLinePointListUV linePointList = new SegmentwiseLinePointListUV(linePoints);
            SegmentwiseExtrudedPointListUV extrudedPointList = new SegmentwiseExtrudedPointListUV(extrudedPoints);
            //Note that the extruded maximum segment distance can be large (order of extrusion distance) when the original line has a kink (discontinuous derivative)

            List<IntersectionPoint> intersectionPoints;
            List<IntersectionPointPair> chunkIntersectionEndpoints;
            SegmentedIntersectionDetermination.FindIntersectionPointsAndChunkEndpoints(extrudedPointList, out intersectionPoints, out chunkIntersectionEndpoints);

            return ExtractLineExtrusionResults_FindContinuousLast(linePointList, extrudedPointList, lineExtrusionConfiguration, intersectionPoints, chunkIntersectionEndpoints);
        }

        /// <summary>
        /// Extract <see cref="LineExtrusionResults"/> containing contiguous chunks of extruded points, after first determining intersection points and removing chunks too close to the original <paramref name="linePoints"/>.
        /// </summary>
        /// <param name="linePoints">The line against which chunks are compared by distance.</param>
        /// <param name="initiallyExtrudedPointList">The initially extruded points list.</param>
        /// <param name="lineExtrusionConfiguration">The extrusion configuration.</param>
        /// <param name="intersectionPoints">All intersection points between extruded segments.</param>
        /// <param name="chunkIntersectionEndpoints">Pairs of neighbouring intersection points, acting as chunk endpoints.</param>
        private static LineExtrusionResults ExtractLineExtrusionResults_FindContinuousLast(SegmentwiseLinePointListUV linePoints, SegmentwiseExtrudedPointListUV initiallyExtrudedPointList, LineExtrusionConfiguration lineExtrusionConfiguration, List<IntersectionPoint> intersectionPoints, List<IntersectionPointPair> chunkIntersectionEndpoints)
        {
            var extrudedChunksBetweenIntersections = ExtractChunksBetweenIntersections(initiallyExtrudedPointList, chunkIntersectionEndpoints);
            var removedChunks = SegmentedExtrudedChunkRemoval.RemoveChunksThatAreTooClose(extrudedChunksBetweenIntersections, linePoints, lineExtrusionConfiguration.ExtrusionAmount, intersectionPoints);
            var chunkCollectionList = ConnectExtrudedChunksBetweenIntersections(extrudedChunksBetweenIntersections);
            var removedChunksConnected = ConnectExtrudedChunksBetweenIntersections(removedChunks);

            var chunkConnector = lineExtrusionConfiguration.GetExtrudedChunkContourConnector();
            List<Vector2WithUV[]> contours = chunkConnector.ConnnectChunksBetweenIntersectionsIntoPointsLists(chunkCollectionList);
            List<Vector2WithUV[]> removedContours = chunkConnector.ConnnectChunksBetweenIntersectionsIntoPointsLists(removedChunksConnected);
            return new LineExtrusionResults(linePoints, contours, removedContours, chunkCollectionList, removedChunksConnected, intersectionPoints, initiallyExtrudedPointList, lineExtrusionConfiguration);
        }

        /// <summary>
        /// Connect extruded chunks between intersections when an end intersection point and another start intersection point are identical (and in-between points form a chunk too close to the original line).
        /// </summary>
        /// <param name="extrudedChunksBetweenIntersections">Extruded chunks between intersection points.</param>
        private static List<ChunkBetweenIntersectionsCollection> ConnectExtrudedChunksBetweenIntersections(List<ChunkBetweenIntersections> extrudedChunksBetweenIntersections)
        {
            var ret = new List<ChunkBetweenIntersectionsCollection>();
            for (int chunkIndex = 0; chunkIndex < extrudedChunksBetweenIntersections.Count; chunkIndex++)
            {
                var currentChunk = extrudedChunksBetweenIntersections[chunkIndex];
                var currentChunkList = new List<ChunkBetweenIntersections>() { currentChunk };
                var currentChunkStartpoint = currentChunk.StartIntersection;
                var currentChunkEndpoint = currentChunk.EndIntersection;
                for (int otherChunkIndex = chunkIndex + 1; otherChunkIndex < extrudedChunksBetweenIntersections.Count; otherChunkIndex++)
                {
                    var otherChunk = extrudedChunksBetweenIntersections[otherChunkIndex];
                    if (ExtrusionNumericalPrecision.IntersectionPointsAreGeometricallyIdentical(otherChunk.StartIntersection, currentChunkEndpoint))
                    {
                        currentChunkList.Add(otherChunk);
                        currentChunkEndpoint = otherChunk.EndIntersection;
                        extrudedChunksBetweenIntersections.RemoveAt(otherChunkIndex);
                        otherChunkIndex--;//So that when for loop increments, it ends up at the next chunk.
                        
                        if (ExtrusionNumericalPrecision.IntersectionPointsAreGeometricallyIdentical(currentChunkEndpoint, currentChunkStartpoint))
                        {
                            break;//If a full loop is made, stop connecting new chunks onto this.
                        }
                    }
                }

                bool isSingleChunkWithOnlyEndpoints = currentChunkList.Count == 1 && currentChunkList[0].ExtrudedPoints.Count == 0;
                if (!isSingleChunkWithOnlyEndpoints)
                {
                    ret.Add(new ChunkBetweenIntersectionsCollection(currentChunkList));
                }
            }

            return ret;
        }

        /// <summary>
        /// Extract a list of chunks of points between intersection points.
        /// </summary>
        /// <param name="extrudedPointList">List of extruded points</param>
        /// <param name="chunkIntersectionEndpoints">List of intersection endpoints to be used to define chunk endpoints.</param>
        private static List<ChunkBetweenIntersections> ExtractChunksBetweenIntersections(SegmentwiseExtrudedPointListUV extrudedPointList, List<IntersectionPointPair> chunkIntersectionEndpoints)
        {
            var extrudedPoints = extrudedPointList.Points;
            List<ChunkBetweenIntersections> ret = new List<ChunkBetweenIntersections>();
            int extrudedIndex = 0;
            for(int i = 0; i < chunkIntersectionEndpoints.Count; i++)
            {
                var currentChunkEndpoints = chunkIntersectionEndpoints[i];
                List<ExtrudedPointUV> chunkPoints = new List<ExtrudedPointUV>() { };
                bool withinChunkParameter = true;
                while (withinChunkParameter && extrudedIndex < extrudedPoints.Count)
                {
                    var extrudedPoint = extrudedPoints[extrudedIndex];
                    withinChunkParameter = extrudedPoint.Parameter < currentChunkEndpoints.End.Parameter;
                    if (withinChunkParameter)
                    {
                        if (extrudedPoint.Parameter > currentChunkEndpoints.Start.Parameter)
                        {
                            chunkPoints.Add(extrudedPoint);
                        }
                        extrudedIndex++;
                    }
                }
                ret.Add(new ChunkBetweenIntersections(chunkPoints, currentChunkEndpoints.Start, currentChunkEndpoints.End));
            }
            return ret;
        }

    }
}
