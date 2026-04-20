using System;
using UnityEngine;

public class AvatarProceduralController : MonoBehaviour
{
    public enum VerticalDirection { Up = 1, Down = -1 }

    [Header("Slider Inputs")]
    [Range(0f,180f)] public float armHeight = 0.0f;
    [Range(0f,1f)] public float armExtension = 1.0f;
    [Range(0f,1f)] public float tempo = 0.1f;
    [Range(0f,1f)] public float jerk = 0.0f;
    [Range(0f,1f)] public float verticalAmplitude = 0.0f; 
    [Range(0f,1f)] public float horizontalAmplitude = 0.0f;
    [Range(0f,1f)] public float forwardAmplitude = 0.0f;
    public VerticalDirection verticalDirection = VerticalDirection.Down;

    [Header("IK Targets")]
    public float floorHeight = 0.1f;
    public Transform hipTarget;
    public Transform leftFootTarget;
    public Transform rightFootTarget;
    public Transform leftArmTarget;
    public Transform rightArmTarget;

    [Header("Upper Body Direct Bones")]
    public Transform hips;
    public Transform spine;
    public Transform chest;
    public Transform head;

    // Lower Body Start Data
    private Vector3 initialHipPos;
    private Vector3 initialLeftFootPos;
    private Vector3 initialRightFootPos;
    private Vector3 initialLeftArmPos;
    private Vector3 initialRightArmPos;
    private Quaternion initialLeftFootRot;
    private Quaternion initialRightFootRot;

    // Upper Body Start Data
    private Quaternion spineStartRot;
    private Quaternion chestStartRot;
    private Quaternion headStartRot;

    
    private float phase;

    void Start() {
        // Capture IK starting positions
        initialHipPos = new Vector3(hipTarget.position.x, hipTarget.position.y, hipTarget.position.z);
        initialLeftFootPos = new Vector3(leftFootTarget.position.x, floorHeight, leftFootTarget.position.z);
        initialRightFootPos = new Vector3(rightFootTarget.position.x, floorHeight, rightFootTarget.position.z);
        initialLeftFootRot = leftFootTarget.localRotation;
        initialRightFootRot = rightFootTarget.localRotation;

        // Local position for arms so that they stay relative to the chest as it moves
        initialLeftArmPos = leftArmTarget.localPosition;
        initialRightArmPos = rightArmTarget.localPosition;
    
        // Capture direct bone starting rotations (upper body)
        spineStartRot = spine.localRotation;
        chestStartRot = chest.localRotation;
        headStartRot = head.localRotation;
    }


