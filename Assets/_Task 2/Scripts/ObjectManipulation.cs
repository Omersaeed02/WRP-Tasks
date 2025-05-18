using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectManipulation : MonoBehaviour
{
    public ShapeHandler selectedShape;
    private Camera _mainCamera;
    private Animator _animator;

    [Header("UI")]
    public TMP_Text shapeName;
    [Space(10)]
    public SliderHandler xPositionSlider;
    public SliderHandler yPositionSlider;
    public SliderHandler zPositionSlider;
    [Space(10)]
    public SliderHandler xRotationSlider;
    public SliderHandler yRotationSlider;
    public SliderHandler zRotationSlider;
    [Space(10)]
    public SliderHandler radiusSlider;
    public SliderHandler xScaleSlider;
    public SliderHandler yScaleSlider;
    public SliderHandler zScaleSlider;

    public List<SliderHandler> sliderHandlers;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
        _animator = GetComponent<Animator>();

        SetObjectManipulation();
    }

    private void SetObjectManipulation()
    {
        sliderHandlers = GetComponentsInChildren<SliderHandler>().ToList();
        sliderHandlers.ForEach(t => t.SetObjectManipulation(this));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) // 0 = left click or tap
        {
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.transform.TryGetComponent<ShapeHandler>(out var shape))
                {
                    SelectObject(shape);
                }
                else
                {
                    DeselectObject();
                }
            }
        }
    }

    public void SelectObject(ShapeHandler shape)
    {
        AudioManager.Instance?.PlaySelectSound();
        selectedShape = shape;
        shapeName.text = selectedShape.name;
        _animator.Play("Menu In");
        
        switch (selectedShape.shapeType)
        {
            case ShapeType.Cube:
                radiusSlider.gameObject.SetActive(false);
                xScaleSlider.gameObject.SetActive(true);
                yScaleSlider.gameObject.SetActive(true);
                zScaleSlider.gameObject.SetActive(true);
                break;
            
            case ShapeType.Sphere:
                radiusSlider.gameObject.SetActive(true);
                xScaleSlider.gameObject.SetActive(false);
                yScaleSlider.gameObject.SetActive(false);
                zScaleSlider.gameObject.SetActive(false);
                break;
        }
        
        ResetSliders();
        
        Debug.Log("Selected: " + selectedShape.name);
    }
    
    public void DeselectObject()
    {
        if (selectedShape != null)
            _animator.Play("Menu Out");
        selectedShape = null;
    }

    public void ResetSliders()
    {
        var shapeTransform = selectedShape.transform;
        var position = shapeTransform.position;
        var rotation = shapeTransform.rotation;
        var localScale = shapeTransform.localScale;
        
        xPositionSlider.slider.value = position.x;
        yPositionSlider.slider.value = position.y;
        zPositionSlider.slider.value = position.z;
        xRotationSlider.slider.value = rotation.eulerAngles.x;
        yRotationSlider.slider.value = rotation.eulerAngles.y;
        zRotationSlider.slider.value = rotation.eulerAngles.z;
        
        if (selectedShape.shapeType == ShapeType.Cube)
        {
            xScaleSlider.slider.value = localScale.x;
            yScaleSlider.slider.value = localScale.y;
            zScaleSlider.slider.value = localScale.z;
        }
        else
        {
            radiusSlider.slider.value = localScale.x;
        }
    }
}
