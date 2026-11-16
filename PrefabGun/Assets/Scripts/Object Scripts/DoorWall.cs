
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class DoorWall : MonoBehaviour
{
    BoxCollider wallCollider;
    Renderer wallRenderer;
    List<GameObject> wallParts = new List<GameObject>();

    void Awake()
    {
        wallCollider = GetComponent<BoxCollider>();
        wallRenderer = GetComponent<Renderer>();
    }

    // Get door width vs wall size
    float GetDoorWidth(float worldWidth)
    {
        return worldWidth / Mathf.Abs(transform.lossyScale.x);
    }

    // get door height vs wall size
    float GetDoorHeight(float worldHeight)
    {
        return worldHeight / Mathf.Abs(transform.lossyScale.y);
    }

    // Can doorway fit on wall
    public bool CanFitDoor(DoorFrame door)
    {
        float doorWidth = GetDoorWidth(door.width);
        float doorHeight = GetDoorHeight(door.height);

        return doorWidth <= wallCollider.size.x && doorHeight <= wallCollider.size.y;
    }

    public void RebuildTheWall()
    {
        foreach (GameObject piece in wallParts)
        {
            Destroy(piece);
        }
        wallCollider.enabled = true;
        wallRenderer.enabled = true;
    }

    // Where should doorframe be on wall
    public Vector3 GetDoorPosition(Vector3 hitPoint, DoorFrame door)
    {
        // Convert hit point to wall's position
        Vector3 localHit = transform.InverseTransformPoint(hitPoint);

        float doorWidth = GetDoorWidth(door.width);
        float doorHeight = GetDoorHeight(door.height);

        // Find the edges of the wall
        float wallLeft = wallCollider.center.x - wallCollider.size.x / 2;
        float wallRight = wallCollider.center.x + wallCollider.size.x / 2;

        float wallBottom = wallCollider.center.y - wallCollider.size.y / 2;
        float wallTop = wallCollider.center.y + wallCollider.size.y / 2;

        // Keep door within horizontal edges of wall
        float doorCenter = Mathf.Clamp(localHit.x, wallLeft + doorWidth / 2, wallRight - doorWidth / 2);

        // Keep door within vertical edges of wall
        float doorBottom = Mathf.Clamp(localHit.y - doorHeight / 2, wallBottom, wallTop - doorHeight);

        // Return door's position but in world space
        Vector3 localPosition = new Vector3(doorCenter, doorBottom, wallCollider.center.z);

        return transform.TransformPoint(localPosition);
    }

    // Create the opening in wall
    public void CreateDoorway(Vector3 hitPoint, float worldDoorWidth, float worldDoorHeight, DoorFrame door)
    {
        Vector3 localHit = transform.InverseTransformPoint(hitPoint);

        // Convert doorway dimensions into wall's size
        float doorWidth = GetDoorWidth(worldDoorWidth);
        float doorHeight = GetDoorHeight(worldDoorHeight);

        // Find the edges of the wall
        float wallLeft = wallCollider.center.x - wallCollider.size.x / 2;
        float wallRight = wallCollider.center.x + wallCollider.size.x / 2;

        float wallBottom = wallCollider.center.y - wallCollider.size.y / 2;
        float wallTop = wallCollider.center.y + wallCollider.size.y / 2;

        // Keep the doorway inside the wall boundaries
        float doorCenter = Mathf.Clamp(localHit.x, wallLeft + doorWidth / 2, wallRight - doorWidth / 2);

        float doorBottom = Mathf.Clamp(localHit.y - doorHeight / 2, wallBottom, wallTop - doorHeight);

        float doorTop = doorBottom + doorHeight;

        // Calculate how much wall should remain on each side of the doorway
        float leftWidth = doorCenter - doorWidth / 2 - wallLeft;
        float rightWidth = wallRight - (doorCenter + doorWidth / 2);

        // Calculate how much wall should remain above and below the doorway
        float bottomHeight = doorBottom - wallBottom;
        float topHeight = wallTop - doorTop;

        // Create the left wall piece
        if (leftWidth > 0)
        {
            MakeDoorPart(new Vector3(wallLeft + leftWidth / 2, wallCollider.center.y, wallCollider.center.z), new Vector3(leftWidth, wallCollider.size.y, wallCollider.size.z));
        }

        // Create the right wall piece
        if (rightWidth > 0)
        {
            MakeDoorPart(new Vector3(doorCenter + doorWidth / 2 + rightWidth / 2, wallCollider.center.y, wallCollider.center.z), new Vector3(rightWidth, wallCollider.size.y, wallCollider.size.z));
        }

        // Create the wall piece below
        if (bottomHeight > 0)
        {
            MakeDoorPart(new Vector3(doorCenter, wallBottom + bottomHeight / 2, wallCollider.center.z), new Vector3(doorWidth, bottomHeight, wallCollider.size.z));
        }

        // Create the wall piece above
        if (topHeight > 0)
        {
            MakeDoorPart(new Vector3(doorCenter, doorTop + topHeight / 2, wallCollider.center.z), new Vector3(doorWidth, topHeight, wallCollider.size.z));
        }

        // Remove the original wall after creating its replacement pieces
        wallCollider.enabled = false;
        wallRenderer.enabled = false;
        door.ogWall = this;
    }

    // Create an individual piece of the remaining wall
    void MakeDoorPart(Vector3 localPosition, Vector3 localSize)
    {
        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Position and rotate the piece to match the original wall
        piece.transform.position = transform.TransformPoint(localPosition);
        piece.transform.rotation = transform.rotation;

        // Apply the original wall's scale to the new piece
        Vector3 scale = transform.lossyScale;

        piece.transform.localScale = new Vector3(localSize.x * Mathf.Abs(scale.x), localSize.y * Mathf.Abs(scale.y), localSize.z * Mathf.Abs(scale.z));

        // Give the new piece the same material as the original wall
        if (wallRenderer != null)
        {
            piece.GetComponent<Renderer>().sharedMaterial = wallRenderer.sharedMaterial;
        }
        wallParts.Add(piece);
    }
}
