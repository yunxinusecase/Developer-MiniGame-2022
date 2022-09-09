using System;
using System.Collections;
using System.Collections.Generic;
using MetaVerse.Animoji;
using MetaVerse.FrameWork;
using Mini.Battle.RTC;
using UnityEngine;
using StarterAssets;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using Mirror;
using Random = UnityEngine.Random;

namespace Mini.Battle.Core
{
    [RequireComponent(typeof(CharacterController))]
    //#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    //    [RequireComponent(typeof(PlayerInput))]
    //#endif
    public class NetPlayerController : NetworkBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 7.62f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // state controller
        private bool mStatusCanAttack = true;

        // animation state
        protected AnimatorStateInfo m_CurrentStateInfo; // Information about the base layer of the animator cached.
        protected AnimatorStateInfo m_NextStateInfo;
        protected bool m_IsAnimatorTransitioning;
        protected AnimatorStateInfo m_PreviousCurrentStateInfo; // Information about the base layer of the animator from last frame.
        protected AnimatorStateInfo m_PreviousNextStateInfo;
        protected bool m_PreviousIsAnimatorTransitioning;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDDeath;
        private int _animIDAttackA;
        private int _animIDAttackB;
        private int _animIDAttackC;
        private int _animIDHashBlockInput;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;
        private bool _hasAnimator;
        private string TAG = "BattlePlayer";
        public Transform cameraLookAtPoint;

        // TODO Debug Information
        // rtcchannelId:userid:xxx:xxx
        // Please Do not change by your own
        [SyncVar(hook = nameof(SetRTCData))]
        public string mStrRTCData = string.Empty;

