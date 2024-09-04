using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.UIElements;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour, IHitable
{
    // Serialized variables (Editor exposed)

    // Movement variables
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int moveSpeedDefault;
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
    [SerializeField] private Transform Cam;
    [SerializeField] private Camera camComponent;
    [SerializeField] private CinemachineVirtualCamera Vcam;
    [SerializeField] private float CamSpeed = 1f;
    [SerializeField] private float RotateSpeed = 1f;
    [SerializeField] private float minXRotation = -45f;   // Minimum X rotation (pitch)
    [SerializeField] private float maxXRotation = 45f;    // Maximum X rotation (pitch)

    // Miscellaneous settings
    [Header("Miscellaneous Settings")]
    [SerializeField] private GameObject MeshToRotate;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private bool JumpUsed = false;
    public NetworkVariable<float> Health = new NetworkVariable<float>(100);
    public NetworkVariable<float> MaxHealth = new NetworkVariable<float>(100);
    public NetworkVariable<float> Mana = new NetworkVariable<float>(100);
    public NetworkVariable<float> MaxMana = new NetworkVariable<float>(100);
    public NetworkVariable<float> Stamina = new NetworkVariable<float>(50);
    public NetworkVariable<float> MaxStamina = new NetworkVariable<float>(50);
    [SerializeField] private float StaminaRegenRate = 1f;
    [SerializeField] private float StaminaConsumptionRate = 10f;
    public bool Charging = false;
    public float ChargeTime = 1f;

    // Input variables
    [Header("Input Settings")]
    [SerializeField] public Vector2 MoveInput;
    [SerializeField] public Vector2 MouseInput;
    [SerializeField] public bool Grounded;
    [SerializeField] public bool IsRunning;

    // Spell variables
    [Header("Spell Settings")]
    public List<Spell> currentSpells = new List<Spell>();
    [SerializeField] private int selectedSpellIndex = 0;
    public List<float> spellCooldownTimers = new List<float>();
    public float CastSpeed = 1f;
    public bool IsCasting = false;
    [SerializeField] private GameObject CastTimeUi;
    [SerializeField] private string CastSpellChargeText;
    [SerializeField] private float CastTimeProgress;
    [SerializeField] private UnityEngine.UI.Image CastTimeProgressUI;


    // Private variables
    private Rigidbody rb;
    private float rotationX = 0f;
    private float rotationY = 0f;
    [SerializeField] private Vector3 CurrentRotation;
    [SerializeField] private GameObject PlayerUi;
    [SerializeField] private Animator Anim;

    // Wall check and wall running logic
    private Vector3 currentWallNormal = Vector3.zero;
    

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            DontDestroyOnLoad(gameObject);
            camComponent.enabled = false;
            camComponent.enabled = true;

            audioListener.enabled = true;
            Vcam.Priority = 1;

            if (PlayerUi != null)
                PlayerUi.SetActive(true);

            rb = GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Initialize spell cooldown timers based on current spells
            UpdateSpellCooldownTimers();
        }
        else
        {
            audioListener.enabled = false;
            Vcam.Priority = 0;
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
            RotateObjectServerRpc(moveDirection);
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

    // Spell selection logic
    public void ScrollSpellSelection(float scrollInput)
    {
        if (!IsOwner) return;

        int newSpellIndex = selectedSpellIndex;
        if (scrollInput > 0)
        {
            newSpellIndex--;
            if (newSpellIndex < 0)
                newSpellIndex = currentSpells.Count - 1;
        }
        else if (scrollInput < 0)
        {
            newSpellIndex++;
            if (newSpellIndex >= currentSpells.Count)
                newSpellIndex = 0;
        }

        SetSelectedSpellIndexServerRpc(newSpellIndex);
    }

    [ServerRpc]
    private void SetSelectedSpellIndexServerRpc(int newSpellIndex)
    {
        selectedSpellIndex = newSpellIndex;
        Debug.Log("Selected Spell: " + selectedSpellIndex + " out of " + currentSpells.Count);
    }

    public void SelectSpellWithKeyBoard(int spellIndex)
    {
        if (!IsOwner) return;

        if (spellIndex >= 0 && spellIndex < currentSpells.Count)
        {
            SetSelectedSpellIndexServerRpc(spellIndex);
            Debug.Log("Selected Spell: " + selectedSpellIndex + " out of " + currentSpells.Count);
        }
        else
        {
            Debug.LogWarning("Spell index out of range");
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

    // Spell casting logic
    public void CastSpell()
    {
        if (!IsOwner) return ;

        if (selectedSpellIndex < 0 || selectedSpellIndex >= currentSpells.Count)
        {
            Debug.LogWarning("Spell index out of range");
            return;
        }

        
        if (spellCooldownTimers[selectedSpellIndex] <= 0 && IsCasting == false)
        {
            IsCasting = true;
            StartCoroutine(ChargeSpell());
        }
        return;
    }

    [ServerRpc]
    private void CastSpellServerRpc(Vector3 position, Quaternion rotation, int spellIndex, Vector3 shotDir)
    {
        if (spellIndex < 0 || spellIndex >= currentSpells.Count)
        {
            Debug.LogWarning("Spell index out of range");
            return;
        }

        if (Mana.Value < currentSpells[spellIndex].ManaCost)
        {
            // Play "cannot cast" sound and animation
            return;
        }

        Spell spellUsed = currentSpells[spellIndex];
        GameObject castedSpell = Instantiate(spellUsed.Spell_Prefab, position, rotation);
        castedSpell.GetComponent<NetworkObject>().Spawn();
        Mana.Value -= spellUsed.ManaCost;
        castedSpell.GetComponent<ISpell_Interface>().Caster = gameObject;

        Rigidbody spellRb = castedSpell.GetComponent<Rigidbody>();
        spellRb.velocity = shotDir * spellUsed.Spell_Speed;
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
        if (context.performed && (Grounded || IsWallRunning))
        {
            IsCasting = false;
            StopCoroutine(ChargeSpell());
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
    }

    public void RemoveSpell(Spell spellToRemove)
    {
        int index = currentSpells.IndexOf(spellToRemove);
        if (index >= 0)
        {
            currentSpells.RemoveAt(index);
            UpdateSpellCooldownTimers();
        }
    }

    private void UpdateSpellCooldownTimers()
    {
        spellCooldownTimers = new List<float>(new float[currentSpells.Count]);
    }

    // Health, Mana, Stamina handling
    public void GotHit(GameObject self, Spell spell, GameObject caster)
    {
        Debug.Log($"GotHit called on {gameObject.name} by {caster.name} with {spell.name}");
        GotHitServerRpc(spell.Spell_Damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GotHitServerRpc(float damage)
    {
        Debug.Log($"GotHitServerRpc called on {gameObject.name} with damage: {damage}");
        Health.Value -= damage;
        Debug.Log($"Updated Health: {Health.Value}");

        if (Health.Value <= 0)
        {
            // Play death animation and sound
            Debug.Log($"Player {gameObject.name} should be killed.");

            // Respawn player
            Health.Value = MaxHealth.Value;
            Mana.Value = MaxMana.Value;
            Stamina.Value = MaxStamina.Value;

            // Implement respawn logic here (e.g., move player to spawn point)
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

    // Regeneration
    private IEnumerator ManaRegen()
    {
        while (true)
        {
            if (IsServer)
            {
                if (Mana.Value < MaxMana.Value)
                {
                    Mana.Value += 1f;
                    Mana.Value = Mathf.Min(Mana.Value, MaxMana.Value); // Clamp to MaxMana
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
                if (Stamina.Value < MaxStamina.Value && !IsWallRunning)
                {
                    Stamina.Value += StaminaRegenRate;
                    Stamina.Value = Mathf.Min(Stamina.Value, MaxStamina.Value); // Clamp to MaxStamina
                }
            }
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

    public IEnumerator ChargeSpell()
    {
        while (IsCasting)
        {
            moveSpeed = moveSpeedDefault / 2;

            yield return new WaitForSeconds(CastSpeed);

            Vector3 spawnPosition = transform.position + Vector3.up * 2; // Adjust spawn position as needed
            Vector3 shotDirection = Cam.forward;
            CastSpellServerRpc(spawnPosition, transform.rotation, selectedSpellIndex, shotDirection);
            spellCooldownTimers[selectedSpellIndex] = currentSpells[selectedSpellIndex].CooldownDuration;
             IsCasting = false;
            if (CastTimeUi != null)
            {
                CastTimeUi.SetActive(false);
            }
        }

        moveSpeed = moveSpeedDefault;
    }

    public IEnumerator ChargeSpellUI()
    {
        if (CastTimeUi != null)
        {
            CastTimeUi.SetActive(true);
        }

        CastTimeProgress += Time.deltaTime;
        CastTimeProgressUI.fillAmount = CastTimeProgress / ChargeTime;
        CastSpellChargeText = "Casting: " + currentSpells[selectedSpellIndex].Spell_Name + " (" + (int)(CastTimeProgress * 100 / ChargeTime) + "%)";
        yield return new WaitForSeconds(.06f);
    }
}
