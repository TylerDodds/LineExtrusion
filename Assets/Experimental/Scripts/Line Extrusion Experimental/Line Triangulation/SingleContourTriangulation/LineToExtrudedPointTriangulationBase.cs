using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;
using System;
using System.Collections.Generic;
using BabyDinoHerd.Extrusion.Line.Interfaces;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.SingleContour.Experimental
{
    /// <summary>
    /// Base class for single-contour triangulation that connects original line points to their corresponding closest extruded points.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public abstract class LineToExtrudedPointTriangulationBase : ISingleContourTriangulation
    {
        /// <summary>
        /// Triangulate a line based on a single closed contour of extruded points and the original line points from which they were extruded.
        /// </summary>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="lineExtrusionResults">Line extrusion results</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        public Mesh TriangulateClosedContour(LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointList, LineExtrusionConfiguration extrusionConfiguration)
        {
            var mesh = new Mesh();

            var extrudedVectorsUVAltered = lineExtrusionResults.ContoursWithAlteredUParameters[0];

            mesh.vertices = GetVerticesFromLineAndExtruded(originalLinePointList.Points, extrudedVectorsUVAltered);
            mesh.uv = GetUVsFromLineAndExtruded(originalLinePointList, extrudedVectorsUVAltered, extrusionConfiguration);
            mesh.triangles = GetTrindicesFromLineAndExtruded(originalLinePointList.Points, extrudedVectorsUVAltered);

            return mesh;
        }

        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        public abstract bool HasUvAlteration { get; }

        /// <summary>
        /// Get an array of uv vectors from those of original line points, followed by extruded points.
        /// </summary>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="extrudedVectorsUvAltered">Extruded points with altered uvs</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        protected abstract Vector2[] GetUVsFromLineAndExtruded(SegmentwiseLinePointListUV originalLinePointList, Vector2WithUV[] extrudedVectorsUvAltered, LineExtrusionConfiguration extrusionConfiguration);

        /// <summary>
        /// Get an array of vertex vectors from those of original line point followed by extruded points.
        /// </summary>
        /// <param name="originalLinePoints">Original line points</param>
        /// <param name="extrudedVectorsUvAltered">Extruded points wioth altered uv</param>
        private static Vector3[] GetVerticesFromLineAndExtruded(IList<LinePointUV> originalLinePoints, Vector2WithUV[] extrudedVectorsUvAltered)
        {
            var numOriginalLinePoints = originalLinePoints.Count;
            Vector3[] verts = new Vector3[extrudedVectorsUvAltered.Length + numOriginalLinePoints];
            for (int i = 0; i < numOriginalLinePoints; i++)
            {
                verts[i] = originalLinePoints[i].Point;
            }
            for (int i = 0; i < extrudedVectorsUvAltered.Length; i++)
            {
                verts[i + numOriginalLinePoints] = extrudedVectorsUvAltered[i].Vector;
            }
            return verts;
        }

        /// <summary>
        /// Gets triangle indices from original line and extruded points
        /// </summary>
        /// <param name="originalLinePoints">Original line points</param>
        /// <param name="extrudedVectorsUvAltered">Extruded points wioth altered uv</param>
        private static int[] GetTrindicesFromLineAndExtruded(IList<LinePointUV> originalLinePoints, Vector2WithUV[] extrudedVectorsUVAltered)
        {
            List<int> trindices = new List<int>();
            int numOriginalPoints = originalLinePoints.Count;

            int previousExtrudedLineIndex = 0;
            int currentExtrudedLineIndex = 1;

            int previousOriginalLineIndex, currentOriginalLineIndex;

            while (currentExtrudedLineIndex < extrudedVectorsUVAltered.Length)
            {
                previousOriginalLineIndex = GetClosestLineIndex(extrudedVectorsUVAltered[previousExtrudedLineIndex].Vector, originalLinePoints);
                currentOriginalLineIndex = GetClosestLineIndex(extrudedVectorsUVAltered[currentExtrudedLineIndex].Vector, originalLinePoints);

                int absDiff = Math.Abs(currentOriginalLineIndex - previousOriginalLineIndex);
                int dir = Math.Sign(currentOriginalLineIndex - previousOriginalLineIndex);

                if (absDiff > 1)
                {
                    //Increment previousOriginalLineIndex
                    while (absDiff > 0)
                    {
                        var previousOriginalLineIndexPlusDir = previousOriginalLineIndex + dir;
                        int previousOriginalPointClosestExtrudedIndex = GetClosestExtrudedLineIndexOfSelectPoints(originalLinePoints[previousOriginalLineIndex], extrudedVectorsUVAltered, previousExtrudedLineIndex, currentExtrudedLineIndex);

                        int previousPlusDirOriginalPointClosestExtrudedIndex = GetClosestExtrudedLineIndexOfSelectPoints(originalLinePoints[previousOriginalLineIndexPlusDir], extrudedVectorsUVAltered, previousExtrudedLineIndex, currentExtrudedLineIndex);
                        var newAbsDiff = Math.Abs(previousOriginalPointClosestExtrudedIndex - previousPlusDirOriginalPointClosestExtrudedIndex);
                        if (newAbsDiff == 0)
                        {
                            //This part is a triangle
                            trindices.Add(previousOriginalLineIndex);
                            trindices.Add(numOriginalPoints + previousOriginalPointClosestExtrudedIndex);
                            trindices.Add(previousOriginalLineIndexPlusDir);

                            previousOriginalLineIndex = previousOriginalLineIndexPlusDir;
                            absDiff = Math.Abs(currentOriginalLineIndex - previousOriginalLineIndex);
                        }
                        else if (newAbsDiff == 1)
                        {
                            //This part is a segment
                            trindices.Add(numOriginalPoints + previousExtrudedLineIndex);
                            trindices.Add(numOriginalPoints + currentExtrudedLineIndex);
                            trindices.Add(previousOriginalLineIndex);

                            trindices.Add(previousOriginalLineIndexPlusDir);
                            trindices.Add(previousOriginalLineIndex);
                            trindices.Add(numOriginalPoints + currentExtrudedLineIndex);

                            previousOriginalLineIndex = previousOriginalLineIndexPlusDir;
                            absDiff = Math.Abs(currentOriginalLineIndex - previousOriginalLineIndex);
                        }
                    }
                }
                else if (absDiff == 0)
                {
                    //It's a fan.
                    trindices.Add(currentOriginalLineIndex);
                    trindices.Add(numOriginalPoints + previousExtrudedLineIndex);
                    trindices.Add(numOriginalPoints + currentExtrudedLineIndex);

                }
                else if (absDiff == 1)
                {
                    //It's a segment

                    trindices.Add(numOriginalPoints + previousExtrudedLineIndex);
                    trindices.Add(numOriginalPoints + currentExtrudedLineIndex);
                    trindices.Add(previousOriginalLineIndex);

                    trindices.Add(currentOriginalLineIndex);
                    trindices.Add(previousOriginalLineIndex);
                    trindices.Add(numOriginalPoints + currentExtrudedLineIndex);
                }

                currentExtrudedLineIndex++;
                previousExtrudedLineIndex++;
            }

            return trindices.ToArray();
        }

        /// <summary>
        /// Of a certain set of indices of an array of extruded vectors, return the index corresponding to the extruded point that is closest to a given line point.
        /// </summary>
        /// <param name="linePoint">The line point.</param>
        /// <param name="extrudedVectors">Array of all extruded vectors.</param>
        /// <param name="extrudedVectorIndices">Indices of the array of extruded vectors under consideration.</param>
        private static int GetClosestExtrudedLineIndexOfSelectPoints(LinePointUV linePoint, Vector2WithUV[] extrudedVectors, params int[] extrudedVectorIndices)
        {
            var point = linePoint.Point;
            int indexMatch = -1;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < extrudedVectorIndices.Length; i++)
            {
                int extrudedIndex = extrudedVectorIndices[i];
                var currDist = (extrudedVectors[extrudedIndex].Vector - point).magnitude;
                if (currDist < distance)
                {
                    distance = currDist;
                    indexMatch = extrudedIndex;
                }
            }
            return indexMatch;
        }

        /// <summary>
        /// Get the index of the closest point on a line to a given test point.
        /// </summary>
        /// <param name="point">The test point</param>
        /// <param name="linePoints">The points defining a line</param>
        private static int GetClosestLineIndex(Vector2 point, IList<LinePointUV> linePoints)
        {
            int index = -1;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < linePoints.Count; i++)
            {
                var currDist = (linePoints[i].Point - point).magnitude;
                if (currDist < distance)
                {
                    distance = currDist;
                    index = i;
                }
            }
            return index;
        }
    }
}