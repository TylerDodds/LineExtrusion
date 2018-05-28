using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// A chunk of extruded point line segments between two <see cref="IntersectionPoint"/> as endpoints.
    /// </summary>
    public class ChunkBetweenIntersections
    {
        /// <summary>
        /// The intersection point serving as the start point of the chunk.
        /// </summary>
        public IntersectionPoint StartIntersection;

        /// <summary>
        /// The intersection point serving as the end point of the chunk.
        /// </summary>
        public IntersectionPoint EndIntersection;

        /// <summary>
        /// The extruded points that lie in between the intersection endpoints, defining the line segments of the chunk.
        /// </summary>
        public List<ExtrudedPointUV> ExtrudedPoints;

        /// <summary>
        /// The extruded points that lie in between the intersection endpoints, defining the line segments of the chunk, as a <see cref="SegmentwiseExtrudedPointListUV"/>.
        /// </summary>
        public SegmentwiseExtrudedPointListUV SegmentwiseExtrudedPointList;

        public Vector2WithUV PointAfterStart
        {
            get
            {
                return ExtrudedPoints.Count > 0 ? new Vector2WithUV(ExtrudedPoints[0]) : new Vector2WithUV(EndIntersection);
            }
        }

        public Vector2WithUV PointBeforeEnd
        {
            get
            {
                return ExtrudedPoints.Count > 0 ? new Vector2WithUV(ExtrudedPoints[ExtrudedPoints.Count - 1]) : new Vector2WithUV(StartIntersection);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ChunkBetweenIntersections"/> with values for its public fields.
        /// </summary>
        /// <param name="extrudedPoints">The extruded points that lie in between the intersection endpoints, defining the line segments of the chunk.</param>
        /// <param name="startIntersection">The intersection point serving as the start point of the chunk.</param>
        /// <param name="endIntersection">The extruded points that lie in between the intersection endpoints, defining the line segments of the chunk.</param>
        public ChunkBetweenIntersections(List<ExtrudedPointUV> extrudedPoints, IntersectionPoint startIntersection, IntersectionPoint endIntersection)
        {
            SegmentwiseExtrudedPointList = new SegmentwiseExtrudedPointListUV(extrudedPoints);
            ExtrudedPoints = extrudedPoints;
            StartIntersection = startIntersection;
            EndIntersection = endIntersection;
        }

        public override string ToString()
        {
            return string.Format("{0} Points from {1} to {2}", ExtrudedPoints.Count, StartIntersection.Point, EndIntersection.Point);
        }
    }
}
