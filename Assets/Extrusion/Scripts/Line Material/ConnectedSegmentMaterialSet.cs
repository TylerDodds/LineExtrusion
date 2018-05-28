using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using BabyDinoHerd.Extrusion.Line.Geometry;
using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.LineMaterial
{
    /// <summary>
    /// Component that sets the GameObject's MeshRenderer's sharedMaterial with results of line extrusion, namely sets of extruded points at equals u-parameters to be taken as connected segments in the shader.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class ConnectedSegmentMaterialSet : LineMaterialSetBase
    {
        /// <summary>
        /// Updates the MeshRenderer's sharedMaterial based on extrusion results.
        /// </summary>
        /// <param name="lineExtrusionConfiguration">The line extrusion configuration</param>
        /// <param name="lineExtrusionResults">The line extrusion results</param>
        protected override void DoUpdateMaterialBasedOnExtrusionResults(LineExtrusionConfiguration lineExtrusionConfiguration, LineExtrusionResults lineExtrusionResults)
        {
            var connectedSegmentsResults = lineExtrusionResults.ConnectedSegmentsExtrusionResults;
            var singleContourConnectedSegments = connectedSegmentsResults.SegmentwiseCoverageOfSingleContour;

            if(singleContourConnectedSegments != null)
            {
                var firstVector4 = Vector2WithUVToLocalVector4(singleContourConnectedSegments.FirstPoint);
                var lastVector4 = Vector2WithUVToLocalVector4(singleContourConnectedSegments.LastPoint);
                var increasingChunkPackedArrays = ConvertMonotonicChunkPointsToLocalVector4Array(singleContourConnectedSegments.IncreasingPortionSegmentPoints);
                var decreasingChunkPackedArrays = ConvertMonotonicChunkPointsToLocalVector4Array(singleContourConnectedSegments.DecreasingPortionSegmentPoints);

                var renderer = MeshRenderer;
                var mpb = MaterialPropertyBlock;

                mpb.SetFloat("_ExtrusionLength", Mathf.Abs(lineExtrusionConfiguration.ExtrusionAmount));

                //TODO Set connected segments floats using textures, not uniforms, to help alleviate uniform limits. The best choice may be platform dependent.
                mpb.SetVector("_connectedSegmentFirstPoint", firstVector4);
                mpb.SetVector("_connectedSegmentLastPoint", lastVector4);
                mpb.SetVectorArray("_increasingChunkConnectedSegmentPoints", increasingChunkPackedArrays.PackedPoints);
                mpb.SetVectorArray("_increasingChunkConnectedSegmentUvs", increasingChunkPackedArrays.PackedUvs);
                mpb.SetVectorArray("_decreasingChunkConnectedSegmentPoints", decreasingChunkPackedArrays.PackedPoints);
                mpb.SetVectorArray("_decreasingChunkConnectedSegmentUvs", decreasingChunkPackedArrays.PackedUvs);
                mpb.SetFloat("_numberOfConnectedSegments", Mathf.Min(increasingChunkPackedArrays.NumberOfPoints, decreasingChunkPackedArrays.NumberOfPoints));

                renderer.SetPropertyBlock(mpb);
            }
        }

        /// <summary>
        /// Converts a List of <see cref="Vector2WithUV"/> of monotonic chunk segment points into a <see cref="PackedPointArrayAndUVArray"/>, where the points are in the local coordinate space.
        /// </summary>
        /// <param name="monotonicChunkSegmentPoints">Points of a monotonic chunk, which form one end of a connecting segment with a matching point on the counterpart monotonic chunk.</param>
        private PackedPointArrayAndUVArray ConvertMonotonicChunkPointsToLocalVector4Array(List<Vector2WithUV> monotonicChunkSegmentPoints)
        {
            int numPoints = monotonicChunkSegmentPoints.Count;
            int numPointsToAdd = Mathf.Min(numPoints, _maxNumMonotonicChunkPackedPoints * 2);
            Vector4[] points = new Vector4[_maxNumMonotonicChunkPackedPoints];
            Vector4[] uvs = new Vector4[_maxNumMonotonicChunkPackedPoints];

            int numberOfFullyPackedVector4 = numPointsToAdd / 2;
            for (int i = 0; i < numberOfFullyPackedVector4; i++)
            {
                var firstPointUV = monotonicChunkSegmentPoints[i * 2 + 0];
                var secondPointUV = monotonicChunkSegmentPoints[i * 2 + 1];
                var firstPointLocal = transform.InverseTransformPoint(firstPointUV.Vector);
                var secondPointLocal = transform.InverseTransformPoint(secondPointUV.Vector);
                Vector4 pointsPacked = new Vector4(firstPointLocal.x, firstPointLocal.y, secondPointLocal.x, secondPointLocal.y);
                Vector4 uvsPacked = new Vector4(firstPointUV.UV.x, firstPointUV.UV.y, secondPointUV.UV.x, secondPointUV.UV.y);
                points[i] = pointsPacked;
                uvs[i] = uvsPacked;
            }

            if(numPointsToAdd % 2 != 0)
            {
                //Add last packed point
                var lastPoint = monotonicChunkSegmentPoints[numPointsToAdd - 1];
                var lastPointLocal = transform.InverseTransformPoint(lastPoint.Vector);
                points[numberOfFullyPackedVector4] = new Vector4(lastPointLocal.x, lastPointLocal.y, 0, 0);
                uvs[numberOfFullyPackedVector4] = new Vector4(lastPoint.UV.x, lastPoint.UV.y, 0, 0);
            }

            return new PackedPointArrayAndUVArray(points, uvs, numPointsToAdd);
        }

        /// <summary> Container class of point list and uv list. </summary>
        private class PackedPointArrayAndUVArray
        {
            /// <summary> Two-dimensional points packed as pairs into an array of <see cref="Vector4"/>. </summary>
            public readonly Vector4[] PackedPoints;
            /// <summary> Two-dimensional UVs packed as pairs into an array of <see cref="Vector4"/>. </summary>
            public readonly Vector4[] PackedUvs;
            /// <summary> Total number of two-dimensional points. </summary>
            public readonly int NumberOfPoints;

            public PackedPointArrayAndUVArray(Vector4[] packedPoints, Vector4[] packedUvs, int numberOfPoints)
            {
                PackedPoints = packedPoints;
                PackedUvs = packedUvs;
                NumberOfPoints = numberOfPoints;
            }
        }

        /// <summary> Maximum length of Vector4-packed array holding points of a monotonic chunk of an extruded contour. </summary>
		//NB Usually Unity's maximum length of arrays passed to shaders is 1023. 
		//However, when including four of them, by experimentation the magic maximum number is 1022 ...
        //This will be platform-dependent, both in terms of shader compilation and linking. Further experimentation may be needed
		// Important! Keep this in sync with ConnectedSegmentsShader.
        private const int _maxNumMonotonicChunkPackedPoints = 1022;
    }

}