    void Update()
    {
        // Keep time
        phase += Time.deltaTime * tempo * 8;
        float rawSine = Mathf.Sin(phase);

        // Apply jerk and modify underlying sine wave
        float sine = ApplyJerk(rawSine);

        // ---- Vertical ----
        float currentHipY = initialHipPos.y;
        float baseFootY = floorHeight;

        // Crouching vs. Jumping
        if (verticalDirection == VerticalDirection.Down)
        {
            // CROUCHING: Hip goes down, but Foot MUST stay at floor
            // Tempo: same `phase` as other body parts
            float bouncePhase = ApplyJerk(Mathf.Sin(phase));
            // Smooth crouching with squared phase so knees don't flare
            float crouchOffset = Mathf.Abs(bouncePhase * bouncePhase) * verticalAmplitude * (int)verticalDirection * 0.25f;
            currentHipY += crouchOffset;
            baseFootY = floorHeight;
        }
        else if (verticalDirection == VerticalDirection.Up)
        {
            // JUMPING: Dip, then jump. Foot stays on ground, then follows Hip up
            // Tempo: twice as fast as `phase`, since this involves a dip and jump,
            // both of which take `phase` time.
            float bouncePhase = ApplyJerk(Mathf.Sin(phase * 2));

            // Anticipation Dip
            if (bouncePhase < 0)
            {
                float dipOffset = bouncePhase * verticalAmplitude * 0.4f;
                currentHipY += dipOffset;
                baseFootY = floorHeight;
            } 
            // Jump
            else
            {
                // JUMPING: Foot follows Hip UP, but never goes BELOW floor
                float jumpOffset = bouncePhase * verticalAmplitude * (int)verticalDirection;
                currentHipY += jumpOffset;
                baseFootY = Mathf.Max(floorHeight, floorHeight + jumpOffset);
                // Edit: Used to be jumpOffset * 1.7, so that feet would travel 
                // faster than hips. Only use if you want knees to bend during jump.
            }
        }

        // ---- Sway and Heel Lift  ----
        // Horizontal and forward sway for hip
        // TODO: 1.5 multiplier too much?
        float horizontalSway = sine * horizontalAmplitude * 0.25f;
        float forwardSway = 0f; //sine * forwardAmplitude * 0.2f;

        // Drop hips when horizontal swaying to avoid feet leaving ground
        // Use sine**2 to smoothen the time series of the knees bending so they don't flare
        float smoothedSway = sine * sine * horizontalAmplitude * 0.25f;
        float swayDrop = Mathf.Abs(smoothedSway) * 0.25f;
        currentHipY -= swayDrop;

        // Calculate max heel lift, apply to one foot depending on direction of sway
        // TODO: fix heel lift, the opposite heel should not be lifitng at all!!
        // though does clamp do that already?
        float maxHeelLift = 0.05f * horizontalAmplitude;
        float leftLift = Mathf.Clamp01(sine) * maxHeelLift;
        float rightLift = Mathf.Clamp01(-sine) * maxHeelLift;
        float currentLeftFootY = baseFootY + leftLift;
        float currentRightFootY = baseFootY + rightLift;

        // Apply IK to targets
        hipTarget.position = new Vector3(initialHipPos.x + horizontalSway, currentHipY, initialHipPos.z + forwardSway);
        leftFootTarget.position = new Vector3(initialLeftFootPos.x, currentLeftFootY, initialLeftFootPos.z);
        rightFootTarget.position = new Vector3(initialRightFootPos.x, currentRightFootY, initialRightFootPos.z);

        // Lock the feet rotations so the toes never dip
        leftFootTarget.localRotation = initialLeftFootRot;
        rightFootTarget.localRotation = initialRightFootRot;

        // ---- Arm Inertia and Height/Extension ----

        // Calculate Velocity as derivative of sway (sin)
        float horizontalVelocity = ApplyJerk(Mathf.Cos(phase));
        float forwardVelocity = ApplyJerk(Mathf.Cos(phase));

        // Inertia pushes opposite to velocity
        float inertiaX = -horizontalVelocity * horizontalAmplitude * 0.2f;
        float inertiaZ = -forwardVelocity * forwardAmplitude * 0.01f; // arms move too much in Z direction

        // Initially, arms were clipping into hips, so had to clamp inertia to make
        // sure they don't. However as the arms are raised, there is more room
        // for them to sway, so dynamically assign the amount they are allowed to move.
        float freeSwayAmount = Mathf.Lerp(0.05f, 0.5f, armHeight * 3f);
        float leftArmX = Mathf.Min(freeSwayAmount, inertiaX);
        float rightArmX = Mathf.Max(-freeSwayAmount, inertiaX);
        
        // Create lift arc for when arm is fully extended (armExtension = 1)
        float liftAngle = armHeight * Mathf.PI / 180;
        float outwardArc = Mathf.Sin(liftAngle);
        float upwardArc = 1f - Mathf.Cos(liftAngle);

        // Calculate arm positions along that arc
        Vector3 leftExtendedPos = initialLeftArmPos + new Vector3(leftArmX - outwardArc, upwardArc, inertiaZ);
        Vector3 rightExtendedPos = initialRightArmPos + new Vector3(rightArmX + outwardArc, upwardArc, inertiaZ);

        // Define shoulder anchor points for distance interpolation
        // (roughly 0.4 meters straight up)
        Vector3 leftShoulderPos = initialLeftArmPos + new Vector3(0f, 0.4f, 0f);
        Vector3 rightShoulderPos = initialRightArmPos + new Vector3(0f, 0.4f, 0f);

        // Move target to armExtension distance away from shoulder
        leftArmTarget.localPosition = Vector3.Lerp(leftShoulderPos, leftExtendedPos, armExtension);
        rightArmTarget.localPosition = Vector3.Lerp(rightShoulderPos, rightExtendedPos, armExtension);
    }

    void LateUpdate()
    {
        // UPPER BODY SWAY
        ApplyBodySway(phase);
    }

    void ApplyBodySway(float wave)
    {
        // Phase shifting (the higher the bone, the further behind in time)
        float spineWave = ApplyJerk(Mathf.Sin(wave - 0.3f)); 
        float chestWave = ApplyJerk(Mathf.Sin(wave - 0.6f));
        float headWave  = ApplyJerk(Mathf.Sin(wave - 0.9f));

        // Spine (Delayed + Attenuated by 0.7)
        float spineSide = horizontalAmplitude * spineWave * 8f * 0.7f;
        float spineFront = forwardAmplitude * spineWave * 20f * 0.7f;
        spine.localRotation = spineStartRot * Quaternion.Euler(-spineFront, 0, spineSide);

        // Chest (Delayed + Attenuated by 0.5)
        float chestSide = horizontalAmplitude * chestWave * 8f * 0.5f;
        float chestFront = forwardAmplitude * chestWave * 20f * 0.5f;
        chest.localRotation = chestStartRot * Quaternion.Euler(-chestFront, 0, chestSide);

        // Head (Pitch, Yaw, and Roll)
        // Pitch: headbang (x-axis)
        float headFront = forwardAmplitude * headWave * 25f;
        
        // Yaw: look slightly left and right as the body sways (y-axis)
        float headYaw = horizontalAmplitude * headWave * 10f;

        // Roll: ear-to-shoulder tilt (z-axis)
        float headRoll = horizontalAmplitude * headWave * 15f;
        head.localRotation = headStartRot * Quaternion.Euler(-headFront, headYaw, headRoll);


        // float headSide = horizontalAmplitude * headWave * 3f * 0.3f;
        // float headFront = forwardAmplitude * headWave * 8f * 0.3f;
        // head.localRotation = headStartRot * Quaternion.Euler(-headFront, 0, headSide);
    }

    float ApplyJerk(float rawWave)
    {
        // When jerk = 0: power = 1, so we get a regular smooth sine wave
        // When jerk = 1: power = 0.15, so we get a highly accelerated square wave
        float power = Mathf.Lerp(1f, 0.25f, jerk);

        // Mathf.Sign used to preserve directionality after losing it in Mathf.Abs
        // Applying fractional powers makes the wave explode towards 1/-1 faster
        return Mathf.Sign(rawWave) *  Mathf.Pow(Mathf.Abs(rawWave), power);
    }
}