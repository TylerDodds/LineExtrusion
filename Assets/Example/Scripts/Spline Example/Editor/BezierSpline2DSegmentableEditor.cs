using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using BabyDinoHerd.Extrusion.Line.Segmentation;
using BabyDinoHerd.Extrusion.Line.Extrusion;

namespace BabyDinoHerd.Extrusion.Spline
{
    /// <summary> Editor for <see cref="BezierSpline2DSegmentable"/>. </summary>
    [CustomEditor(typeof(BezierSpline2DSegmentable))]
    public class BezierSpline2DSegmentableEditor : BezierSplineInspector
    {
        /// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable.OnSingleContourExtrusionUpdated"/> backing field. </summary>
        SerializedProperty _onSingleContourExtrusionUpdatedEventProperty;
        /// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable.OnMultipleContourExtrusionUpdated"/> backing field. </summary>
        SerializedProperty _onMultipleContourExtrusionUpdatedEventProperty;
        /// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable.Extrusion"/> backing field. </summary>
        SerializedProperty _extrusionProperty;
        /// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable.Discretization"/> backing field. </summary>
        SerializedProperty _discretizationProperty;
		/// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable.SingleContourMaterial"/> backing field. </summary>
		SerializedProperty _singleContourMaterialProperty;
		/// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable.MultipleContourMaterial"/> backing field. </summary>
		SerializedProperty _multipleContourMaterialProperty;

        /// <summary> MonoBehaviour OnEnable method. </summary>
        public virtual void OnEnable()
        {
            _onSingleContourExtrusionUpdatedEventProperty = serializedObject.FindProperty("OnSingleContourExtrusionUpdated");
            _onMultipleContourExtrusionUpdatedEventProperty = serializedObject.FindProperty("OnMultipleContourExtrusionUpdated");
            _discretizationProperty = serializedObject.FindProperty("_discretization");
            _extrusionProperty = serializedObject.FindProperty("_extrusion");
			_singleContourMaterialProperty = serializedObject.FindProperty("_singleContourMaterial");
			_multipleContourMaterialProperty = serializedObject.FindProperty("_multipleContourMaterial");
        }

        /// <summary> Draw the Inspector GUI. </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var configuration = (target as BezierSpline2DSegmentable).LineExtrusionConfiguration; 

            serializedObject.Update();

            EditorGUILayout.Slider(_discretizationProperty, 0.005f, 0.3f, new GUIContent("Discretization"));
            EditorGUILayout.Slider(_extrusionProperty, 0, 1, new GUIContent("Extrusion"));

			EditorGUILayout.PropertyField(_singleContourMaterialProperty);
			EditorGUILayout.PropertyField(_multipleContourMaterialProperty);

            EditorGUILayout.PropertyField(_onSingleContourExtrusionUpdatedEventProperty);
            EditorGUILayout.PropertyField(_onMultipleContourExtrusionUpdatedEventProperty);

            bool changed = GUI.changed;

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                UpdateSplineDependencies();
            }
        }

        /// <summary> Draw scene GUI for the spline and extrusion. </summary>
        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();

            var bezierSpline2D = target as BezierSpline2DSegmentable;
            var segmentedLine = LineSegmentation.GetLinePointsUV(bezierSpline2D);

            bool draw = true;

            if (draw)
            {
                Handles.color = Color.grey;
                for (int i = 0; i < segmentedLine.Count - 1; i++)
                {
                    var p0 = segmentedLine[i].Point;
                    var p1 = segmentedLine[i + 1].Point;
                    Handles.DrawLine(p0, p1);
                    Handles.RectangleHandleCap(-1, p0, Quaternion.identity, 0.001f, EventType.Repaint);
                }


                var extrudedSegmentContours = bezierSpline2D.ExtrusionResults.Contours;

                var colors = new List<Color>() {Color.red, Color.yellow, Color.blue, Color.magenta, Color.cyan, Color.green, Color.grey,
                                            Color.red * 0.5f, Color.yellow * 0.5f, Color.blue * 0.5f, Color.magenta * 0.5f, Color.cyan * 0.5f, Color.green * 0.5f, Color.grey * 0.5f, };

                for (int i = 0; i < extrudedSegmentContours.Count; i++)
                {
                    Handles.color = colors[i % colors.Count];
                    var points = extrudedSegmentContours[i];
                    for (int j = 0; j < points.Length - 1; j++)
                    {
                        var p0 = points[j].Vector;
                        var p1 = points[j + 1].Vector;
                        Handles.DrawLine(p0, p1);
                        Handles.RectangleHandleCap(-1, p0, Quaternion.identity, 0.001f, EventType.Repaint);
                    }
                    Handles.RectangleHandleCap(-1, extrudedSegmentContours[i][extrudedSegmentContours[i].Length - 1].Vector, Quaternion.identity, 0.001f, EventType.Repaint);
                }

                var removedColors = new List<Color>() { Color.black, Color.blue * 0.25f, Color.green * 0.25f, Color.red * 0.25f, };

                var removedContours = bezierSpline2D.ExtrusionResults.RemovedContours;
                for (int i = 0; i < removedContours.Count; i++)
                {
                    Handles.color = removedColors[i % removedColors.Count];
                    var points = removedContours[i];
                    for (int j = 0; j < points.Length - 1; j++)
                    {
                        var p0 = points[j].Vector;
                        var p1 = points[j + 1].Vector;
                        Handles.DrawLine(p0, p1);
                        Handles.RectangleHandleCap(-1, p0, Quaternion.identity, 0.001f, EventType.Repaint);
                    }
                    Handles.RectangleHandleCap(-1, removedContours[i][removedContours[i].Length - 1].Vector, Quaternion.identity, 0.001f, EventType.Repaint);
                }

                bool drawExtrudedOriginal = false;
                if (drawExtrudedOriginal)
                {
                    var extrudedOrig = SegmentedLineExtrusionFromSegments.GetExtrudedSubspaceAllSides(segmentedLine, bezierSpline2D.Extrusion);
                    for (int i = 0; i < extrudedOrig.Count - 1; i++)
                    {
                        float fraction = i / (float)extrudedOrig.Count;
                        Handles.color = (Color.green * fraction + Color.black * (1f - fraction)) * 0.2f;
                        Handles.DrawLine(extrudedOrig[i].Point, extrudedOrig[i + 1].Point);
                        Handles.RectangleHandleCap(-1, extrudedOrig[i].Point, Quaternion.identity, 0.00125f, EventType.Repaint);
                    }
                    Handles.RectangleHandleCap(-1, extrudedOrig[extrudedOrig.Count - 1].Point, Quaternion.identity, 0.00125f, EventType.Repaint);
                }
            }
        }

        /// <summary>
        /// Update the spline dependencies, as well as the extrusion.
        /// </summary>
        protected override void UpdateSplineDependencies()
        {
            base.UpdateSplineDependencies();
            var bezSpline2D = target as BezierSpline2DSegmentable;
            bezSpline2D.UpdateExtrusion();
        }

        /// <summary>
        /// Sets the spline as <see cref="BezierSplineInspector"/> based on the <paramref name="editorTarget"/>.
        /// </summary>
        /// <param name="editorTarget">The editor target.</param>
        protected override void SetSpline(Object editorTarget)
        {
            spline = editorTarget as BezierSpline2DSegmentable;
        }

        /// <summary>
        /// Constrain a point on the spline to be two-dimensional.
        /// </summary>
        /// <param name="point"> The point to be constrained. </param>
        protected override Vector3 ConstrainPoint(Vector3 point)
        {
            return ((Vector2)point);
        }
    }
}
