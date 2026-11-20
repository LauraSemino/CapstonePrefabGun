using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<string> budgetUpgrades = new List<string>();
    public List<string> scannedObjects = new List<string>();
    public string lastCheckpoint;
}
