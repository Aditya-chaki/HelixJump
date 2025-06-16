using System.Collections.Generic;
using UnityEngine;

public class SP_PiecePositioning : MonoBehaviour
{
    public GameObject SafePiece;
    public GameObject UnsafePiece;
    [SerializeField] private int aiSafePiecesMin = 3; // Lower bound for AI safe pieces
    [SerializeField] private int aiSafePiecesMax = 6; // Upper bound for AI safe pieces
    [SerializeField] private int aiUnsafePiecesMin = 0; // Lower bound for AI unsafe pieces
    [SerializeField] private int aiUnsafePiecesMax = 1; // Upper bound for AI unsafe pieces
    List<int> Angles = new List<int>();
    List<GameObject> Pieces = new List<GameObject>();

    public int PieceSeed { get; set; }

    void Start()
    {
        // Validate serialized fields
        if (aiSafePiecesMin < 0) aiSafePiecesMin = 0;
        if (aiSafePiecesMax < aiSafePiecesMin) aiSafePiecesMax = aiSafePiecesMin;
        if (aiUnsafePiecesMin < 0) aiUnsafePiecesMin = 0;
        if (aiUnsafePiecesMax < aiUnsafePiecesMin) aiUnsafePiecesMax = aiUnsafePiecesMin;

        PieceSeed = Random.Range(0, int.MaxValue);
        Angles.AddRange(new int[] { 0, 45, 90, 135, 180, 225, 270, 315 });

        var oldState = Random.state;
        Random.InitState(PieceSeed);
        PieceCreation();
        Random.state = oldState;
    }

    private void PieceCreation()
    {
        RandomAngulation(Angles);

        bool isAIRing = GetComponentInParent<SP_AIRotation>() != null;
        int NumberOfSafePieces = isAIRing ? Random.Range(aiSafePiecesMin, aiSafePiecesMax + 1) : Random.Range(4, 5);
        int NumberOfUnsafePieces = isAIRing ? Random.Range(aiUnsafePiecesMin, aiUnsafePiecesMax + 1) : Random.Range(1, 3);

        for (int i = 0; i < NumberOfSafePieces; i++)
        {
            if (i < Angles.Count) // Ensure we don't exceed available angles
            {
                Pieces.Add(Instantiate(SafePiece, transform.position, Quaternion.Euler(0, Angles[i], 0)));
                Pieces[i].transform.parent = gameObject.transform;
            }
        }

        for (int j = NumberOfSafePieces; j < NumberOfSafePieces + NumberOfUnsafePieces; j++)
        {
            if (j < Angles.Count) // Ensure we don't exceed available angles
            {
                Pieces.Add(Instantiate(UnsafePiece, transform.position, Quaternion.Euler(0, Angles[j], 0)));
                Pieces[j].transform.parent = gameObject.transform;
            }
        }

        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_PiecePositioning] Created {NumberOfSafePieces} safe pieces and {NumberOfUnsafePieces} unsafe pieces on {gameObject.name} (AI: {isAIRing})");
    }

    private void RandomAngulation(List<int> Angles)
    {
        for (int i = 0; i < Angles.Count; i++)
        {
            int t = Angles[i];
            int r = Random.Range(0, Angles.Count - i);
            Angles[i] = Angles[i + r];
            Angles[i + r] = t;
        }
    }

    void RandomPositioning(List<GameObject> Pieces, List<int> angles)
    {
        RandomAngulation(angles);
        for (int i = 0; i < Pieces.Count; i++)
        {
            Pieces[i].transform.rotation = Quaternion.Euler(0, angles[i], 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == transform.parent && other.name == "BALL")
        {
            int seed = Random.Range(0, int.MaxValue);
            var oldState = Random.state;
            Random.InitState(seed);
            RandomPositioning(Pieces, Angles);
            Random.state = oldState;
        }
    }

    public float[] GetPieceAngles()
    {
        List<float> pieceAngles = new List<float>();
        foreach (var piece in Pieces)
        {
            if (piece != null)
            {
                float angle = piece.transform.eulerAngles.y;
                if (angle < 0) angle += 360f;
                pieceAngles.Add(angle);
            }
        }
        pieceAngles.Sort();
        return pieceAngles.ToArray();
    }
}