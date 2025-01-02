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

public class PlayerController : NetworkBehaviour
{

    // Movement variables
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float moveSpeedDefault;
    [SerializeField] Vector3 moveDirection;
    [SerializeField] private bool Gravity = true;
    [SerializeField] private float JumpHeight = 5f;

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
    public Vector3 CameraRotation;
    [SerializeField] private float CamSpeed = 1f;
    [SerializeField] private float RotateSpeed = 1f;
    [SerializeField] private float minXRotation = -45f;   // Minimum X rotation (pitch)
    [SerializeField] private float maxXRotation = 45f;    // Maximum X rotation (pitch)

    // Miscellaneous settings
    [Header("Miscellaneous Settings")]
    [SerializeField] public Character CharacterChosen;
    [SerializeField] private GameObject MeshToRotate;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private bool JumpUsed = false;
    public NetworkVariable<float> Health = new NetworkVariable<float>(100f); // Example initial value
    public float MaxHealth;
    public NetworkVariable<float> Mana = new NetworkVariable<float>(100f); // Example initial value
    public float MaxMana;
    public NetworkVariable<float> Stamina = new NetworkVariable<float>(100f); // Example initial value
    public float MaxStamina;
    [SerializeField] private float StaminaRegenRate = 1f;
    [SerializeField] private float StaminaConsumptionRate = 10f;
    public bool Charging = false;
    public float ChargeTime = 1f;
    public bool CanDie = true;
    [SerializeField] private bool IsDebug = true;
    public bool isSpawned;
    public Transform WandSpawnLocation;
    public ulong MyClientID;

    // Input variables
    [Header("Input Settings")]
    [SerializeField] public Vector2 MoveInput;
    [SerializeField] public Vector2 MouseInput;
    [SerializeField] public bool Grounded;
    [SerializeField] public bool IsRunning;



    // Spell variables
    [Header("Spell Settings")]
    public List<Spell> currentSpells = new List<Spell>();
    [SerializeField] public int selectedSpellIndex = 0;
    public SpellCaster SpellCasterScript;
    public bool IsCasting = false;
    public float CastSpeed = 1f;
    public Coroutine CastIenum;
    public Vector3 CastDir;


    // Private variables
    public Rigidbody rb;
    private float rotationX = 0f;
    private float rotationY = 0f;
    [SerializeField] private NetworkVariable<Vector3> CurrentRotation;
    [SerializeField] public PlayerUI PlayerUi;
    [SerializeField] private Animator Anim;
    [SerializeField] private GameObject DevMenu;


    // Wall check and wall running logic
    private Vector3 currentWallNormal = Vector3.zero;
    
