using UnityEngine;

public class TorreIA : MonoBehaviour
{
    [Header("Atributos da Torre")]
    public float alcance = 5f;
    public float tempoEntreDisparos = 1f;
    // O dano agora está no projétil, mas podemos deixar aqui para referência se quisermos
    // public float danoPorDisparo = 25f; 

    [Header("Referências (Arrastar no Inspector)")]
    public GameObject projetilPrefab; // O "molde" do nosso projétil
    public Transform pontoDeDisparo;  // Um ponto de onde o projétil vai sair

    private Transform alvo;
    private float timerDisparo;

    void Update()
    {
        if (alvo == null || !alvo.gameObject.activeInHierarchy)
        {
            EncontrarNovoAlvo();
        }

        if (alvo != null)
        {
            if (Vector2.Distance(transform.position, alvo.position) > alcance)
            {
                alvo = null;
                return;
            }

            // Não precisamos mais mirar, a torre é estática

            timerDisparo += Time.deltaTime;
            if (timerDisparo >= tempoEntreDisparos)
            {
                timerDisparo = 0f;
                Atirar();
            }
        }
    }

    void EncontrarNovoAlvo()
    {
        MonstroAI[] monstros = FindObjectsByType<MonstroAI>(FindObjectsSortMode.None);
        Transform alvoMaisProximo = null;
        float menorDistancia = Mathf.Infinity;

        foreach (MonstroAI monstro in monstros)
        {
            float distancia = Vector2.Distance(transform.position, monstro.transform.position);
            if (distancia < menorDistancia && distancia <= alcance)
            {
                menorDistancia = distancia;
                alvoMaisProximo = monstro.transform;
            }
        }
        alvo = alvoMaisProximo;
    }

    void Atirar()
    {
        if (alvo == null || projetilPrefab == null || pontoDeDisparo == null) return;

        Debug.Log(gameObject.name + " atirando em " + alvo.name);

        // 1. Cria uma cópia do nosso projétil no ponto de disparo
        GameObject novoProjetilGO = Instantiate(projetilPrefab, pontoDeDisparo.position, Quaternion.identity);
        
        // 2. Pega o script do projétil que acabamos de criar
        Projetil projetilScript = novoProjetilGO.GetComponent<Projetil>();
        
        // 3. Calcula a direção para o alvo e manda o projétil ir nessa direção
        Vector2 direcao = alvo.position - pontoDeDisparo.position;
        projetilScript.Iniciar(direcao);
    }

    // Desenha o alcance no Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcance);
    }
}