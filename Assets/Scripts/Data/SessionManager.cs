using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject inputScreen;
    public GameObject outputScreen;

    [Header("Input UI Elements")]
    public TMP_InputField userIdInput;
    public TMP_InputField excerptIdInput;
    public Button startButton;

    [Header("Main Tool UI Elements")]
    public Slider armHeightSlider;
    public Slider armExtensionSlider;
    public Slider tempoSlider;
    public Slider jerkSlider;
    public Slider verticalSlider;
    public ToggleSwitch verticalToggle;
    public Slider horizontalSlider;
    public Slider forwardSlider;

    [Header("Output UI Elements")]
    public TMP_InputField outputTextBox;

    [Header("Camera Control")]
    public CameraOrbit cameraOrbit;

    void Start()
    {
        ResetTool();
    }

    void Update()
    {
        // Only enable startButton if user has filled out both fields
        bool hasBothInputs = !string.IsNullOrEmpty(userIdInput.text) && !string.IsNullOrEmpty(excerptIdInput.text);
        startButton.interactable = hasBothInputs;
    }

    public void BeginSession()
    {
        // Hide input overlay
        inputScreen.SetActive(false);

        // Turn on camera
        cameraOrbit.enabled = true;
    }

    public void ResetTool()
    {
        // Reset slider values
        armHeightSlider.value = 0f;
        armExtensionSlider.value = 1f;
        tempoSlider.value = 0.1f;
        jerkSlider.value = 0f;
        verticalSlider.value = 0f;
        horizontalSlider.value = 0f;
        forwardSlider.value = 0f;

        // Clear excerptId (but not userId so they do not have to retype their id)
        excerptIdInput.text = "";

        // Reset screens
        outputScreen.SetActive(false);
        inputScreen.SetActive(true);

        // Reset camera
        cameraOrbit.enabled = false;
        cameraOrbit.ResetCamera();
    }

    public void GenerateJson()
    {
        // Retrieve data from session
        SessionData data = new SessionData();
        data.userId = userIdInput.text;
        data.excerptId = excerptIdInput.text;
        data.armHeight = armHeightSlider.value;
        data.armExtension = armExtensionSlider.value;
        data.tempo = tempoSlider.value;
        data.jerk = jerkSlider.value;
        data.verticalAmplitude = verticalSlider.value;
        data.verticalToggle = verticalToggle.isOn ? 1 : -1;
        data.horizontalAmplitude = horizontalSlider.value;
        data.forwardAmplitude = forwardSlider.value;

        // Create json response
        string jsonOutput = JsonUtility.ToJson(data, true);
        outputTextBox.text = jsonOutput;

        // Enable output screen
        outputScreen.SetActive(true);

        // Disable camera
        cameraOrbit.enabled = false;
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = outputTextBox.text;
        Debug.Log("Copied to clipboard.");
    }
}

[Serializable]
public class SessionData
{
    public string userId;
    public string excerptId;
    public float armHeight;
    public float armExtension;
    public float tempo;
    public float jerk;
    public float verticalAmplitude;
    public float verticalToggle; // -1 for down, 1 for up
    public float horizontalAmplitude;
    public float forwardAmplitude;
}