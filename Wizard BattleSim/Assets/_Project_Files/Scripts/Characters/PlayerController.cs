using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;

// Require the component Rigidbody to be attached to the GameObject
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    // Serialize fields to make them editable in the Unity Inspector
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Camera Cam;
    [SerializeField] private float Gravity = 15f;
    private Rigidbody rb; // Rigidbody component
    [SerializeField] public Vector2 MoveInput; // Player input for movement

    public List<Spell> currentSpells; // List of available spells
    [SerializeField] private int selectedSpellIndex = 0; // Index of the selected spell
    public List<float> spellCooldownTimers; // List of spell cooldown timers

    [SerializeField] private CinemachineFreeLook freeLookCam; // Cinemachine free look camera
    [SerializeField] private AudioListener audioListener; // Audio listener component
    [SerializeField] private float CamSpeed = 10f; // Camera rotation speed

    // Called when the script instance is being loaded
    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
        // Initialize cooldown timers for each spell
        UpdateSpellCooldownTimers();
    }

    // Called when the object is spawned on the network
    public override void OnNetworkSpawn()
    {
        if (IsOwner) // Check if this is the local player
        {
            audioListener.enabled = true; // Enable audio listener for the local player
            freeLookCam.Priority = 1; // Set camera priority for the local player
        }
        else
        {
            audioListener.enabled = false; // Disable audio listener for remote players
            freeLookCam.Priority = 0; // Set lower camera priority for remote players
        }
    }

    // Called every frame
    private void Update()
    {
        if (!IsOwner) return; // Only the local player should control this object

        SpellCooldown(); // Update spell cooldown timers
    }

    // Called at a fixed time interval for physics updates
    private void FixedUpdate()
    {
        if (!IsOwner) return; // Only the local player should control this object

        MoveObject(); // Handle player movement
    }

    // Handle player movement
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

        // Rotate the player object with the camera if there is movement
        if (moveDirection.sqrMagnitude > 0f)
        {
            RotateObjectWithCam(moveDirection);
        }
    }

    // Scroll through the spell selection
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

        Debug.Log("Selected Spell: " + selectedSpellIndex); // Debug the selected spell index
    }

    // Get player movement input from the Input System
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    // Cast the selected spell
    public void CastSpell()
    {
        // Check if the selected spell index is valid
        if (selectedSpellIndex < 0 || selectedSpellIndex >= currentSpells.Count)
        {
            Debug.LogWarning("Spell index out of range");
            return;
        }

        // Check if the spell is off cooldown
        if (spellCooldownTimers[selectedSpellIndex] <= 0)
        {
            // Call the server RPC to spawn the spell
            if (!IsOwner) return; // Only the local player should control this object
            CastSpellServerRpc(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), transform.rotation, selectedSpellIndex, Cam.transform.forward);
            spellCooldownTimers[selectedSpellIndex] = currentSpells[selectedSpellIndex].CooldownDuration; // Reset the cooldown timer for the spell
        }

    }

    // Server RPC to handle spell casting
    [ServerRpc]
    private void CastSpellServerRpc(Vector3 position, Quaternion rotation, int spellIndex, Vector3 ShotDir)
    {
        // Check if the spell index is valid
        if (spellIndex < 0 || spellIndex >= currentSpells.Count)
        {
            Debug.LogWarning("Spell index out of range");
            return;
        }

        // Instantiate the spell object and set its velocity
        Spell spellUsed = currentSpells[spellIndex];
        GameObject castedSpell = Instantiate(spellUsed.Spell_Prefab, position, rotation);
        castedSpell.GetComponent<NetworkObject>().Spawn(); // Spawn the spell object on the network
        castedSpell.GetComponent<ISpell_Interface>().Caster = gameObject; // Set the caster of the spell
        print(castedSpell.GetComponent<ISpell_Interface>().Caster.name  + " this is the name of the caster");
        Rigidbody spellRb = castedSpell.GetComponent<Rigidbody>();
        spellRb.velocity = ShotDir * spellUsed.Spell_Speed; // Set the spell's velocity
    }

    // Update spell cooldown timers
    private void SpellCooldown()
    {
        for (int i = 0; i < spellCooldownTimers.Count; i++)
        {
            if (spellCooldownTimers[i] > 0)
            {
                spellCooldownTimers[i] -= Time.deltaTime; // Reduce the cooldown timer by the elapsed time
            }
        }
    }

    // Rotate the player object to face the direction of movement
    private void RotateObjectWithCam(Vector3 lookAtDir)
    {
        if (lookAtDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookAtDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * CamSpeed);
        }
    }

    // Add a new spell to the player's spell list
    public void AddSpell(Spell newSpell)
    {
        currentSpells.Add(newSpell);
        UpdateSpellCooldownTimers(); // Update the cooldown timers to include the new spell
    }

    // Remove a spell from the player's spell list
    public void RemoveSpell(Spell spellToRemove)
    {
        int index = currentSpells.IndexOf(spellToRemove);
        if (index >= 0)
        {
            currentSpells.RemoveAt(index);
            UpdateSpellCooldownTimers(); // Update the cooldown timers to remove the spell
        }
    }

    // Initialize or update the spell cooldown timers
    private void UpdateSpellCooldownTimers()
    {
        spellCooldownTimers = new List<float>(new float[currentSpells.Count]); // Initialize the list with default values
    }
}
