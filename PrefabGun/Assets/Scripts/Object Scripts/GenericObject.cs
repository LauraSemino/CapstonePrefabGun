using UnityEngine;

[CreateAssetMenu(fileName = "GenericObject", menuName = "Scriptable Objects/GenericObject")]
public class GenericObject : ScriptableObject
{
    public int id = 0;
    public string displayName = string.Empty;
}
