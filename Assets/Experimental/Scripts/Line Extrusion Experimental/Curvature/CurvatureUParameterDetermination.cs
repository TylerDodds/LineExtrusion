using UnityEngine;
using System.Collections.Generic;
using BabyDinoHerd.Extrusion.Line.Geometry;

namespace BabyDinoHerd.Extrusion.Line.Curvature.Experimental
{
    /// <summary>
    /// Determines u-paramaters between which to perform u-parameter stretching, based on line curvature.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class CurvatureUParameterDetermination
    {
        /// <summary>
        /// Gets the uv u parameters of the middle of line segments that have curvature inflection points (when curvature changes sign).
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        public static List<float> GetUParametersOfMiddleOfSegmentsThatHaveCurvatureInflectionPoints(IList<Vector2WithUV> linePoints)
        {
            List<float> ret = new List<float>();

            bool isNextInflectionPoint;
            int startingIndex = 0;
            DetermineIndexOfNextCurvatureInflectionPoint(linePoints, startingIndex, out isNextInflectionPoint, out startingIndex);
            int endingIndex = startingIndex;

            if (isNextInflectionPoint)
            {
                ret.Add(CurvatureDetermination.GetSegmentAverageUParameter(linePoints, endingIndex));
            }

            while (isNextInflectionPoint)
            {
                DetermineIndexOfNextCurvatureInflectionPoint(linePoints, startingIndex, out isNextInflectionPoint, out endingIndex);
                if (isNextInflectionPoint)
                {
                    ret.Add(CurvatureDetermination.GetSegmentAverageUParameter(linePoints, endingIndex));
                }
                startingIndex = endingIndex;
            }

            return ret;
        }

        /// <summary>
        /// Gets the uv u parameters of the midpoints between points that are a local maximum in curvature.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="minimumCurvatureDeltaInDegrees">An additional minimum difference of curvature, in degrees, from neighbouring curvature values for a curvature local maximum to be considered.</param>
        public static List<float> GetUParametersMidpointsBetweenCurvatureLocalMaxima(IList<Vector2WithUV> linePoints, float minimumCurvatureDeltaInDegrees)
        {
            var minimumCurvatureDeltaInRadians = minimumCurvatureDeltaInDegrees * Mathf.Deg2Rad;

            List<int> indices = new List<int>();
            if (linePoints.Count >= 5)
            {
                var prevCurvatureAngle = CurvatureDetermination.CurvatureAngle_NoIndexCheck(linePoints, 1);
                var prevAbsCurv = Mathf.Abs(prevCurvatureAngle);

                var currCurvatureAngle = CurvatureDetermination.CurvatureAngle_NoIndexCheck(linePoints, 2);
                var currAbsCurv = Mathf.Abs(currCurvatureAngle);

                for (int i = 3; i < linePoints.Count - 2; i++)
                {

                    var nextCurvatureAngle = CurvatureDetermination.CurvatureAngle_NoIndexCheck(linePoints, i);
                    var nextAbsCurv = Mathf.Abs(nextCurvatureAngle);

                    if (currAbsCurv > prevAbsCurv + minimumCurvatureDeltaInRadians && currAbsCurv > nextAbsCurv + minimumCurvatureDeltaInRadians)
                    {
                        indices.Add(i - 1);
                    }

                    prevCurvatureAngle = currCurvatureAngle;
                    prevAbsCurv = currAbsCurv;

                    currCurvatureAngle = nextCurvatureAngle;
                    currAbsCurv = nextAbsCurv;
                }
            }

            List<float> uParameters = GetUParametersOfMidpointsBetweenIndices(linePoints, indices);

            return uParameters;
        }

        /// <summary>
        /// Determines the u parameters of the midpoints between anchor indices of a set of line points.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="anchorIndices">The indices of the anchor points that form the end of the chunks to take midpoints of.</param>
        private static List<float> GetUParametersOfMidpointsBetweenIndices(IList<Vector2WithUV> linePoints, List<int> anchorIndices)
        {
            List<float> ret = new List<float>();
            List<Vector2WithUV> averagedPointsBetweenAnchorPoints = new List<Vector2WithUV>();
            for (int i = 0; i < anchorIndices.Count - 1; i++)
            {
                var startIndex = anchorIndices[i];
                var endIndex = anchorIndices[i + 1];
                Vector2WithUV halfwayPoint = GetChunkMidpointBetweenIndices(linePoints, startIndex, endIndex);

                averagedPointsBetweenAnchorPoints.Add(halfwayPoint);
            }

            foreach (var averagedPoint in averagedPointsBetweenAnchorPoints)
            {
                ret.Add(averagedPoint.UV.x);
            }

            return ret;
        }

        /// <summary>
        /// Returns the arcdistance midpoint between a start and end index of a list of points.
        /// </summary>
        /// <param name="linePoints">The list of points defining the line</param>
        /// <param name="startIndex">The index defining the start point.</param>
        /// <param name="endIndex">The index defining the end point.</param>
        private static Vector2WithUV GetChunkMidpointBetweenIndices(IList<Vector2WithUV> linePoints, int startIndex, int endIndex)
        {
            Vector2WithUV halfwayPoint = linePoints[Mathf.RoundToInt(0.5f * (startIndex + endIndex))];
            float chunkDistanceTotal = 0f;
            for (int j = startIndex + 1; j <= endIndex; j++)
            {
                chunkDistanceTotal += (linePoints[j].Vector - linePoints[j - 1].Vector).magnitude;
            }

            float targetChunkDistance = 0.5f * chunkDistanceTotal;
            float chunkDistancePartial = 0f;
            for (int j = startIndex + 1; j <= endIndex; j++)
            {
                float segmentLength = (linePoints[j].Vector - linePoints[j - 1].Vector).magnitude;
                chunkDistancePartial += segmentLength;
                if (chunkDistancePartial > targetChunkDistance)
                {
                    float fractionOfSegment = 1f - (chunkDistancePartial - targetChunkDistance) / segmentLength;
                    var averagedPointAlongSegment = linePoints[j - 1].AverageWith(linePoints[j], fractionOfSegment);
                    halfwayPoint = averagedPointAlongSegment;
                    break;
                }
            }

            return halfwayPoint;
        }

        /// <summary>
        /// Gets the uv u-parameter corresponding to a point in the middle of when the curvature falls below (and rises above) the curvature cutoff <paramref name="curvatureAngleCutoffDegrees"/>.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="curvatureAngleCutoffDegrees">Curvature angle cutoff, defined as a cutoff between 'small' and 'large' curvature angles.</param>
        /// </summary>
        public static List<float> GetUParameterBetweenCurvatureFallAndRise(IList<Vector2WithUV> linePoints, float curvatureAngleCutoffDegrees)
        {
            List<float> ret = new List<float>();
            List<int> risingIndices = new List<int>();
            List<int> fallingIndices = new List<int>();
            float curvatureAngleCutoffRad = curvatureAngleCutoffDegrees * Mathf.Deg2Rad;

            if (linePoints.Count >= 3)
            {
                var prevCurvatureAngle = CurvatureDetermination.CurvatureAngle_NoIndexCheck(linePoints, 1);
                var prevAbsCurv = Mathf.Abs(prevCurvatureAngle);

                for (int i = 2; i < linePoints.Count - 2; i++)
                {
                    var currCurvatureAngle = CurvatureDetermination.CurvatureAngle_NoIndexCheck(linePoints, i);
                    var currAbsCurv = Mathf.Abs(currCurvatureAngle);

                    if (currAbsCurv >= curvatureAngleCutoffRad && prevAbsCurv < curvatureAngleCutoffRad)
                    {
                        risingIndices.Add(i - 1);
                    }
                    else if (prevAbsCurv >= curvatureAngleCutoffRad && currAbsCurv < curvatureAngleCutoffRad)
                    {
                        fallingIndices.Add(i - 1);
                    }

                    prevCurvatureAngle = currCurvatureAngle;
                    prevAbsCurv = currAbsCurv;
                }
            }

            if (fallingIndices.Count > 0 && risingIndices.Count > 0)
            {
                int fIndex = 0;
                int rIndex = 0;
                while (rIndex < risingIndices.Count && fIndex < fallingIndices.Count)
                {
                    int fallingIndex = fallingIndices[fIndex];
                    int risingIndex = risingIndices[rIndex];
                    if (risingIndex <= fallingIndex)
                    {
                        rIndex++;
                    }
                    else
                    {
                        var midpoint = GetChunkMidpointBetweenIndices(linePoints, fallingIndex, risingIndex);
                        ret.Add(midpoint.UV.x);
                        rIndex++;
                        fIndex++;
                    }
                }
            }

            return ret;
        }




        /// <summary>
        /// Gets the uv u parameters corresponding to the middle of line segments that are on the border of having large curvature (defined by <paramref name="curvatureAngleCutoffDegrees"/>).
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        public static List<float> GetUParametersOfMiddleOfSegmentsOnBorderOfLargeCurvature(IList<Vector2WithUV> linePoints, float curvatureAngleCutoffDegrees)
        {
            float curvatureAngleCutoffRadians = curvatureAngleCutoffDegrees * Mathf.Deg2Rad;
            List<float> ret = new List<float>();
            if (linePoints.Count >= 3)
            {
                var prevCurvatureAngle = CurvatureDetermination.CurvatureAngle_NoIndexCheck(linePoints, 1);
                var prevAbsCurv = Mathf.Abs(prevCurvatureAngle);

                for (int i = 2; i < linePoints.Count - 2; i++)
                {
                    var currCurvatureAngle = CurvatureDetermination.CurvatureAngle_NoIndexCheck(linePoints, i);
                    var currAbsCurv = Mathf.Abs(currCurvatureAngle);

                    var minAbsCurv = Mathf.Min(prevAbsCurv, currAbsCurv);
                    var maxAbsCurv = Mathf.Max(prevAbsCurv, currAbsCurv);

                    if (minAbsCurv < curvatureAngleCutoffRadians && maxAbsCurv >= curvatureAngleCutoffRadians)
                    {
                        ret.Add(CurvatureDetermination.GetSegmentAverageUParameter(linePoints, i - 1));
                    }

                    prevCurvatureAngle = currCurvatureAngle;
                    prevAbsCurv = currAbsCurv;
                }
            }

            return ret;
        }

        /// <summary>
        /// Determine the index of the start point of the line segment that is the next curvature inflection point in the given line.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="startingIndex">The index of the start point of the line segment.</param>
        /// <param name="isNextInflectionPoint">If another inflection point exists.</param>
        /// <param name="indexOfInflection">The index of the next inflection point (if it exists) after the starting index.</param>
        private static void DetermineIndexOfNextCurvatureInflectionPoint(IList<Vector2WithUV> linePoints, int startingIndex, out bool isNextInflectionPoint, out int indexOfInflection)
        {
            bool isInflection = false;
            int index = startingIndex;
            bool canDetermine = true;
            while (!isInflection)
            {
                index++;
                DetermineCurvatureInflectionPoint(linePoints, index, out canDetermine, out isInflection);
                if (!canDetermine)
                {
                    break;
                }
            }

            isNextInflectionPoint = canDetermine;
            indexOfInflection = index;
        }

        /// <summary>
        /// Determine if the line segment starting from <paramref name="startingIndex"/> is a curvature inflection point.
        /// Note that this method has difficulty handling curvature discontinuities, potentially (for instance, at 'kinks' in a line).
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="startingIndex">The index of the start point of the line segment.</param>
        /// <param name="canDetermine">Whether the curvature can be determined (not implemented currently for endpoints of the line). </param>
        /// <param name="isCurvatureInflectionPoint">If this segment is a curvature inflection point.</param>
        private static void DetermineCurvatureInflectionPoint(IList<Vector2WithUV> linePoints, int startingIndex, out bool canDetermine, out bool isCurvatureInflectionPoint)
        {
            DetermineCurvatureInflectionPoint_FromStraightSignCheck(linePoints, startingIndex, out canDetermine, out isCurvatureInflectionPoint);
        }

        /// <summary>
        /// Determine if the line segment starting from <paramref name="startingIndex"/> is a curvature inflection point, by comparing the sign of the curvature at the endpoints of the segment.
        /// </summary>
        /// <param name="linePoints">The points of the line in question.</param>
        /// <param name="startingIndex">The index of the start point of the line segment.</param>
        /// <param name="canDetermine">Whether the curvature can be determined (not implemented currently for endpoints of the line). </param>
        /// <param name="isCurvatureInflectionPoint">If this segment is a curvature inflection point.</param>
        private static void DetermineCurvatureInflectionPoint_FromStraightSignCheck(IList<Vector2WithUV> linePoints, int startingIndex, out bool canDetermine, out bool isCurvatureInflectionPoint)
        {
            canDetermine = linePoints.Count > 3 && startingIndex + 2 < linePoints.Count && startingIndex > 0;
            if (canDetermine)
            {
                var prevCurvatureSign = CurvatureDetermination.CurvatureSign_NoIndexCheck(linePoints, startingIndex);
                var nextCurvatureSign = CurvatureDetermination.CurvatureSign_NoIndexCheck(linePoints, startingIndex + 1);
                isCurvatureInflectionPoint = prevCurvatureSign * nextCurvatureSign == -1;
            }
            else
            {
                isCurvatureInflectionPoint = false;
            }
        }



    }
}