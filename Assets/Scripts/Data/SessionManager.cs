using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

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
    public Toggle orbitToggle;

    // NEW — added for the web bridge. Assign this in the Inspector to the
    // Submit button that calls GenerateJson(). Used to block double-submits
    // while the parent page's write is in flight, and to re-enable Submit
    // if the parent rejects a malformed payload without resetting the pose.
    // Safe to leave unassigned if you're not using the web bridge yet —
    // every place that touches it null-checks first.
    [Header("Web Bridge (WebGL builds only)")]
    public Button submitButton;

    // ============================================================
    // WEB BRIDGE — outbound calls (Unity -> parent page)
    // Implemented in Assets/Plugins/WebGL/UnityBridge.jslib.
    // These extern declarations only compile into an actual WebGL
    // build; in the Editor and in desktop builds they're skipped
    // entirely, so nothing here changes how you test the tool
    // locally.
    // ============================================================
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void Bridge_SendUnityReady();

    [DllImport("__Internal")]
    private static extern void Bridge_SendAvatarResponse(string json);
#endif

    private void SendUnityReadyToParent()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Bridge_SendUnityReady();
#endif
    }

    private void SendAvatarResponseToParent(string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Bridge_SendAvatarResponse(json);
#endif
    }

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Integrated flow: IDs arrive via InitializeSessionFromWeb(), called
        // from the parent page once it receives our 'unity-ready' signal.
        // Hide both screens immediately so there's no flash of the manual
        // ID-entry UI while that round trip happens.
        inputScreen.SetActive(false);
        outputScreen.SetActive(false);
        cameraOrbit.enabled = false;
        SendUnityReadyToParent();
#else
        // Desktop build / Editor Play mode: unchanged manual-entry flow,
        // so local testing of the tool itself still works exactly as before.
        ResetTool();
#endif
    }

    void Update()
    {
        // Only enable startButton if user has filled out both fields.
        // Harmless to leave running in WebGL builds too — inputScreen is
        // inactive there, so startButton's interactable state is moot.
        bool hasBothInputs = !string.IsNullOrEmpty(userIdInput.text) && !string.IsNullOrEmpty(excerptIdInput.text);
        startButton.interactable = hasBothInputs;
    }

    public void BeginSession()
    {
        // Hide input overlay
        inputScreen.SetActive(false);

        // Turn on camera
        orbitToggle.isOn = false;
    }

    // NEW — extracted from ResetTool() so the web bridge can reset the avatar's
    // pose to default WITHOUT also re-showing the manual ID-entry screen (which
    // ResetTool() below still does, for the desktop flow).
    //
    // Also resets verticalToggle to Down, via SetState() — the same method
    // ControlPanel.cs already calls to set the tool's initial crouching pose,
    // so this matches that existing default exactly. (The original ResetTool()
    // did not reset this toggle at all; confirmed with the team that it should
    // reset across trials, so this closes that gap rather than just flagging it.)
    private void ResetAvatarState()
    {
        armHeightSlider.value = 0f;
        tempoSlider.value = 0.55f;
        jerkSlider.value = 0.5f;
        verticalSlider.value = 0f;
        horizontalSlider.value = 0f;
        forwardSlider.value = 0f;
        verticalToggle.SetState(AvatarProceduralController.VerticalDirection.Down);
    }

    public void ResetTool()
    {
        ResetAvatarState();

        // Clear excerptId (but not userId so they do not have to retype their id)
        excerptIdInput.text = "";

        // Reset screens
        outputScreen.SetActive(false);
        inputScreen.SetActive(true);

        // Reset camera
        orbitToggle.isOn = false;
        cameraOrbit.ResetCamera();
    }

    // NEW — the JSON shape the parent page sends via postMessage 'init',
    // parsed here with JsonUtility to match the rest of this file's style.
    // Field names deliberately differ from SessionData below: the web
    // harness's postMessage contract uses "participantId" (established
    // across the whole website side of this project), while this tool's
    // own JSON schema uses "userId" (SessionData, below) — that's an
    // existing naming difference between the two systems, not a bug, and
    // this is the one place the mapping happens.
    [Serializable]
    private class WebInitPayload
    {
        public string participantId;
        public string excerptId;
    }

    // NEW — call via unityInstance.SendMessage("SessionManager", "InitializeSessionFromWeb", json)
    // from the parent page's JS, once per trial. Per the bridge design this call
    // doubles as the reset trigger: every call resets the avatar to default
    // BEFORE applying the new IDs, so trial N+1 never starts from trial N's pose.
    public void InitializeSessionFromWeb(string json)
    {
        WebInitPayload payload;
        try
        {
            payload = JsonUtility.FromJson<WebInitPayload>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"InitializeSessionFromWeb: failed to parse '{json}': {e.Message}");
            return;
        }

        if (payload == null || string.IsNullOrEmpty(payload.participantId) || string.IsNullOrEmpty(payload.excerptId))
        {
            Debug.LogError($"InitializeSessionFromWeb: payload missing participantId/excerptId: {json}");
            return;
        }

        ResetAvatarState();

        userIdInput.text = payload.participantId;
        excerptIdInput.text = payload.excerptId;

        if (submitButton != null) submitButton.interactable = true;

        BeginSession();
    }

    // NEW — call via unityInstance.SendMessage("SessionManager", "OnSubmitRejected")
    // if the parent page's own validation rejects the last avatar-response payload
    // as malformed. Re-enables Submit WITHOUT resetting the avatar, so the
    // participant doesn't lose the pose they just tried to submit.
    public void OnSubmitRejected()
    {
        if (submitButton != null) submitButton.interactable = true;
        Debug.LogWarning("OnSubmitRejected: parent page rejected the last submission; Submit re-enabled.");
    }

    public void GenerateJson()
    {
        // Retrieve data from session
        SessionData data = new SessionData();
        data.userId = userIdInput.text;
        data.excerptId = excerptIdInput.text;
        data.armPosition = armHeightSlider.value;
        data.speed = tempoSlider.value;
        data.smoothness = jerkSlider.value;
        data.verticalAmplitude = verticalSlider.value;
        data.verticalToggle = verticalToggle.isOn ? 1 : -1;
        data.horizontalAmplitude = horizontalSlider.value;
        data.forwardAmplitude = forwardSlider.value;

        // Create json response
        string jsonOutput = JsonUtility.ToJson(data, true);

#if UNITY_WEBGL && !UNITY_EDITOR
        // Integrated flow: send straight to the parent page. No textbox,
        // no copy/paste, ever, in this build path.
        if (submitButton != null) submitButton.interactable = false; // block double-submit until the parent responds
        cameraOrbit.enabled = false;
        SendAvatarResponseToParent(jsonOutput);
#else
        // Desktop build / Editor: unchanged textbox+clipboard flow.
        outputTextBox.text = jsonOutput;
        outputScreen.SetActive(true);
        orbitToggle.isOn = false;
#endif
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
    public float armPosition;
    public float speed;
    public float smoothness;
    public float verticalAmplitude;
    public float verticalToggle; // -1 for down, 1 for up
    public float horizontalAmplitude;
    public float forwardAmplitude;
}
