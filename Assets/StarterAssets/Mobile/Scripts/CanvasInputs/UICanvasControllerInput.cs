using Mini.Battle.Core;
using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {
        [Header("Output")] private StarterAssetsInputs starterAssetsInputs;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs?.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs?.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs?.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            starterAssetsInputs?.SprintInput(virtualSprintState);
        }

        public void VirtualAttackAInput(bool state)
        {
            starterAssetsInputs?.AttackA(state);
        }

        public void VirtualAttackBInput(bool state)
        {
            starterAssetsInputs?.AttackB(state);
        }

        public void VirtualAttackCInput(bool state)
        {
            starterAssetsInputs?.AttackC(state);
        }

        public void SetPlayerInput(StarterAssetsInputs input)
        {
            starterAssetsInputs = input;
        }
    }
}
