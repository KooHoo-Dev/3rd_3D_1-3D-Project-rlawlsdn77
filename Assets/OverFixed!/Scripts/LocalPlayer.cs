using System;
using UnityEngine;

namespace OverFixed
{
    public class LocalPlayer : MonoBehaviour
    {
        [SerializeField] 
        private InteractionSphere interactionSphere;
        
        // 아이템 프리팹
        public GameObject Item { get; private set; }

        private void Start()
        {
            interactionSphere = GetComponentInChildren<InteractionSphere>();
        }

        private void OnEnable()
        {
            interactionSphere.InteractItem += TakeItem;
            interactionSphere.InteractTerminal += OnTerminal;
            interactionSphere.InteractPanel += FixPanel;
        }

        private void OnDisable()
        {
            
        }

        private void TakeItem()
        {
            
        }

        private void OnTerminal()
        {
            
        }

        private void FixPanel()
        {
            
        }
    }
}