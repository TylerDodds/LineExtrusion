using UnityEngine;

using UnityEditor;
using BabyDinoHerd.Extrusion.Spline.Experimental;

namespace BabyDinoHerd.Extrusion.Spline
{
    /// <summary> Editor for <see cref="BezierSpline2DSegmentable_Experimental"/>. </summary>
    [CustomEditor(typeof(BezierSpline2DSegmentable_Experimental))]
    public class BezierSpline2DSegmentableExperimentalEditor : BezierSpline2DSegmentableEditor
    {
        /// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable_Experimental.SingleContourTriangulationType"/> backing field. </summary>
        SerializedProperty _singleContourTriangulationTypeProperty;
        /// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable_Experimental.SingleContourUParameterAlterationType"/> backing field. </summary>
        SerializedProperty _singleContourUParameterCurvatureAlterationProperty;
        /// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable_Experimental.MultipleContoursUParameterDeterminationType"/> backing field. </summary>
        SerializedProperty _multipleContoursUParameterCurvatureAlterationProperty;
        /// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable_Experimental.MultipleContourTriangulationType"/> backing field. </summary>
        SerializedProperty _multipleContourTriangulationTypeProperty;
        /// <summary> <see cref="SerializedProperty"/> for <see cref="BezierSpline2DSegmentable_Experimental.IntersectionUVCalculation"/> backing field. </summary>
        SerializedProperty _intersectionUVCalculationProperty;

        /// <summary> MonoBehaviour OnEnable method. </summary>
        public override void OnEnable()
        {
            base.OnEnable();
            _singleContourTriangulationTypeProperty = serializedObject.FindProperty("_singleContourTriangulationType");
            _singleContourUParameterCurvatureAlterationProperty = serializedObject.FindProperty("_singleContourUParameterAlterationType");
            _multipleContourTriangulationTypeProperty = serializedObject.FindProperty("_multipleContourTriangulationType");
            _multipleContoursUParameterCurvatureAlterationProperty = serializedObject.FindProperty("_multipleContoursUParameterAlterationType");
            _intersectionUVCalculationProperty = serializedObject.FindProperty("_intersectionUVCalculation");
        }

        /// <summary> Draw the Inspector GUI. </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var configuration = (target as BezierSpline2DSegmentable_Experimental).LineExtrusionConfiguration; 

            serializedObject.Update();

            EditorGUILayout.PropertyField(_intersectionUVCalculationProperty, new GUIContent("Intersection Point UV Calculation Type"));
            EditorGUILayout.PropertyField(_singleContourTriangulationTypeProperty, new GUIContent("Single Contour UV Determination"));
            if (configuration.GetSingleContourTriangulation().HasUvAlteration)
            {
                EditorGUILayout.PropertyField(_singleContourUParameterCurvatureAlterationProperty, new GUIContent("Single Contour UV Alteration"));
            }

            EditorGUILayout.PropertyField(_multipleContourTriangulationTypeProperty, new GUIContent("Multiple Contours UV Determination"));
            if (configuration.GetMultipleContourTriangulation().HasUvAlteration)
            {
                EditorGUILayout.PropertyField(_multipleContoursUParameterCurvatureAlterationProperty, new GUIContent("Multiple Contours UV Alteration"));
            }

            bool changed = GUI.changed;

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                UpdateSplineDependencies();
            }
        }
    }
}
