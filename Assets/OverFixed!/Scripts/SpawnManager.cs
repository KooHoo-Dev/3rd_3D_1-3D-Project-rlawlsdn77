using DG.Tweening;
using Jay;
using UnityEngine;

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

        // 마지막 생성 뒤 지난 시간. 생성 간격(1 / BaseSpawnRate)을 넘으면 하나 생성한다
        private float spawnTimer;

        // Items 배열과 같은 순서의 생성 가중치 (ItemTable의 SpawnWeight)
        private float[] itemSpawnWeights;

        protected override void Awake()
        {
            base.Awake();

            // 구 사이즈 구하기
            SpawnSphereRadius = SpawnSphere.transform.lossyScale.x * 0.5f;
            SafeSphereRadius = SafeSphere.transform.lossyScale.x * 0.5f;

            PooledItems = new GameObject[Items.Length * spawnConfig.PoolCount];
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
                for (int j = 0; j < spawnConfig.PoolCount; j++)
                {
                    GameObject SpawnItem = Instantiate(Items[i], transform);

                    SpawnItem.SetActive(false);
                    SpawnItem.GetComponent<Rigidbody>().isKinematic = true;
                    SpawnItem.transform.localScale = Vector3.one * spawnConfig.FlowItemScale;

                    PooledItems[i * spawnConfig.PoolCount + j] = SpawnItem;
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
            for (int j = 0; j < spawnConfig.PoolCount; j++)
            {
                GameObject item = PooledItems[itemIndex * spawnConfig.PoolCount + j];
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
            item.transform.localScale = Vector3.one * spawnConfig.FlowItemScale;
            item.GetComponent<Collider>().enabled = true;

            Rigidbody itemBody = item.GetComponent<Rigidbody>();
            itemBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            itemBody.isKinematic = true;

            item.SetActive(false);
        }
        
        // 잡은 아이템은 풀에서 빠지지 않는다. 풀 개수가 줄지 않아 생성 비율(ItemTable의 SpawnWeight)이 유지된다
        // 아이템을 잡을 때 호출. 풀 아이템이면 풀로 되돌리고, 같은 종류 프리팹의 새 인스턴스를 player 자식으로 만들어 돌려준다
        // 풀 인스턴스를 복제하면 꺼진 채로 생기고 OriginalScale(Awake에서 저장)이 풀 크기로 잡히므로 프리팹(Items)에서 복제한다
        // 풀 아이템이 아니면(바닥에 놓인 복제본) 그대로 돌려준다
        public GameObject CloneToPooledItem(GameObject item, Transform player)
        {
            if (item.transform.parent != transform) return item;

            ItemType itemId = item.GetComponent<ItemObject>().ItemID;
            ReturnItem(item);
            return Instantiate(Items[GetItemIndex(itemId)], player);
        }

        // Items 배열에서 itemId 종류의 위치
        private int GetItemIndex(ItemType itemId)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i].GetComponent<ItemObject>().ItemID == itemId) return i;
            }
            return -1;
        }
        
        
        private void Update()
        {
            spawnTimer += Time.deltaTime;

            // 생성 간격이 지나면 아이템이 스폰된다. 넘친 시간은 남겨서 초당 생성 개수를 맞춘다
            float spawnInterval = 1f / spawnConfig.BaseSpawnRate;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer -= spawnInterval;
                
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

            // 이동 시간 = 이동 거리 / FlowSpeed. Ease.Linear로 처음부터 끝까지 같은 속도로 흐른다 (DOTween 기본값은 점점 느려짐)
            // 도착하면 비활성화해서 풀로 반환
            float moveDuration = moveDistance / spawnConfig.FlowSpeed;
            spawnItem.transform.DOMove(endPos, moveDuration).SetEase(Ease.Linear).OnComplete(() => ReturnItem(spawnItem));
        }
    }
}