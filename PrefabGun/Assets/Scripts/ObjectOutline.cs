using UnityEngine;

public class ObjectOutline : MonoBehaviour
{
    private Renderer[] renderers;
    private Material[][] normalMaterials;
    private Material[][] outlineMaterials;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        normalMaterials = new Material[renderers.Length][];
        outlineMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;

            // Save both materials
            outlineMaterials[i] = materials;

            // Save only the first material
            normalMaterials[i] = new Material[] { 
                materials[0] 
            };

            // Hide the outline
            renderers[i].sharedMaterials = normalMaterials[i];
        }
    }

    public void SetOutline(bool show)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (show)
                renderers[i].sharedMaterials = outlineMaterials[i];
            else
                renderers[i].sharedMaterials = normalMaterials[i];
        }
    }
}
