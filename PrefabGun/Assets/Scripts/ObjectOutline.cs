using UnityEngine;

public class ObjectOutline : MonoBehaviour
{
    private MeshRenderer[] renderers;
    private Material[][] original;

    void Awake()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        original = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
            original[i] = renderers[i].sharedMaterials;

        SetOutline(false);
    }

    public void SetOutline(bool show, bool seeThrough = false, Material outlineOverride = null)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = original[i];

            int count = 1;
            if (show) count = seeThrough ? 3 : 2;
            count = Mathf.Min(count, mats.Length);

            Material[] result = new Material[count];
            for (int j = 0; j < count; j++)
                result[j] = mats[j];

            if (show && outlineOverride != null && count >= 2)
                result[1] = outlineOverride;

            renderers[i].sharedMaterials = result;
        }
    }
}
