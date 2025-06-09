using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode.Components;
using System.Threading;
using UnityEngine.Video;

[RequireComponent(typeof(PlayerStats))]
public class PlayerController : NetworkBehaviour
{


    // Movement variables
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float moveSpeedDefault;
    [SerializeField] public Vector3 moveDirection;
    [SerializeField] private bool Gravity = true;
    [SerializeField] private float JumpHeight = 5f;
    private NetworkVariable<Quaternion> syncedRotation = new NetworkVariable<Quaternion>(
    writePerm: NetworkVariableWritePermission.Owner
);


    // Ground check variables
    [Header("Ground Check Settings")]
    [SerializeField] private float GroundCheck_Distance = 0.5f;
    [SerializeField] private Vector3 GroundCheck_Start = new Vector3(0, 0.5f, 0);

    // Wall check variables
    [Header("Wall Check Settings")]
    [SerializeField] private Vector3 WallCheck_Start = new Vector3(0, 1.5f, 0);
    [SerializeField] private float wallCheckDistance = 1f;
    [SerializeField] private float DistanceFromFloorRequiredToWallRun = 1.5f;
    [SerializeField] private bool IsWallRunning = false;
    [SerializeField] private bool AbleToWallRun = true;

    // Camera settings
    [Header("Camera Settings")]
    [SerializeField] public Transform Cam;
    [SerializeField] public Camera camComponent;
    [SerializeField] private CinemachineVirtualCamera Vcam;

    // Miscellaneous settings
    [Header("Miscellaneous Settings")]
    [SerializeField] public Character CharacterChosen;
    [SerializeField] private GameObject MeshToRotate;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private bool JumpUsed = false;
    public bool Charging = false;
    public float ChargeTime = 1f;
    public bool CanDie = true;
    [SerializeField] private bool IsDebug = true;
    public bool isSpawned;
    public Transform WandSpawnLocation;
    public ulong MyClientID;
    public PlayerStats Stats; // Reference to PlayerStats
    // Input variables
    [Header("Input Settings")]
    [SerializeField] public Vector2 MoveInput;
    [SerializeField] public Vector2 MouseInput;
    [SerializeField] public bool Grounded;
    [SerializeField] public bool IsRunning;
    public bool CanRun = true;
    public InputActionAsset PlayerControls;
    public InputAction MoveAction;
    // Store the last valid cardinal rotation
    private Quaternion lastValidRotation;


    // Spell variables
    [Header("Spell Settings")]
    public List<Spell> currentSpells = new List<Spell>();
    [SerializeField] public int selectedSpellIndex = 0;
    public SpellCaster SpellCasterScript;
    public bool IsCasting = false;
    public float CastSpeed = 1f;
    public Coroutine CastIenum;


    // Private variables
    public Rigidbody rb;
    private float rotationX = 0f;
    private float rotationY = 0f;
    [SerializeField] private NetworkVariable<Vector3> CurrentRotation;
    [SerializeField] public PlayerUI PlayerUi;
    [SerializeField] public Animator Anim;
    [SerializeField] private GameObject DevMenu;


    // Wall check and wall running logic
    private Vector3 currentWallNormal = Vector3.zero;

    //Is called When the player is spawned on the network
    public override void OnNetworkSpawn()
    {


        if (IsOwner)
        {
            print("Player spawned with ownership: " + OwnerClientId);
            rb = GetComponent<Rigidbody>(); // Ensure Rigidbody is assigned
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            MoveAction = PlayerControls.FindAction("Move");
            MoveAction.Enable();
            MyClientID = OwnerClientId;
            camComponent.enabled = true;
            audioListener.enabled = true;
            Vcam.Priority = 1;
            if (PlayerUi != null)
                PlayerUi.gameObject.SetActive(true);
            PlayerUi.UpdateUI();
            rb = GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            // Initialize spell cooldown timers based on current spells
            StartCoroutine(PrintData());
        }
        else
        {
            camComponent.enabled = false;
            audioListener.enabled = false;
            Vcam.Priority = 0;
            PlayerUi.gameObject.SetActive(false);
        }

    }

