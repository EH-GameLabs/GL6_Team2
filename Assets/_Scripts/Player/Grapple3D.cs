using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class Grapple3D : MonoBehaviour
{
    [SerializeField] private Transform anchorPoint;       // Punto fisso sul soffitto
    [SerializeField] private float climbForce = 2f;        // Velocità salita/discesa
    [SerializeField] private float pendulumForce = 5f;
    [SerializeField] private float minRopeLength = 2f;    // Lunghezza minima della corda
    [SerializeField] private float maxRopeLength = 20f;   // Lunghezza massima della corda

    [Header("Debug")]
    [SerializeField][ReadOnly] private Rigidbody rb;
    [SerializeField][ReadOnly] private ConfigurableJoint joint;
    private LineRenderer rope;
    private PlayerInputData playerInputData;
    [HideInInspector] public bool isGrappling = false;
    [HideInInspector] public bool canGrapple = false;
    bool adjustingRope = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rope = GetComponent<LineRenderer>();
        rope.material.color = Color.white;
    }

    void LateUpdate()
    {
        if (GameManager.Instance.gameMode == GameMode.OnlineMultiplayer)
        {
            AdjustRopePositionServerRpc();
        }
        else
        {
            AdjustRopePosition();
        }
    }

   /* void FixedUpdate()
   {
        if (joint != null)
        {
            float input = Input.GetAxis("Horizontal");
            Vector3 force = transform.right * input * pendulumForce; // Puoi cambiare la forza
            rb.AddForce(force);
       }
    }*/
    void FixedUpdate()
    {
        if (joint == null) return;
        if (adjustingRope) return;

        Vector3 dirFromAnchor = rb.position - anchorPoint.position;
        Vector3 radialDir = dirFromAnchor.normalized;

        // Direzione tangente
        Vector3 tangent = Vector3.Cross(radialDir, Vector3.forward).normalized;

        float input = -playerInputData.Move.x;
        Vector3 force = input * pendulumForce * tangent;

        rb.AddForce(force, ForceMode.Force);

        Vector3 velocity = rb.linearVelocity;
        float radialSpeed = Vector3.Dot(velocity, radialDir);
        Vector3 radialVelocity = radialDir * radialSpeed;
        rb.linearVelocity = velocity - radialVelocity;
    }



    void Update()
    {
        if (!canGrapple && !isGrappling) return;

        // Ottieni gli input
        playerInputData = InputManager.Instance.GetInputForCharacter(CharacterID.CharacterA, GameManager.Instance.gameMode);

        if (playerInputData.FirePressed && !isGrappling)
        {
            if (joint == null)
            {
                SoundManager.Instance.PlaySFXSound(SoundManager.Instance.WebThrow);
                Attach();
                isGrappling = true;
                Debug.Log("Grapple attached to anchor point");
            }
        }
        else if (!playerInputData.FirePressed && isGrappling)
        {
            if (joint != null)
            {
                Detach();
                isGrappling = false;
                Debug.Log("Grapple detached from anchor point.");
            }
        }

        if (joint != null)
        {
            AdjustRopeLength();
        }
    }

    public void SetAnchorPoint(Transform newAnchorPoint)
    {
        anchorPoint = newAnchorPoint;
    }

    void Attach()
    {
        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = anchorPoint.position;
        joint.anchor = Vector3.zero;

        // Blocca il movimento oltre una certa distanza
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit limit = new SoftJointLimit();
        float initialDistance = Vector3.Distance(transform.position, anchorPoint.position);
        limit.limit = Mathf.Clamp(initialDistance, minRopeLength, maxRopeLength);
        limit.contactDistance = limit.limit / 10;
        joint.linearLimit = limit;

        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        joint.enableCollision = true;
    }

    void Detach()
    {
        Destroy(joint);
    }

    void AdjustRopeLength()
    {
        float input = playerInputData.Move.y; // W/S o ↑/↓

        if (Mathf.Abs(input) > 0.01f)
        {
            adjustingRope = true;
            float currentLimit = joint.linearLimit.limit;
            float newLimit = currentLimit - input * climbForce * Time.deltaTime;
            newLimit = Mathf.Clamp(newLimit, minRopeLength, maxRopeLength);

            // Applica nuovo limite
            SoftJointLimit limit = joint.linearLimit;
            limit.limit = newLimit;
            limit.contactDistance = limit.limit / 10;
            joint.linearLimit = limit;
        }
        else { adjustingRope = false; }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AdjustRopePositionServerRpc()
    {
        AdjustRopePositionClientRpc();
    }

    [ClientRpc]
    private void AdjustRopePositionClientRpc()
    {
        AdjustRopePosition();
    }

    private void AdjustRopePosition()
    {
        if (joint != null)
        {
            rope.enabled = true;
            rope.SetPosition(0, anchorPoint.position);
            rope.SetPosition(1, transform.position);
            rope.startWidth = 0.1f;
            rope.endWidth = 0.1f;
        }
        else { rope.enabled = false; }
    }
}
