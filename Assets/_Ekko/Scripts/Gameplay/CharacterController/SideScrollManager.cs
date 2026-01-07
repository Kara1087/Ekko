using System;
using UnityEngine;

namespace _Ekko.Scripts.Gameplay.CharacterController
{
    public class SideScrollManager : MonoBehaviour
    {
        public static SideScrollManager Instance { get; private set; }

        [SerializeField] private SideScrollDirection startDirection = SideScrollDirection.X;
    
        public SideScrollDirection CurrentDirection { get; private set; }
    
        public event Action<SideScrollDirection> OnDirectionChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        
            Instance = this;
            CurrentDirection = startDirection;
        }

        public void SetDirection(SideScrollDirection newDirection)
        {
            if (CurrentDirection == newDirection) return;
        
            CurrentDirection = newDirection;
            OnDirectionChanged?.Invoke(newDirection);
        }
    }
}