    public void Start()
    {
        MoveAction = PlayerControls.FindAction("Move");
    }

    private void Update()
    {

        if (!IsOwner)
        {
            MeshToRotate.transform.rotation = Quaternion.Slerp(
                MeshToRotate.transform.rotation,
                syncedRotation.Value,
                Time.deltaTime * 10f
            );
            return;
        }

        if (!CanRun) return;

        if (Anim != null)
        {
            AnimateObject();

        }

    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (gameController.GC == null) return;

        MoveObject();
        GroundCheck();
        WallCheck();

        RotateCharacter();
        PlayerUi.UpdateUI();

        if (Charging)
        {
            Stats.ChargeManaServerRpc();
        }
    }
    // Movement logic
    private void MoveObject()
    {
        if (!IsOwner) return;
        print("running Move Object Object shoudl be moving in a direction");
        // Apply Gravity if enabled
        if (Gravity)
        {
            rb.AddForce(Vector3.down * gameController.GC.Gravity_force, ForceMode.Acceleration);
        }

        if (Charging) return;
        
        IsRunning = MoveInput.x != 0 || MoveInput.y != 0;

        moveDirection = Cam.right.normalized * MoveInput.x + Cam.forward.normalized * MoveInput.y;
        moveDirection.y = 0;
        // Apply movement
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        print("just SetRb velocity to " + rb.linearVelocity + "Move direction is " + moveDirection + "Move speed is " + moveSpeed);
        // If no input and grounded, stop horizontal movement
        if (MoveInput == Vector2.zero && Grounded)
        {

            print("MOve input  is zero");
            // If no input and grounded, stop horizontal movement
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
    /* else
     {
         // If wall running, adjust velocity to maintain wall run
         Vector3 wallRunDirection = Vector3.Cross(currentWallNormal, Vector3.up).normalized;
         // Determine if player is moving forward along the wall
         if (Vector3.Dot(wallRunDirection, Cam.forward) < 0)
         {
             wallRunDirection = -wallRunDirection;
         }
         rb.linearVelocity = wallRunDirection * moveSpeed + Vector3.up * rb.linearVelocity.y;

}*/

    //Rotation logic
    private void RotateCharacter()
    {
        if (!IsOwner || moveDirection == Vector3.zero) return;

        // Desired rotation based on movement direction
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        // Smooth rotation using Slerp or RotateTowards
        MeshToRotate.transform.rotation = Quaternion.Slerp(
            MeshToRotate.transform.rotation,
            targetRotation,
            Time.deltaTime * 10f // Adjust rotation speed
        );

        //sync to other clients if needed
        syncedRotation.Value = MeshToRotate.transform.rotation;
    }




    // Input handlers
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        print("Can run = " + CanRun);
        // Check if movement is allowed
        if (!CanRun)
        {
            /*            MoveInput = Vector2.zero; // Reset movement input
            */
            if (Anim != null)
            {
                Anim.SetFloat("X", 0);
                Anim.SetFloat("Y", 0);
            }
            return;
        }

        // Always read the input when CanRun is true
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Started)
        {
            MoveInput = context.ReadValue<Vector2>();
            AdjustCameraDamping();
            print("Move input is " + context.ReadValue<Vector2>());

            if (Anim != null)
            {
                Anim.SetFloat("X", MoveInput.x);
                Anim.SetFloat("Y", MoveInput.y);
            }
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            print(context.phase);
            // Reset movement on input cancellation
            MoveInput = Vector2.zero;

            if (Anim != null)
            {
                Anim.SetFloat("X", 0);
                Anim.SetFloat("Y", 0);
            }
        }
    }

    public void GetMouseInput(InputAction.CallbackContext context)
    {
        MouseInput = context.ReadValue<Vector2>();
    }
    public void GetChargeStatus(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Canceled)
        {
            CanRun = true;
            Charging = false;
            Anim.SetBool("IsCharging", false);
        }

