using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    SaveData data = new SaveData();
    string savePath;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        savePath = Application.persistentDataPath + "/save.json";

        LoadGame();
    }

    void Start()
    {
        ApplySaveData();
    }

    public void CollectBudgetUpgrade(string id)
    {
        if (!data.budgetUpgrades.Contains(id))
            data.budgetUpgrades.Add(id);

        SaveGame();
    }

    public void SetLastDeleter(string id)
    {
        data.lastCheckpoint = id;
        SaveGame();
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved!");
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
                data = new SaveData();

            if (data.budgetUpgrades == null)
                data.budgetUpgrades = new List<string>();

            if (data.scannedObjects == null)
                data.scannedObjects = new List<string>();
        }
    }

    void ApplySaveData()
    {
        PrefabGun pg = FindFirstObjectByType<PrefabGun>();

        if (pg != null)
        {
            foreach (string id in data.scannedObjects)
            {
                if (int.TryParse(id, out int objectID))
                {
                    pg.RestoreScannedObject(objectID);
                }
            }
        }

        CalculateBudget();

        ApplyBudgetUpgrades();
        ApplyCheckpoint();
    }

    void ApplyBudgetUpgrades()
    {
        BudgetChanger[] upgrades = FindObjectsByType<BudgetChanger>(FindObjectsSortMode.None);

        foreach (BudgetChanger upgrade in upgrades)
        {
            SaveID saveID = upgrade.GetComponent<SaveID>();

            if (saveID != null && data.budgetUpgrades.Contains(saveID.id))
            {
                upgrade.isCollected = true;
                upgrade.gameObject.SetActive(false);
            }
        }
    }

    void ApplyCheckpoint()
    {
        if (string.IsNullOrEmpty(data.lastCheckpoint))
            return;

        SaveID[] saveIDs = FindObjectsByType<SaveID>(FindObjectsSortMode.None);

        foreach (SaveID saveID in saveIDs)
        {
            if (saveID.id == data.lastCheckpoint)
            {
                PlayerControl player = FindFirstObjectByType<PlayerControl>();

                if (player != null)
                {
                    player.spawnPos = saveID.transform.position;

                    player.rb.position = saveID.transform.position;
                    player.rb.linearVelocity = Vector3.zero;

                    Debug.Log("We load this checkpoint: " + data.lastCheckpoint);
                }

                return;
            }
        }
    }

    public void CalculateBudget()
    {
        PrefabGun pg = FindFirstObjectByType<PrefabGun>();

        if (pg == null)
            return;

        float budget = 5f;

        foreach (BudgetChanger upgrade in FindObjectsByType<BudgetChanger>(FindObjectsSortMode.None))
        {
            SaveID saveID = upgrade.GetComponent<SaveID>();

            if (saveID != null && data.budgetUpgrades.Contains(saveID.id))
            {
                budget += upgrade.setBudget;
            }
        }

        pg.maxBudget = budget;
        pg.BuildBudgetLines();
        pg.UpdateBudgetUI();
    }

    public void AddScannedObject(int id)
    {
        string objectID = id.ToString();

        if (!data.scannedObjects.Contains(objectID))
            data.scannedObjects.Add(objectID);

        SaveGame();
    }
    void Update()
    {
        if (Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            ClearSave();
        }

        if (Keyboard.current.insertKey.wasPressedThisFrame)
        {
            OpenSaveFolder();
        }
    }

    public void ClearSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            data = new SaveData();

            Debug.Log("Save file deleted!");
        }
    }

    public void OpenSaveFolder()
    {
        if (!File.Exists(savePath))
            SaveGame();

        Application.OpenURL(Application.persistentDataPath);
    }
}
