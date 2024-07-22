using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using Unity.VisualScripting;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System.Data;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    // Serialized variables
    [SerializeField] private GameObject MeshToRotate;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform Cam;
    [SerializeField] private float Gravity = 15f;
    [SerializeField] private CinemachineVirtualCamera Vcam;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private float JumpHeight = 5f;
    [SerializeField] private float GroundCheck_Distance = 0.5f;
    [SerializeField] private float CamSpeed = 1f;         // Variables for camera rotation
    [SerializeField] private float RotateSpeed = 1f;
    [SerializeField] private float minXRotation = -45f;   // Minimum X rotation (pitch)
    [SerializeField] private float maxXRotation = 45f;    // Maximum X rotation (pitch)

    // Public variables
    [SerializeField] public Vector2 MoveInput;
    [SerializeField] public Vector2 MouseInput;
    [SerializeField] public bool Grounded;
    public List<Spell> currentSpells;
    [SerializeField] private int selectedSpellIndex = 0;
    public List<float> spellCooldownTimers;

    // Private variables
    private Rigidbody rb;
    private float rotationX = 0f;
    private float rotationY = 0f;

    // Unity methods
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
    }

    // Movement logic
    private void MoveObject()
    {
        // Calculate movement direction
        Vector3 moveDirection = (Cam.right.normalized * MoveInput.x + Cam.transform.forward.normalized * MoveInput.y);
        moveDirection.y = 0;
        Vector3 targetPosition = rb.position + moveSpeed * Time.fixedDeltaTime * moveDirection;
        rb.MovePosition(targetPosition);

        // Rotate the player towards camera forward
        if (MoveInput != Vector2.zero)
        {
            MeshToRotate.transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    // Spell selection logic
    public void ScrollSpellSelection(float scrollInput)
    {
        if (!IsOwner) return;

        if (scrollInput > 0)
        {
            selectedSpellIndex--;
            if (selectedSpellIndex < 0)
                selectedSpellIndex = currentSpells.Count - 1;
        }
        else if (scrollInput < 0)
        {
            selectedSpellIndex++;
            if (selectedSpellIndex > currentSpells.Count - 1)
                selectedSpellIndex = 0;
        }

        Debug.Log("Selected Spell: " + selectedSpellIndex + " out of " + currentSpells.Count);
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
        if(context.performed && Grounded)
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
        Ray GroundCheckRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(GroundCheckRay, out hit, GroundCheck_Distance))
        {
            Grounded = true;
        }
        else
        {
            Grounded = false;
        }

        //show raycast 
        Debug.DrawRay(transform.position, Vector3.down * GroundCheck_Distance, Color.red);

        ApplyGravity();
    }

    // Custom gravity application
    private void ApplyGravity()
    {
        if (!Grounded)
        {
            rb.AddForce(Vector3.down * Gravity, ForceMode.Acceleration);
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }
    }
}
