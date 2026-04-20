using UnityEngine;

public class FullBodyMovement : MonoBehaviour
{
    [Header("Controls")]
    [Range(0f,5f)] public float tempo = 1f;
    [Range(-1f,1f)] public float verticalAmplitude = 0f;
    [Range(0f,1f)] public float horizontalAmplitude = 0f;
    [Range(0f,1f)] public float forwardAmplitude = 0f;

    [Header("Settings")]
    public float groundY = 0f;
    public float verticalRange = 0.25f;

    Animator animator;

    Transform hips;
    Transform spine;
    Transform chest;
    Transform head;
    Transform leftFoot;
    Transform rightFoot;

    Vector3 hipsStartPos;
    Quaternion hipsStartRot;
    Quaternion spineStartRot;
    Quaternion chestStartRot;
    Quaternion headStartRot;

    Vector3 leftFootTarget;
    Vector3 rightFootTarget;
    Quaternion leftFootRot;
    Quaternion rightFootRot;

    float timeCounter;

    void Start()
    {
        animator = GetComponent<Animator>();

        hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        head = animator.GetBoneTransform(HumanBodyBones.Head);

        leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        // Fix body position
        // Transform armature = hips.parent;
        // float hipHeight = hips.position.y;
        // float offset = 0.9f - hipHeight;   // 0.9 is typical standing hip height

        //armature.position += Vector3.up * offset;

        hipsStartPos = hips.localPosition;
        hipsStartRot = hips.localRotation;
        spineStartRot = spine.localRotation;
        chestStartRot = chest.localRotation;
        headStartRot = head.localRotation;

        // AlignCharacterToGround();

        //leftFootTarget = leftFoot.position;
        //rightFootTarget = rightFoot.position;

        leftFootRot = leftFoot.rotation;
        rightFootRot = rightFoot.rotation;
    }

    void AlignCharacterToGround()
    {
        float lowestFoot =
            Mathf.Min(leftFoot.position.y, rightFoot.position.y);

        Debug.Log(lowestFoot);

        float offset = groundY - lowestFoot;

        leftFootTarget = leftFoot.position - (Vector3.up * offset);
        rightFootTarget = rightFoot.position - (Vector3.up * offset);

        // transform.position += Vector3.up * offset;
    }

    void LateUpdate()
    {
        timeCounter += Time.deltaTime * tempo;

        float wave = Mathf.Sin(timeCounter);

        ApplyVerticalMotion(wave);
        ApplyBodySway(wave);
    }

    void ApplyVerticalMotion(float wave)
    {
        Vector3 pos = hipsStartPos;
        pos.y += verticalAmplitude * wave;

        hips.localPosition = pos;
    }

    void ApplyBodySway(float wave)
    {
        float side = horizontalAmplitude * wave * 8f;
        float front = forwardAmplitude * wave * 8f;

        hips.localRotation =
            hipsStartRot *
            Quaternion.Euler(-front, 0, side);

        spine.localRotation =
            spineStartRot *
            Quaternion.Euler(-front * 0.7f, 0, side * 0.7f);

        chest.localRotation =
            chestStartRot *
            Quaternion.Euler(-front * 0.5f, 0, side * 0.5f);

        head.localRotation =
            headStartRot *
            Quaternion.Euler(-front * 0.3f, 0, side * 0.3f);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!animator) return;

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,1f);

        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,1f);

        animator.SetIKPosition(AvatarIKGoal.LeftFoot,leftFootTarget);
        animator.SetIKPosition(AvatarIKGoal.RightFoot,rightFootTarget);

        animator.SetIKRotation(AvatarIKGoal.LeftFoot,leftFootRot);
        animator.SetIKRotation(AvatarIKGoal.RightFoot,rightFootRot);
    }
}