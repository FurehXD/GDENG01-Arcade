using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerEffect : MonoBehaviour
{
    [SerializeField]
    private Material flickerMaterial;
    [SerializeField]
    private float minIntensity; // Minimum intensity of the flicker
    [SerializeField]
    private float maxIntensity; // Maximum intensity of the flicker
    [SerializeField]
    private float flickerSpeed; // Speed of the flicker effect

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
