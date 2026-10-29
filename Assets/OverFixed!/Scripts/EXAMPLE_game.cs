using System;
using System.Collections;
using NUnit.Framework;
using Study.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace OverFixed
{
    public class EXAMPLE_game : MonoBehaviour
    {
        // 슬라이더가 좌우로 이동한다.
        // 슬라이더의 특정 구역에 공간이 있다.
        // 그 공간 안에 Value가 진입했을 때 SPACE 바를 누르면 성공
        // 3번 성공 시 사고 해결
        
        // 유효 범위는 Mathf.Clamp로 0 ~ 1 사이로 고정되어야함 
        // 그 범위 안에서 validField가 정해짐
        
        // ex. 게임 시나리오
        // 1차에는 0.5 ~ 0.9 (0.4 범위)
        // 2차에는 0.0 ~ 0.3 (0.3 범위)
        // 3차에는 0.8 ~ 1.0 (0.2 범위)
        
        // 0 ~ 1 사이에서 범위를 초과하지 않는 범위를 구하는 법
        // 현재 범위가 0.4 이면 0 ~ 0.6 사이의 값 중 랜덤으로 구한다 (시작 범위를 기준으로 한다.)
        // 현재 범위가 0.2 이면 0 ~ 0.8 사이의 값 중 랜덤으로 구한다
        
        [Header("미니게임 패널")]
        [SerializeField] private GameObject gamePanel;

        
        [Header("성공 / 실패 텍스트")]
        [SerializeField] private GameObject textPanel;
        [SerializeField] private TMP_Text popUpText;

        [Header("슬라이더")]
        [SerializeField] private Slider gameSlider;

        [Header("슬라이더 Value 이동 속도")]
        [SerializeField] private float sliderSpeed = 1f;
        
        [Header("유효 공간 범위 : (0 ~ 1)")]
        [SerializeField] private float startValidField;
        [SerializeField] private float endValidField;
        [SerializeField] private float validValue = 0.4f;

        [Tooltip("성공 시마다 유효공간 범위에서 해당 weight 값만큼 뺀다.")]
        [Header("가중치 : (0 ~ 1)")] 
        [SerializeField] private float weight = 0.1f;
        
        // 성공 횟수
        public int SuccessCount { get; private set; } = 0;
        
        private void OnEnable()
        {
            textPanel.gameObject.SetActive(true);
            ResetGameData();
        }

        private void ResetGameData()
        {
            gameSlider.value = 0f;
            SuccessCount = 0;
            sliderSpeed = 1f;
            SetRandomValid();
        }

        private float currentTimer = 0;
        
        // 현재 개발 목적 : 일단 독립적인 게임 하나 만들기 (다른 게임에 의존성 없음)
        
        // 맨 처음에 게임에 진입하면 Random한 ValidValue 값 + 시작 / 끝 범위 지정
        // 성공 여부에 상관없이 SPACE 입력하면 계속 랜덤한 범위로 변경됨 + 성공 여부 Text 표시
        // 성공할 시 Weight 가중치가 적용된 범위로 Random이 적용됨 + 슬라이더 이동 속도 증가 + 성공 횟수 +1
        // 성공 횟수 3회 달성 시 게임 패널 SetActive(false);

        private void Update()
        {
            // 지속적으로 slider의 Value가 움직인다.
            currentTimer += Time.deltaTime * sliderSpeed;
            
            gameSlider.value = Mathf.PingPong(currentTimer, 1f);
            
            // Space 바를 눌렀을 때 gameSlider의 value가 유효 공간 범위 안에 있을 때
            if (SimpleInput.GetKeyDown(Key.Space))
            {
                if (CheckInput()) SuccessGame();

                // 결과 출력 + 랜덤 범위 재지정 (결과 상관없이 실행됨)
                popUpText.text = CheckInput() ? "성공" : "실패";
                SetRandomValid();
            }
        }

        private bool CheckInput()
        {
            // 범위 안에 value 있는지 확인 후 bool값 리턴
            if (gameSlider.value >= startValidField && gameSlider.value <= endValidField) return true;
            else return false;
        }

        private void SuccessGame()
        {
            // 게임 성공 시 로직들 
            validValue -= weight;   // 가중치 적용 
            SuccessCount++;         // 성공 카운트 ++
            sliderSpeed += 0.5f;    // 슬라이더 속도 증가
            
            // 성공 횟수 3회 달성 시 사고 해결 완료
            if (SuccessCount == 3)
            {
                // 게임 자체를 끈다 (Update 문도 안돌아감)
                gameObject.SetActive(false);
            }
        }
        
        
        
        private void SetRandomValid()
        {
            // 랜덤한 ValidValue 값 + start / end 필드값 설정
            // 순수하게 validValue 값으로 모든것을 결정한다.
            
            float value = Random.Range(0f, 1 - validValue);
            startValidField = value;
            endValidField = value + validValue;
        }
    }
}