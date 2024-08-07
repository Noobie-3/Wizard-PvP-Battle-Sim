using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour, IHitable
{
    // Serialized variables (Editor exposed)

    // Movement variables
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool Gravity;
    [SerializeField] private float JumpHeight = 5f;

    // Ground check variables
    [Header("Ground Check Settings")]
    [SerializeField] private float GroundCheck_Distance = 0.5f;
    [SerializeField] private Vector3 GroundCheck_Start = new Vector3(0, 0.5f, 0);

    // Wall check variables
    [Header("Wall Check Settings")]
    [SerializeField] private Vector3 WallCheck_Start = new Vector3(0, 3, 0);
    [SerializeField] private float wallCheckDistance = 1f;
    [SerializeField] private float DistanceFromFloorRequiredToWallRun = 3f;
    [SerializeField] private bool IsWallRunning = false;
    [SerializeField] private bool AbleToWallRun = false;

    // Camera settings
    [Header("Camera Settings")]
    [SerializeField] private Transform Cam;
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
    [SerializeField] private float StaminaRegenRate = 1;
    [SerializeField] private float StaminaRegenRateDefault = 1;
    public bool Charging = false;
    public float ChargeTime = 1f;

    // Input variables
    [Header("Input Settings")]
    [SerializeField] public Vector2 MoveInput;
    [SerializeField] public Vector2 MouseInput;
    [SerializeField] public bool Grounded;

    // Spell variables
    [Header("Spell Settings")]
    public List<Spell> currentSpells;
    [SerializeField] private int selectedSpellIndex = 0;
    public List<float> spellCooldownTimers;

    // Private variables
    private Rigidbody rb;
    private float rotationX = 0f;
    private float rotationY = 0f;
    [SerializeField] private GameObject PlayerUi;



    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            audioListener.enabled = true;
            Vcam.Priority = 1;

            if (PlayerUi != null)
                PlayerUi.SetActive(true);

            rb = GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
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
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        MoveObject();
        GroundCheck();
        WallCheck();
    }

    // Movement logic
    private void MoveObject()
    {
        if (Charging)
        {
            print("Charging: cannot move");
            return;
        }

        //Apply Gravity
        if (Gravity)
        {
            rb.AddForce(Vector3.down * gameController.GC.Gravity_force, ForceMode.Acceleration);
        }

        // Calculate movement direction
        Vector3 moveDirection = (Cam.right.normalized * MoveInput.x + Cam.forward.normalized * MoveInput.y);
        moveDirection.y = 0;

        // Apply movement
        rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);

        // Rotate the player towards camera forward
        if (MoveInput != Vector2.zero)
        {
            MeshToRotate.transform.rotation = Quaternion.LookRotation(moveDirection);
        }
        else if (Grounded)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    // Spell selection logic
    public void ScrollSpellSelection(float scrollInput)
    {
        if (!IsOwner) return;

        int newSpellIndex = selectedSpellIndex;
        if (scrollInput >= 0)
        {
            newSpellIndex--;
            if (newSpellIndex < 0)
                newSpellIndex = currentSpells.Count - 1;
        }
        else if (scrollInput <= 0)
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
        if (!IsOwner) return;

        if (selectedSpellIndex < 0 || selectedSpellIndex >= currentSpells.Count)
        {
            Debug.LogWarning("Spell index out of range");
            return;
        }

        if (spellCooldownTimers[selectedSpellIndex] <= 0)
        {
            CastSpellServerRpc(new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), transform.rotation, selectedSpellIndex, Cam.transform.forward);
            spellCooldownTimers[selectedSpellIndex] = currentSpells[selectedSpellIndex].CooldownDuration;
        }
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

        print(castedSpell.GetComponent<ISpell_Interface>().Caster.name + " is the name of the caster");

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
        if (context.performed && Grounded)
        {
            rb.AddForce(Vector3.up * JumpHeight, ForceMode.Impulse);
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

    [ServerRpc]
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
        }
    }
    // Ground check
    private void GroundCheck()
    {
        Grounded = Physics.Raycast(transform.position + GroundCheck_Start, -transform.up, GroundCheck_Distance, gameController.GC.GroundLayer);

        Debug.DrawRay(transform.position + GroundCheck_Start, -transform.up * GroundCheck_Distance, Color.red);
    }

    // Wall check
    private void WallCheck()
    {
        if (!IsOwner) return;
        if (MoveInput.x == 0 && MoveInput.y == 0) return;

        if (!Grounded)
        {
            if (!AbleToWallRun) return;

            RaycastHit hit;

            if (!Physics.Raycast(transform.position, -transform.up, DistanceFromFloorRequiredToWallRun, gameController.GC.GroundLayer))
            {
                if (Physics.Raycast(transform.position + WallCheck_Start, transform.right, out hit, wallCheckDistance, LayerMask.GetMask("Wall")) ||
                    Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, LayerMask.GetMask("Wall")) ||
                    Physics.Raycast(transform.position + WallCheck_Start, transform.forward, out hit, wallCheckDistance, LayerMask.GetMask("Wall")) ||
                    Physics.Raycast(transform.position, -transform.forward, out hit, wallCheckDistance, LayerMask.GetMask("Wall")))
                {
                    SetWallRunningServerRpc(true);
                    Debug.Log("Wall Running");
                }
                else
                {
                    SetWallRunningServerRpc(false);
                    Gravity = true;
                }

                if (IsWallRunning && AbleToWallRun)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    ReduceStaminaServerRpc(Time.deltaTime * 10);

                    if (Stamina.Value <= 0)
                    {
                        SetWallRunningServerRpc(false);
                        Gravity = true;
                        AbleToWallRun = false;
                    }
                }

                Vector3 hitDirection = hit.normal;
                if ((MoveInput.x > 0 && hitDirection == transform.right) ||
                    (MoveInput.x < 0 && hitDirection == -transform.right) ||
                    (MoveInput.y > 0 && hitDirection == transform.forward) ||
                    (MoveInput.y < 0 && hitDirection == -transform.forward))
                {
                    SetWallRunningServerRpc(false);
                }

                if (Vector3.Dot(transform.forward, hitDirection) > 0.5f)
                {
                    SetWallRunningServerRpc(false);
                }

                Debug.DrawRay(transform.position + WallCheck_Start, transform.right * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, -transform.right * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, transform.forward * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, -transform.forward * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, -transform.up * DistanceFromFloorRequiredToWallRun, Color.blue);

                Debug.Log(IsWallRunning + " wallrunning State");
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
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    [ServerRpc]
    private void Start_ServerRpc()
    {
        Debug.Log("ServerRpc started");
    }
}
