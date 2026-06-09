using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class PlatformMeshCombiner : MonoBehaviour
{
    private void Awake()
    {
        GenerateCombinedCollisionMesh();
    }

    [ContextMenu("Generate Collision Mesh Now")]
    private void GenerateCombinedCollisionMesh()
    {
        // Gather all mesh filters in the children of this object
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            // Calculate the transform matrix relative to this parent object
            combine[i].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        // Create a new mesh to hold the combined geometry
        Mesh combinedMesh = new Mesh();
        // Use 32-bit index format to support large platforms with many vertices
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 
        combinedMesh.CombineMeshes(combine);

        // Assign the combined mesh to the MeshCollider
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = combinedMesh;
    }
}