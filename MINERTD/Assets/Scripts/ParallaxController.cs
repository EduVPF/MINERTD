using UnityEngine;
using UnityEngine.InputSystem;

public class ParallaxController : MonoBehaviour
{
    [Header("Camadas e Velocidades Relativas")]
    [SerializeField] private Transform[] layers;
    // Agora, a escala representa a velocidade relativa (1 = 100%, 0.1 = 10%)
    [Range(0f, 1f)]
    [SerializeField] private float[] parallaxScales;

    [Header("Controle Geral do Efeito")]
    // Esta variável controla a força MÁXIMA do efeito.
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
        // Pega a posição do mouse
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Calcula a posição do mouse como uma fração da tela (-1 a +1)
        float offsetX = (mousePosition.x - screenCenter.x) / screenCenter.x;
        float offsetY = (mousePosition.y - screenCenter.y) / screenCenter.y;

        // Itera por cada camada para movê-la
        for (int i = 0; i < layers.Length; i++)
        {
            // Calcula o deslocamento com base na força geral e na escala individual da camada
            float moveX = offsetX * parallaxStrength * parallaxScales[i];
            float moveY = offsetY * parallaxStrength * parallaxScales[i];

            // Define a nova posição alvo
            Vector3 targetPosition = new Vector3(
                initialLayerPositions[i].x + moveX,
                initialLayerPositions[i].y + moveY,
                initialLayerPositions[i].z
            );

            // Move a camada suavemente
            layers[i].position = Vector3.Lerp(layers[i].position, targetPosition, smoothing);
        }
    }
}