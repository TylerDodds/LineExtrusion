using BabyDinoHerd.Extrusion.Line.Geometry;
using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping
{
    /// <summary>
    /// Generates UVs based on closest segments connecting equal u-parameters of extruded contours.
    /// </summary>
    public class ConnectedSegmentsUVGeneration
    {
        /// <summary>
        /// Gets the two portions of a closed contour corresponding to the part where the u-parameter is monotonic increasing and where it is monotonic decreasing.
        /// </summary>
        /// <param name="contourPoints">The set of points belonging to the contour. </param>
        /// <param name="increasingChunk">The <see cref="ExtrudedContourMonotonicChunk"/> with increasing u-parameters. </param>
        /// <param name="decreasingChunk">The <see cref="ExtrudedContourMonotonicChunk"/> with decreasing u-parameters. </param>
        /// <returns> True if the two monotonic chunks were found; False if the contour points didn't consist of exactly two monotonic (in u-parameter) portions, one increasing and the other decreasing. </returns>
        internal static bool GetExtrudedContourMonotonicChunks(IList<Vector2WithUV> contourPoints, out ExtrudedContourMonotonicChunk increasingChunk, out ExtrudedContourMonotonicChunk decreasingChunk)
        {
            increasingChunk = null;
            decreasingChunk = null;
            bool gotChunks = false;

            int minUIndex = -1, maxUIndex = -1;
            float minU = float.PositiveInfinity, maxU = float.NegativeInfinity;

            bool isPeriodic = contourPoints[0].Equals(contourPoints[contourPoints.Count - 1]);
            int numPoints = isPeriodic ? contourPoints.Count - 1 : contourPoints.Count;

            if (numPoints > 0)
            {
                for (int i = 0; i < numPoints; i++)
                {
                    var point = contourPoints[i];
                    var u = point.UV.x;
                    if (u < minU)
                    {
                        minU = u;
                        minUIndex = i;
                    }
                    if (u > maxU)
                    {
                        maxU = u;
                        maxUIndex = i;
                    }
                }

                Vector2WithUV previousPoint = contourPoints[minUIndex];
                List<Vector2WithUV> increasingPoints = new List<Vector2WithUV>() { previousPoint };
                int maxIndexShifted = maxUIndex < minUIndex ? maxUIndex + numPoints : maxUIndex;
                bool correctUDirection = true;
                for (int increasingIndex = minUIndex + 1; increasingIndex <= maxIndexShifted; increasingIndex++)
                {
                    var point = contourPoints[increasingIndex % numPoints];
                    if (point.UV.x >= previousPoint.UV.x)
                    {
                        increasingPoints.Add(point);
                    }
                    else
                    {
                        correctUDirection = false;
                        break;
                    }
                }
                if (correctUDirection)
                {
                    increasingChunk = new ExtrudedContourMonotonicChunk(increasingPoints, minU, maxU, isReversed: false);
                }

                int minIndexShifted = minUIndex < maxUIndex ? minUIndex + numPoints : minUIndex;
                previousPoint = contourPoints[minIndexShifted % numPoints];
                List<Vector2WithUV> decreasingPoints = new List<Vector2WithUV>() { previousPoint };
                correctUDirection = true;
                for (int decreasingIndex = minIndexShifted - 1; decreasingIndex >= maxUIndex; decreasingIndex--)
                {
                    var point = contourPoints[decreasingIndex % numPoints];
                    if (point.UV.x >= previousPoint.UV.x)//Since we're going through decreasing points in reverse order
                    {
                        decreasingPoints.Add(point);
                    }
                    else
                    {
                        correctUDirection = false;
                        break;
                    }
                }
                if (correctUDirection)
                {
                    decreasingChunk = new ExtrudedContourMonotonicChunk(decreasingPoints, minU, maxU, isReversed: true);
                }

                gotChunks = increasingChunk != null && decreasingChunk != null && increasingChunk.Points.Count >= 1 && decreasingChunk.Points.Count >= 1;
            }
            return gotChunks;
        }

        /// <summary>
        /// For a contour consisting of one portion monotonically increasing in u-parameter and the other monotonically decreasing, return the <see cref="SingleContourSegmentwiseCoverage"/>
        /// where there are matching points (at the same u-parameter) on both the increasing and decreasing portions.
        /// </summary>
        /// <param name="monotonicIncreasingChunk">Portion of contour with monotonically increasing u-parameters</param>
        /// <param name="monotonicDecreasingChunk">Portion of contour with monotonically decreasing u-parameters</param>
        internal static SingleContourSegmentwiseCoverage GetSingleContourSegmentwiseCoverage(ExtrudedContourMonotonicChunk monotonicIncreasingChunk, ExtrudedContourMonotonicChunk monotonicDecreasingChunk)
        {
            List<UParameterIndexAndMonotonicity> allContourPointUParameters = new List<UParameterIndexAndMonotonicity>();
            List<Vector2WithUV> monotonicIncreasingPoints = monotonicIncreasingChunk.Points;
            List<Vector2WithUV> monotonicDecreasingPoints = monotonicDecreasingChunk.Points;

            //Skip the last point in each chunk so as to not double-count the 'endpoints' of min/max u-parameter
            for (int i = 0; i < monotonicIncreasingPoints.Count - 1; i++)
            {
                allContourPointUParameters.Add(new UParameterIndexAndMonotonicity(monotonicIncreasingPoints[i].UV.x, i, true));
            }
            for (int i = 1; i < monotonicDecreasingPoints.Count; i++)
            {
                allContourPointUParameters.Add(new UParameterIndexAndMonotonicity(monotonicDecreasingPoints[i].UV.x, i, false));
            }
            allContourPointUParameters.Sort(CompareUParameterIndexAndMonotonicityByUParameter);

            allContourPointUParameters.RemoveAt(allContourPointUParameters.Count - 1);
            allContourPointUParameters.RemoveAt(0);
            int numInBetweenUParameters = allContourPointUParameters.Count;

            var firstPoint = monotonicIncreasingPoints[0];
            var lastPoint = monotonicDecreasingPoints[monotonicDecreasingPoints.Count - 1];
            List<Vector2WithUV> increasingChunkPoints = new List<Vector2WithUV>();
            List<Vector2WithUV> decreasingChunkPoints = new List<Vector2WithUV>();

            for (int i = 0; i < numInBetweenUParameters; i++)
            {
                UParameterIndexAndMonotonicity uParameterIndexAndMonotonicity = allContourPointUParameters[i];
                ExtrudedContourMonotonicChunk givenMonotonicChunk = uParameterIndexAndMonotonicity.IsMonotonicIncreasing ? monotonicIncreasingChunk : monotonicDecreasingChunk;
                ExtrudedContourMonotonicChunk otherMonotonicChunk = uParameterIndexAndMonotonicity.IsMonotonicIncreasing ? monotonicDecreasingChunk : monotonicIncreasingChunk;
                Vector2WithUV givenMonotonicChunkPoint = givenMonotonicChunk.Points[uParameterIndexAndMonotonicity.Index];
                Vector2WithUV matchingMonotonicChunkPoint;
                bool gotMatchingPoint = otherMonotonicChunk.GetClosestPoint(uParameterIndexAndMonotonicity.UParameter, out matchingMonotonicChunkPoint);
                if (gotMatchingPoint)
                {
                    var increasingPoint = uParameterIndexAndMonotonicity.IsMonotonicIncreasing ? givenMonotonicChunkPoint : matchingMonotonicChunkPoint;
                    var decreasingPoint = uParameterIndexAndMonotonicity.IsMonotonicIncreasing ? matchingMonotonicChunkPoint : givenMonotonicChunkPoint;
                    increasingChunkPoints.Add(increasingPoint);
                    decreasingChunkPoints.Add(decreasingPoint);
                }
            }

            //Now remove connected segments pairs that are effectively duplicates of a neighbouring segment
            for(int i = increasingChunkPoints.Count - 1; i > 0; i--)
            {
                var increasingPoint = increasingChunkPoints[i];
                var decreasingPoint = increasingChunkPoints[i];
                var prevIncreasingPoint = increasingChunkPoints[i - 1];
                var prevDecreasingPoint = increasingChunkPoints[i - 1];

                bool increasingAreClose = ConnectedSegmentPointsAreClose(prevIncreasingPoint, increasingPoint);
                bool decreasingAreClose = ConnectedSegmentPointsAreClose(prevDecreasingPoint, decreasingPoint);
                if (increasingAreClose && decreasingAreClose)
                {
                    increasingChunkPoints.RemoveAt(i);
                    decreasingChunkPoints.RemoveAt(i);
                }
            }

            return new SingleContourSegmentwiseCoverage(firstPoint, lastPoint, increasingChunkPoints, decreasingChunkPoints);
        }

        /// <summary>
        /// Returns if two connected segment points are close enough to be considered essentially duplicates.
        /// </summary>
        /// <param name="connectedSegmentPoint">A connected segment point.</param>
        /// <param name="neighbouringConnectedSegmentPoint">A neighbouring connected segment point.</param>
        private static bool ConnectedSegmentPointsAreClose(Vector2WithUV connectedSegmentPoint, Vector2WithUV neighbouringConnectedSegmentPoint)
        {
            const float epsilonDistance = 1e-5f;
            const float epsilonU = 1e-5f;
            bool withinDistance = (connectedSegmentPoint.Vector - neighbouringConnectedSegmentPoint.Vector).magnitude < epsilonDistance;
            bool withinU = System.Math.Abs(connectedSegmentPoint.UV.x - neighbouringConnectedSegmentPoint.UV.x) < epsilonU;
            return withinDistance && withinU;
        }

        /// <summary>
        /// Compares two <see cref="UParameterIndexAndMonotonicity"/> values by their u parameters.
        /// </summary>
        /// <param name="first">The first value</param>
        /// <param name="second">The second value</param>
        private static int CompareUParameterIndexAndMonotonicityByUParameter(UParameterIndexAndMonotonicity first, UParameterIndexAndMonotonicity second)
        {
            return first.UParameter.CompareTo(second.UParameter);
        }

        /// <summary>
        /// Data structure containing a point's u-parameter, index, and if it belongs to a monotonically-increasing (in u-parameter) portion of a contour.
        /// </summary>
        private struct UParameterIndexAndMonotonicity
        {
            /// <summary> Point's u-parameter </summary>
            public readonly float UParameter;
            /// <summary> Point's index </summary>
            public readonly int Index;
            /// <summary> If the point belongs to a monotonically-increasing (in u-parameter) portion of a contour </summary>
            public readonly bool IsMonotonicIncreasing;

            public UParameterIndexAndMonotonicity(float uParameter, int index, bool isMonotonicIncreasing)
            {
                UParameter = uParameter;
                Index = index;
                IsMonotonicIncreasing = isMonotonicIncreasing;
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2}", UParameter, Index, IsMonotonicIncreasing);
            }
        }
    }
}