        private void SetRTCData(string oldid, string newid)
        {
            YXUnityLog.LogInfo(TAG, $"oldid: {oldid}, newid: {newid}");
            string[] strs = newid.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length != 2)
            {
                YXUnityLog.LogError(TAG, "Fatal Error SetRTCData Error");
            }
            else
            {
                string chid = strs[0];
                string userid = strs[1];

                mRTCUserId = 0;
                ulong.TryParse(userid, out mRTCUserId);
                mRTCChannelId = chid;
            }
            YXUnityLog.LogInfo(TAG, $"set rtc data chid: {mRTCChannelId}, userid: {mRTCUserId}");
            // 当前Client连接创建完成，数据完成同步表示创建完毕
            OnNetPlayerCreated();
        }

        public ulong mRTCUserId = 0;
        public string mRTCChannelId = string.Empty;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        // Animoji Agent
        private AnimojiAgent mAnimojiAgent;

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            mAnimojiAgent = this.gameObject.GetComponent<AnimojiAgent>();
            mAnimojiAgent.SetUpAnimojiAgent();
        }

        public void PushFaceData(float[] datas, float[] bboxdata, float width, float height)
        {
            // YXUnityLog.LogInfo(TAG, "face data here:" + datas.Length);
            mAnimojiAgent.PushFaceData(datas, bboxdata, width, height);
        }

        private int inputActiveFlag = 2;
        private void Start()
        {
            YXUnityLog.LogInfo(TAG, $"Start called Local Player: {isLocalPlayer}");
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            // 如果是本机角色，设置输入和更新相机位置
            if (isLocalPlayer)
            {
                inputActiveFlag = 2;
                PlayerInput pi = gameObject.AddComponent<PlayerInput>();
                pi.actions = GameManager.Instance.mInputAssets;
                StarterAssetsInputs ss = gameObject.AddComponent<StarterAssetsInputs>();
                ss.cursorLocked = false;
                ss.analogMovement = false;
                ss.cursorInputForLook = true;
                pi.actions.Enable();
                _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                _playerInput = GetComponent<PlayerInput>();
                Debug.Log("use new input system for player controller");
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
                // SetUp Camera Follow Target
                //Transform followtarget = this.transform.Find("");
                EventManager.TriggerEvent(GameEventDefine.Event_SetMainCameraFollow, CinemachineCameraTarget.transform);
                EventManager.TriggerEvent(GameEventDefine.Event_SetInputAssets, ss, pi);
            }
            else
            {
            }

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            AssignAnimationIDs();
            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        // Called at the start of FixedUpdate to record the current state of the base layer of the animator.
        void CacheAnimatorState()
        {
            m_PreviousCurrentStateInfo = m_CurrentStateInfo;
            m_PreviousNextStateInfo = m_NextStateInfo;
            m_PreviousIsAnimatorTransitioning = m_IsAnimatorTransitioning;

            m_CurrentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            m_NextStateInfo = _animator.GetNextAnimatorStateInfo(0);
            m_IsAnimatorTransitioning = _animator.IsInTransition(0);
        }

        // Called after the animator state has been cached to determine whether this script should block user input.
        void UpdateInputBlocking()
        {
            bool inputBlocked = m_CurrentStateInfo.tagHash == _animIDHashBlockInput && !m_IsAnimatorTransitioning;
            inputBlocked |= m_NextStateInfo.tagHash == _animIDHashBlockInput;
            _input.playerControllerInputBlocked = inputBlocked;
        }

        private bool CheckLocalPlayerInputValid
        {
            get
            {
                return GameManager.Instance.EnablePlayerInput;
            }
        }

        // private void FixedUpdate()
        private void Update()
        {
            if (isLocalPlayer)
            {
                bool enableInput = CheckLocalPlayerInputValid;
                if (enableInput)
                {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                    if (inputActiveFlag > 0)
                    {
                        inputActiveFlag--;
                        _playerInput.enabled = false;
                    }
                    else
                    {
                        _playerInput.enabled = true;
                    }
#endif
                    _hasAnimator = TryGetComponent(out _animator);
                    CacheAnimatorState();
                    UpdateInputBlocking();
                    if (_hasAnimator)
                    {
                        _animator.ResetTrigger(_animIDAttackA);
                        _animator.ResetTrigger(_animIDAttackB);
                        _animator.ResetTrigger(_animIDAttackC);
                    }

                    JumpAndGravity();
                    GroundedCheck();
                    Move();
                    AttackLogic();
                }
            }
        }

        private void AttackLogic()
        {
            if (mStatusCanAttack && _hasAnimator)
            {
                int attackid = -1;
                if (_input.inputAttackA)
                    attackid = _animIDAttackA;
                else if (_input.inputAttackB)
                    attackid = _animIDAttackB;
                else if (_input.inputAttackC)
                    attackid = _animIDAttackC;
                if (attackid != -1)
                    _animator.SetTrigger(attackid);
            }
            _input.ResetAttackInput();
        }

        private void LateUpdate()
        {
            if (isLocalPlayer)
            {
                CameraRotation();
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDAttackA = Animator.StringToHash("AttackA");
            _animIDAttackB = Animator.StringToHash("AttackB");
            _animIDAttackC = Animator.StringToHash("AttackC");
            _animIDDeath = Animator.StringToHash("Death");
            _animIDHashBlockInput = Animator.StringToHash("BlockInput");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.inputSprint ? SprintSpeed : MoveSpeed;
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.inputJump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.inputJump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
            // Debug.Log("vertical v: " + _verticalVelocity.ToString());
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        // Mirror NetWork Content

        #region Start & Stop Callbacks
        /*
            Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
            API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
        */
        // NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer()
        {
            YXUnityLog.LogInfo(TAG, "OnStartServer");
        }

        /// <summary>
        /// Invoked on the server when the object is unspawned
        /// <para>Useful for saving object data in persistent storage</para>
        /// </summary>
        public override void OnStopServer()
        {
            YXUnityLog.LogInfo(TAG, "OnStopServer");
        }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient()
        {
            YXUnityLog.LogInfo(TAG, "OnStartClient");
        }

        /// <summary>
        /// This is invoked on clients when the server has caused this object to be destroyed.
        /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
        /// </summary>
        public override void OnStopClient()
        {
            YXUnityLog.LogInfo(TAG, "OnStopClient");
            // 当前Client从服务器退出
            OnNetPlayerDestroy();
        }

        /// <summary>
        /// Called when the local player object has been set up.
        /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            YXUnityLog.LogInfo(TAG, "OnStartLocalPlayer");
        }

        /// <summary>
        /// Called when the local player object is being stopped.
        /// <para>This happens before OnStopClient(), as it may be triggered by an ownership message from the server, or because the player object is being destroyed. This is an appropriate place to deactivate components or functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public override void OnStopLocalPlayer()
        {
            YXUnityLog.LogInfo(TAG, "OnStopLocalPlayer");
        }

        /// <summary>
        /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
        /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
        /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStartAuthority()
        {
            YXUnityLog.LogInfo(TAG, "OnStartAuthority");
        }

        /// <summary>
        /// This is invoked on behaviours when authority is removed.
        /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStopAuthority()
        {
            YXUnityLog.LogInfo(TAG, "OnStopAuthority");
        }

        #endregion

        // 角色实例化完毕
        private void OnNetPlayerCreated()
        {
            YXUnityLog.LogInfo(TAG, $"OnPlayerCreated rtcId: {mRTCUserId}, netId: {netId}");
            EventManager.TriggerEvent<ulong, NetPlayerController>(GameEventDefine.Event_OnNetPlayerCreated, mRTCUserId, this);
        }

        // 角色卸载完毕
        private void OnNetPlayerDestroy()
        {
            YXUnityLog.LogInfo(TAG, $"OnPlayerDestroy playerId: {mRTCUserId}, netId: {netId}");
            EventManager.TriggerEvent<ulong, NetPlayerController>(GameEventDefine.Event_OnNetPlayerDestroy, mRTCUserId, this);
        }

        private void OnDestroy()
        {
            YXUnityLog.LogInfo(TAG, $"OnDestroy player: {this.gameObject.name}");
        }

        // Animoji Agent
    }
}