    //Is called When the player is spawned on the network
    public override void OnNetworkSpawn()
    {

        if (!IsHost && DevMenu != null)
        {
            Destroy(DevMenu);
        }


        if (IsOwner)
        {
            print("My location is " + transform.position + name);
            MyClientID = OwnerClientId;
            camComponent.enabled = false;
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
            Destroy(camComponent);
            audioListener.enabled = false;
            Vcam.Priority = 0;
            PlayerUi.gameObject.SetActive(false);
        }

        if (IsServer)
        {

            StartCoroutine(ManaRegen());
            StartCoroutine(StaminaRegen());

            
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        AnimateObject();
        
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if(gameController.GC == null) return;
        MoveObject();
        GroundCheck();
        WallCheck();
        if (MoveInput != Vector2.zero)
        {
            MeshToRotate.transform.rotation = Quaternion.LookRotation(moveDirection);
        }
        PlayerUi.UpdateUI();
    }
    // Movement logic
    private void MoveObject()
    {
        if (Charging)
        {
            // If charging a spell, prevent movement
            return;
        }
        // Apply Gravity if enabled
        if (Gravity)
        {
            rb.AddForce(Vector3.down * gameController.GC.Gravity_force, ForceMode.Acceleration);
        }
        // Calculate movement direction
        if (!IsWallRunning)
        {
            moveDirection = Cam.right.normalized * MoveInput.x + Cam.forward.normalized * MoveInput.y;
            moveDirection.y = 0;
            // Apply movement
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
            if (MoveInput == Vector2.zero && Grounded)
            {
                // If no input and grounded, stop horizontal movement
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
        else
        {
            // If wall running, adjust velocity to maintain wall run
            Vector3 wallRunDirection = Vector3.Cross(currentWallNormal, Vector3.up).normalized;
            // Determine if player is moving forward along the wall
            if (Vector3.Dot(wallRunDirection, Cam.forward) < 0)
            {
                wallRunDirection = -wallRunDirection;
            }
            rb.linearVelocity = wallRunDirection * moveSpeed + Vector3.up * rb.linearVelocity.y;
        }
    }
    //Rotation logic
    [ServerRpc]
    private void RotateObjectServerRpc(Vector3 moveDirection)
    {
        RotateObjectClientRpc(moveDirection);
    }
    [ClientRpc]
    private void RotateObjectClientRpc(Vector3 moveDirection)
    {
        if (MeshToRotate != null)
        {
            MeshToRotate.transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
    // Input handlers
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        MoveInput = context.ReadValue<Vector2>();
        IsRunning = MoveInput.x != 0 || MoveInput.y != 0;
        if(Anim != null)
        {
            Anim.SetFloat("X", MoveInput.x);
            Anim.SetFloat("Y", MoveInput.y);
        }
    }
    public void GetMouseInput(InputAction.CallbackContext context)
    {
        MouseInput = context.ReadValue<Vector2>();
    }
    public void GetChargeStatus(InputAction.CallbackContext context)
    {
        if (!Grounded) return;
        Charging = context.ReadValueAsButton();
    }
    // Jump logic
    public void JumpInput(InputAction.CallbackContext context)
    {        
        //SpellCasterScript.ResetChargeSpell();
        if (context.performed && (Grounded || IsWallRunning))
        {
            SpellCasterScript.TristanCast();            
            rb.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
            if (IsWallRunning)
            {
                // Upon jumping off the wall, reset wall running state
                SetWallRunningServerRpc(false);
                AbleToWallRun = false; // Prevent immediate wall running again
            }
        }
    }
    // Ground check
    private void GroundCheck()
    {
        Grounded = Physics.Raycast(MeshToRotate.transform.position + GroundCheck_Start, Vector3.down, GroundCheck_Distance, gameController.GC.GroundLayer);
        if (Grounded)
        {
            JumpUsed = false;
        }
        Anim.SetBool("Grounded", Grounded);
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
                        SetWallRunningServerRpc(true);
                    }
                }
                else
                {
                    SetWallRunningServerRpc(false);
                    Gravity = true;
                }

                if (IsWallRunning && AbleToWallRun)
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
                Debug.DrawRay(transform.position + WallCheck_Start, transform.right * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, -transform.right * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, transform.forward * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, -transform.forward * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, (transform.right + transform.forward).normalized * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, (-transform.right + transform.forward).normalized * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, (transform.right - transform.forward).normalized * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, (-transform.right - transform.forward).normalized * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, Vector3.down * DistanceFromFloorRequiredToWallRun, Color.blue);
                PlayerUi.UpdateUI();
            }
        }
        else
        {
            SetWallRunningServerRpc(false);
            AbleToWallRun = true;
        }
    }
    [ServerRpc]
    private void SetWallRunningServerRpc(bool isWallRunning)
    {
        IsWallRunning = isWallRunning;
        if (!IsWallRunning)
        {
            // Reset rotation to face the direction of the camera smoothly
            Quaternion targetRotation = Quaternion.Euler(0, Cam.eulerAngles.y, 0);
            MeshToRotate.transform.rotation = Quaternion.Slerp(MeshToRotate.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
    [ServerRpc]
    private void ReduceStaminaServerRpc(float amount)
    {
        Stamina.Value -= amount;
    }
    // Regeneration make it one function later
    private IEnumerator ManaRegen()
    {
        while (true)
        {
            if (IsServer)
            {
                if (Mana.Value < MaxMana)
                {
                    Mana.Value  += 1f;
                    Mana.Value = Mathf.Min(Mana.Value, MaxMana); // Clamp to MaxMana
                }
            }
            yield return new WaitForSeconds(1f);
            if(!IsOwner) yield return new WaitForSeconds(0f);
            PlayerUi.UpdateUI();
        }
    }

    private IEnumerator StaminaRegen()
    {
        while (true)
        {
            if (IsServer)
            {
                if (Stamina.Value < MaxStamina && !IsWallRunning)
                {
                    Stamina.Value += StaminaRegenRate;
                    Stamina.Value= Mathf.Min(Stamina.Value, MaxStamina); // Clamp to MaxStamina
                }
            }
            PlayerUi.UpdateUI();
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Animation handling
    private void AnimateObject()
    {
        if (!IsOwner) return;
        if (Anim == null) return;
        // Update animation parameters
        Anim.SetBool("IsRunning", IsRunning);
        Anim.SetBool("IsCharging", Charging);
        Anim.SetBool("IsWallRunning", IsWallRunning);
    }
    public IEnumerator PrintData()
    {
        if (!IsOwner) yield return null;
        while (IsDebug)
        {
           yield  return new WaitForSeconds(5f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DieServerRpc(ulong Hitter, ulong Sender)
    {
        print("Sender is " + Sender + "Hitter is " + Hitter);
        if(!IsServer) return;
        if (WinTracker.Singleton != null)
        {

            for (int i = 0; i < gameController.GC.Players.Length; i++)
            {
                if (gameController.GC.Players[i].OwnerClientId == Hitter)
                {
                    WinTracker.Singleton.AddWin(Hitter);

                    if (WinTracker.Singleton.CheckWin(Hitter))
                    {
                        WinTracker.Singleton.EndGame();
                    }
                    print("Win added");
                }

            }
            print("Win added");
        }

        Destroy(transform.root.gameObject);

    }

    private void OnParticleCollision(GameObject other)
    {
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
        if(spell_Interface.hitagainTime > 0)
        {
            print("Hit again time is not 0");
            return;
        }
        TakeDamage(spell_Interface.spell, spell_Interface.CasterId);
        spell_Interface.hitagainTime = spell_Interface.spell.MultiHitCooldown;
        print("Should have taken damage from " + other.name);
    }

    public void TakeDamage(Spell spell, ulong whoHitMeName)
    {
        print(name + "PLayer took dmaage from" + spell + " Cast by  " + CharacterChosen.DisplayName);
        TakeDamageServerRpc(spell.Spell_Damage);
        PlayerUi.UpdateUI();
        if (Health.Value > 0)
        {
            print("Health is not low enough for death on " + name);
            return;
        }
        if (CanDie)
        {
            DieServerRpc(whoHitMeName, this.OwnerClientId);
            print("called Die On " + CharacterChosen.DisplayName);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float Damage)
    {
        if(!IsServer) return;
        Health.Value -= Damage;
        PlayerUi.UpdateUI();

    }
}
