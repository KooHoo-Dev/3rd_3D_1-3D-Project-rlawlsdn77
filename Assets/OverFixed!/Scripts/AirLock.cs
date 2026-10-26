using System;
using UnityEngine;

namespace OverFixed
{
    public class AirLock : MonoBehaviour
    {
        [Header("AirLock Transform")]
        [SerializeField] private Transform EntryTransform;
        [SerializeField] private Transform ExitTransform;

        [SerializeField] private Transform PlayerTransform;
        
        public void ToExit()
        {
            PlayerTransform.position = ExitTransform.position;
        }

        public void ToEntry()
        {
            PlayerTransform.position = EntryTransform.position;
        }
    }
}