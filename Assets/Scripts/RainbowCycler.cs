using UnityEngine;

public class RainbowCycler : MonoBehaviour
{
    [SerializeField]
    private Material material;
    [SerializeField]
    private float speed = 1.0f;

    private void Update()
    {
        float offset = Time.time * speed;
        material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }
}
