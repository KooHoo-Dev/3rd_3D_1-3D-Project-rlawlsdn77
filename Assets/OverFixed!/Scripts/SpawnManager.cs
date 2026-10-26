using System;
using DG.Tweening;
using Jay;
using Study.Utilities;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace OverFixed
{
    
    // 스폰 위치 : 중앙 코어 모듈부터 가장 멀리 떨어진 모듈을 기준을 반지름 + 보정값으로 구_A를 만들고
    //  구_A보다 일정 배수만큼 더 큰 형태의 구_B를 만든 후,
    // 구_B에서 구_A를 제외한 영역에서 생성되게 한다.
    // 구_A를 생성할 때 각 모듈의 중심이 피벗이 되기 때문에 모듈에 겹쳐서 생성될 가능성이 있으므로
    // 오프셋을 설정해서 일정거리 더 띄운 거리를 반지름으로 설정한다.
    
    // 현재는 안전구역 Sphere와 스폰 Sphere를 오브젝트로 만들어서 프로토타입으로 진행한다.    
    
    // 스폰할 아이템 선정 방식
    // 1. Awake 때 먼저 아이템 오브젝트를 각각 PoolCount 만큼 풀링하여 PooledItems 배열에 넣는다 (비활성화 + Size 맞추기)
    // 2. GetRandomPooledItem으로 PooledItems 배열 내 랜덤한 인덱스의 오브젝트를 스폰할 오브젝트로 선정한다.
    
    // 스폰 형식
    // 1. 아이템은 SpawnSphere 구 표면의 무작위 위치에서 스폰되어 함선 방향으로 이동한다.
    // 2. 아이템은 SafeSphere 구와 충돌하지 않는 방향으로 이동한다.
    // 2. 아이템이 SpawnSphere 범위 밖으로 사라지면 풀링 배열로 돌아간다.
    
    
    
    public class SpawnManager : SingletonBase<SpawnManager>
    {
        private Transform coreModuleTransform;
        private Transform[] moduleTransformList;

        [SerializeField] private GameObject[] Items;
        [SerializeField] private GameObject[] PooledItems;
            
        [SerializeField] private GameObject SpawnSphere;
        [SerializeField] private GameObject SafeSphere;

        private float SpawnSphereRadius;
        private float SafeSphereRadius;

        private int PoolCount = 30;

        [SerializeField] private float SpawnEndTick  = 0f;
        [SerializeField] private float SpawnTick     = 0.5f;
        [SerializeField] private float MoveItemSpeed = 2f;

        private void Awake()
        {
            // 구 사이즈 구하기
            SpawnSphereRadius = SpawnSphere.transform.localScale.x * 0.5f;
            SafeSphereRadius = SafeSphere.transform.localScale.x * 0.5f;
            
            PooledItems = new GameObject[Items.Length * PoolCount];
            Pooling();
        }

        private void Start()
        {
        }

        // 오브젝트 풀링
        // 각 오브젝트를 PoolCount 만큼 생성하고 비활성화 한다.
        private void Pooling()
        {
            for (int i = 0; i < Items.Length; i++)
            {
                for (int j = 0; j < PoolCount; j++)
                {
                    GameObject SpawnItem = Instantiate(Items[i], transform);
                    
                    SpawnItem.SetActive(false);
                    SpawnItem.GetComponent<Rigidbody>().isKinematic = true;
                    SpawnItem.transform.localScale = Vector3.one * 10f;
                    
                    PooledItems[i * PoolCount + j] = SpawnItem;
                }
            }
        }

        private GameObject GetPooledItems()
        {
            GameObject spawnItem = Items[Random.Range(0, Items.Length)];
            
            return spawnItem;
        }

        // 풀링한 아이템 중 랜덤한 아이템 리턴
        private GameObject GetRandomPooledItem()
        {
            GameObject item = PooledItems[Random.Range(0, PooledItems.Length)];
            
            return item;
        }
        
        private void Update()
        {
            SpawnEndTick += Time.deltaTime;

            // 시간이 되면 아이템이 스폰된다.
            if (SpawnEndTick >= SpawnTick)
            {
                SpawnEndTick = 0f;
                
                GameObject spawnItem = GetRandomPooledItem();
                SpawnItem(spawnItem);
                ItemMove(spawnItem);
            }
        }

        private void SpawnItem(GameObject spawnItem)
        {
            Vector3 SpawnPoint = SpawnSphere.transform.position + Random.onUnitSphere * SpawnSphereRadius ;

            spawnItem.transform.position = SpawnPoint;
            spawnItem.transform.forward = Vector3.forward; // World 기준 Z 방향 바라보게 하기
            spawnItem.SetActive(true);
            
            Debug.Log(spawnItem.transform.position);
        }

        // 아이템 이동방식
        // 1. SpawnSphere의 무작위 표면 위치에서 함선 방향으로 이동하되, SafeSphere를 충돌하지 않게 이동한다. (결국 표면 -> 표면 이동)
        // 2. 근데 둘다 랜덤 표면 위치를 리턴받으면 무조건 SafeSphere를 통과하는 루트도 나옴
        // 3. 접선 이동해야할듯 (SpawnSphere 랜덤 위치에서 접선 위치로 (탄젠트인가?) Ray를 쏘기..?) 방향만 normalized로 알면 EndPos 알수 있을듯..
        // 4. 이동 방향은 원뿔 형태의 경로 중 하나로 선택될듯
        // 5. 원뿔의 수많은 경로 중 하나를 선택하는 기준..? ->
        // 아이템 스폰 위치가 worldPosition 기준으로
        // y가 0보다 같거나 크면 위쪽으로, y가 0보다 작으면 아래쪽으로 이동한다.
        
        private void ItemMove(GameObject spawnItem)
        { 
            // 스폰한 아이템 위치
            Vector3 startPos = spawnItem.transform.position;
            
            // 스폰 위치가 worldPos 기준 위인지 아래인지 확인
            bool IsSpawnUpper = startPos.y >= 0;
            
            // 안전지역 구 위치
            Vector3 safeSpherePos = SafeSphere.transform.position;
            
            // 스폰 위치 -> 안전지역 구 위치 (벡터)
            Vector3 SpawnToSafeDir = safeSpherePos - startPos;
            
            // 안전지역 구 중심에서 SpawnToSafeDir를 수직으로 하는 벡터
            Vector3 moveDir = SpawnToSafeDir.normalized;

            spawnItem.transform.DOMove(moveDir, 5f);
            Debug.DrawLine(startPos, moveDir, Color.red, 5f);
        }
    }
}