        if (!Grounded) return;
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Started)
        {
            Charging = true;
            //stop movment
            CanRun = false;

            rb.linearVelocity = Vector3.zero;
            Anim.SetBool("IsCharging", true);
            //start charging
        }



    }
    // Jump logic
    public void JumpInput(InputAction.CallbackContext context)
    {
        //SpellCasterScript.ResetChargeSpell();
        if (context.performed && (Grounded || IsWallRunning))
        {
            SpellCasterScript.TristanCast();
            rb.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
            /*            if (IsWallRunning)
                        {
                            // Upon jumping off the wall, reset wall running state
            *//*                SetWallRunningServerRpc(false);
            *//*                AbleToWallRun = false; // Prevent immediate wall running again
                        }*/
        }
    }
    // Ground check
    private void GroundCheck()
    {
        if(!IsOwner) return;
        Grounded = Physics.Raycast(MeshToRotate.transform.position + GroundCheck_Start, Vector3.down, GroundCheck_Distance, gameController.GC.GroundLayer);
        if (Grounded)
        {
            JumpUsed = false;
        }
        if (Anim != null)
        {
            Anim.SetBool("Grounded", Grounded);
        }
        Debug.DrawRay(transform.position + GroundCheck_Start, Vector3.down * GroundCheck_Distance, Color.red);
    }
    private void WallCheck()
    {
        if (!IsOwner) return;
        if (MoveInput == Vector2.zero) return;
        if (!Grounded)
        {
            if (!AbleToWallRun) return;
            RaycastHit hit;
            // Check if player is high enough from the ground to wall run
            if (!Physics.Raycast(MeshToRotate.transform.position, Vector3.down, DistanceFromFloorRequiredToWallRun, gameController.GC.GroundLayer))
            {// Perform raycasts in multiple directions to detect walls
                if (Physics.Raycast(MeshToRotate.transform.position + WallCheck_Start, MeshToRotate.transform.right, out hit, wallCheckDistance, gameController.GC.WallLayer) ||
                    Physics.Raycast(MeshToRotate.transform.position + WallCheck_Start, -transform.right, out hit, wallCheckDistance, gameController.GC.WallLayer) ||
                    Physics.Raycast(MeshToRotate.transform.position + WallCheck_Start, transform.forward, out hit, wallCheckDistance, gameController.GC.WallLayer) ||
                    Physics.Raycast(MeshToRotate.transform.position + WallCheck_Start, -transform.forward, out hit, wallCheckDistance, gameController.GC.WallLayer) ||
                    Physics.Raycast(MeshToRotate.transform.position + WallCheck_Start, (MeshToRotate.transform.right + MeshToRotate.transform.forward).normalized, out hit, wallCheckDistance, gameController.GC.WallLayer) ||
                    Physics.Raycast(MeshToRotate.transform.position + WallCheck_Start, (-MeshToRotate.transform.right + MeshToRotate.transform.forward).normalized, out hit, wallCheckDistance, gameController.GC.WallLayer) ||
                    Physics.Raycast(MeshToRotate.transform.position + WallCheck_Start, (transform.right - MeshToRotate.transform.forward).normalized, out hit, wallCheckDistance, gameController.GC.WallLayer) ||
                    Physics.Raycast(MeshToRotate.transform.position + WallCheck_Start, (-transform.right - MeshToRotate.transform.forward).normalized, out hit, wallCheckDistance, gameController.GC.WallLayer))
                {
                    Vector3 wallNormal = hit.normal;
                    // Determine if the wall is suitable for wall running (not too steep)
                    if (Vector3.Dot(wallNormal, Vector3.up) < 0.1f)
                    {
                        currentWallNormal = wallNormal;
                        // Rotate the player to face 90 degrees along the wall
                        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up).normalized;
                        // Determine the correct direction based on player's input
                        if (Vector3.Dot(wallForward, Cam.forward) < 0)
                        {
                            wallForward = -wallForward;
                        }
                        Quaternion targetRotation = Quaternion.LookRotation(wallForward, Vector3.up);
                        MeshToRotate.transform.rotation = targetRotation;
                        /*                        SetWallRunningServerRpc(true);
                        */
                    }
                }
                else
                {
                    //  SetWallRunningServerRpc(false);
                    Gravity = true;
                }

                /*                if (IsWallRunning && AbleToWallRun)
                                {   // Zero out vertical velocity to prevent falling
                                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                                    ReduceStaminaServerRpc(Time.deltaTime * StaminaConsumptionRate);
                                    if (Stamina.Value <= 0)
                                    {
                                        SetWallRunningServerRpc(false);
                                        Gravity = true;
                                        AbleToWallRun = false;
                                    }
                                }
                                // Debugging raycasts
                                /*Debug.DrawRay(transform.position + WallCheck_Start, transform.right * wallCheckDistance, Color.blue);
                                Debug.DrawRay(transform.position + WallCheck_Start, -transform.right * wallCheckDistance, Color.blue);
                                Debug.DrawRay(transform.position + WallCheck_Start, transform.forward * wallCheckDistance, Color.blue);
                                Debug.DrawRay(transform.position + WallCheck_Start, -transform.forward * wallCheckDistance, Color.blue);
                                Debug.DrawRay(transform.position + WallCheck_Start, (transform.right + transform.forward).normalized * wallCheckDistance, Color.blue);
                                Debug.DrawRay(transform.position + WallCheck_Start, (-transform.right + transform.forward).normalized * wallCheckDistance, Color.blue);
                                Debug.DrawRay(transform.position + WallCheck_Start, (transform.right - transform.forward).normalized * wallCheckDistance, Color.blue);
                                Debug.DrawRay(transform.position + WallCheck_Start, (-transform.right - transform.forward).normalized * wallCheckDistance, Color.blue);
                                Debug.DrawRay(transform.position + WallCheck_Start, Vector3.down * DistanceFromFloorRequiredToWallRun, Color.blue);
                                PlayerUi.UpdateUI();*/
            }
        }
        /*        else
                {
                    SetWallRunningServerRpc(false);
                    AbleToWallRun = true;
                }
        */
    }




    // Animation handling
    private void AnimateObject()
    {
        if (!IsOwner) return;
        if (Anim == null) return;
        // Update animation parameters
        Anim.SetBool("IsRunning", IsRunning);
        //Anim.SetBool("IsCharging", Charging);
        Anim.SetBool("IsWallRunning", IsWallRunning);
    }
    public IEnumerator PrintData()
    {
        if (!IsOwner) yield return null;
        while (IsDebug)
        {
            yield return new WaitForSeconds(5f);
        }
    }


