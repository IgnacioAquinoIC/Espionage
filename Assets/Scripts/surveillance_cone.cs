using UnityEngine;

public class SecurityCameraVisual : MonoBehaviour
{
    [Header("Detection")]
    public float detectionDistance = 15f;
    public float detectionRadius = 1f;
    public LayerMask detectionMask;

    [Header("Colors")]
    public Color safeColor = Color.green;
    public Color detectedColor = Color.red;

    [Header("Transparency")]
    [Range(0f, 1f)]
    public float transparency = 0.25f;

    private Renderer coneRenderer;
    private Material coneMaterial;

    void Start()
    {
        coneRenderer = GetComponent<Renderer>();

        if (coneRenderer != null)
        {
            coneMaterial = coneRenderer.material;
            SetColor(safeColor);
        }
    }

    void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        RaycastHit hit;

        bool detected = Physics.SphereCast(
            transform.parent.position,
            detectionRadius,
            transform.parent.forward,
            out hit,
            detectionDistance,
            detectionMask
        );

        if (detected)
        {
            SetColor(detectedColor);
        }
        else
        {
            SetColor(safeColor);
        }
    }

    void SetColor(Color color)
    {
        if (coneMaterial == null)
            return;

        color.a = transparency;
        coneMaterial.color = color;
    }
}