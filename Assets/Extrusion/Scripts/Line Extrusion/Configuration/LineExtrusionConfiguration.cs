using BabyDinoHerd.Extrusion.Line.Geometry.ChunkConnection;
using BabyDinoHerd.Extrusion.Line.Interfaces;
using BabyDinoHerd.Extrusion.Line.TextureMapping.Alteration;
using BabyDinoHerd.Extrusion.Line.Triangulation.MultipleContour;
using BabyDinoHerd.Extrusion.Line.Triangulation.SingleContour;
using System;

namespace BabyDinoHerd.Extrusion.Line.Configuration
{
    /// <summary>
    /// Configuration class for line extrusion parameters, particularly the <see cref="ExtrusionAmount"/>.
    /// </summary>
    [Serializable]
    public class LineExtrusionConfiguration
    {
        /// <summary> Empty configuration. </summary>
        public static readonly LineExtrusionConfiguration Empty = new LineExtrusionConfiguration(0f);

        /// <summary>
        /// The distance for points to be extruded.
        /// </summary>
        public float ExtrusionAmount = 0f;

        #region Implementations

        /// <summary> Returns an implementation of <see cref="IExtrudedChunkContourConnector"/> based on <see cref="IntersectionUVCalculation"/> parameter. </summary>
        public virtual IExtrudedChunkContourConnector GetExtrudedChunkContourConnector()
        {
            return new ExtrudedChunkContourConnector_AveragedEqually();
        }

        /// <summary> Returns an implementation of <see cref="IExtrudedContourUVAlteration"/> based on <see cref="SingleContourUParameterAlterationType"/> value. </summary>
        public virtual IExtrudedContourUVAlteration GetSingleContourUVAlteration()
        {
            return new ConvolutionUVAlteration();
        }

        /// <summary> Returns an implementation of <see cref="IExtrudedContourUVAlteration"/> based on <see cref="MultipleContoursUParameterAlterationType"/> value. </summary>
        public virtual IExtrudedContourUVAlteration GetMultipleContourUVAlteration()
        {
            return new ConvolutionUVAlteration();
        }

        /// <summary> Returns an implementation of <see cref="ISingleContourTriangulation"/> based on <see cref="SingleContourLineUVDeterminationType"/> parameter. </summary>
        public virtual ISingleContourTriangulation GetSingleContourTriangulation()
        {
            return new SingleContourTriangulationFromConnectedSegments();
        }

        /// <summary> Returns an implementation of <see cref="IMultipleContourTriangulation"/> based on <see cref="TriangulatedPointsUVDeterminationType"/> parameter. </summary>
        public virtual IMultipleContourTriangulation GetMultipleContourTriangulation()
        {
            return new MultipleContourTriangulationNoUVs();
        }

        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="LineExtrusionConfiguration"/>.
        /// </summary>
        /// <param name="extrusionAmount">The distance for points to be extruded.</param>
        /// 
        public LineExtrusionConfiguration(float extrusionAmount)
        {
            ExtrusionAmount = extrusionAmount;
        }
    }
}