private void AdjustCameraDamping()
{
        if (MoveInput.y >= 0 || Vcam == null)
        {
            Vcam.GetCinemachineComponent<CinemachineFramingTransposer>().m_ZDamping = 0.6f;
            return;
        }
    print("Adjusting camera damping" + MoveInput.y + " " + MoveInput.x + " " + MoveInput.magnitude);
        // Get the player's velocity in camera space
        // If player is moving toward the camera (negative Z)
        if(Vcam == null) return;
        Vcam.GetCinemachineComponent<CinemachineFramingTransposer>().m_ZDamping = 0;
}

    private void OnParticleCollision(GameObject other)
    {
        if(!IsOwner) return;
        print("Collider with a partical" + other.name);
        ISpell_Interface spell_Interface = other.GetComponent<ISpell_Interface>();
        if (spell_Interface == null)
        {
            print("No spell interface found on " + other.name);
            return;
        }
        if (spell_Interface.CasterId == this.OwnerClientId)
        {
            print("Caster is the same as the player");
            return;
        }
        if (spell_Interface.hitagainTime > 0)
        {
            print("Hit again time is not 0");
            return;
        }
        TakeDamage(spell_Interface.spell, spell_Interface.CasterId);
        spell_Interface.hitagainTime = spell_Interface.spell.MultiHitCooldown;
        print("Should have taken damage from " + other.name);
    }

    public void TakeDamage(Spell spell, ulong whoHitMe)
    {
        print(name + "PLayer took dmaage from" + spell + " Cast by  " + CharacterChosen.DisplayName);
        Stats.TakeDamage(spell.Spell_Damage, whoHitMe);
        PlayerUi.UpdateUI();


    }
}