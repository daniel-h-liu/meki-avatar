using System;
using JetBrains.Annotations;
using NUnit.Framework;
using TMPro;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public RectTransform handle;
    public RectTransform background;
    public TextMeshProUGUI directionText;

    [Header("Positions")]
    public float downPosition = 75f; // left position handle should move to
    public float upPosition = 95f; // right position
    //public float downTextPosition = -27f;
    //public float upTextPosition = -60f;

    [Header("Animation")]
    public float animationSpeed = 40f;

    [Header("State")]
    public bool isOn = false; // false = Down, true = Up

    // Callback function for ControlPanel
    public UnityEngine.Events.UnityEvent<bool> onValueChanged;

    
    private float targetX;
    private bool isAnimating = false;
    void Start()
    {
        if (handle == null)
        {
            Debug.LogError("Handle not assigned.");
            return;
        }

        // Set initial position (down)
        targetX = isOn ? upPosition : downPosition;
        Vector2 handlePos = handle.anchoredPosition;
        handlePos.x = targetX;
        handle.anchoredPosition = handlePos;
        
        // directionText.SetText("Down");
        //Vector2 textPos = directionText.rectTransform.anchoredPosition;
        //textPos.x = downTextPosition;
        //directionText.rectTransform.anchoredPosition = textPos;
    }

    void Update()
    {
        if (handle == null) return;

        // Animate to target position
        Vector2 currentPos = handle.anchoredPosition;
        if (Mathf.Abs(currentPos.x - targetX) > 0.1f)
        {
            // Linearly interpolate the distance to the target, relative to time and animation speed
            currentPos.x = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * animationSpeed);
            handle.anchoredPosition = currentPos;
            isAnimating = true;
        }
        else if (isAnimating) // close enough, but still hasn't reached end
        {
            // SNAP!
            currentPos.x = targetX;
            handle.anchoredPosition = currentPos;
            isAnimating = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData){
        Toggle();
    }

    public void Toggle()
    {
        // Swap toggle
        isOn = !isOn;
        targetX = isOn ? upPosition : downPosition;

        //string direction = isOn ? "Up" : "Down";
        //directionText.SetText(direction);
        //Vector2 textPos = directionText.rectTransform.anchoredPosition;
        //textPos.x = isOn ? upTextPosition : downTextPosition;
        //directionText.rectTransform.anchoredPosition = textPos;

        onValueChanged.Invoke(isOn);
    }

    public void SetState(AvatarProceduralController.VerticalDirection value)
    {
        if (value == AvatarProceduralController.VerticalDirection.Up) {
            isOn = true;
            targetX = upPosition;
            //directionText.SetText("Up");
        }
        else if (value == AvatarProceduralController.VerticalDirection.Down)
        {
            isOn = false;
            targetX = downPosition;
            //directionText.SetText("Down");
        }
        onValueChanged.Invoke(isOn);
    }
}
