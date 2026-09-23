using UnityEngine;

namespace Scripts.Components
{
    struct MoveComponent
    {
        public bool InstaMove;
        public bool Started;
        public Vector2 StartPosition;
        public float ElapsedTime;
        public float Duration;
        public int TargetCell;
    }
}