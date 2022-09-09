using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using MetaVerse.FrameWork;

namespace StarterAssets
{
    //
    // UnityEngine's new Input System
    //

    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")] protected Vector2 m_Movement;
        [HideInInspector] public bool playerControllerInputBlocked;

        public Vector2 move
        {
            get
            {
                if (playerControllerInputBlocked)
                    return Vector2.zero;
                else
                    return m_Movement;
            }
        }

        private Vector2 m_Camera;

        public Vector2 look
        {
            get
            {
                if (playerControllerInputBlocked)
                    return Vector2.zero;
                else
                    return m_Camera;
            }
        }

        private bool m_InputJump = false;

        public bool inputJump
        {
            get
            {
                return m_InputJump && !playerControllerInputBlocked;
            }
            set
            {
                m_InputJump = value;
            }
        }

        private bool m_InputSprit = false;

        public bool inputSprint
        {
            get { return m_InputSprit && !playerControllerInputBlocked; }
        }

        // Attack
        private bool m_InputAttackA;

        public bool inputAttackA
        {
            get { return m_InputAttackA && !playerControllerInputBlocked; }
        }

        private bool m_InputAttackB;

        public bool inputAttackB
        {
            get { return m_InputAttackB && !playerControllerInputBlocked; }
        }

        private bool m_InputAttackC;

        public bool inputAttackC
        {
            get { return m_InputAttackC && !playerControllerInputBlocked; }
        }

        [Header("Movement Settings")] public bool analogMovement;

        [Header("Mouse Cursor Settings")] public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        private string TAG = "StartAssetsInput";

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnAttack_Button_A(InputValue value)
        {
            AttackA(value.isPressed);
        }

        public void OnAttack_Button_B(InputValue value)
        {
            AttackB(value.isPressed);
        }

        public void OnAttack_Button_C(InputValue value)
        {
            AttackC(value.isPressed);
        }

#endif
        public void MoveInput(Vector2 newMoveDirection)
        {
            m_Movement = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            m_Camera = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            m_InputJump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            m_InputSprit = newSprintState;
        }

        public void AttackA(bool newstate)
        {
            m_InputAttackA = newstate;
        }

        public void AttackB(bool newstate)
        {
            m_InputAttackB = newstate;
        }

        public void AttackC(bool newstate)
        {
            m_InputAttackC = newstate;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

        public void ResetAttackInput()
        {
            m_InputAttackA = m_InputAttackB = m_InputAttackC;
        }
    }
}