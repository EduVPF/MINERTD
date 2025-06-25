using UnityEngine;
using UnityEngine.InputSystem;

public class ParallaxController : MonoBehaviour
{
    [Header("Camadas e Velocidades Relativas")]
    [SerializeField] private Transform[] layers;

    [Range(0f, 1f)]
    [SerializeField] private float[] parallaxScales;

    [Header("Controle Geral do Efeito")]

    [SerializeField] private float parallaxStrength = 0.6f;
    [SerializeField] private float smoothing = 0.1f;

    private Vector3[] initialLayerPositions;
    private Vector2 screenCenter;

    void Start()
    {
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        initialLayerPositions = new Vector3[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            initialLayerPositions[i] = layers[i].position;
        }
    }

    void Update()
    {

        Vector2 mousePosition = Mouse.current.position.ReadValue();


        float offsetX = (mousePosition.x - screenCenter.x) / screenCenter.x;
        float offsetY = (mousePosition.y - screenCenter.y) / screenCenter.y;

        for (int i = 0; i < layers.Length; i++)
        {

            float moveX = offsetX * parallaxStrength * parallaxScales[i];
            float moveY = offsetY * parallaxStrength * parallaxScales[i];

            Vector3 targetPosition = new Vector3(
                initialLayerPositions[i].x + moveX,
                initialLayerPositions[i].y + moveY,
                initialLayerPositions[i].z
            );

            layers[i].position = Vector3.Lerp(layers[i].position, targetPosition, smoothing);
        }
    }
}