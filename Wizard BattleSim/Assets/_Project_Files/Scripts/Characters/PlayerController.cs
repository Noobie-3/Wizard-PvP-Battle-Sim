using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Camera Cam;
    [SerializeField] private float Gravity = 15f;
    private Rigidbody rb;
    [SerializeField] public Vector2 MoveInput;

    public List<Spell> currentSpells;
    [SerializeField] private int selectedSpellIndex = 0;
    public List<float> spellCooldownTimers;

    [SerializeField] private CinemachineFreeLook freeLookCam;
    [SerializeField] private AudioListener audioListener;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Initialize cooldown timers for each spell
        UpdateSpellCooldownTimers();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            audioListener.enabled = true;
            freeLookCam.Priority = 1;
        }
        else
        {
            audioListener.enabled = false;
            freeLookCam.Priority = 0;
        }
    }

    private void Update()
    {
        if (!IsOwner) return; // Only the local player should control this object

        SpellCooldown();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return; // Only the local player should control this object

        MoveObject();
    }

    private void MoveObject()
    {
        // Get the camera's forward and right vectors, projected onto the horizontal plane
        Vector3 camForward = Vector3.ProjectOnPlane(Cam.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(Cam.transform.right, Vector3.up).normalized;

        // Calculate the movement direction based on the input and the projected camera vectors
        Vector3 moveDirection = (camRight * MoveInput.x + camForward * MoveInput.y).normalized;
        // Apply movement force
        Vector3 targetPosition = rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        // Apply custom gravity
        rb.AddForce(Vector3.down * Gravity, ForceMode.Acceleration);

        if (moveDirection.sqrMagnitude > 0f)
        {
            RotateObjectWithCam(moveDirection);
        }
    }

    public void ScrollSpellSelection(float scrollInput)
    {
        if (!IsOwner) return; // Only the local player should control this object

        // Scroll up
        if (scrollInput > 0)
        {
            selectedSpellIndex--;
            if (selectedSpellIndex < 0)
                selectedSpellIndex = currentSpells.Count - 1;
        }
        // Scroll down
        else if (scrollInput < 0)
        {
            selectedSpellIndex++;
            if (selectedSpellIndex >= currentSpells.Count)
                selectedSpellIndex = 0;
        }

        Debug.Log("Selected Spell: " + selectedSpellIndex);
    }

    public void GetMoveInput(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void CastSpell()
    {
        if (selectedSpellIndex < 0 || selectedSpellIndex >= currentSpells.Count)
        {
            Debug.LogWarning("Spell index out of range");
            return;
        }

        if (spellCooldownTimers[selectedSpellIndex] <= 0)
        {
            // Call the server RPC to spawn the spell
            if(!IsOwner) return; // Only the local player should control this object
            CastSpellServerRpc(new Vector3 (transform.position.x , transform.position.y + 1, transform.position.z), transform.rotation, selectedSpellIndex, Cam.transform.forward);
            spellCooldownTimers[selectedSpellIndex] = currentSpells[selectedSpellIndex].CooldownDuration;
        }
        else
        {
            Debug.Log("Spell is on cooldown");
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

    private void RotateObjectWithCam(Vector3 lookAtDir)
    {
        if (lookAtDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookAtDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

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
}
