using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.Extrusion;

namespace BabyDinoHerd.Extrusion.LineMaterial
{
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class LineMaterialSetBase : MonoBehaviour
    {
        /// <summary>
        /// Updates the MeshRenderer's sharedMaterial based on extrusion results.
        /// </summary>
        public void UpdateMaterialBasedOnExtrusionResults()
        {
            var configAndResults = GetLineExtrusionConfigurationAndResults();
            var lineExtrusionConfiguration = configAndResults.LineExtrusionConfiguration;
            var lineExtrusionResults = configAndResults.LineExtrusionResults;

            DoUpdateMaterialBasedOnExtrusionResults(lineExtrusionConfiguration, lineExtrusionResults);
        }

        /// <summary>
        /// Updates the MeshRenderer's sharedMaterial based on extrusion results.
        /// </summary>
        /// <param name="lineExtrusionConfiguration">The line extrusion configuration</param>
        /// <param name="lineExtrusionResults">The line extrusion results</param>
        protected abstract void DoUpdateMaterialBasedOnExtrusionResults(LineExtrusionConfiguration lineExtrusionConfiguration, LineExtrusionResults lineExtrusionResults);

        /// <summary>
        /// Gets the extrusion configuration and results.
        /// </summary>
        protected abstract ExtrusionConfigurationAndResults GetLineExtrusionConfigurationAndResults();

        /// <summary> The GameObject's <see cref="UnityEngine.MeshRenderer"/>. </summary>
        protected MeshRenderer MeshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                {
                    _meshRenderer = GetComponent<MeshRenderer>();
                }
                return _meshRenderer;
            }
        }
        private MeshRenderer _meshRenderer;

        /// <summary> <see cref="MaterialPropertyBlock"/> for setting uniforms. </summary>
        protected MaterialPropertyBlock MaterialPropertyBlock
        {
            get
            {
                if(_materialPropertyBlock == null)
                {
                    _materialPropertyBlock = new MaterialPropertyBlock();
                }
                return _materialPropertyBlock;
            }
        }
        private MaterialPropertyBlock _materialPropertyBlock;

        /// <summary>
        /// Converts a <see cref="SegmentwiseLinePointListUV"/> of original line points converts it into an array of <see cref="Vector4"/>, with the local point and uv packed into four components.
        /// </summary>
        /// <param name="originalLinePointList">The list oriignal line points and UVs.</param>
        protected Vector4[] ConvertOriginalLineToLocalVector4Array(SegmentwiseLinePointListUV originalLinePointList)
        {
            var points = originalLinePointList.Points;
            int numPoints = points.Count;
            int numPointsToAdd = Mathf.Min(numPoints, _maxOriginalLineArrayLength);
            Vector4[] array = new Vector4[_maxOriginalLineArrayLength];

            for (int i = 0; i < numPointsToAdd; i++)
            {
                array[i] = LinePointUVToLocalVector4(points[i]);
            }
            return array;
        }

        /// <summary>
        /// Converts a line point and uv in world coordinates into a Vector4 with the local point and uv packed into four components.
        /// </summary>
        /// <param name="linePointUV">The list of line points.</param>
        protected Vector4 LinePointUVToLocalVector4(LinePointUV linePointUV)
        {
            var localPoint = transform.InverseTransformPoint(linePointUV.Point);
            return new Vector4(localPoint.x, localPoint.y, linePointUV.UV.x, linePointUV.UV.y);
        }

        /// <summary>
        /// Converts a Vector2 and uv in world coordinates into a Vector4 with the local point and uv packed into four components.
        /// </summary>
        /// <param name="vector2WithUV">The list of line points.</param>
        protected Vector4 Vector2WithUVToLocalVector4(Vector2WithUV vector2WithUV)
        {
            var localPoint = transform.InverseTransformPoint(vector2WithUV.Vector);
            return new Vector4(localPoint.x, localPoint.y, vector2WithUV.UV.x, vector2WithUV.UV.y);
        }

        /// <summary>
        /// Container for <see cref="LineExtrusionConfiguration"/> and <see cref="LineExtrusionResults"/>.
        /// </summary>
        protected struct ExtrusionConfigurationAndResults
        {
            /// <summary> Line extrusion configuration. </summary>
            public LineExtrusionConfiguration LineExtrusionConfiguration;

            /// <summary> Resulting contours from 2D line extrusion. </summary>
            public LineExtrusionResults LineExtrusionResults;

            public ExtrusionConfigurationAndResults(LineExtrusionConfiguration lineExtrusionConfiguration, LineExtrusionResults lineExtrusionResults)
            {
                LineExtrusionConfiguration = lineExtrusionConfiguration;
                LineExtrusionResults = lineExtrusionResults;
            }
        }

        /// <summary> Maximum length of the array holding the original line. </summary>
        private const int _maxOriginalLineArrayLength = 1023;
    }
}