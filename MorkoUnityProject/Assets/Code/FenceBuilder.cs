﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FenceBuilder : MonoBehaviour // Fence Builder v0.5.1 by Irtsa
{
    [Header("Fence prefab and its width")]
    public GameObject fencePiece;
    public float pieceWidth;
    [Header("Fence points")]
    public Vector3[] points;
    [Header("Random degree rotations across pieces (deg)")]
    public float xRotation;
    public float yRotation;
    public float zRotation;
    [Header("Random offsets across pieces (cm)")]
    public float xOffset;
    public float yOffset;
    public float zOffset;
    [HideInInspector]
    public bool anyPosModified;
    [Header("Make pieces point 100% up")]
    public bool useOnlyYRotation = false;
    [Range(0, 360)]
    [Header("Fence pieces' Y-axis rotation offset (deg)")]
    public int flipFactor;
    [Header("Fence pieces pivot misalignment offset (cm)")]
    public Vector3 pieceOffset = Vector3.zero;
    private int fencePieceAmount;
    [Space(30)]
    public bool autoUpdateFence = false;

    public bool autoParentFence = true;

    List<GameObject> fenceSides;
    List<List<GameObject>> fenceContents;

    private const int ARBITRARY_PIECE_LIMIT     = 500;
    private const int ARBITRARY_POINT_LIMIT     = 20;
    private const float ARBITRARY_MINIMUM_WIDTH = 0.01f;


    public void SetPoint(Vector3 newPos,int index)
    {
        anyPosModified = true;
        points[index] = newPos;
    }
    public void DestroyFence()
    {
        anyPosModified = false;
        fenceSides = new List<GameObject>();
        fenceContents = new List<List<GameObject>>();

        int originalChildCount = transform.childCount;

        for (int i = originalChildCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        if (fenceSides != null && fenceContents != null)
        {
            for (int i = 0; i < fenceSides.Count; i++)
            {
                DestroyImmediate(fenceSides[i]);
            }
            for (int i = 0; i < fenceSides.Count; i++)
            {
                for (int j = 0; j < fenceContents[i].Count; j++)
                {
                    DestroyImmediate(fenceContents[i][j]);
                }
            }
        }
    }

    public void BuildFence(bool forceBuild = false)
    {
        int fencePieces = 0;

        for(int i = 0; i < transform.childCount; i++)
        {
            fencePieces += transform.GetChild(i).childCount;
        }

        if (forceBuild || fencePieceAmount == fencePieces || transform.childCount == 0)
        {
            anyPosModified = false;

            if (pieceWidth > ARBITRARY_MINIMUM_WIDTH && points.Length < ARBITRARY_POINT_LIMIT && points.Length >= 2)
            {
                if (transform.childCount > 0)
                {
                    int originalChildCount = transform.childCount;

                    for (int i = originalChildCount - 1; i >= 0; i--)
                    {
                        DestroyImmediate(transform.GetChild(i).gameObject);
                    }
                    

                }
                if (fenceSides != null && fenceContents != null)
                {
                    for (int i = 0; i < fenceSides.Count; i++)
                    {
                        DestroyImmediate(fenceSides[i]);
                    }
                    for (int i = 0; i < fenceSides.Count; i++)
                    {
                        for (int j = 0; j < fenceContents[i].Count; j++)
                        {
                            DestroyImmediate(fenceContents[i][j]);
                        }
                    }
                }
 
                fenceSides = new List<GameObject>();
                fenceContents = new List<List<GameObject>>();
                for (int i = 0; i < points.Length - 1; i++)
                {
                    fenceSides.Add(new GameObject("Fence Side " + (i + 1).ToString()));
                }

                int totalObjects = 0;
                for (int j = 1; j < points.Length; j++)
                {
                    Vector3 endPosFinal = points[j-1];
                    Vector3 startPosFinal = points[j];

                    // Piece separation
                    float distanceToEndPos = Vector3.Distance(startPosFinal, endPosFinal);
                    int steps = (int)(distanceToEndPos / pieceWidth);
                    float amplitude = 1 / (float)steps;

                    // Base angle for pieces
                    Quaternion fenceAngle = Quaternion.LookRotation(endPosFinal - startPosFinal, Vector3.up) * Quaternion.Euler(0, flipFactor, 0);

                    // Arbitrary step limit check
                    if (steps < ARBITRARY_PIECE_LIMIT)
                    {
                        List<GameObject> tempList = new List<GameObject>();
                        for (int i = 0; i < steps; i++)
                        {
                            Quaternion pieceRotation = fenceAngle;

                            pieceRotation = Quaternion.Euler(
                                Random.Range(-xRotation, xRotation), 
                                fenceAngle.eulerAngles.y + Random.Range(-yRotation, yRotation),
                                Random.Range(-zRotation, zRotation));

                            if (useOnlyYRotation)
                            {
                                pieceRotation = Quaternion.Euler(0, fenceAngle.eulerAngles.y, 0);
                            }
                            GameObject instance = Instantiate(fencePiece, Vector3.zero, pieceRotation);

                            Vector3 combinedRandomOffsets = new Vector3(
                                Random.Range(-xOffset, xOffset) / 100,
                                Random.Range(-yOffset, yOffset) / 100,
                                Random.Range(-zOffset, zOffset) / 100
                                );
                            instance.transform.position = 
                                (startPosFinal + (pieceOffset/100) + combinedRandomOffsets) + 
                                (endPosFinal - startPosFinal) * (amplitude * i);

                            tempList.Add(instance);//
                        }
                        totalObjects += steps;
                        fenceContents.Add(tempList);//
                    }
                    else
                    {
                        Debug.LogError($"(FenceBuilder) Trying to instantiate more than {ARBITRARY_PIECE_LIMIT} fence pieces ({steps})");
                    }
                }

                fencePieceAmount = totalObjects;          
            }

            else
            {
                if (pieceWidth < ARBITRARY_MINIMUM_WIDTH)
                    Debug.LogError($"(FenceBuilder) Insert legal fence width value above {ARBITRARY_MINIMUM_WIDTH}");
                if(points.Length > ARBITRARY_POINT_LIMIT)
                    Debug.LogError($"(FenceBuilder) Insert legal point amount under {ARBITRARY_POINT_LIMIT}");
                if(points.Length < 2)
                    Debug.LogError($"(FenceBuilder) Not enough points");
            }
                
        }
        else
        {
            Debug.LogWarning("(FenceBuilder) Fence modified, click Build Fence button to override");
        }
        if(autoParentFence)
            ParentFenceToRoot();
    }
    public void ParentFenceToRoot()
    {
        if(fenceSides != null && fenceContents != null)
        {
            for (int i = 0; i < fenceSides.Count; i++)
            {
                for (int j = 0; j < fenceContents[i].Count; j++)
                {
                    if (fenceContents[i][j] != null)
                        fenceContents[i][j].transform.SetParent(fenceSides[i].transform);
                    else
                        fenceContents[i].Remove(fenceContents[i][j]);
                }

            }
            for (int i = fenceSides.Count - 1; i >= 0; i--)
            {
                fenceSides[i].transform.SetParent(transform);
            }
        }
        

    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;

        foreach(var pos in points)
        {
            Gizmos.DrawLine(pos, pos + (Vector3.up * 3f));
        }

        for (int i = 1; i < points.Length; i++)
        {
            Gizmos.DrawLine(points[i - 1], points[i]);
            Gizmos.DrawLine(points[i - 1] + (Vector3.up * 3f), points[i] + (Vector3.up * 3f));
        }
    }
}
