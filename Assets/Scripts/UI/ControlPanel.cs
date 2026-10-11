using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class ControlPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject controlPanel;

    [Header("Sliders")]
    public Slider armHeightSlider;
    public Slider tempoSlider;
    public Slider jerkSlider;
    public Slider verticalSlider;
    public Slider horizontalSlider;
    public Slider forwardSlider;

    [Header("Toggles")]
    public ToggleSwitch verticalDirectionToggle;

    private AvatarProceduralController bodyMovement;

    void Start()
    {
        bodyMovement = FindFirstObjectByType<AvatarProceduralController>();
        if (bodyMovement == null)
        {
            Debug.LogError("AvatarProceduralController not found.");
            return;
        }

        // TODO: REMOVE and make control panel dynamic
        // Set initial slider values from movement component
        if (tempoSlider != null)
        {
            tempoSlider.value = bodyMovement.tempo;
            tempoSlider.onValueChanged.AddListener(OnTempoChanged);
        }

        if (jerkSlider != null)
        {
            jerkSlider.value = bodyMovement.jerk;
            jerkSlider.onValueChanged.AddListener(OnJerkChanged);
        }
        
        if (verticalSlider != null)
        {
            verticalSlider.value = bodyMovement.verticalAmplitude;
            verticalSlider.onValueChanged.AddListener(OnVerticalChanged);
        }
        if (verticalDirectionToggle != null)
        {
            verticalDirectionToggle.SetState(AvatarProceduralController.VerticalDirection.Down); // start: crouching
            verticalDirectionToggle.onValueChanged.AddListener(OnVerticalDirectionChanged);
        }
        
        if (horizontalSlider != null)
        {
            horizontalSlider.value = bodyMovement.horizontalAmplitude;
            horizontalSlider.onValueChanged.AddListener(OnHorizontalChanged);
        }
        
        if (forwardSlider != null)
        {
            forwardSlider.value = bodyMovement.forwardAmplitude;
            forwardSlider.onValueChanged.AddListener(OnForwardChanged);
        }

        if (armHeightSlider != null)
        {
            armHeightSlider.value = bodyMovement.armHeight;
            armHeightSlider.onValueChanged.AddListener(OnArmHeightChanged);
        }
    }

    void OnTempoChanged(float value)
    {
        if (bodyMovement != null)
            bodyMovement.tempo = value;
    }
    void OnJerkChanged(float value)
    {
        if (bodyMovement != null)
            bodyMovement.jerk = value;
    }
    void OnVerticalChanged(float value)
    {
        if (bodyMovement != null)
            bodyMovement.verticalAmplitude = value;
    }
    void OnVerticalDirectionChanged(bool value)
    {
        if (bodyMovement != null)
        {
            if (value)
            {
                bodyMovement.verticalDirection = AvatarProceduralController.VerticalDirection.Up;
            } else
            {
                bodyMovement.verticalDirection = AvatarProceduralController.VerticalDirection.Down;
            }
        }
    }
    void OnHorizontalChanged(float value)
    {
        if (bodyMovement != null)
            bodyMovement.horizontalAmplitude = value;
    }
    void OnForwardChanged(float value)
    {
        if (bodyMovement != null)
            bodyMovement.forwardAmplitude = value;
    }
    void OnArmHeightChanged(float value)
    {
        if (bodyMovement != null)
            bodyMovement.armHeight = value;
    }
}
