using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerEffect : MonoBehaviour
{
    [SerializeField]
    private Material flickerMaterial; 
    [SerializeField]
    private float minIntensity = 0.5f; // Minimum intensity of the flicker
    [SerializeField]
    private float maxIntensity = 1.5f; // Maximum intensity of the flicker
    [SerializeField]
    private float flickerSpeed = 0.1f; // Speed of the flicker effect

    private Coroutine flickerRoutine;

    void Start()
    {
        if (flickerMaterial != null)
        {
            flickerRoutine = StartCoroutine(Flicker());
        }
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            float intensity = Random.Range(minIntensity, maxIntensity);
            flickerMaterial.SetColor("_EmissionColor", flickerMaterial.color * intensity);
            yield return new WaitForSeconds(flickerSpeed);
        }
    }

    void OnDestroy()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
        }
    }
}
