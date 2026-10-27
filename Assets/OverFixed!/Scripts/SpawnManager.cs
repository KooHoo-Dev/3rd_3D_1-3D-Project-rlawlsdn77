using System;
using DG.Tweening;
using Jay;
using Study.Utilities;
// 변경!!! using UnityEditor 삭제 (에디터 전용 어셈블리라 게임 빌드 시 에러)
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
        
        [Header("Spawn Settings")]
        [SerializeField] private SpawnConfig spawnConfig;
        
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

        [SerializeField] private float ItemMoveDuration = 30f;
        
        // 오브젝트 크기 변경 함수 (디버깅용 가시성 확보)
        private int ItemLocalScale = 5;

        // Items 배열과 같은 순서의 생성 가중치 (ItemTable의 SpawnWeight)
        private float[] itemSpawnWeights;

        private void Awake()
        {
            // 구 사이즈 구하기
            SpawnSphereRadius = SpawnSphere.transform.lossyScale.x * 0.5f;
            SafeSphereRadius = SafeSphere.transform.lossyScale.x * 0.5f;

            PooledItems = new GameObject[Items.Length * PoolCount];
            Pooling();
            
            itemSpawnWeights = new float[Items.Length];
            for (int i = 0; i < Items.Length; i++)
            {
                ItemType itemId = Items[i].GetComponent<ItemObject>().ItemID;
                itemSpawnWeights[i] = DataManager.GetItem(itemId).SpawnWeight;
            }
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
                    SpawnItem.transform.localScale = Vector3.one * ItemLocalScale;
                    
                    PooledItems[i * PoolCount + j] = SpawnItem;
                }
            }
        }

        // 풀링한 아이템 중 랜덤한 아이템 리턴
        // SpawnWeight로 종류를 먼저 고르고, 그 종류에서 꺼진 아이템을 꺼낸다
        private GameObject GetRandomPooledItem()
        {
            float totalWeight = 0f;
            foreach (float weight in itemSpawnWeights) totalWeight += weight;

            float pick = Random.Range(0f, totalWeight);
            for (int i = 0; i < Items.Length; i++)
            {
                pick -= itemSpawnWeights[i];
                if (pick <= 0f) return GetInactiveItem(i);
            }
            return GetInactiveItem(Items.Length - 1);
        }

        // itemIndex 종류의 꺼진 아이템. 그 종류가 모두 켜져 있으면 null
        private GameObject GetInactiveItem(int itemIndex)
        {
            for (int j = 0; j < PoolCount; j++)
            {
                GameObject item = PooledItems[itemIndex * PoolCount + j];
                // 사용중이지 않으면 가져옴
                if (item.activeSelf == false) return item;
            }
            return null;
        }

        // 아이템을 풀로 반환. 집기, 내려놓기에서 바뀐 상태(부모, 크기, 콜라이더, 물리)를 풀링 때 상태로 되돌리고 끈다
        // 흐름 끝, 터미널 납품처럼 아이템이 사라지는 곳에서 호출한다
        public void ReturnItem(GameObject item)
        {
            item.transform.DOKill();
            item.transform.SetParent(transform);
            item.transform.localScale = Vector3.one * ItemLocalScale;
            item.GetComponent<Collider>().enabled = true;

            Rigidbody itemBody = item.GetComponent<Rigidbody>();
            itemBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            itemBody.isKinematic = true;

            item.SetActive(false);
        }
        
        // 풀링 + 스폰되어서 이동하는 아이템 잡음
        // 그 아이템 집고 바닥에 내려놓음 (이거는 풀링으로 다시 들어가야하나?
        // ---> NO (풀링으로 들어가면 스폰 위치로 돌아가서 움직일 가능성 있음, Player 자식에서 해제되면 될듯)
        // 근데 그러면 풀링에 빠진 아이템때문에 특정 아이템이 더 적게 or 더 많게 나올 수 있음 
        // 일단 풀링으로 돌아가는건 터미널에서 오더 아이템 제출할때는 필수로 들어가야함
        // 일단 아이템마다 가중치가 있어서 모든 아이템이 동일한 확률은 아님
        // 근데 이러면 ItemTable에 맞는 확률이 나오지 않음 (플레이마다 다른 확률이 적용됨)
        
        // 해결방법 (1번)
        // 1. 선외에서 아이템 집을 때 그 아이템을 풀링 대기로 리턴시키고 잡은 아이템을 새 인스턴스로 만들어서 Pooling 목록과는 별개로 진행
        // 2. 잡기 / 놓기 / 제출 때마다 풀링을 그때그때 추가로 진행한다.
        
        // 근데 바닥에 떨어뜨린 아이템을 다시 주울 때는?
        // 그건 그냥 기존 TakeItem을 쓰면 될듯
        // 그리고 LocalPlayer가 아이템의 kinematic이나 collider을 직접 조정하는게 의존성 부분에서 맞는 디자인인지 모르겠음
        
        // LocalPlayer의 TakeItem 시 Collider / RigidBody 상태 변경 로직을 위임할 스크립트(오브젝트) => GameManager?

        public GameObject CloneToPooledItem(GameObject item, Transform player)
        {
            // 풀링 목록에 있는 아이템이면 풀링 배열로 되돌린다.
            if (item.transform.parent == transform)
            {
                ReturnItem(item);
            }
            
            GameObject getItem = Instantiate(item, player);
            return getItem;
        }
        
        
        private void Update()
        {
            SpawnEndTick += Time.deltaTime;

            // 시간이 되면 아이템이 스폰된다.
            if (SpawnEndTick >= SpawnTick)
            {
                SpawnEndTick = 0f;
                
                GameObject spawnItem = GetRandomPooledItem();
                
                if (spawnItem != null)
                {
                    SpawnItem(spawnItem);
                    ItemMove(spawnItem);
                }
            }
            
            
        }

        private void SpawnItem(GameObject spawnItem)
        {
            Vector3 SpawnPoint = SpawnSphere.transform.position + Random.onUnitSphere * SpawnSphereRadius ;

            spawnItem.transform.position = SpawnPoint;
            spawnItem.transform.forward = Vector3.forward; // World 기준 Z 방향 바라보게 하기
            spawnItem.SetActive(true);
            
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
            
            // 접선 원뿔의 반각 (sin = 안전지역 구 반지름 / 안전지역 구 중심까지 거리)
            float tangentAngle = Mathf.Asin(SafeSphereRadius / SpawnToSafeDir.magnitude);

            // SpawnToSafeDir에 수직이면서 위(아래)를 향하는 벡터
            Vector3 SafeToDot = Vector3.ProjectOnPlane(IsSpawnUpper ? Vector3.up : Vector3.down, SpawnToSafeDir).normalized;

            // 안전지역 구 중심 방향에서 tangentAngle만큼 위(아래)로 꺾은 접선 방향 (기존: 중심 방향 그대로라 SafeSphere 관통)
            Vector3 moveDir = Mathf.Cos(tangentAngle) * SpawnToSafeDir.normalized + Mathf.Sin(tangentAngle) * SafeToDot;

            // moveDir로 직진했을 때 SpawnSphere 반대편 표면과 만나는 지점
            // 시작점이 구 표면 위이므로 이동 거리 = -2 * Dot(구 중심 -> 시작점, moveDir)
            Vector3 spawnCenterToStart = startPos - SpawnSphere.transform.position;
            float moveDistance = -2f * Vector3.Dot(spawnCenterToStart, moveDir);
            Vector3 endPos = startPos + moveDir * moveDistance;

            // DOMove/DrawLine에 도착 위치(endPos) 전달 + 도착하면 비활성화해서 풀로 반환
            spawnItem.transform.DOMove(endPos, ItemMoveDuration).OnComplete(() => ReturnItem(spawnItem));
            Debug.DrawLine(startPos, endPos, Color.red, 5f);
        }

        // 스폰된 아이템 집을 때 DoTween 정지
        public void StopItemMove(GameObject item)
        {
            item.transform.DOKill();
        }
    }
}