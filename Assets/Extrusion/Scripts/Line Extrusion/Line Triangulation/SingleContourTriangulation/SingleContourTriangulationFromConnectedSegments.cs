using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Interfaces;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.SingleContour
{
    /// <summary>
    /// Single-contour triangulation using quads connecting extruded points of equal u-parameter.
    /// </summary>
    public class SingleContourTriangulationFromConnectedSegments : ISingleContourTriangulation
    {

        /// <summary>
        /// Triangulate a line based on a single closed contour of extruded points by connecting points of equal u-parameter.
        /// </summary>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="lineExtrusionResults">Line extrusion results</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        public Mesh TriangulateClosedContour(LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointList, LineExtrusionConfiguration extrusionConfiguration)
        {
            return TriangulateLine_ConnectExtrudedPointsOfSameUParameter(lineExtrusionResults, extrusionConfiguration);
        }

        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        public bool HasUvAlteration
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Triangulate a line based on a single closed contour of extruded points, by creating quads connecting points of the the same u-parameter on opposite sides of the original line.
        /// </summary>
        /// <param name="lineExtrusionResults">Line extrusion results</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        private static Mesh TriangulateLine_ConnectExtrudedPointsOfSameUParameter(LineExtrusionResults lineExtrusionResults, LineExtrusionConfiguration extrusionConfiguration)
        {
            var mesh = new Mesh();

            var originalLinePointsList = lineExtrusionResults.OriginalLinePointList;
            //NB Setting closest original segment index is not even needed in this implementation, but it's a reasonable choice to sent as the second half of the addition uv we're sending.
            var extrusionAmountAbs = Mathf.Abs(extrusionConfiguration.ExtrusionAmount);
            var connectedSegmentsResults = lineExtrusionResults.ConnectedSegmentsExtrusionResults;
            var contourConnectedSegmentsResults = connectedSegmentsResults.SegmentwiseCoverageOfSingleContour;

            if (contourConnectedSegmentsResults != null)
            {
                var firstPoint = contourConnectedSegmentsResults.FirstPoint;
                var lastPoint = contourConnectedSegmentsResults.LastPoint;
                var increasingSegmentPoints = contourConnectedSegmentsResults.IncreasingPortionSegmentPoints;
                var decreasingSegmentPoints = contourConnectedSegmentsResults.DecreasingPortionSegmentPoints;

                if (increasingSegmentPoints.Count == decreasingSegmentPoints.Count && increasingSegmentPoints.Count >= 1)
                {
                    int numInBetweenUParameters = decreasingSegmentPoints.Count;
                    Vector3[] vertices = new Vector3[numInBetweenUParameters * 2 + 2];
                    Vector2[] uvs = new Vector2[numInBetweenUParameters * 2 + 2];
                    Vector2[] indexUvs = new Vector2[numInBetweenUParameters * 2 + 2];
                    int[] trindices = new int[(numInBetweenUParameters - 1) * 6 + 6];

                    vertices[0] = firstPoint.Vector;
                    uvs[0] = firstPoint.UV;
                    int closestOriginalSegmentIndex = SegmentedLineUtil.ClosestIndexAlongSegmentwiseLine(firstPoint.Vector, originalLinePointsList, extrusionAmountAbs);
                    indexUvs[0] = new Vector2(-1, closestOriginalSegmentIndex);
                    vertices[numInBetweenUParameters * 2 + 1] = lastPoint.Vector;
                    uvs[numInBetweenUParameters * 2 + 1] = lastPoint.UV;
                    closestOriginalSegmentIndex = SegmentedLineUtil.ClosestIndexAlongSegmentwiseLine(lastPoint.Vector, originalLinePointsList, extrusionAmountAbs);
                    indexUvs[numInBetweenUParameters * 2 + 1] = new Vector2(numInBetweenUParameters, closestOriginalSegmentIndex);

                    for (int i = 0; i < numInBetweenUParameters; i++)
                    {
                        var increasingPoint = increasingSegmentPoints[i];
                        var decreasingPoint = decreasingSegmentPoints[i];

                        vertices[i * 2 + 1] = increasingPoint.Vector;
                        uvs[i * 2 + 1] = increasingPoint.UV;
                        closestOriginalSegmentIndex = SegmentedLineUtil.ClosestIndexAlongSegmentwiseLine(increasingPoint.Vector, originalLinePointsList, extrusionAmountAbs);
                        indexUvs[i * 2 + 1] = new Vector2(i, closestOriginalSegmentIndex);

                        vertices[i * 2 + 2] = decreasingPoint.Vector;
                        uvs[i * 2 + 2] = decreasingPoint.UV;
                        closestOriginalSegmentIndex = SegmentedLineUtil.ClosestIndexAlongSegmentwiseLine(decreasingPoint.Vector, originalLinePointsList, extrusionAmountAbs);
                        indexUvs[i * 2 + 2] = new Vector2(i, closestOriginalSegmentIndex);
                    }

                    //NB This order of triangle indices is consistent with our direction of extrusion points winding around the original line
                    trindices[0] = 0;
                    trindices[1] = 1;
                    trindices[2] = 2;
                    trindices[numInBetweenUParameters * 6 - 3] = numInBetweenUParameters * 2 + 0;
                    trindices[numInBetweenUParameters * 6 - 2] = numInBetweenUParameters * 2 - 1;
                    trindices[numInBetweenUParameters * 6 - 1] = numInBetweenUParameters * 2 + 1;

                    for (int j = 0; j < numInBetweenUParameters - 1; j++)
                    {
                        trindices[3 + j * 6 + 0] = 1 + j * 2 + 1;
                        trindices[3 + j * 6 + 1] = 1 + j * 2 + 0;
                        trindices[3 + j * 6 + 2] = 1 + j * 2 + 2;
                        trindices[3 + j * 6 + 3] = 1 + j * 2 + 2;
                        trindices[3 + j * 6 + 4] = 1 + j * 2 + 3;
                        trindices[3 + j * 6 + 5] = 1 + j * 2 + 1;
                    }

                    mesh.vertices = vertices;
                    mesh.uv = uvs;
                    mesh.triangles = trindices;
                    mesh.uv4 = indexUvs;
                }
                else
                {
                    Debug.LogError("Not enough connected extruded points to create mesh.");
                }
            }
            else
            {
                Debug.LogError("Single contour connected segments results are not calculated.");
            }

            return mesh;
        }
    }
}