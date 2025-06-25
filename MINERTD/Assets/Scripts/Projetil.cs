using UnityEngine;

public class Projetil : MonoBehaviour
{
    public float velocidade = 10f;
    public float dano = 25f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 3f);
    }

    public void Iniciar(Vector2 direcao)
    {
        if (rb != null)
        {
            rb.linearVelocity = direcao.normalized * velocidade;
        }
    }


    void OnTriggerEnter2D(Collider2D colisao)
    {

        Debug.Log("Projétil colidiu com o objeto: '" + colisao.gameObject.name + "' que tem a tag: '" + colisao.gameObject.tag + "'");


        if (colisao.CompareTag("Monstro"))
        {
            Debug.Log("ACERTOU! O objeto tem a tag 'Monstro'. Tentando aplicar dano...");
            
            Saude saudeDoMonstro = colisao.GetComponent<Saude>();
            if (saudeDoMonstro != null)
            {
                saudeDoMonstro.ReceberDano(dano);
                Debug.Log("Dano aplicado com sucesso!");
            }
            else
            {
                Debug.LogError("ERRO: O objeto com tag 'Monstro' não tem o script 'Saude'!");
            }
            
            Destroy(gameObject); 
        }
        else
        {
            Debug.LogWarning("ACERTOU, MAS NÃO É UM MONSTRO. O objeto não tem a tag 'Monstro' ou ela está escrita errada.");
        }
    }
}