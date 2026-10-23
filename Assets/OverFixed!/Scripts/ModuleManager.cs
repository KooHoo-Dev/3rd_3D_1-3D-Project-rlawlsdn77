using System;
using System.Reflection;
using Jay;
using Study.Utilities;
using UnityEngine;

namespace OverFixed
{
    // 모듈 : 함선에서 특별한 기능을 수행하는 정육면체 형태의 구역
    // 1. 모듈은 (홀수 * 홀수) 크기의 배열로 이루어져 있다. 배열 인덱스는 좌측 하단부터 시작한다.
    // 2. 중앙 모듈은 정 중앙에 위치해 있고,
    // 중앙 모듈을 제외한 추가 모듈을 배치하여 모듈끼리 연결 가능하다.
    // 3. 게임 시작 시 "보조 에어락"과 "납품소"가 기본적으로 지급된다 (각각 코어모듈의 위 / 아래 위치에 배정된다.)
    // 4. 중앙 모듈을 제외한 각 모듈은 내구도를 가지고 있으며
    // 함선 사고로 내구도가 감소한다. (운석 충돌은 추후 업데이트)
    public class ModuleManager : SingletonBase<ModuleManager>
    {
        [Header("설정값 (ModuleConfig로 조절 가능)")]
        [SerializeField] private ModuleConfig moduleConfig;
        
        public SpaceModule CoreModule { get; private set; }
        
        // 모듈 틀
        public SpaceModule[,] ModuleFrame { get; private set; }
        
        // 현재 모듈
        public SpaceModule[,] CurrentModules { get; private set; }
        
        
        private void SetModules()
        {
            ModuleFrame = new SpaceModule[moduleConfig.MaxWidth, moduleConfig.MaxHeight];
        }

        // 코어 모듈은 2차원 모듈 배열에서 항상 가운데에 존재한다.
        private void SetCoreModule()
        {
            // 모듈 중앙에 코어 모듈 저장
            ModuleFrame[moduleConfig.MaxWidth / 2, moduleConfig.MaxHeight / 2] = CoreModule;
        }

        private void ChoiceModule(SpaceModule module)
        {
            
        }
        
        private void UpdateModules()
        {
            
            for (int i = 0; i < moduleConfig.MaxWidth; i++)
            {
                for (int j = 0; j < moduleConfig.MaxWidth; j++)
                {
                    ModuleFrame[i, j] = null;
                }
            }
        }

        private void AddModule(GameObject module)
        {
            
        }

        private void RemoveModule(GameObject module)
        {
            
        }
    }
}
