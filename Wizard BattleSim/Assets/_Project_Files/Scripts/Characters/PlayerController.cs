using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerController : NetworkBehaviour
{


    // Movement variables
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public Vector3 moveDirection;
    [SerializeField] private bool Gravity = true;
    [SerializeField] private float JumpHeight = 5f;
    public float FallSpeed;
    private NetworkVariable<Quaternion> syncedRotation = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

    // Ground check variables
    [Header("Ground Check Settings")]
    [SerializeField] private float GroundCheck_Distance = 0.5f;
    [SerializeField] private Vector3 GroundCheck_Start = new Vector3(0, 0.5f, 0);

    float lastGroundedTime;
    float coyoteTime = 0.15f;



    // Camera settings
    [Header("Camera Settings")]
    [SerializeField] public Transform Cam;
    [SerializeField] public Camera camComponent;
    [SerializeField] private CinemachineVirtualCamera Vcam;
    [SerializeField] CinemachineImpulseSource impulseSource;

    // Miscellaneous settings
    [Header("Miscellaneous Settings")]
    public NetworkObject DamageBilboard;
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
    [SerializeField] private GameObject ChargeVfx;
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
    public Vector3 TargetRot;
    


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
    private NetworkObject spawnedChargeVfx;

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


        GroundCheck();

        PlayerUi.UpdateUI();

        if (Charging)
        {
            Stats.ChargeManaServerRpc();
        }



        //rotate the object based on the camrea
        //camera forward and right vectors:
        var forward = Cam.transform.forward;
        var right = Cam.transform.right;

        //project forward and right vectors on the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();


        //this is the direction in the world space we want to move:
        TargetRot = forward * MoveInput.y + right * MoveInput.x;
        RotateCharacter();

        MoveObject();

        AlterGravity();
    }
    // Movement logic
    private void MoveObject()
    {
        if (!IsOwner) return;
        if (Charging) return;
        print("Moveing Object");
        IsRunning = MoveInput.x != 0 || MoveInput.y != 0;

        Vector3 targetVelocity = TargetRot.normalized * moveSpeed;
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - new Vector3(currentVelocity.x, 0, currentVelocity.z);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

    }

    public void AlterGravity()
    {
        rb.linearVelocity += Vector3.up * -FallSpeed * Time.fixedDeltaTime;
    }

    //Rotation logic
    private void RotateCharacter()
    {
        if (!IsOwner) return;
        if (TargetRot.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(TargetRot, Vector3.up);
            MeshToRotate.transform.rotation = Quaternion.Slerp(MeshToRotate.transform.rotation, targetRotation, 0.15f);
        }
        //sync to other clients if needed
        syncedRotation.Value = MeshToRotate.transform.rotation;
    }




    // Input handlers
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.performed)
        {
            Vector2 input = context.ReadValue<Vector2>();
            MoveInput = new Vector2(input.x, input.y);
        }
        else if (context.canceled)
        {
            MoveInput = Vector3.zero;
        }
        if (Anim != null)
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
        if (!IsOwner) return;

        if (context.phase == InputActionPhase.Canceled)
        {
            CanRun = true;
            Charging = false;
            Anim?.SetBool("IsCharging", false);
            StopChargeServerRpc();   // <- despawn
            return;
        }

        if (!Grounded) return;

        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Started)
        {
            Charging = true;
            CanRun = false;
            rb.linearVelocity = Vector3.zero;
            Anim?.SetBool("IsCharging", true);
            StartChargeServerRpc(transform.position);  // <- spawn

        }
    }
    // Jump logic
    public void JumpInput(InputAction.CallbackContext context)
    {
        //SpellCasterScript.ResetChargeSpell();
        if (context.performed && (Grounded))
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
        if (Grounded) lastGroundedTime = Time.time;

        bool groundedBuffered = (Time.time - lastGroundedTime) <= coyoteTime;
        Anim?.SetBool("Grounded", groundedBuffered);

        Debug.DrawRay(transform.position + GroundCheck_Start, Vector3.down * GroundCheck_Distance, Color.red);
    }

    // Animation handling
    private void AnimateObject()
    {
        if (!IsOwner) return;
        if (Anim == null) return;
        // Update animation parameters
        Anim.SetBool("IsRunning", IsRunning);
        //Anim.SetBool("IsCharging", Charging);
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

    /*    private void OnParticleCollision(GameObject other)
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
    */
    public void TakeDamage(Spell spell, ulong whoHitMe)
    {
        // Only send the raw data to the server
        TakeDamageServerRPC(spell.Spell_Damage, whoHitMe);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRPC(float SpellDamage, ulong WhoHitMe)
    {

        Vector2 Jitter;
        Jitter.x = Random.Range(-1, 1);
        Jitter.y = Random.Range(-1, 1);



        DamagePopupSpawner.Instance.CreatePopUp(new Vector3(Jitter.x + transform.position.x, Jitter.y + transform.position.y, transform.position.z ) + Vector3.up * 2, SpellDamage.ToString(), new Color(0, 255, 252, 1));
        Stats.TakeDamage(SpellDamage, WhoHitMe);
        PlayerUi.UpdateUI();

        // Only tell the OWNER of this PlayerController to shake their camera
        ClientRpcParams sendToOwner = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };

        DoCamShakeClientRpc(sendToOwner);
       // ShowDamageBilboard();
    }

    [ClientRpc]
    public void DoCamShakeClientRpc(ClientRpcParams rpcParams = default)
    {
        impulseSource.GenerateImpulse();
    }

    [ClientRpc]
    public void ShowDamageBilboardClientRpc()
    {
       var Bilboard = Instantiate(DamageBilboard);
        Bilboard.Spawn();
        Destroy(Bilboard, 2f);
    }


    [ServerRpc]
    private void StartChargeServerRpc(Vector3 pos)
    {
        if (spawnedChargeVfx && spawnedChargeVfx.IsSpawned) return;
        if (!ChargeVfx) return;

        var vfx = Instantiate(ChargeVfx, pos, default);
        vfx.transform.position = pos;
        print(vfx.transform.position + " vfx position " + "This is pos sent: "+ pos);
        var netObj = vfx.GetComponent<NetworkObject>();
        

        // If you want the player to be the owner (not necessary unless they need to drive it):
        // netObj.SpawnWithOwnership(OwnerClientId, true);
        netObj.Spawn(true); // server-owned is fine for a visual
        print(vfx.transform.position + " vfx position after spawn");
        // Parent to the wand/socket so it follows the player on all clients

        spawnedChargeVfx = netObj;
    }

    [ServerRpc]
    private void StopChargeServerRpc()
    {
        if (spawnedChargeVfx && spawnedChargeVfx.IsSpawned)
        {

            spawnedChargeVfx.Despawn(true);
            spawnedChargeVfx = null;
        }
    }


}