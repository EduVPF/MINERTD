using UnityEngine;

public class TorreIA : MonoBehaviour
{
    [Header("Atributos da Torre")]
    public float alcance = 5f;
    public float tempoEntreDisparos = 1f;

    [Header("Referências (Arrastar no Inspector)")]
    public GameObject projetilPrefab; 
    public Transform pontoDeDisparo;  

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


        GameObject novoProjetilGO = Instantiate(projetilPrefab, pontoDeDisparo.position, Quaternion.identity);
        
 
        Projetil projetilScript = novoProjetilGO.GetComponent<Projetil>();
        

        Vector2 direcao = alvo.position - pontoDeDisparo.position;
        projetilScript.Iniciar(direcao);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcance);
    }
}