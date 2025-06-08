//using UnityEngine;
//public class Grapple3D : MonoBehaviour
//{
//    public Transform anchorPoint; // punto d'aggancio sul soffitto
//    private Rigidbody rb;
//    private ConfigurableJoint joint;
//    public float movemenForce = 5f;

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//    }

//    public LineRenderer rope;

//    void LateUpdate()
//    {
//        if (joint != null)
//        {
//            rope.enabled = true;
//            rope.SetPosition(0, anchorPoint.position);
//            rope.SetPosition(1, transform.position);
//        }
//        else { rope.enabled = false; }
//    }

//    void FixedUpdate()
//    {
//        if (joint != null)
//        {
//            float input = Input.GetAxis("Horizontal");
//            Vector3 force = transform.right * input * movemenForce; // Puoi cambiare la forza
//            rb.AddForce(force);
//        }
//    }

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.E))
//        {
//            if (joint == null)
//            {
//                Attach();
//            }
//        }

//        if (Input.GetKeyDown(KeyCode.Q))
//        {
//            if (joint != null)
//            {
//                Destroy(joint);
//            }
//        }
//    }

//    void Attach()
//    {
//        joint = gameObject.AddComponent<ConfigurableJoint>();
//        joint.connectedAnchor = anchorPoint.position;
//        joint.xMotion = ConfigurableJointMotion.Limited;
//        joint.yMotion = ConfigurableJointMotion.Limited;
//        joint.zMotion = ConfigurableJointMotion.Limited;

//        SoftJointLimit limit = new SoftJointLimit();
//        limit.limit = Vector3.Distance(transform.position, anchorPoint.position);
//        joint.linearLimit = limit;

//        joint.angularXMotion = ConfigurableJointMotion.Free;
//        joint.angularYMotion = ConfigurableJointMotion.Free;
//        joint.angularZMotion = ConfigurableJointMotion.Free;

//        joint.enableCollision = true;
//    }
//}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grapple3D : MonoBehaviour
{
    [SerializeField] private Transform anchorPoint;       // Punto fisso sul soffitto
    [SerializeField] private float climbForce = 2f;        // Velocità salita/discesa
    [SerializeField] private float pendulumForce = 5f;
    [SerializeField] private float minRopeLength = 2f;    // Lunghezza minima della corda
    [SerializeField] private float maxRopeLength = 20f;   // Lunghezza massima della corda

    private Rigidbody rb;
    private ConfigurableJoint joint;
    private LineRenderer rope;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rope = GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        if (joint != null)
        {
            rope.enabled = true;
            rope.SetPosition(0, anchorPoint.position);
            rope.SetPosition(1, transform.position);
        }
        else { rope.enabled = false; }
    }

    void FixedUpdate()
    {
        if (joint != null)
        {
            float input = Input.GetAxis("Horizontal");
            Vector3 force = transform.right * input * pendulumForce; // puoi cambiare la forza
            rb.AddForce(force);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (joint == null)
                Attach();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (joint != null)
                Detach();
        }

        if (joint != null)
        {
            AdjustRopeLength();
        }
    }

    void Attach()
    {
        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = anchorPoint.position;

        // Blocca il movimento oltre una certa distanza
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit limit = new SoftJointLimit();
        float initialDistance = Vector3.Distance(transform.position, anchorPoint.position);
        limit.limit = Mathf.Clamp(initialDistance, minRopeLength, maxRopeLength);
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
        float input = Input.GetAxis("Vertical"); // W/S o ↑/↓

        if (Mathf.Abs(input) > 0.01f)
        {
            float currentLimit = joint.linearLimit.limit;
            float newLimit = currentLimit - input * climbForce * Time.deltaTime;
            newLimit = Mathf.Clamp(newLimit, minRopeLength, maxRopeLength);

            // Applica nuovo limite
            SoftJointLimit limit = joint.linearLimit;
            limit.limit = newLimit;
            joint.linearLimit = limit;

            // Applica forza verso l'anchor o lontano da esso
            if (input > 0) { return;}
            rb.MovePosition(transform.position + input * climbForce * 10 * Time.deltaTime * transform.up);
        }
    }
}
