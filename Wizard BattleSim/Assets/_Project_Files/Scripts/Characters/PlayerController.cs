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

[RequireComponent(typeof(Rigidbody), typeof(SpellCaster))]
public class PlayerController : NetworkBehaviour, IHitable
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
    [SerializeField] private GameObject MeshToRotate;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private bool JumpUsed = false;
    public float Health;
    public float MaxHealth;
    public float Mana;
    public float MaxMana;
    public float Stamina;
    public float MaxStamina;
    [SerializeField] private float StaminaRegenRate = 1f;
    [SerializeField] private float StaminaConsumptionRate = 10f;
    public bool Charging = false;
    public float ChargeTime = 1f;
    public bool CanDie = true;
    [SerializeField] private bool IsDebug = true;
    public bool isSpawned;

    public ulong MyClientID => NetworkObject.OwnerClientId;

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
    public List<float> spellCooldownTimers = new List<float>();
    public SpellCaster SpellCasterScript;
    public bool IsCasting = false;
    public float CastSpeed = 1f;
    public Coroutine CastIenum;
    public Vector3 CastDir;


    // Private variables
    private Rigidbody rb;
    private float rotationX = 0f;
    private float rotationY = 0f;
    [SerializeField] private NetworkVariable<Vector3> CurrentRotation;
    [SerializeField] private PlayerUI PlayerUi;
    [SerializeField] private Animator Anim;
    [SerializeField] private GameObject DevMenu;


    // Wall check and wall running logic
    private Vector3 currentWallNormal = Vector3.zero;
    

    public override void OnNetworkSpawn()
    {
        if (!IsHost && DevMenu != null)
        {
            Destroy(DevMenu);
        }


        if (IsOwner)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += this.OnSceneLoaded;

            DontDestroyOnLoad(gameObject);
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
            UpdateSpellCooldownTimers();
            StartCoroutine(PrintData());

        }
        else
        {
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

        SpellCooldown();
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
            rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);

            if (MoveInput == Vector2.zero && Grounded)
            {
                // If no input and grounded, stop horizontal movement
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
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

            rb.velocity = wallRunDirection * moveSpeed + Vector3.up * rb.velocity.y;
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
        MoveInput = context.ReadValue<Vector2>();
        IsRunning = MoveInput.x != 0 || MoveInput.y != 0;
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


    private void SpellCooldown()
    {
        for (int i = 0; i < spellCooldownTimers.Count; i++)
        {
            if (spellCooldownTimers[i] > 0)
            {
                spellCooldownTimers[i] -= Time.deltaTime;
            }
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

            if (IsWallRunning)
            {
                // Upon jumping off the wall, reset wall running state
                SetWallRunningServerRpc(false);
                AbleToWallRun = false; // Prevent immediate wall running again
            }
        }
    }


    // Spell management
    public void AddSpell(Spell newSpell)
    {
        currentSpells.Add(newSpell);
        UpdateSpellCooldownTimers();
        PlayerUi.UpdateUI();
    }

    public void RemoveSpell(Spell spellToRemove)
    {
        int index = currentSpells.IndexOf(spellToRemove);
        if (index >= 0)
        {
            currentSpells.RemoveAt(index);
            UpdateSpellCooldownTimers();
            PlayerUi.UpdateUI();

        }
    }

    private void UpdateSpellCooldownTimers()
    {
        spellCooldownTimers = new List<float>(new float[currentSpells.Count]);
    }

    // Health, Mana, Stamina handling
    public void GotHit(GameObject ThingThatHitMe, Spell spell, ulong casterID)
    {if(!IsOwner) return;

        //check to make sure not caster
        if (casterID == NetworkObject.OwnerClientId)
        {
            return;
        }


        else
        {
            GotHitServerRpc(spell.Spell_Damage, casterID);
        }
        PlayerUi.UpdateUI();

        print("Got hit by caster Id : " + casterID + " My Caster Id Is : " + NetworkObject.OwnerClientId);

    }

    [ServerRpc]
    public void GotHitServerRpc(float SpellDamage, ulong casterID)
    {
        Health -= SpellDamage;
        print("Player got hit by a spell from " + casterID + " for " + SpellDamage + " damage");

        if (Health <= 0 && CanDie)
        {
            print("Player( " + gameObject.name + " )has died");
        }
        //Update UI
        PlayerUi.UpdateUI();
    }



    // Ground check
    private void GroundCheck()
    {
        Grounded = Physics.Raycast(MeshToRotate.transform.position + GroundCheck_Start, Vector3.down, GroundCheck_Distance, gameController.GC.GroundLayer);

        if (Grounded)
        {
            JumpUsed = false;
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
            {
                // Perform raycasts in multiple directions to detect walls
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

                    // Determine if the wall is suitable for wall running (e.g., not too steep)
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
                {
                    // Zero out vertical velocity to prevent falling
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    ReduceStaminaServerRpc(Time.deltaTime * StaminaConsumptionRate);

                    if (Stamina <= 0)
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
        Stamina -= amount;
    }

    // Regeneration
    private IEnumerator ManaRegen()
    {
        while (true)
        {
            if (IsServer)
            {
                if (Mana < MaxMana)
                {
                    Mana += 1f;
                    Mana = Mathf.Min(Mana, MaxMana); // Clamp to MaxMana
                    PlayerUi.UpdateUI();


                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator StaminaRegen()
    {
        while (true)
        {
            if (IsServer)
            {
                if (Stamina < MaxStamina && !IsWallRunning)
                {
                    Stamina += StaminaRegenRate;
                    Stamina = Mathf.Min(Stamina, MaxStamina); // Clamp to MaxStamina
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

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if(!IsOwner) return;
        var spawnLocations = FindObjectsOfType<PlayerSpawnLocation>();

        for (int i = 0; i < spawnLocations.Length; ++i)
        {
            if (spawnLocations[i] == null)
            {
                return;
            }
            if (spawnLocations[i].CanSpawnPlayer && isSpawned == false)
            {
                isSpawned = true;
                rb.MovePosition(spawnLocations[i].transform.position);
                spawnLocations[i].CanSpawnPlayer = false;
                isSpawned = true;
                print("the player " + name + " has been spawned at " + spawnLocations[i].transform.position);
                print(transform.position);
                break;
            }

        }


    }



}
