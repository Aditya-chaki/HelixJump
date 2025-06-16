using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RandomImageEnabler : MonoBehaviour
{
    [SerializeField] private List<Image> images = new List<Image>(); // List of UI Images to choose from

    void Start()
    {
        // Ensure there's at least one image in the list
        if (images.Count == 0)
        {
            Debug.LogWarning("No images assigned to the list!");
            return;
        }

        // Disable all images initially
        foreach (Image img in images)
        {
            img.enabled = false;
        }

        // Randomly select and enable one image
        int randomIndex = Random.Range(0, images.Count);
        images[randomIndex].enabled = true;
    }
}