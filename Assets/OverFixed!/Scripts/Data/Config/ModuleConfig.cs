using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/ModuleConfig")]
    public class ModuleConfig : ScriptableObject
    {
        [Tooltip("모듈은 무조건 (홀수 * 홀수) 형식의 배열이어야 한다. (코어 모듈을 가운데에 두기 위해서")]
        [Header("모듈 최대 크기")]
        public int MaxWidth = 5;
        public int MaxHeight = 5;
    }
}
