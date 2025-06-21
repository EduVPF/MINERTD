using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Recursos do Jogador")]
    public int moedasAzuis;
    public TextMeshProUGUI textoMoedas;

    [Header("Configurações de Spawn")]
    public GameObject mineradorPrefab;
    public Transform pontoDeSpawn;

    [Header("Alvos do Nível")]
    public Transform localDeMineracao;
    public Transform localDaBase;

    private int custoMinerador = 250;

    void Start()
    {
        moedasAzuis = 500;
        AtualizarTextoMoedas();
    }

    public void TentarSpawnarMinerador()
    {
        if (GastarMoedas(custoMinerador))
        {
            Debug.Log("Contratando novo minerador!");
            
            GameObject novoMineradorGO = Instantiate(mineradorPrefab, pontoDeSpawn.position, Quaternion.identity);

            SeguirCaminho scriptDoMinerador = novoMineradorGO.GetComponent<SeguirCaminho>();

            if (scriptDoMinerador != null)
            {
                // AQUI ESTÁ A MUDANÇA: Chamamos o método de inicialização
                // e passamos os alvos que o GameManager conhece.
                scriptDoMinerador.Inicializar(this.localDeMineracao, this.localDaBase);
            }
        }
    }
    
    // ... resto do código sem alterações ...
    public bool GastarMoedas(int quantidade)
    {
        if (moedasAzuis >= quantidade)
        {
            moedasAzuis -= quantidade;
            AtualizarTextoMoedas();
            return true;
        }
        else
        {
            Debug.Log("Moedas insuficientes!");
            return false;
        }
    }

    void AtualizarTextoMoedas()
    {
        if (textoMoedas != null)
        {
            textoMoedas.text = "Moedas: " + moedasAzuis;
        }
    }
}