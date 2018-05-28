using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Segmentation;
using BabyDinoHerd.Extrusion.Line.Triangulation;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Configuration;
using UnityEngine.Events;
using BabyDinoHerd.Extrusion.Line.Interfaces;

namespace BabyDinoHerd.Extrusion.Spline
{
    /// <summary>
    /// 2D spline that is segmentable, and is extruded to create a 2D mesh.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
    public class BezierSpline2DSegmentable : BezierSpline, ILineSegmentation
    {
        /// <summary> UnityEvent called when single-contour extrusion results are updated. </summary>
        public UnityEvent OnSingleContourExtrusionUpdated = new UnityEvent();

        /// <summary> UnityEvent called when single-contour extrusion results are updated. </summary>
        public UnityEvent OnMultipleContourExtrusionUpdated = new UnityEvent();

        /// <summary> Line extrusion configuration. </summary>
        public LineExtrusionConfiguration LineExtrusionConfiguration { get { return _lineExtrusionConfiguration; } }
        private LineExtrusionConfiguration _lineExtrusionConfiguration = LineExtrusionConfiguration.Empty;

        /// <summary> Resulting contours from 2D line extrusion. </summary>
        public LineExtrusionResults ExtrusionResults { get { return _extrusionResults; } }
        private LineExtrusionResults _extrusionResults = LineExtrusionResults.Empty;

        /// <summary> The <see cref="Mesh"/> resulting from extrusion. </summary>
        public Mesh ExtrudedMesh{ get { return _extrudedMesh; } }
        private Mesh _extrudedMesh;

        /// <summary> The extrusion distance. </summary>
        public float Extrusion { get { return _extrusion; } set { _extrusion = value; UpdateExtrusion(); } }
        [SerializeField]
        private float _extrusion = 0.1f;

        /// <summary> The desired distance between points when segmentizing the spline.</summary>
        public float Discretization { get { return _discretization; } }
        [SerializeField]
        private float _discretization = 0.01f;

		/// <summary> The material used when extrusion results in a single contour. </summary>
		public Material SingleContourMaterial { get { return _singleContourMaterial; } }
		[SerializeField]
		private Material _singleContourMaterial;

		/// <summary> The material used when extrusion results in multiple contours. </summary>
		public Material MultipleContourMaterial { get { return _multipleContourMaterial; } }
		[SerializeField]
		private Material _multipleContourMaterial;

		/// <summary> The <see cref="MeshRenderer"/> Component for this GameObject.</summary>
		private MeshRenderer _meshRenderer;

        /// <summary> The <see cref="MeshFilter"/> Component for this GameObject.</summary>
        private MeshFilter _meshFilter;

        /// <summary> MonoBehaviour Awake method. </summary>
        private void Awake()
        {
            
        }

        /// <summary> MonoBehaviour Start method. </summary>
        public void Start()
        {
            UpdateExtrusion();
        }

        void OnEnable()
        {
            
        }

        /// <summary>
        /// Updates the extrusion with the given <see cref="_discretization"/> and <see cref="Extrusion"/> values.
        /// Updates the <see cref="MeshFilter"/> component with the resulting Mesh.
        /// </summary>
        public void UpdateExtrusion()
        {
            if(_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }

			if(_meshRenderer == null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();
			}

            bool doExtrusion = Math.Abs(_extrusion) > ExtrusionNumericalPrecision.MinimumExtrusionDistanceExclusive;
            if (doExtrusion)
            {
                try
                {
                    _lineExtrusionConfiguration = GetLineExtrusionConfiguration(_extrusion);
                    _extrusionResults = SegmentedLineExtrusionFromIntersection.ExtrudeSegmentedLineBothDirections(this, _lineExtrusionConfiguration);
                    var linePointsWithUVs = LineSegmentation.GetLinePointsUV(this);
                    var segmentwiseLinePointList = new SegmentwiseLinePointListUV(linePointsWithUVs);
                    _extrudedMesh = ExtrusionTriangulator2D.TriangulatePolygon(_extrusionResults, segmentwiseLinePointList, _lineExtrusionConfiguration);
                    var verts = _extrudedMesh.vertices;
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i] = transform.InverseTransformPoint(verts[i]);
                    }
                    _extrudedMesh.vertices = verts;
                    _meshFilter.mesh = _extrudedMesh;
                    bool singleContour = _extrusionResults.Contours.Count <= 1;
                    _meshRenderer.material = singleContour ? _singleContourMaterial : _multipleContourMaterial;
                    (singleContour ? OnSingleContourExtrusionUpdated : OnMultipleContourExtrusionUpdated).Invoke();
                }
                catch(Exception e)
                {
                    Debug.LogWarning(string.Format("Error in extrusion of amount {0}: {1}\n{2}.", _extrusion, e.Message, e.StackTrace));
                    _extrudedMesh = new Mesh();
                    _meshFilter.mesh = _extrudedMesh;
                }
                finally
                {
                }
            }
            else
            {
                _extrudedMesh = new Mesh();
                _meshFilter.mesh = _extrudedMesh;
            }
        }

        /// <summary>
        /// Returns the <see cref="LineExtrusionConfiguration"/> to be used for extrusion.
        /// </summary>
        /// <param name="extrusion">Extrusion amount.</param>
        protected virtual LineExtrusionConfiguration GetLineExtrusionConfiguration(float extrusion)
        {
            return new LineExtrusionConfiguration(extrusion);
        }
        
        /// <summary>
        /// Returns a list of <see cref="LinePointStandard"/> corresponding to a piecewise segmentation of the spline.
        /// Implementation for <see cref="ILineSegmentation"/>.
        /// </summary>
        public List<LinePointStandard> GetLineSegmentsPoints()
        {
            var roughLength = 0f;
            for(int i = 1; i < ControlPointCount; i++)
            {
                roughLength += Vector3.Distance(GetControlPoint(i), GetControlPoint(i - 1));
            }

            int numPoints = 1 + Mathf.RoundToInt(roughLength / _discretization);//include ending point

            List<LinePointStandard> linePoints = new List<LinePointStandard>(numPoints);
            for(int i = 0; i < numPoints; i++)
            {
                float fraction = i / ((float) (numPoints - 1));
                var point = GetPoint(fraction);
                linePoints.Add(new LinePointStandard(fraction, point));
            }

            return linePoints;
        }
    }
}