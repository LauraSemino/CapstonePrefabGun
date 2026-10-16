using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeleteMenuManager : MonoBehaviour
{
    public static DeleteMenuManager instance;
    public static bool IsOpen = false; // Other scripts check this to know if the menu is open

    public PrefabGun prefabGun;
    public GameObject menuPanel;
    public TextMeshProUGUI listText;

    int selected = 0;
    ObjectOutline currentOutline;

    void Awake()
    {
        instance = this;
        IsOpen = false;
        menuPanel.SetActive(false);
    }

    // PrefabGun calls this when middle mouse is clicked
    public void Toggle()
    {
        IsOpen = !IsOpen;
        menuPanel.SetActive(IsOpen);
        selected = 0; // Start at the top of the list
        ClearOutline();

        if (IsOpen)
        {
            // Remove any objects that no longer exist
            prefabGun.placedObjects.RemoveAll(nuhuh => nuhuh == null);
        }
    }

    void Update()
    {
        if (!IsOpen) return;

        List<GameObject> list = prefabGun.placedObjects;

        if (list.Count == 0)
        {
            listText.text = "No placed objects :(";
            ClearOutline();
            return;
        }

        // Scroll wheel change selection
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0) selected--;
        if (scroll < 0) selected++;

        // Space deletes the selected object
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            DeleteSelected();
            if (list.Count == 0) return;
        }

        // Loop around at the ends of the list
        if (selected < 0) selected = list.Count - 1;
        if (selected >= list.Count) selected = 0;

        ShowList(list);
        Highlight(list[selected]);
    }

    // Show the list to player
    void ShowList(List<GameObject> list)
    {
        string text = "";
        for (int i = 0; i < list.Count; i++)
        {
            // If selected, show color yellow
            if (i == selected)
                text += "<color=yellow>> " + GetLabel(i) + "</color>\n";
            else
                text += "   " + GetLabel(i) + "\n";
        }
        listText.text = text;
    }

    // Name and number
    string GetLabel(int index)
    {
        ObjectData data = prefabGun.placedObjects[index].GetComponent<ObjectData>();
        return data.objData.displayName + " #" + (index + 1);
    }

    // Turn on the outline of the selected object
    void Highlight(GameObject obj)
    {
        ObjectOutline outline = obj.GetComponent<ObjectOutline>();
        if (outline == currentOutline) return;

        ClearOutline();
        currentOutline = outline;
        if (currentOutline != null) currentOutline.SetOutline(true, true);
    }

    void ClearOutline()
    {
        if (currentOutline != null) currentOutline.SetOutline(false);
        currentOutline = null;
    }

    void DeleteSelected()
    {
        GameObject obj = prefabGun.placedObjects[selected];
        ObjectData data = obj.GetComponent<ObjectData>();

        // Give the budget back then remove the object
        prefabGun.Refund(data.objData.cost);
        prefabGun.placedObjects.Remove(obj);
        Destroy(obj);
    }
}


