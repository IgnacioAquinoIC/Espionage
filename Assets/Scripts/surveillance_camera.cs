using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Camera Sweep")]
    public float sweepAngle = 60f;
    public float sweepSpeed = 1f;

    [Header("Detection")]
    public float detectionDistance = 15f;
    public float detectionRadius = 1f;

    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Enemy Response")]
    public EnemyPatrol[] enemies;

    [Header("Debug Visualization")]
    public bool showDetectionSphere = true;
    public Color detectionSphereColor = Color.yellow;
    [Range(0.01f, 1f)]
    public float detectionSphereTransparency = 0.25f;

    private Quaternion startingRotation;
    private float sweepTime;

    private GameObject debugSphere;
    private Material debugMaterial;

    void Start()
    {
        startingRotation = transform.localRotation;

        CreateDebugSphere();
    }

    void Update()
    {
        Sweep();
        DetectPlayer();
        UpdateDebugSphere();
    }

    void Sweep()
    {
        sweepTime += Time.deltaTime * sweepSpeed;

        float angle = Mathf.PingPong(
            sweepTime,
            sweepAngle * 2f
        ) - sweepAngle;

        transform.localRotation =
            startingRotation * Quaternion.Euler(0f, angle, 0f);
    }

    void DetectPlayer()
    {
        LayerMask detectionMask = playerMask | obstacleMask;

        RaycastHit sphereHit;

        bool sphereDetected = Physics.SphereCast(
            transform.position,
            detectionRadius,
            transform.forward,
            out sphereHit,
            detectionDistance,
            detectionMask
        );

        if (!sphereDetected)
            return;

        // Something was hit, but it wasn't the player.
        if (((1 << sphereHit.collider.gameObject.layer) & playerMask) == 0)
            return;

        // Find the center of the player's collider.
        Vector3 playerPosition = sphereHit.collider.bounds.center;

        Vector3 directionToPlayer = playerPosition - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check whether a wall is blocking the camera.
        if (Physics.Raycast(
            transform.position,
            directionToPlayer.normalized,
            out RaycastHit lineOfSightHit,
            distanceToPlayer,
            obstacleMask
        ))
        {
            return;
        }

        // Camera has a clear view of the player.
        Debug.Log("Security camera detected: " + sphereHit.collider.name);

        // Alert all linked enemies.
        foreach (EnemyPatrol enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.CameraDetectedPlayer(playerPosition);
            }
        }
    }

    void CreateDebugSphere()
    {
        if (!showDetectionSphere)
            return;

        debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.name = "Detection Debug Sphere";

        // We don't want the debug sphere interfering with physics.
        Destroy(debugSphere.GetComponent<Collider>());

        // Make the sphere twice the radius of the SphereCast.
        debugSphere.transform.localScale =
            Vector3.one * (detectionRadius * 2f);

        // Create a transparent material.
        debugMaterial = new Material(Shader.Find("Standard"));

        debugMaterial.SetFloat(
            "_Mode",
            3
        );

        debugMaterial.SetInt(
            "_SrcBlend",
            (int)UnityEngine.Rendering.BlendMode.SrcAlpha
        );

        debugMaterial.SetInt(
            "_DstBlend",
            (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
        );

        debugMaterial.SetInt(
            "_ZWrite",
            0
        );

        debugMaterial.DisableKeyword("_ALPHATEST_ON");
        debugMaterial.EnableKeyword("_ALPHABLEND_ON");
        debugMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        debugMaterial.renderQueue = 3000;

        debugSphere.GetComponent<Renderer>().material = debugMaterial;

        UpdateDebugSphereAppearance();
    }

    void UpdateDebugSphere()
    {
        if (!showDetectionSphere || debugSphere == null)
            return;

        debugSphere.transform.position =
            transform.position +
            transform.forward * detectionDistance;

        // Keep the sphere size synchronized with the SphereCast radius.
        debugSphere.transform.localScale =
            Vector3.one * (detectionRadius * 2f);
    }

    void UpdateDebugSphereAppearance()
    {
        if (debugMaterial == null)
            return;

        Color color = detectionSphereColor;
        color.a = detectionSphereTransparency;

        debugMaterial.color = color;
    }

    void OnDestroy()
    {
        if (debugSphere != null)
        {
            Destroy(debugSphere);
        }

        if (debugMaterial != null)
        {
            Destroy(debugMaterial);
        }
    }
}
