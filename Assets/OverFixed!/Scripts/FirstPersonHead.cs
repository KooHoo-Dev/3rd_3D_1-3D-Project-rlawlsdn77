using UnityEngine;

namespace OverFixed
{
    // 1인칭에서 내 로봇 머리가 카메라를 가리지 않게 머리 뼈를 0 크기로 줄인다 (로컬 플레이어의 로봇에만 붙인다)
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
