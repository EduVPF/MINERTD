using UnityEngine;

public class ValidadorDePosicaoTorre : MonoBehaviour
{
    public bool podeSerPosicionada = true;

    private SpriteRenderer spriteRenderer;
    private Color corOriginal;

    void Awake()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            corOriginal = spriteRenderer.color;
        }
    }


    public void DefinirValidade(bool estado)
    {
        if (podeSerPosicionada)
        {
            AtualizarCor(estado);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("ZonaProibida"))
        {
            podeSerPosicionada = false;
            AtualizarCor(false);
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ZonaProibida"))
        {
            podeSerPosicionada = true;
            AtualizarCor(true);
        }
    }


    private void AtualizarCor(bool posicaoValida)
    {
        if (spriteRenderer == null) return;

        if (posicaoValida)
        {
            spriteRenderer.color = Color.green;
        }
        else
        {
            spriteRenderer.color = Color.red;
        }
    }
    

    public void RestaurarCor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = corOriginal;
        }
    }
}