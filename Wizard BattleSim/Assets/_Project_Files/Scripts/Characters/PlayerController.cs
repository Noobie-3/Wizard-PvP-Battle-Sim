using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Timeline.TimelinePlaybackControls;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    public static PlayerController self;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Camera Cam;
    [SerializeField] private float Gravity = 15f;
    private Rigidbody rb;
    [SerializeField] public Vector2 MoveInput;

    public List<Spell> currentSpells;
    [SerializeField] private int selectedSpellIndex = 0;
    public List<float> spellCooldownTimers;


    private void Awake() {
        rb = GetComponent<Rigidbody>();
        if (self == null) {
            self = this;
        }
        else if (self != this) {
            Destroy(gameObject);
        }

        // Initialize cooldown timers for each spell
        UpdateSpellCooldownTimers();
    }


    private void Update() {
        SpellCooldown();
    }

    private void FixedUpdate() {
        MoveObject();
    }

    private void MoveObject() {
        // Get the camera's forward and right vectors, projected onto the horizontal plane
        Vector3 camForward = Vector3.ProjectOnPlane(Cam.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(Cam.transform.right, Vector3.up).normalized;

        // Calculate the movement direction based on the input and the projected camera vectors
        Vector3 moveDirection = (camRight * MoveInput.x + camForward * MoveInput.y).normalized;
        // Apply movement force
        rb.MovePosition(rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime);

        // Apply custom gravity
        rb.AddForce(Vector3.down * Gravity, ForceMode.Acceleration);

        if (moveDirection.sqrMagnitude > 0f) {
            RotateObjectWithCam(moveDirection);
        }
    }

    public void ScrollSpellSelection(float scrollInput) {
        // Scroll up
        if (scrollInput > 0) {
            selectedSpellIndex--;
            if (selectedSpellIndex < 0)
                selectedSpellIndex = currentSpells.Count - 1;
        }
        // Scroll down
        else if (scrollInput < 0) {
            selectedSpellIndex++;
            if (selectedSpellIndex >= currentSpells.Count)
                selectedSpellIndex = 0;
        }

        Debug.Log("Selected Spell: " + selectedSpellIndex);
    }

    public void GetMoveInput(InputAction.CallbackContext Context) {
        MoveInput = Context.ReadValue<Vector2>();
    }


    private void CastSelectedSpell() {
        CastSpell(selectedSpellIndex);

    }

    public void CastSpell(int spellIndex) {
        if (spellIndex < 0 || spellIndex >= currentSpells.Count) {
            Debug.LogWarning("Spell index out of range");
            return;
        }

        if (spellCooldownTimers[spellIndex] <= 0) {
            Spell spellUsed = currentSpells[spellIndex];
             GameObject CastedSpell = Instantiate(spellUsed.Spell_Prefab, transform.position, transform.rotation);
             Rigidbody rb = CastedSpell.GetComponent<Rigidbody>();
            rb.velocity = Cam.transform.forward * spellUsed.Spell_Speed;
            spellCooldownTimers[spellIndex] = spellUsed.CooldownDuration;
        }
        else {
            print("Spell is on cooldown");
        }
    }

    private void SpellCooldown() {
        for (int i = 0; i < spellCooldownTimers.Count; i++) {
            if (spellCooldownTimers[i] > 0) {
                spellCooldownTimers[i] -= Time.deltaTime;
            }
        }
    }

    private void RotateObjectWithCam(Vector3 lookAtDir) {
        if (lookAtDir.sqrMagnitude > 0.01f) {
            Quaternion targetRotation = Quaternion.LookRotation(lookAtDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

    public void AddSpell(Spell newSpell) {
        currentSpells.Add(newSpell);
        UpdateSpellCooldownTimers();
    }

    public void RemoveSpell(Spell spellToRemove) {
        int index = currentSpells.IndexOf(spellToRemove);
        if (index >= 0) {
            currentSpells.RemoveAt(index);
            UpdateSpellCooldownTimers();
        }
    }

    private void UpdateSpellCooldownTimers() {
        spellCooldownTimers = new List<float>(new float[currentSpells.Count]);
    }
}
