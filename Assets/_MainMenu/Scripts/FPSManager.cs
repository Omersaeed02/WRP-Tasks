using System;
using UnityEngine;
using System.Collections;

public class FPSManager : MonoBehaviour
{
    private float _count;

    private IEnumerator Start()
    {
        GUI.depth = 2;
        while (true)
        {
            _count = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void OnGUI()
    {
        var text = $"FPS: {Mathf.Round(_count)}";
        GUI.skin.label.fontSize = 18;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        var textSize = GUI.skin.label.CalcSize(new GUIContent(text));

        var location = new Rect(
            (Screen.width - textSize.x) / 2f,
            5,                               
            textSize.x + 10,                 
            textSize.y                       
        );

        Texture black = Texture2D.linearGrayTexture;
        GUI.DrawTexture(location, black, ScaleMode.StretchToFill);
        GUI.color = Color.black;
        GUI.Label(location, text);
    }
}