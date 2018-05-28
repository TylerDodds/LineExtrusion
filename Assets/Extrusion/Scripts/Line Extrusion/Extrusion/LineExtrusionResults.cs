using BabyDinoHerd.Extrusion.Line.Geometry;
using System.Collections.Generic;
using BabyDinoHerd.Extrusion.Line.Configuration;

namespace BabyDinoHerd.Extrusion.Line.Extrusion
{
    /// <summary>
    /// Resulting contours from 2D line extrusion.
    /// </summary>
    public class LineExtrusionResults 
    {
        /// <summary>
        /// The original line points that were extruded.
        /// </summary>
        public SegmentwiseLinePointListUV OriginalLinePointList { get; private set; }

        /// <summary>
        /// List of connected contours resulting from line extrusion.
        /// </summary>
        public List<Vector2WithUV[]> Contours { get; private set; }

        /// <summary>
        /// List of connected contours resulting from line extrusion, with altered u parameters.
        /// </summary>
        public List<Vector2WithUV[]> ContoursWithAlteredUParameters { get; private set; }

        /// <summary>
        /// List of contours removed from initially-extruded surface because they were too close to the original line.
        /// </summary>
        public List<Vector2WithUV[]> RemovedContours { get; private set; }

        /// <summary>
        /// List of connected contours resulting from line extrusion, as <see cref="ChunkBetweenIntersectionsCollection"/>.
        /// </summary>
        public List<ChunkBetweenIntersectionsCollection> ContourChunkCollections { get; private set; }

        /// <summary>
        /// List of contours removed from initially-extruded surface because they were too close to the original line, as <see cref="ChunkBetweenIntersectionsCollection"/>.
        /// </summary>
        public List<ChunkBetweenIntersectionsCollection> RemovedContourChunkCollections { get; private set; }

        /// <summary>
        /// Results from connecting extruded contours to each other via equal-u-parameter segments.
        /// </summary>
        public ConnectedSegmentsExtrusionResults ConnectedSegmentsExtrusionResults { get; private set; }

        /// <summary>
        /// The intersection points between the extruded contours.
        /// </summary>
        public List<IntersectionPoint> IntersectionPoints { get; private set; }

        /// <summary>
        /// Set of initially-extruded points, before contours removed for being too close to the original line.
        /// </summary>
        public SegmentwiseExtrudedPointListUV InitiallyExtrudedPoints { get; private set; }

        /// <summary>
        /// Empty results.
        /// </summary>
        public static LineExtrusionResults Empty = new LineExtrusionResults(new SegmentwiseLinePointListUV(new List<LinePointUV>()), new List<Vector2WithUV[]>(), new List<Vector2WithUV[]>(), new List<ChunkBetweenIntersectionsCollection>(), new List<ChunkBetweenIntersectionsCollection>(), new List<IntersectionPoint>(), new SegmentwiseExtrudedPointListUV(new List<ExtrudedPointUV>()), LineExtrusionConfiguration.Empty);

        /// <summary>
        /// Creates a new instance of <see cref="LineExtrusionResults"/> with values for its public fields.
        /// </summary>
        /// <param name="originalLinePointList">The original line points</param>
        /// <param name="contours">List of connected contours resulting from line extrusion.</param>
        /// <param name="removedContours">List of contours removed from initially-extruded surface because they were too close to the original line.</param>
        /// <param name="contourChunkCollections">List of connected contours resulting from line extrusion, as <see cref="ChunkBetweenIntersectionsCollection"/>.</param>
        /// <param name="removedContourChunkCollections">List of contours removed from initially-extruded surface because they were too close to the original line, as <see cref="ChunkBetweenIntersectionsCollection"/>.</param>
        /// <param name="intersectionPoints">The intersection points of extruded segments.</param>
        /// <param name="initiallyExtrudedPoints">Set of initially-extruded points, before contours removed for being too close to the original line.</param>
        /// <param name="lineExtrusionConfiguration">Line extrusion configuration.</param>
        public LineExtrusionResults(SegmentwiseLinePointListUV originalLinePointList, List<Vector2WithUV[]> contours, List<Vector2WithUV[]> removedContours, List<ChunkBetweenIntersectionsCollection> contourChunkCollections, List<ChunkBetweenIntersectionsCollection> removedContourChunkCollections, List<IntersectionPoint> intersectionPoints, SegmentwiseExtrudedPointListUV initiallyExtrudedPoints, LineExtrusionConfiguration lineExtrusionConfiguration)
        {
            OriginalLinePointList = originalLinePointList;
            Contours = contours;
            ContoursWithAlteredUParameters = GetContoursWithAlteredUParameters(contours, lineExtrusionConfiguration);
            RemovedContours = removedContours;
            ContourChunkCollections = contourChunkCollections;
            RemovedContourChunkCollections = removedContourChunkCollections;
            IntersectionPoints = intersectionPoints;
            InitiallyExtrudedPoints = initiallyExtrudedPoints;
            ConnectedSegmentsExtrusionResults = new ConnectedSegmentsExtrusionResults(ContoursWithAlteredUParameters);
        }

        /// <summary>
        /// Returns a set of connected contours resulting from line extrusion with altered texture u parameters based on <paramref name="lineExtrusionConfiguration"/>.
        /// </summary>
        /// <param name="contours">List of connected contours resulting from line extrusion.</param>
        /// <param name="lineExtrusionConfiguration">Line extrusion configuration.</param>
        private static List<Vector2WithUV[]> GetContoursWithAlteredUParameters(List<Vector2WithUV[]> contours, LineExtrusionConfiguration lineExtrusionConfiguration)
        {
            List<Vector2WithUV[]> uvAlteredContours;
            int numContours = contours.Count;
            bool multipleContours = numContours > 1;

            bool performAlteration = multipleContours ? lineExtrusionConfiguration.GetMultipleContourTriangulation().HasUvAlteration : lineExtrusionConfiguration.GetSingleContourTriangulation().HasUvAlteration;
            var alteration = multipleContours ? lineExtrusionConfiguration.GetMultipleContourUVAlteration() : lineExtrusionConfiguration.GetSingleContourUVAlteration();

            if (performAlteration && alteration != null)
            {
                uvAlteredContours = new List<Vector2WithUV[]>();
                for (int i = 0; i < numContours; i++)
                {
                    var contour = contours[i];
                    var altered = alteration.GetUvAlteredExtrudedContour(contours[i], lineExtrusionConfiguration.ExtrusionAmount);
                    uvAlteredContours.Add(altered);
                }
            }
            else
            {
                uvAlteredContours = new List<Vector2WithUV[]>(contours);
            }
            return uvAlteredContours;
        }
    }
}