using System;
using UnityEngine;

namespace CryptoQuest.Gameplay.Ship
{
    public enum ESailState
    {
        [Tooltip("All ship are available")]
        NotSail = 0,
        [Tooltip("Only one ship is available and it is sailing")]
        Sailing = 1,
        [Tooltip("Only one ship is available and it is landed in LastPosition.")]
        Landed = 2
    }

    public class ShipBus : ScriptableObject
    {
        public event Action Changed;

        [SerializeField] private SerializableVector2 _lastPosition;

        [SerializeField] private ESailState _currentSailState;
        
        [Tooltip("The ships will not spawn if this is false.")]
        [SerializeField] private bool _isShipActivated;

        public SerializableVector2 LastPosition
        {
            get => _lastPosition;
            set
            {
                if (value == LastPosition) return;
                _lastPosition = value;
                Changed?.Invoke();
            }
        }
        
        public ESailState CurrentSailState 
        {
            get => _currentSailState;
            set
            {
                if (_currentSailState == value) return;
                _currentSailState = value;
                Changed?.Invoke();
            }
        }

        public bool IsShipActivated 
        {
            get => _isShipActivated;
            set
            {
                if (_isShipActivated == value) return;
                _isShipActivated = value;
                Changed?.Invoke();
            }
        }
    }

    [Serializable]
    public class SerializableVector2 : IEquatable<SerializableVector2>
    {
        public float X;
        public float Y;
    
        public SerializableVector2(Vector3 vec3){
            X = vec3.x;
            Y = vec3.y;
        }
    
        public SerializableVector2(Vector2 vec2){
            X = vec2.x;
            Y = vec2.y;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, 0);
        }

        public Vector3 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public bool Equals(SerializableVector2 other)
        {
            return Mathf.Approximately(X, other.X) 
                && Mathf.Approximately(Y, other.Y);
        }
    }
}