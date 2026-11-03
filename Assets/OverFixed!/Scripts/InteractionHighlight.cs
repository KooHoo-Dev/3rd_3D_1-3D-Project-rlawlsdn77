using UnityEngine;
using UnityEngine.Rendering;

namespace OverFixed
{
    // 상호작용 대상(터미널, 에어락 패드, 사고 패널)의 겉면 빛. 시작할 때 이 오브젝트의 메시를 그대로 따라 그리는 빛 껍데기를 자식으로 만든다
    // 밝기는 항상 같다 (재질 InteractionHighlight.mat의 Intensity)
    [RequireComponent(typeof(MeshRenderer))]
    public class InteractionHighlight : MonoBehaviour
    {
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Mesh mesh;     // 이 오브젝트의 원본 메시. 정적 배칭 때문에 플레이 중 MeshFilter에는 씬을 합친 메시가 들어 있어 따로 둔다

        // 같은 자리·같은 메시에 빛 재질만 입힌 자식 (서브메시마다 같은 재질)
        private void Awake()
        {
            GameObject shellObject = new GameObject("Highlight");
            shellObject.transform.SetParent(transform, false);
            shellObject.AddComponent<MeshFilter>().sharedMesh = mesh;

            MeshRenderer shell = shellObject.AddComponent<MeshRenderer>();
            Material[] materials = new Material[mesh.subMeshCount];
            for (int index = 0; index < materials.Length; index++) materials[index] = highlightMaterial;
            shell.sharedMaterials = materials;
            shell.shadowCastingMode = ShadowCastingMode.Off;
            shell.receiveShadows = false;
            shell.lightProbeUsage = LightProbeUsage.Off;
            shell.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }
    }
}
