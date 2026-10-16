using UnityEngine;

public class ObjectOutline : MonoBehaviour
{
    private Renderer[] renderers;
    private Material[][] normalMaterials;
    private Material[][] outlineMaterials;
    private Material[][] xrayMaterials;

    void Awake()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();

        normalMaterials = new Material[renderers.Length][];
        outlineMaterials = new Material[renderers.Length][];
        xrayMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] all = renderers[i].sharedMaterials;

            xrayMaterials[i] = all;
            normalMaterials[i] = new Material[] { all[0] };

            // Save materials but xray
            if (all.Length >= 2)
                outlineMaterials[i] = new Material[] { all[0], all[1] };

            // Save only the first material
            else
                outlineMaterials[i] = normalMaterials[i];

            // Hide the outline
            renderers[i].sharedMaterials = normalMaterials[i];
        }
    }

    public void SetOutline(bool show, bool seeThrough = false)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (!show)
                renderers[i].sharedMaterials = normalMaterials[i];
            else if (seeThrough)
                renderers[i].sharedMaterials = xrayMaterials[i];
            else
                renderers[i].sharedMaterials = outlineMaterials[i];
        }
    }
}
