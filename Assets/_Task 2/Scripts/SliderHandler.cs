using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderHandler : MonoBehaviour
{
    private ObjectManipulation _objectManipulation;
    public SliderType type;
    public TMP_Text valueText;
    public Slider slider;

    public void Awake()
    {
        slider.onValueChanged.AddListener(_ =>
        {
            var val = ((int)(slider.value * 100)) / 100f;
                valueText.text = val.ToString();
                ChangeObjectTransform();
            });
    }

    private void ChangeObjectTransform()
    {
        if (_objectManipulation.selectedShape == null) return;
        
        var position = _objectManipulation.selectedShape.transform.position;
        var rotation = _objectManipulation.selectedShape.transform.rotation.eulerAngles;
        var scale = _objectManipulation.selectedShape.transform.localScale;
        
        switch (type)
        {
            case SliderType.PositionX:
                _objectManipulation.selectedShape.transform.position = new Vector3(slider.value, position.y, position.z);
                break;
            
            case SliderType.PositionY:
                _objectManipulation.selectedShape.transform.position = new Vector3(position.x, slider.value, position.z);
                break;
            
            case SliderType.PositionZ:
                _objectManipulation.selectedShape.transform.position = new Vector3(position.x, position.y, slider.value);
                break;
            
            case SliderType.RotationX:
                _objectManipulation.selectedShape.transform.rotation = Quaternion.Euler(slider.value, rotation.y, rotation.z);
                break;
            
            case SliderType.RotationY:
                _objectManipulation.selectedShape.transform.rotation = Quaternion.Euler(rotation.x, slider.value, rotation.z);
                break;
            
            case SliderType.RotationZ:
                _objectManipulation.selectedShape.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, slider.value);
                break;
            
            case SliderType.Radius:
                _objectManipulation.selectedShape.transform.localScale = new Vector3(slider.value, slider.value, slider.value);
                break;
            
            case SliderType.ScaleX:
                _objectManipulation.selectedShape.transform.localScale = new Vector3(slider.value, scale.y, scale.z);
                break;
            
            case SliderType.ScaleY:
                _objectManipulation.selectedShape.transform.localScale = new Vector3(scale.x, slider.value, scale.z);
                break;
            
            case SliderType.ScaleZ:
                _objectManipulation.selectedShape.transform.localScale = new Vector3(scale.x, scale.y, slider.value);
                break;
        }
    }
    
    public void SetObjectManipulation(ObjectManipulation objectManipulation)
    {
        _objectManipulation = objectManipulation;
    }
}   

public enum SliderType
{
    PositionX,
    PositionY,
    PositionZ,
    RotationX,
    RotationY,
    RotationZ,
    Radius,
    ScaleX,
    ScaleY,
    ScaleZ,
}