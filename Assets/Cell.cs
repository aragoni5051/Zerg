using UnityEngine;

public class Cell : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private float energy = 0f;
    [SerializeField] private string genome = "ATGC";

    [Header("Prefab / Body References")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform body;
    [SerializeField] private Rigidbody bodyRb;
    [SerializeField] private SphereCollider bodyCol;

    private float time = 0;

    private void Awake()
    {
        if (body == null)
        {
            if (transform.childCount > 0) body = transform.GetChild(0);
        }

        if (body != null)
        {
            if (bodyRb == null) bodyRb = body.GetComponent<Rigidbody>();
            if (bodyCol == null) bodyCol = body.GetComponent<SphereCollider>();
        }

        if (body == null) Debug.LogError($"{name}: Body Transform not assigned.");
        if (bodyRb == null) Debug.LogError($"{name}: Body Rigidbody not assigned/found.");
        if (bodyCol == null) Debug.LogError($"{name}: Body SphereCollider not assigned/found.");
    }

    private void FixedUpdate()
    {
        energy += Time.fixedDeltaTime * 0.3f;
        if (energy > 4)
        {
            Mitosis();
        }
        Debug.Log(energy);
        Resize();
    }

    public void SetGenome(string newGenome) => genome = newGenome;

    public void SetEnergy(float newEnergy)
    {
        energy = newEnergy;
    }

    public void Mitosis()
    {  
        // Sanity checks
        if (cellPrefab == null)
        {
            Debug.LogError($"{name}: cellPrefab is not assigned.");
            return;
        }

        if (body == null || bodyRb == null)
        {
            Debug.LogError($"{name}: body/bodyRb missing. Assign the child sphere references.");
            return;
        }

        //Dividing
        Vector3 reservedVelocity = bodyRb.linearVelocity;
        bodyCol.enabled = false;

        //Create new cell
        Vector3 direction = Random.onUnitSphere;
        GameObject newCellObj1 = Instantiate(cellPrefab, transform.position + direction * bodyCol.radius * body.lossyScale.x * 0.5f, Quaternion.identity);
        GameObject newCellObj2 = Instantiate(cellPrefab, transform.position - direction * bodyCol.radius * body.lossyScale.x * 0.5f, Quaternion.identity);
        
        Cell newCell1 = newCellObj1.GetComponent<Cell>();
        Cell newCell2 = newCellObj2.GetComponent<Cell>();

        // Copy genome + split energy
        float childEnergy = energy * 0.5f;
        newCell1.SetEnergy(childEnergy);
        newCell2.SetEnergy(childEnergy);

        newCell1.SetGenome(genome);
        newCell2.SetGenome(genome);

        // Set child scales
        float childScale = Mathf.Sqrt(childEnergy);
        newCell1.body.localScale = Vector3.one * childScale;
        newCell2.body.localScale = Vector3.one * childScale;

        // Ensure child colliders are enabled
        newCell1.bodyCol.enabled = true;
        newCell2.bodyCol.enabled = true;

        float gap = 0.02f; // tweak if you want tighter/looser
        float rChildWorld = 0.5f * newCell1.body.lossyScale.x;

        // Place them side-by-side around the parent center
        Vector3 offset = direction * (rChildWorld + gap * 0.5f);
        newCellObj1.transform.position = transform.position + offset;
        newCellObj2.transform.position = transform.position - offset;

        // Inherit velocity so they move with the swarm/cluster
        newCell1.bodyRb.linearVelocity = reservedVelocity;
        newCell2.bodyRb.linearVelocity = reservedVelocity;

        // Destroy parent cell
        Destroy(gameObject);
    }

    public void Apoptosis()
    {
        Destroy(gameObject);
    }

    public void Resize()
    {
        if(body.localScale.x < Mathf.Sqrt(energy))
        {
            body.localScale += Vector3.one * 0.01f;
        }
        else if (body.localScale.x > Mathf.Sqrt(energy))    
        {
            body.localScale -= Vector3.one * 0.01f;
        }
    }
}
