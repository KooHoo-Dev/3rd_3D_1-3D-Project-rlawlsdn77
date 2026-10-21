using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace OverFixed
{
    // InteractionSphere Collider 안에 있는 물체의 Tag를 판단하는 건 InteractionSphere이,
    // Tag에 따른 특정 행동을 수행하는 것은 LocalPlayer이 담당한다.
    
    public class InteractionSphere : MonoBehaviour
    {
        public Action InteractItem;
        public Action InteractTerminal;
        public Action InteractPanel;
        
        [SerializeField] private LocalPlayer localPlayer;
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.contactCount == 0) return;
            
            if (collision.gameObject.CompareTag("Item"))
            {
                
            }

            if (collision.gameObject.CompareTag("Terminal"))
            {
                
            }

            if (collision.gameObject.CompareTag("Panel"))
            {
                
            }
        }
    }
}