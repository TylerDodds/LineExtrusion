using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// Contains a contour consisting of one portion monotonically increasing in u-parameter and the other monotonically decreasing, 
    /// with matching points (at the same u-parameter) on both the increasing and decreasing portions.
    /// </summary>
    public class SingleContourSegmentwiseCoverage
    {
        /// <summary> The first point (with minimum u-parameter), common to both increasing and decreasing portions. </summary>
        public readonly Vector2WithUV FirstPoint;
        /// <summary> The last point (with maximum u-parameter), common to both increasing and decreasing portions. </summary>
        public readonly Vector2WithUV LastPoint;
        /// <summary> The set of matching u-parameter points lying on the monotonically-increasing portion. </summary>
        public readonly List<Vector2WithUV> IncreasingPortionSegmentPoints;
        /// <summary> The set of matching u-parameter points lying on the monotonically-decreasing portion. </summary>
        public readonly List<Vector2WithUV> DecreasingPortionSegmentPoints;

        public SingleContourSegmentwiseCoverage(Vector2WithUV firstPoint, Vector2WithUV lastPoint, List<Vector2WithUV> increasingPortionPoints, List<Vector2WithUV> decreasingPortaionPoints)
        {
            FirstPoint = firstPoint;
            LastPoint = lastPoint;
            IncreasingPortionSegmentPoints = increasingPortionPoints;
            DecreasingPortionSegmentPoints = decreasingPortaionPoints;
        }
    }
}