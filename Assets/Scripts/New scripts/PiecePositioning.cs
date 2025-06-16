using System.Collections.Generic;
using UnityEngine;

public class PiecePositioning : MonoBehaviour
{
    public GameObject SafePiece;
    public GameObject UnsafePiece;
    private List<int> Angles = new List<int>();
    private List<GameObject> Pieces = new List<GameObject>();

    void Start()
    {
        // Initialize angles
        Angles.AddRange(new int[] { 0, 45, 90, 135, 180, 225, 270, 315 });
        
        // Create and arrange pieces once
        PieceCreation();
    }

    private void PieceCreation()
    {
        RandomAngulation(Angles);

        // Create safe pieces
        int NumberOfSafePieces = Random.Range(3, 5);
        for (int i = 0; i < NumberOfSafePieces; i++)
        {
            GameObject piece = Instantiate(SafePiece, transform.position, Quaternion.Euler(0, Angles[i], 0));
            piece.transform.parent = gameObject.transform;
            Pieces.Add(piece);
        }

        // Create unsafe pieces
        int NumberOfUnsafePieces = Random.Range(2, 4);
        for (int j = NumberOfSafePieces; j < NumberOfSafePieces + NumberOfUnsafePieces; j++)
        {
            GameObject piece = Instantiate(UnsafePiece, transform.position, Quaternion.Euler(0, Angles[j], 0));
            piece.transform.parent = gameObject.transform;
            Pieces.Add(piece);
        }
    }

    private void RandomAngulation(List<int> angles)
    {
        for (int i = 0; i < angles.Count; i++)
        {
            int t = angles[i];
            int r = Random.Range(0, angles.Count - i);
            angles[i] = angles[i + r];
            angles[i + r] = t;
        }
    }
}