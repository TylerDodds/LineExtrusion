using BabyDinoHerd.Extrusion.Spline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleSceneSplineSelection : MonoBehaviour
{
    public UnityEngine.UI.Slider SplineSelectionSlider;
    public UnityEngine.UI.Slider ExtrusionAmountSlider;
    public GameObject SplinesContainer;

    public void HandleExtrusionAmountChanged(float sliderValue)
    {
        _currentExtrusionAmount = sliderValue / ExtrusionAmountSlider.maxValue;
        if(_currentSpline != null)
        {
            _currentSpline.Extrusion = _currentExtrusionAmount;
        }
    }

    public void HandleSplineIndexChanged(float sliderValue)
    {
        _currentSplineIndex = Mathf.RoundToInt(sliderValue - 1);
        Transform matchingChild = SplinesContainer.transform.GetChild(_currentSplineIndex);
        foreach(Transform t in SplinesContainer.transform)
        {
            t.gameObject.SetActive(false);
        }
        if (matchingChild != null)
        {
            var spline = matchingChild.GetComponent<BezierSpline2DSegmentable>();
            if (spline != null)
            {
                _currentSpline = spline;
                spline.gameObject.SetActive(true);
                _currentSpline.Extrusion = _currentExtrusionAmount;
            }
        }
    }

    private void Awake()
    {
        var splines = SplinesContainer.transform.GetComponentsInChildren<BezierSpline2DSegmentable>(true);
        _numSplines = splines.Length;
    }

    private void Start()
    {
        SplineSelectionSlider.minValue = 1;
        SplineSelectionSlider.maxValue = _numSplines;
        HandleSplineIndexChanged(SplineSelectionSlider.value);
        HandleExtrusionAmountChanged(ExtrusionAmountSlider.value);
    }

    private int _numSplines;
    private int _currentSplineIndex;
    private float _currentExtrusionAmount = 0.1f;
    private BezierSpline2DSegmentable _currentSpline;
}
