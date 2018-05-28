using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;
using System.Collections.Generic;
using BabyDinoHerd.Extrusion.Line.Interfaces;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.MultipleContour
{
    /// <summary>
    /// Base class for multiple-contour triangulation of an outer extruded contour and inner hole contours.
    /// </summary>
    public abstract class MultipleContourTriangulationBase : IMultipleContourTriangulation
    {
        /// <summary>
        /// Triangulate a polygon based on an outer closed contour of extruded points, further inner hole contours of extruded points, and the original line points from which they were extruded.
        /// </summary>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="lineExtrusionResults">Line extrusion results</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        public Mesh TriangulatePolygonWithHoleContours(LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointList, LineExtrusionConfiguration extrusionConfiguration)
        {
            var polygon = new Polygon();

            var outerThenAllInnerPointsList = lineExtrusionResults.Contours;
            var originalLinePoints = originalLinePointList.Points;

            int marker = 0;
            AddContourToPolygon(polygon, outerThenAllInnerPointsList[0], ref marker, contourIsHole: false);

            if (IncludeOriginalLinePoints)
            {
                AddOriginalLinePointsVerticesAndSegmentsToPolygon(originalLinePoints, polygon, ref marker);
            }

            if (IncludeRemovedContours)
            {
                List<Vector2WithUV[]> removedContours = lineExtrusionResults.RemovedContours;
                for (int i = 0; i < removedContours.Count; i++)
                {
                    AddContourToPolygon(polygon, removedContours[i], ref marker, contourIsHole: false);
                }
            }

            for (int i = 1; i < outerThenAllInnerPointsList.Count; i++)
            {
                AddContourToPolygon(polygon, outerThenAllInnerPointsList[i], ref marker, contourIsHole: true);
            }

            var options = ConstraintOptions;
            var quality = QualityOptions;

            var iMesh = polygon.Triangulate(options, quality);
            var unityMesh = new Mesh();

            Vector3[] vectorArray; Dictionary<int, int> vertexIdToArrayIndexDictionary;
            GetVector3ArrayAndIndexIdDictionaryFromVertices(iMesh.Vertices, out vectorArray, out vertexIdToArrayIndexDictionary);
            var trindicesArray = GetTrindicesArrayFromTriangles(iMesh.Triangles, vertexIdToArrayIndexDictionary);
            Vector2[] uvArray = GenerateTriangulatedPointUVs(vectorArray, lineExtrusionResults, originalLinePointList, extrusionConfiguration);

            unityMesh.vertices = vectorArray;
            unityMesh.triangles = trindicesArray;
            unityMesh.uv = uvArray;

            return unityMesh;
        }

        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        public abstract bool HasUvAlteration { get; }

        /// <summary>
        /// Generates UV values for triangulated points on the interior of an extruded surface.
        /// </summary>
        /// <param name="triangulatedPoints">The triangulated points</param>
        /// <param name="lineExtrusionResults">Line extrusion resulting contours.</param>
        /// <param name="originalLinePointsList">Original line points</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        protected abstract Vector2[] GenerateTriangulatedPointUVs(Vector3[] triangulatedPoints, LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointsList, LineExtrusionConfiguration extrusionConfiguration);

        /// <summary>
        /// If original line points should be included in triangulation.
        /// </summary>
        protected abstract bool IncludeOriginalLinePoints { get; }

        /// <summary>
        /// If removed extruded contours should be included in triangulation.
        /// </summary>
        protected abstract bool IncludeRemovedContours { get; }

        /// <summary>
        /// Triangulation constraint options.
        /// </summary>
        protected virtual ConstraintOptions ConstraintOptions
        {
            get
            {
                return new ConstraintOptions() { ConformingDelaunay = true };
            }
        }

        /// <summary>
        /// Triangulation quality options.
        /// </summary>
        protected virtual QualityOptions QualityOptions
        {
            get
            {
                return new QualityOptions() { MinimumAngle = 25 };
            }
        }

        /// <summary>
        /// Adds the original line points, as vertices and segments, to the polygon to be triangulated.
        /// </summary>
        /// <param name="originalLinePoints">Original line points.</param>
        /// <param name="polygon">The polygon to be triangulated.</param>
        /// <param name="marker">Marker index for the vertices.</param>
        private static void AddOriginalLinePointsVerticesAndSegmentsToPolygon(IList<LinePointUV> originalLinePoints, Polygon polygon, ref int marker)
        {
            Vertex prevVertex = null;
            for (int origIndex = 0; origIndex < originalLinePoints.Count; origIndex++)
            {
                var point = originalLinePoints[origIndex].Point;
                Vertex currVertex = new Vertex(point.x, point.y, marker);
                polygon.Add(currVertex);
                if (origIndex > 0)
                {
                    var segment = new Segment(prevVertex, currVertex);
                    polygon.Add(segment, insert: false);
                }
                prevVertex = currVertex;
            }
            marker++;
        }

        /// <summary>
        /// Add a contour to a <see cref="Polygon"/> Polygon.
        /// </summary>
        /// <param name="polygon">Thee polygon.</param>
        /// <param name="contourPoints">The contour of <see cref="Vector2WithUV"/> points.</param>
        /// <param name="marker">Marker index for contour.</param>
        /// <param name="contourIsHole">Whether the contour is a hole.</param>
        /// 
        private static void AddContourToPolygon(Polygon polygon, Vector2WithUV[] contourPoints, ref int marker, bool contourIsHole)
        {
            var contour = new Contour(GetVertexListFromVectors(contourPoints, SkipLast.Yes), marker);

            if (contour.Points.Count == 3)
            {
                if (contourIsHole)
                {
                    var x = (contour.Points[0].X + contour.Points[1].X + contour.Points[2].X) / 3;
                    var y = (contour.Points[0].Y + contour.Points[1].Y + contour.Points[2].Y) / 3;
                    //Get centroid point as hole point.
                    var holePoint = new Point((float)x, (float)y);
                    polygon.Add(contour, holePoint);
                }
                else
                {
                    polygon.Add(contour, contourIsHole);
                }
            }
            else
            {
                polygon.Add(contour, contourIsHole);
            }

            marker++;
        }

        /// <summary>
        /// Get a list of Vertex from a set of <see cref="Vector2WithUV"/> points.
        /// </summary>
        /// <param name="points">The points</param>
        /// <param name="skipLast">Whether the last point should be skipped.</param>
        private static List<Vertex> GetVertexListFromVectors(Vector2WithUV[] points, SkipLast skipLast)
        {
            List<Vertex> ret = new List<Vertex>();
            var endIndex = skipLast == SkipLast.Yes ? points.Length - 2 : points.Length - 1;
            for (int i = 0; i <= endIndex; i++)
            {
                var point = points[i];
                ret.Add(new Vertex(point.Vector.x, point.Vector.y));
            }
            return ret;
        }

        /// <summary>
        /// Get an array of vectors representing vertices as well as a dictionary mapping triangle vertex id to the vertex's position in the returned array.
        /// </summary>
        /// <param name="vertices">Collection of Vertices.</param>
        /// <param name="vectorArray">The resulting array of vectors representing the vertices.</param>
        /// <param name="vertexIdToArrayIndexDictionary">Dictionary mapping triangle vertex id to vertex array index.</param>
        private static void GetVector3ArrayAndIndexIdDictionaryFromVertices(ICollection<Vertex> vertices, out Vector3[] vectorArray, out Dictionary<int, int> vertexIdToArrayIndexDictionary)
        {
            vertexIdToArrayIndexDictionary = new Dictionary<int, int>();
            vectorArray = new Vector3[vertices.Count];
            int index = 0;
            foreach (var vert in vertices)
            {
                vertexIdToArrayIndexDictionary[vert.ID] = index;
                vectorArray[index++] = new Vector3((float)vert.X, (float)vert.Y);
            }
        }

        /// <summary>
        /// Get an array of triangle indices from TriangleNet triangles.
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <param name="vertexIdToArrayIndexDictionary">Dictionary mapping triangle vertex id to vertex array index.</param>
        private static int[] GetTrindicesArrayFromTriangles(ICollection<TriangleNet.Topology.Triangle> triangles, Dictionary<int, int> vertexIdToArrayIndexDictionary)
        {
            int[] ret = new int[triangles.Count * 3];
            int index = 0;
            foreach (var tri in triangles)
            {
                ret[index++] = vertexIdToArrayIndexDictionary[tri.GetVertexID(0)];
                ret[index++] = vertexIdToArrayIndexDictionary[tri.GetVertexID(2)];
                ret[index++] = vertexIdToArrayIndexDictionary[tri.GetVertexID(1)];
            }
            return ret;
        }

        /// <summary>
        /// If the last point should be skipped.
        /// </summary>
        private enum SkipLast
        {
            No,
            Yes,
        }
    }
}