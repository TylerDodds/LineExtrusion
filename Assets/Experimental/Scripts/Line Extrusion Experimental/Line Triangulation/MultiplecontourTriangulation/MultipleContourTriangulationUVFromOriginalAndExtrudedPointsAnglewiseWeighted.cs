using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using System;
using System.Collections.Generic;
using BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.MultipleContour.Experimental
{
    /// <summary>
    /// Triangulates an outer extruded contour with inner hole contours, with uvs generated from closest original line and an anglewise weighting between closest extruded contour chunks between intersection.
    /// </summary>
    public class MultipleContourTriangulationUVFromOriginalAndExtrudedPointsAnglewiseWeighted : MultipleContourTriangulationBase
    {
        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        [BabyDinoHerd.Experimental]
        public override bool HasUvAlteration
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Generates UV values for triangulated points on the interior of an extruded surface, from closest original line and an anglewise weighting between closest extruded contour chunks between intersection.
        /// </summary>
        /// <param name="triangulatedPoints">The triangulated points</param>
        /// <param name="lineExtrusionResults">Line extrusion resulting contours.</param>
        /// <param name="originalLinePointsList">Original line points</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        protected override Vector2[] GenerateTriangulatedPointUVs(Vector3[] triangulatedPoints, LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointsList, LineExtrusionConfiguration extrusionConfiguration)
        {
            Vector2[] uvs = new Vector2[triangulatedPoints.Length];
            float extrusionAmountAbs = Math.Abs(extrusionConfiguration.ExtrusionAmount);

            for (int i = 0; i < uvs.Length; i++)
            {
                var triangulatedPoint = triangulatedPoints[i];
                uvs[i] = GenerateTriangulatedPointUvAnglewiseWeightedBetweenExtrudedChunks(triangulatedPoint, originalLinePointsList, lineExtrusionResults, extrusionAmountAbs);
            }

            return uvs;
        }

        /// <summary>
        /// If original line points should be included in triangulation.
        /// </summary>
        protected override bool IncludeOriginalLinePoints
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// If removed extruded contours should be included in triangulation.
        /// </summary>
        protected override bool IncludeRemovedContours
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Generates UV value for a triangulated points on the interior of an extruded surface, based on the original line and an anglewise weighting between closest extruded contour chunks between intersection.
        /// </summary>
        /// <param name="triangulatedPoint">The triangulated point</param>
        /// <param name="originalLinePointList">List of original line points</param>
        /// <param name="lineExtrusionResults">Line extrusion resulting contours.</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        private static Vector2 GenerateTriangulatedPointUvAnglewiseWeightedBetweenExtrudedChunks(Vector3 triangulatedPoint, SegmentwiseLinePointListUV originalLinePointList, LineExtrusionResults lineExtrusionResults, float extrusionAmountAbs)
        {

            Vector2 closestSegmentDiff; int closestSegmentIndex;
            var closestPointOnOriginalLine = SegmentedLineUtil.ClosestPointAlongSegmentwiseLine(triangulatedPoint, originalLinePointList, extrusionAmountAbs, out closestSegmentDiff, out closestSegmentIndex);
            var segmentDirectionOrig = closestSegmentDiff.normalized;

            Vector2 closestSegmentDiffOnExtrudedContours;
            var closestPointOnExtrudedContours = GetClosestPointAndUVOnContoursBetweenIntersections(triangulatedPoint, lineExtrusionResults.ContourChunkCollections, extrusionAmountAbs, out closestSegmentDiffOnExtrudedContours);
            var segmentDirectionExtruded = closestSegmentDiffOnExtrudedContours.normalized;

            var uv = PointUVGenerationBetweenOriginalAndExtrudedPoints.GetTriangulatedPointUvFromClosestOriginalAndExtrudedSegments(triangulatedPoint, closestPointOnOriginalLine, segmentDirectionOrig, closestPointOnExtrudedContours, segmentDirectionExtruded, extrusionAmountAbs);

            return uv;
        }

        /// <summary>
        /// Gets the closest point to <paramref name="triangulatedPoint"/> on all extruded contours (<paramref name="extrudedContoursCurvatureAlteredPointsLists"/>).
        /// </summary>
        /// <param name="triangulatedPoint">The triangulated point</param>
        /// <param name="extrudedContoursCurvatureAlteredPointsLists">Extruded contours that form boundaries of triangulated mesh, with uv parameters.</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        /// <param name="closestSegmentDifference">Start-to-end vector of the segment containing the closest point</param>
        private static Vector2WithUV GetClosestPointAndUVOnContoursBetweenIntersections(Vector2 triangulatedPoint, List<ChunkBetweenIntersectionsCollection> extrudedContoursCurvatureAlteredPointsLists, float extrusionAmountAbs, out Vector2 closestSegmentDiff)
        {
            float minDistanceSquared = float.MaxValue;
            Vector2WithUV fullyClosestPoint = new Vector2WithUV();
            closestSegmentDiff = new Vector2();
            for (int chunkCollectionIndex = 0; chunkCollectionIndex < extrudedContoursCurvatureAlteredPointsLists.Count; chunkCollectionIndex++)
            {
                var chunkCollection = extrudedContoursCurvatureAlteredPointsLists[chunkCollectionIndex];
                for (int chunkInCollectionIndex = 0; chunkInCollectionIndex < chunkCollection.Chunks.Count; chunkInCollectionIndex++)
                {
                    var chunk = chunkCollection.Chunks[chunkInCollectionIndex];

                    Vector2 closestSegmentDiffOnChunkContour; int closestSegmentIndexOnChunkContour;
                    var closestPointOnThisContour = SegmentedLineUtil.ClosestPointAlongSegmentwiseLine(triangulatedPoint, chunk.SegmentwiseExtrudedPointList, extrusionAmountAbs, out closestSegmentDiffOnChunkContour, out closestSegmentIndexOnChunkContour);
                    var distanceSquared = (closestPointOnThisContour.Vector - triangulatedPoint).sqrMagnitude;
                    if (distanceSquared < minDistanceSquared)
                    {
                        minDistanceSquared = distanceSquared;
                        closestSegmentDiff = closestSegmentDiffOnChunkContour;
                        fullyClosestPoint = closestPointOnThisContour;
                    }

                    Vector2WithUV chunkStartIntersection = new Vector2WithUV(chunk.StartIntersection);
                    Vector2WithUV chunkPointAfterStart = chunk.PointAfterStart;
                    UpdateClosestPointFromAdditionalSegment(triangulatedPoint, ref closestSegmentDiff, ref minDistanceSquared, ref fullyClosestPoint, chunkPointAfterStart, chunkStartIntersection, chunkCollection.Chunks, chunkInCollectionIndex, SegmentIntersectionPoint.StartPoint);

                    Vector2WithUV chunkEndIntersection = new Vector2WithUV(chunk.EndIntersection);
                    Vector2WithUV chunkPointBeforeEnd = chunk.PointBeforeEnd;

                    UpdateClosestPointFromAdditionalSegment(triangulatedPoint, ref closestSegmentDiff, ref minDistanceSquared, ref fullyClosestPoint, chunkPointBeforeEnd, chunkEndIntersection, chunkCollection.Chunks, chunkInCollectionIndex, SegmentIntersectionPoint.EndPoint);
                }
            }
            return fullyClosestPoint;
        }

        /// <summary>
        /// Given an additional segment defined by two endpoints, determine if that segment is the closer to the triangulated point than previous results, and update those results if so.
        /// </summary>
        /// <param name="triangulatedPoint">The triangulated point.</param>
        /// <param name="closestSegmentDiff">Reference to the difference vector along the closest segment to the triangulated point.</param>
        /// <param name="minDistanceSquared">Reference to the distance squared to the closest segment to the triangulated point.</param>
        /// <param name="fullyClosestPoint">Reference to the closest point along the closest segment to the triangulated point.</param>
        /// <param name="segmentStart">The segment start point.</param>
        /// <param name="segmentEnd">The segment end point.</param>
        /// <param name="chunks">The list of chunks in the current contour.</param>
        /// <param name="chunkInCollectionIndex">The index of the chunk that the segment belongs to.</param>
        /// <param name="segmentIntersectionPoint">Which point (if any) of the segment is an intersection point.</param>
        private static void UpdateClosestPointFromAdditionalSegment(Vector2 triangulatedPoint, ref Vector2 closestSegmentDiff, ref float minDistanceSquared, ref Vector2WithUV fullyClosestPoint, Vector2WithUV segmentStart, Vector2WithUV segmentEnd, List<ChunkBetweenIntersections> chunks, int chunkInCollectionIndex, SegmentIntersectionPoint segmentIntersectionPoint)
        {
            Vector2 segmentStartPoint = segmentStart.Vector;
            Vector2 segmentEndPoint = segmentEnd.Vector;
            float distanceSquaredToSegment;
            var currentClosestFractionToSegment = LineSegmentUtil.ClosestFractionAlongSegment(segmentStartPoint, segmentEndPoint, triangulatedPoint, out distanceSquaredToSegment);
            if (distanceSquaredToSegment < minDistanceSquared)
            {
                minDistanceSquared = distanceSquaredToSegment;
                closestSegmentDiff = segmentEndPoint - segmentStartPoint;
                fullyClosestPoint = segmentStart.AverageWith(segmentEnd, currentClosestFractionToSegment);

                //NB In the following two cases, the closest point is actually an intersection point, so we expect a 'jump' in u-parameter as a result. 
                if (segmentIntersectionPoint == SegmentIntersectionPoint.StartPoint && currentClosestFractionToSegment <= 0)
                {
                    var segmentAfterIntersectionNormal = NormalUtil.NormalFromTangent(closestSegmentDiff.normalized);
                    var previousChunk = chunks[(chunkInCollectionIndex - 1) % chunks.Count];
                    var previousChunkEndSegmentDifference = previousChunk.EndIntersection.Point - previousChunk.PointBeforeEnd.Vector;
                    var segmentBeforeIntersectionNormal = NormalUtil.NormalFromTangent(previousChunkEndSegmentDifference.normalized);

                    var angleFractionTowardNextSegment = GetAngleFractionTowardNextSegment(segmentBeforeIntersectionNormal, segmentAfterIntersectionNormal, fullyClosestPoint.Vector - triangulatedPoint);
                    fullyClosestPoint.UV = new Vector2WithUV(previousChunk.EndIntersection).AverageWith(segmentStart, angleFractionTowardNextSegment).UV;
                }
                else if (segmentIntersectionPoint == SegmentIntersectionPoint.EndPoint && currentClosestFractionToSegment >= 1f)
                {
                    var segmentBeforeIntersectionNormal = NormalUtil.NormalFromTangent(closestSegmentDiff.normalized);
                    var nextChunk = chunks[(chunkInCollectionIndex + 1) % chunks.Count];
                    var nextChunkStartSegmentDifference = nextChunk.PointAfterStart.Vector - nextChunk.StartIntersection.Point;
                    var segmentAfterIntersectionNormal = NormalUtil.NormalFromTangent(nextChunkStartSegmentDifference.normalized);

                    var angleFractionTowardNextSegment = GetAngleFractionTowardNextSegment(segmentAfterIntersectionNormal, segmentBeforeIntersectionNormal, fullyClosestPoint.Vector - triangulatedPoint);
                    fullyClosestPoint.UV = segmentEnd.AverageWith(new Vector2WithUV(nextChunk.StartIntersection), angleFractionTowardNextSegment).UV;
                }
            }
        }

        /// <summary>
        /// Gets the angular fraction of a vector, from two segments' common midpoints to a test point, relative to the two segments' normal vector.
        /// </summary>
        /// <param name="previousSegmentNormal">The first segment normal vector.</param>
        /// <param name="nextSegmentNormal">The second segment normal vector.</param>
        /// <param name="deltaSegmentsMiddlePointToTestPoint">Vector from the segments' common middle point to the test point.</param>
        private static float GetAngleFractionTowardNextSegment(Vector2 previousSegmentNormal, Vector2 nextSegmentNormal, Vector2 deltaSegmentsMiddlePointToTestPoint)
        {
            float fraction = 0.5f;

            var deltaDirection = deltaSegmentsMiddlePointToTestPoint.normalized;
            if (deltaDirection.sqrMagnitude > 0)
            {
                var dotDeltaPrevious = Vector2.Dot(deltaDirection, previousSegmentNormal);
                var dotDeltaNext = Vector2.Dot(deltaDirection, nextSegmentNormal);
                var signDotTotals = Mathf.Sign(dotDeltaNext * dotDeltaPrevious);
                if (signDotTotals != 0)
                {
                    dotDeltaPrevious *= signDotTotals;
                    dotDeltaNext *= signDotTotals;

                    var absAngleToPrev = Mathf.Acos(Mathf.Clamp(dotDeltaPrevious, -1, 1));
                    var absAngleToNext = Mathf.Acos(Mathf.Clamp(dotDeltaNext, -1, 1));

                    fraction = absAngleToNext / (absAngleToPrev + absAngleToNext);
                }
            }

            return fraction;
        }

        /// <summary>
        /// Which point of a line segment is an intersection point.
        /// </summary>
        private enum SegmentIntersectionPoint
        {
            None,
            StartPoint,
            EndPoint
        }
    }
}