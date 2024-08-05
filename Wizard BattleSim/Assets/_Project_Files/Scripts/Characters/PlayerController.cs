using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
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
    [SerializeField] private float wallRunTimerDefault = 4.5f;
    [SerializeField] private float wallRunTimer_Tick = 4.5f;
    [SerializeField] private float DistanceFromFloorRequiredToWallRun = 3f;
    [SerializeField] private bool IsWallRunning = false;

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

    // Public variables

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

    // Rigidbody
    private Rigidbody rb;

    // Rotation variables
    private float rotationX = 0f;
    private float rotationY = 0f;

    // Unity methods
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        UpdateSpellCooldownTimers();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            audioListener.enabled = true;
            Vcam.Priority = 1;
        }
        else
        {
            audioListener.enabled = false;
            Vcam.Priority = 0;
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

        if (scrollInput >= 0)
        {
            selectedSpellIndex--;
            if (selectedSpellIndex < 0)
                selectedSpellIndex = currentSpells.Count - 1;
        }
        else if (scrollInput <= 0)
        {
            selectedSpellIndex++;
            if (selectedSpellIndex > currentSpells.Count - 1)
                selectedSpellIndex = 0;
        }

        Debug.Log("Selected Spell: " + selectedSpellIndex + " out of " + currentSpells.Count);
    }

    public void SelectSpellWithKeyBoard(int spellIndex)
    {
        if (!IsOwner) return;

        if (spellIndex >= 0 && spellIndex < currentSpells.Count)
        {
            selectedSpellIndex = spellIndex;
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

    // Spell casting logic
    public void CastSpell()
    {
        if (selectedSpellIndex < 0 || selectedSpellIndex >= currentSpells.Count)
        {
            Debug.LogWarning("Spell index out of range");
            return;
        }

        if (spellCooldownTimers[selectedSpellIndex] <= 0)
        {
            if (!IsOwner) return;
            CastSpellServerRpc(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), transform.rotation, selectedSpellIndex, Cam.transform.forward);
            spellCooldownTimers[selectedSpellIndex] = currentSpells[selectedSpellIndex].CooldownDuration;
        }
    }

    [ServerRpc]
    private void CastSpellServerRpc(Vector3 position, Quaternion rotation, int spellIndex, Vector3 ShotDir)
    {
        if (spellIndex < 0 || spellIndex >= currentSpells.Count)
        {
            Debug.LogWarning("Spell index out of range");
            return;
        }

        Spell spellUsed = currentSpells[spellIndex];
        GameObject castedSpell = Instantiate(spellUsed.Spell_Prefab, position, rotation);
        castedSpell.GetComponent<NetworkObject>().Spawn();
        castedSpell.GetComponent<ISpell_Interface>().Caster = gameObject;
        print(castedSpell.GetComponent<ISpell_Interface>().Caster.name + " this is the name of the caster");
        Rigidbody spellRb = castedSpell.GetComponent<Rigidbody>();
        spellRb.velocity = ShotDir * spellUsed.Spell_Speed;
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

    // Ground check logic
    private void GroundCheck()
    {
        // Shoot Raycast down to check if player is grounded with an offset from the ground
        Grounded = Physics.Raycast(transform.position + GroundCheck_Start, Vector3.down, GroundCheck_Distance, gameController.GC.GroundLayer);
        // Show raycast
        Debug.DrawRay(transform.position + GroundCheck_Start, Vector3.down * GroundCheck_Distance, Color.red);

        ApplyGravity();
    }

    private void WallCheck()
    {
        if (!IsOwner) return;
        if (MoveInput.x == 0 && MoveInput.y == 0) return;

        // Wall Run When Not On ground and input is towards the wall
        if (!Grounded)
        {
            // Grab Wall
            // Shoot RayCast to all sides of player to check for wall
            RaycastHit hit;
            if (!Physics.Raycast(transform.position, -transform.up, DistanceFromFloorRequiredToWallRun, gameController.GC.GroundLayer))
            {
                if (Physics.Raycast(transform.position + WallCheck_Start, transform.right, out hit, wallCheckDistance, LayerMask.GetMask("Wall")) ||
                    Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, LayerMask.GetMask("Wall")) ||
                    Physics.Raycast(transform.position + WallCheck_Start, transform.forward, out hit, wallCheckDistance, LayerMask.GetMask("Wall")) ||
                    Physics.Raycast(transform.position, -transform.forward, out hit, wallCheckDistance, LayerMask.GetMask("Wall")))
                {
                    IsWallRunning = true;
                    Debug.Log("Wall Running");
                }
                else
                {
                    // If no wall is detected, stop wall running
                    IsWallRunning = false;
                    wallRunTimer_Tick = wallRunTimerDefault;
                    Gravity = true;
                }


                // Start ticking down the wall run timer
                if (IsWallRunning)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

                    wallRunTimer_Tick -= Time.deltaTime;
                    if (wallRunTimer_Tick <= 0)
                    {
                        // If the timer runs out, stop wall running
                        wallRunTimer_Tick = wallRunTimerDefault;
                        IsWallRunning = false;
                        Gravity = true;
                    }
                }

                // If Player moves away from the wall, stop wall running
                Vector3 hitDirection = hit.normal;
                if ((MoveInput.x > 0 && hitDirection == transform.right) ||
                    (MoveInput.x < 0 && hitDirection == -transform.right) ||
                    (MoveInput.y > 0 && hitDirection == transform.forward) ||
                    (MoveInput.y < 0 && hitDirection == -transform.forward))
                {
                    IsWallRunning = false;
                }

                // Only be able to wall run when not looking at the wall
                if (Vector3.Dot(transform.forward, hitDirection) > 0.5f)
                {
                    IsWallRunning = false;
                }

                Debug.DrawRay(transform.position + WallCheck_Start, transform.right * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, -transform.right * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, transform.forward * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, -transform.forward * wallCheckDistance, Color.blue);
                Debug.DrawRay(transform.position + WallCheck_Start, -transform.up * DistanceFromFloorRequiredToWallRun, Color.blue);
                Debug.Log(IsWallRunning + " wallrunning State");
            }
        }
    }

    // Custom gravity application
    private void ApplyGravity()
    {
        if (!Gravity) return;

        if (!Grounded || !IsWallRunning)
        {
            rb.AddForce(Vector3.down * gameController.GC.Gravity_force, ForceMode.Acceleration);
        }
    }
}
