using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpin : MonoBehaviour
{
    public Transform cubeT;
    public Renderer cubeRenderer;
    public float hueSpeed;
    public float saturation;
    public float value;
    
    public Vector3 rootRot;
    public float magnitude;
    [Header("Initialization Variables:")]
    public float scrollSpeed;

    float hue;

    NoiseScroller xNoise;
    NoiseScroller yNoise;

    private void Awake()
    {
        xNoise = new(scrollSpeed);
        yNoise = new(scrollSpeed);
        hue = Random.value;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hue += hueSpeed * Time.deltaTime;
        hue %= 1;
        Color c = Color.HSVToRGB(hue, value, saturation);
        cubeRenderer.material.SetColor("_Color", c);

        xNoise.Update(Time.deltaTime);
        yNoise.Update(Time.deltaTime);

        cubeT.localRotation = Quaternion.Euler(rootRot) * Quaternion.Euler(xNoise.Value * magnitude, yNoise.Value * magnitude, 0);
    }

    public class NoiseScroller
    {
        Vector2 directionSpeed;
        Vector2 pos;

        const float START_OFFSET = 10f;

        public NoiseScroller(float speed)
        {
            directionSpeed = Random.insideUnitCircle * speed;
            pos = directionSpeed * START_OFFSET;
            UpdateValue();
        }

        public float Value { get; private set; }

        void UpdateValue() => Value = (Mathf.PerlinNoise(pos.x, pos.y) - 0.5f) * 2;

        public void Update(float elapsedTime)
        {
            pos += directionSpeed * elapsedTime;
            UpdateValue();
        }
    }
}
