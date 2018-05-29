using System.Collections.Generic;
using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Interfaces;
using BabyDinoHerd.Extrusion.Line.Curvature.Experimental;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping.Alteration.Experimental
{
    /// <summary>
    /// Alters uv parameters of extruded points in some attempt to avoid pinching of u parameter when a line intersects with itself (and similar issues).
    /// Uses line curvature to determine landmark points and stretches u-parameters evenly (based on arcdistance) between them.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public abstract class CurvatureUVStretchingBase : IExtrudedContourUVAlteration
    {
        /// <summary>
        /// Returns a new list of points and uvs of an extruded contour, where the uv parameters have been altered to try to smooth out discontinuities due to extruded contour intersections.
        /// </summary>
        /// <param name="extrudedLinePoints">Points comprising the extruded contour.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        public Vector2WithUV[] GetUvAlteredExtrudedContour(Vector2WithUV[] extrudedLinePoints, float extrusionAmount)
        {
            Vector2WithUV[] altered;

            if (extrudedLinePoints.Length > 1)
            {
                var extrudedsPointToAlter = new List<Vector2WithUV>(extrudedLinePoints);
                var indicesToStretchBetween = GetIndicesToStretchUParameterBetween(extrudedsPointToAlter);
                StretchExtrudedUParameters(extrudedsPointToAlter, indicesToStretchBetween);
                altered = new Vector2WithUV[extrudedsPointToAlter.Count];
                for (int i = 0; i < extrudedsPointToAlter.Count; i++)
                {
                    altered[i] = extrudedsPointToAlter[i];
                }
            }
            else
            {
                altered = extrudedLinePoints;
            }

            return altered;
        }

        /// <summary>
        /// Gets the set of u-parameters used as anchors to stretch <paramref name="extrudedLinePoints"/> between.
        /// </summary>
        /// <param name="extrudedLinePoints">Points comprising the extruded line</param>
        protected abstract List<float> GetUParametersToStretchBetween(IList<Vector2WithUV> extrudedLinePoints);

        /// <summary>
        /// Gets the <see cref="UvStretchingIndexesInformation"/> information determining how to perform u-parameter stretching of the <paramref name="extrudedLinePoints"/>.
        /// </summary>
        /// <param name="extrudedLinePoints">Points comprising the extruded line</param>
        private List<UvStretchingIndexesInformation> GetIndicesToStretchUParameterBetween(IList<Vector2WithUV> extrudedLinePoints)
        {
            var uParametersToStretchBetween = GetUParametersToStretchBetween(extrudedLinePoints);
            var indicesToStretchBetween = GetIndicesToStretchUParameterBetween_BothSidesLine(extrudedLinePoints, uParametersToStretchBetween);
            return indicesToStretchBetween;
        }

        /// <summary>
        /// Gets the <see cref="UvStretchingIndexesInformation"/> information determining how to perform u-parameter stretching of the <paramref name="extrudedLinePoints"/>.
        /// </summary>
        /// <param name="extrudedLinePoints">Points comprising the extruded line</param>
        /// <param name="uParameters">The u parameters to stretch between.</param>
        private static List<UvStretchingIndexesInformation> GetIndicesToStretchUParameterBetween_BothSidesLine(IList<Vector2WithUV> extrudedLinePoints, List<float> uParameters)
        {
            List<UvStretchingIndexesInformation> indicesToStretchBetween = new List<UvStretchingIndexesInformation>();

            if (uParameters.Count >= 2)
            {
                int indexOfStartUParameter = GetIndexOfStartUParameter(extrudedLinePoints, UParameterDirection.Positive);
                int numPoints = extrudedLinePoints.Count;
                int index = indexOfStartUParameter;
                bool previousIndexIncludeHalfSegment = true;
                bool nextIndexIncludeHalfSegment = true;

                while (index < indexOfStartUParameter + numPoints)
                {
                    int nextIndex = DetermineNextIndexFromUOrVParameterCrossing(extrudedLinePoints, indexOfStartUParameter, index, uParameters);

                    indicesToStretchBetween.Add(new UvStretchingIndexesInformation(index, nextIndex, previousIndexIncludeHalfSegment, nextIndexIncludeHalfSegment));

                    index = nextIndex;
                    previousIndexIncludeHalfSegment = nextIndexIncludeHalfSegment;
                }
                indicesToStretchBetween.RemoveAt(0);//Remove the first pair, containing the placeholder 'StartUParameter' placeholder, since it is superceded by the last pair.

            }

            return indicesToStretchBetween;
        }

        /// <summary>
        /// Stretch extruded line point u parameters based on a list of <see cref="UvStretchingIndexesInformation"/> indicating the groups of segments to stretch over.
        /// </summary>
        /// <param name="extrudedPoints">The extruded line points.</param>
        /// <param name="indicesToStretchBetween">The groups of segments to stretch over.</param>
        private static void StretchExtrudedUParameters(IList<Vector2WithUV> extrudedPoints, List<UvStretchingIndexesInformation> indicesToStretchBetween)
        {
            for (int i = 0; i < indicesToStretchBetween.Count; i++)
            {
                UvStretchingIndexesInformation currentSegmentIndicesInformation = indicesToStretchBetween[i];
                int pIndex = currentSegmentIndicesInformation.StartIndex;
                int nIndex = currentSegmentIndicesInformation.NextIndex;
                bool includePrevHalfSeg = currentSegmentIndicesInformation.IncludeStartHalfSegment;
                bool includeNextHalfSeg = currentSegmentIndicesInformation.IncludeEndHalfSegment;
                float distance = GetChunkDistancePeriodic(extrudedPoints, pIndex, nIndex, includePrevHalfSeg, includeNextHalfSeg);
                StretchUVParameters(extrudedPoints, pIndex, nIndex, includePrevHalfSeg, includeNextHalfSeg, distance);
            }
        }

        /// <summary>
        /// Determine the index of the next point on the line (where indices are periodic) where the line segment of that index crosses any u-parameter in a list <paramref name="uParameters"/>, or the half v parameter <see cref="_vParameterHalf"/>.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="indexOfStartU">Index of the point where line's u parameter is minimum or maximum. Essentially the 'starting index' of the line, where we treat the point indices periodically.</param>
        /// <param name="currentIndex">The current index to begin checking from.</param>
        /// <param name="uParameters">The u parameters against which to check for crossings.</param>
        private static int DetermineNextIndexFromUOrVParameterCrossing(IList<Vector2WithUV> linePoints, int indexOfStartU, int currentIndex, List<float> uParameters)
        {
            int numPoints = linePoints.Count;
            int nextIndex = currentIndex + 1;
            var indexPointUV = linePoints[currentIndex % numPoints].UV;
            bool continueToFindNextIndex = currentIndex < indexOfStartU + numPoints;
            while (continueToFindNextIndex)
            {
                var nextIndexPointUV = linePoints[nextIndex % numPoints].UV;
                bool doBracketUParameter = PointsBracketUParameter(uParameters, indexPointUV, nextIndexPointUV);
                bool doBracketVParameterHalf = PointsBracketVParameterHalf(indexPointUV, nextIndexPointUV);
                bool atEndOfSegment = currentIndex == indexOfStartU + numPoints - 1;
                continueToFindNextIndex = !(doBracketUParameter || doBracketVParameterHalf || atEndOfSegment);
                if (continueToFindNextIndex)
                {
                    nextIndex++;
                }
            }

            return nextIndex;
        }

        /// <summary>
        /// Whether the uv v-parameter values of two points bracket the v parameter of <see cref="_vParameterHalf"/>.
        /// </summary>
        /// <param name="firstPointUv">The first point's uv values</param>
        /// <param name="secondPointUv">The second point's uv values</param>
        protected static bool PointsBracketVParameterHalf(Vector2 firstPointUv, Vector2 secondPointUv)
        {
            return Mathf.Min(firstPointUv.y, secondPointUv.y) < _vParameterHalf && Mathf.Max(firstPointUv.y, secondPointUv.y) >= _vParameterHalf;
        }

        /// <summary>
        /// Whether the uv u-parameter values of two points bracket any of the u values in a given list.
        /// </summary>
        /// <param name="uParameters">The list of u-parameter values to be compared against.</param>
        /// <param name="firstPointUv">The first point's uv values</param>
        /// <param name="secondPointUv">The second point's uv values</param>
        private static bool PointsBracketUParameter(List<float> uParameters, Vector2 firstPointUv, Vector2 secondPointUv)
        {
            bool bracket = false;
            for(int i = 0; i < uParameters.Count; i++)
            {
                float uParameter = uParameters[i];
                bracket = Mathf.Min(firstPointUv.x, secondPointUv.x) < uParameter && Mathf.Max(firstPointUv.x, secondPointUv.x) >= uParameter;
                if(bracket)
                {
                    break;
                }
            }
            return bracket;
        }

        /// <summary>
        /// Stretches uv u parameter of all points between the start and end indices. The u parameter is reassigned uniformly in distance between the start and end points.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="endIndex">The ending index.</param>
        /// <param name="previousIndexIncludeHalfSegment">If half of the previous line segment from the start point should be included in the stretching.</param>
        /// <param name="nextIndexIncludeHalfSegment">If half of the next line segment from the end point should be included in the stretching.</param>
        /// <param name="totalSegmentsDistance">The total distance of the chunk of segments from the start point to end point.</param>
        protected static void StretchUVParameters(IList<Vector2WithUV> linePoints, int startIndex, int endIndex, bool previousIndexIncludeHalfSegment, bool nextIndexIncludeHalfSegment, float totalSegmentsDistance)
        {
            int numPoints = linePoints.Count;
            var beginPt = linePoints[startIndex % numPoints];
            var endPt = linePoints[endIndex % numPoints];

            float distanceSoFar = 0f;
            for(int index = startIndex; index <= endIndex; index++)
            {
                if(index == startIndex && previousIndexIncludeHalfSegment)
                {
                    distanceSoFar += 0.5f * DistanceBetweenPointsPeriodic(linePoints, index, index - 1);
                }
                else if(index == endIndex && nextIndexIncludeHalfSegment)
                {
                    distanceSoFar += 0.5f * DistanceBetweenPointsPeriodic(linePoints, index, index + 1);
                }
                else
                {
                    distanceSoFar += DistanceBetweenPointsPeriodic(linePoints, index, index - 1);
                }
                float distanceFraction = distanceSoFar / totalSegmentsDistance;
                float uParameterFromDistanceFraction = beginPt.UV.x + (endPt.UV.x - beginPt.UV.x) * distanceFraction;

                var currentPoint = linePoints[index % numPoints];
                var uv = new Vector2(uParameterFromDistanceFraction, currentPoint.UV.y);
                currentPoint.UV = uv;
                linePoints[index % numPoints] = currentPoint;   
            }
        }

        /// <summary>
        /// Gets the distance along a chunk of line segments, between two indices, where the line indices are periodic. 
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="endIndex">The ending index.</param>
        /// <param name="previousIndexIncludeHalfSegment">If half of the previous line segment from the start point should be included in the stretching.</param>
        /// <param name="nextIndexIncludeHalfSegment">If half of the next line segment from the end point should be included in the stretching.</param>
        protected static float GetChunkDistancePeriodic(IList<Vector2WithUV> linePoints, int startIndex, int endIndex, bool previousIndexIncludeHalfSegment, bool nextIndexIncludeHalfSegment)
        {
            float distance = 0f;
            int numPoints = linePoints.Count;
            for(int i = startIndex; i < endIndex; i++)
            {
                distance += DistanceBetweenPointsPeriodic(linePoints, i, i + 1);
            }
            if(previousIndexIncludeHalfSegment)
            {
                distance += 0.5f * DistanceBetweenPointsPeriodic(linePoints, startIndex, startIndex - 1);
            }
            if(nextIndexIncludeHalfSegment)
            {
                distance += 0.5f * DistanceBetweenPointsPeriodic(linePoints, endIndex, endIndex + 1);
            }
            return distance;
        }

        /// <summary>
        /// The distance between two points on a line, where the line indices are periodic.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="index1">The index of the first point.</param>
        /// <param name="index2">The index of the second point.</param>
        private static float DistanceBetweenPointsPeriodic(IList<Vector2WithUV> linePoints, int index1, int index2)
        {
            int numPoints = linePoints.Count;
            var p1 = linePoints[index1 % numPoints];
            var p2 = linePoints[index2 % numPoints];
            return (p2.Vector - p1.Vector).magnitude;
        }

        /// <summary>
        /// Gets the index of the point where the uv u parameter is at a 'starting' value (minimum when u increases along the line, maximum when u decreases along the line).
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="uParameterDirection">The direction (positive or negative) that the u parameter changes along the line.</param>
        protected static int GetIndexOfStartUParameter(IList<Vector2WithUV> linePoints, UParameterDirection uParameterDirection)
        {
            if(uParameterDirection == UParameterDirection.Positive)
            {
                return GetIndexOfMinUParameter(linePoints);
            }
            else
            {
                return GetIndexOfMaxUParameter(linePoints);
            }
        }

        /// <summary>
        /// Gets the index of the point in the line that has the maximum u parameter.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        private static int GetIndexOfMaxUParameter(IList<Vector2WithUV> linePoints)
        {
            int indexOfMaxU = -1;
            float maxU = float.NegativeInfinity;
            for (int i = 0; i < linePoints.Count; i++)
            {
                float uParam = linePoints[i].UV.x;
                if (uParam > maxU)
                {
                    maxU = uParam;
                    indexOfMaxU = i;
                }
            }
            return indexOfMaxU;
        }

        /// <summary>
        /// Gets the index of the point in the line that has the minimum u parameter.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        private static int GetIndexOfMinUParameter(IList<Vector2WithUV> linePoints)
        { 
            int indexOfMinU = -1;
            float minU = float.PositiveInfinity;
            for (int i = 0; i < linePoints.Count; i++)
            {
                float uParam = linePoints[i].UV.x;
                if (uParam < minU)
                {
                    minU = uParam;
                    indexOfMinU = i;
                }
            }
            return indexOfMinU;
        }

        /// <summary>
        /// The direction that u parameter changes along a line (if u parameter increases or decreasing going along the line).
        /// </summary>
        protected enum UParameterDirection
        {
            Positive,
            Negative,
        }

        /// <summary>
        /// The half v parameter. This marks the boundary between the right-hand and left-hand halves of the extruded line.
        /// </summary>
        const float _vParameterHalf = 0.5f;
    }
}