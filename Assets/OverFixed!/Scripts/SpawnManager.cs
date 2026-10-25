using Jay;
using UnityEngine;

namespace OverFixed
{
    
    // 스폰 위치 : 중앙 코어 모듈부터 가장 멀리 떨어진 모듈을 기준을 반지름 + 보정값으로 구_A를 만들고
    //  구_A보다 일정 배수만큼 더 큰 형태의 구_B를 만든 후,
    // 구_B에서 구_A를 제외한 영역에서 생성되게 한다.
    // 구_A를 생성할 때 각 모듈의 중심이 피벗이 되기 때문에 모듈에 겹쳐서 생성될 가능성이 있으므로
    // 오프셋을 설정해서 일정거리 더 띄운 거리를 반지름으로 설정한다.
    
    public class SpawnManager : SingletonBase<SpawnManager>
    {
        private Transform coreModuleTransform;
        private Transform[] moduleTransformList;
        
        private void SafetyZone()
        {
            
        }

        private void MaxSpaceZone()
        {
            
        }

        private void SpawnZone()
        {
            
        }
        
        private void GetSpawn()
        {
            
        }
    }
}