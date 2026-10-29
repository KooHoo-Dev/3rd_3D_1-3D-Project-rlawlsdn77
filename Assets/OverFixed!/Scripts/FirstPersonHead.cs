using UnityEngine;

namespace OverFixed
{
    // 1인칭에서 내 로봇의 머리가 카메라를 가리지 않도록 머리 뼈를 0 크기로 줄인다 (몸, 팔, 다리는 그대로 보인다)
    // 내 화면에서만 숨기는 것이라 로컬 플레이어의 로봇에만 붙인다. 멀티플레이를 붙이면 다른 사람의 로봇에는 붙이지 않아야 얼굴이 보인다
    [RequireComponent(typeof(Animator))]
    public class FirstPersonHead : MonoBehaviour
    {
        private Transform headBone;

        private void Awake()
        {
            headBone = GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
        }

        // 애니메이션이 뼈를 움직인 뒤, 그리기 전에 줄인다
        private void LateUpdate()
        {
            headBone.localScale = Vector3.zero;
        }
    }
}
