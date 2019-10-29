using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FenceBuilder : MonoBehaviour
{
    public GameObject fencePiece;
    public float fenceWidth;
    [HideInInspector]
    public Vector3 endPos;
    public float yRotation;
    public float xRotation;
    [HideInInspector]
    public bool endPosModified;
    public bool useOnlyYRotation = false;
    [Range(0, 360)]
    [Header("Add Degrees To Fence Pieces")]
    public int flipFactor;
    private int fencePieceAmount;
    public void SetEndPosCorrected(Vector3 newPos)
    {
        endPosModified = true;
        endPos = newPos;
    }
    private const int arbitraryPieceLimit = 500;
    public void BuildFence(bool forceBuild = false)
    {
        
        if(forceBuild || fencePieceAmount == transform.GetChild(0).childCount)
        {

            endPosModified = false;

            if (fenceWidth > 0)
            {
                if(transform.childCount > 0)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        DestroyImmediate(transform.GetChild(i).gameObject);
                    }
                }


                Vector3 endPosFinal = endPos - transform.position;
                
                // Piece separation
                float distanceToEndPos = Vector3.Distance(transform.position, endPos);
                int steps = (int)(distanceToEndPos / fenceWidth);
                float amplitude = 1 / (float)steps;

                // Proxy parent for less destroy events
                GameObject fenceProxyParent = new GameObject("Parent");
                fenceProxyParent.transform.SetParent(transform);

                // Base angle for pieces
                Quaternion fenceAngle = Quaternion.LookRotation(endPosFinal, Vector3.up)*Quaternion.Euler(0,flipFactor,0);

                // Arbitrary step limit check
                if (steps < arbitraryPieceLimit)
                {
                    for (int i = 0; i < steps; i++)
                    {
                        Quaternion pieceRotation = fenceAngle;
                        if(useOnlyYRotation)
                        {
                            pieceRotation = Quaternion.Euler(0, fenceAngle.eulerAngles.y, 0);
                        }
                        
                        GameObject instance = Instantiate(fencePiece,Vector3.zero, pieceRotation, fenceProxyParent.transform);

                        instance.transform.position = transform.position + endPosFinal * (amplitude * i);

                    }

                    fencePieceAmount = steps;
                }  
                else
                {
                    Debug.LogError($"(FenceBuilder) Trying to instantiate more than {arbitraryPieceLimit} fence pieces ({steps})");
                }
            }
        

            else
                Debug.LogError("(FenceBuilder) Insert legal fence width value");
        }
        else
        {
            Debug.LogWarning("(FenceBuilder) Fence modified, click Build Fence button to override");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPos,0.5f);

        Gizmos.DrawLine(transform.position, endPos);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position,0.5f);
    }
}
