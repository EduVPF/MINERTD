using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

// --- ESTRUTURA PARA DEFINIR UMA ONDA ---
[System.Serializable]
public class Onda
{
    public string nomeDaOnda;
    public GameObject prefabDoMonstro;
    public int quantidade;
    public float tempoEntreSpawns;
}

public class GameManager : MonoBehaviour
{
    // --- EVENTO PARA O DESTRUIDOR DE TORRES ---
    public static event Action<TorreIA> OnTorreConstruida;

    [Header("Dados do Jogo")]
    public int moedasAzuis = 500;
    public int limiteMaximoDeMineradores = 5;

    [Header("Referências da Cena (Arraste no Inspector)")]
    public TextMeshProUGUI textoMoedas;
    public TextMeshProUGUI textoContagemMineradores;
    public GameObject mineradorPrefab;
    public GameObject torrePrefab;
    public Transform pontoDeSpawn;
    public Transform localDeMineracao;
    public Transform localDaBase;
    public RecursoMineravel minaPrincipal;
    public GameObject painelGameOver;

    [Header("Referências da Loja")]
    public GameObject painelLoja;
    public Button botaoComprarTorre;
    public Button botaoUpgradeVida;
    public Button botaoUpgradeVelocidade;
    public Button botaoUpgradeCapacidade;
    public TextMeshProUGUI textoCustoVelocidade;
    public TextMeshProUGUI textoCustoVida;
    public TextMeshProUGUI textoCustoCapacidade;

    [Header("Sistema de Ondas")]
    public Onda[] ondas;
    public Transform[] pontosDeSpawnDosMonstros;
    public float tempoEntreOndas = 10f;
    public float tempoParaIniciarPrimeiraOnda = 5f;

    // --- Variáveis Internas ---
    private int mineradoresAtuais = 0;
    private int nivelUpgradeVelocidade = 0;
    private int nivelUpgradeVida = 0;
    private int nivelUpgradeCapacidade = 0;
    private GameObject torreParaPosicionar;
    private bool upgradesDesbloqueados = false;
    private int ondaAtualIndex = 0;
    private bool jogoAcabou = false;
    private int custoMinerador = 250;
    private int custoTorre = 400;

    void Start()
    {
        jogoAcabou = false;
        Time.timeScale = 1f;
        
        if (painelGameOver != null) painelGameOver.SetActive(false);
        
        mineradoresAtuais = 0;
        AtualizarTextoMoedas();
        AtualizarTextosDaLoja();
        AtualizarContagemMineradores();
        
        if (painelLoja != null) painelLoja.SetActive(false);
        if (botaoComprarTorre != null) botaoComprarTorre.interactable = false;
        if (botaoUpgradeVida != null) botaoUpgradeVida.interactable = false;
        if (botaoUpgradeVelocidade != null) botaoUpgradeVelocidade.interactable = false;
        if (botaoUpgradeCapacidade != null) botaoUpgradeCapacidade.interactable = false;

        StartCoroutine(CicloDeOndas());
    }

    void Update()
    {
        if (jogoAcabou) return;

        if (torreParaPosicionar != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            torreParaPosicionar.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
            if (Mouse.current.leftButton.wasPressedThisFrame) { PosicionarTorre(); }
            else if (Mouse.current.rightButton.wasPressedThisFrame) { CancelarPosicionamento(); }
        }

        if (upgradesDesbloqueados && mineradoresAtuais <= 0 && moedasAzuis < custoMinerador)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        jogoAcabou = true;
        Time.timeScale = 0f;
        if (painelGameOver != null)
        {
            painelGameOver.SetActive(true);
        }
    }

    public void ReiniciarJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator CicloDeOndas() { yield return new WaitForSeconds(tempoParaIniciarPrimeiraOnda); while (ondaAtualIndex < ondas.Length) { yield return StartCoroutine(SpawnOnda(ondas[ondaAtualIndex])); ondaAtualIndex++; if (ondaAtualIndex < ondas.Length) { yield return new WaitForSeconds(tempoEntreOndas); } } Debug.Log("Todas as ondas foram concluídas! VOCÊ VENCEU!"); }
    IEnumerator SpawnOnda(Onda onda) { for (int i = 0; i < onda.quantidade; i++) { SpawnMonstro(onda.prefabDoMonstro); yield return new WaitForSeconds(onda.tempoEntreSpawns); } }
    void SpawnMonstro(GameObject monstroPrefab) { if (pontosDeSpawnDosMonstros.Length == 0) return; Transform pontoDeSpawnAleatorio = pontosDeSpawnDosMonstros[UnityEngine.Random.Range(0, pontosDeSpawnDosMonstros.Length)];
 }
    public void ToggleLoja() { if (painelLoja != null) { painelLoja.SetActive(!painelLoja.activeSelf); } }
    
    public void TentarSpawnarMinerador()
    {
        if (mineradoresAtuais >= limiteMaximoDeMineradores) { return; }
        if (GastarMoedas(custoMinerador))
        {
            mineradoresAtuais++;
            AtualizarContagemMineradores();
            float vBonus = nivelUpgradeVelocidade * 0.2f;
            float hBonus = nivelUpgradeVida * 10f;
            GameObject novoGO = Instantiate(mineradorPrefab, pontoDeSpawn.position, Quaternion.identity);
            SeguirCaminho sc = novoGO.GetComponent<SeguirCaminho>();
            if (sc != null)
            {
                sc.AplicarUpgrades(vBonus, hBonus);
                sc.Inicializar(localDeMineracao, localDaBase);
            }
            if (!upgradesDesbloqueados)
            {
                upgradesDesbloqueados = true;
                if (botaoComprarTorre != null) botaoComprarTorre.interactable = true;
                if (botaoUpgradeVida != null) botaoUpgradeVida.interactable = true;
                if (botaoUpgradeVelocidade != null) botaoUpgradeVelocidade.interactable = true;
                if (botaoUpgradeCapacidade != null) botaoUpgradeCapacidade.interactable = true;
            }
        }
    }

    public void TentarComprarTorre()
    {
        if (!upgradesDesbloqueados) return;
        if (torreParaPosicionar != null) return;
        if (GastarMoedas(custoTorre))
        {
            torreParaPosicionar = Instantiate(torrePrefab);
            if(torreParaPosicionar.GetComponent<TorreIA>() != null) { torreParaPosicionar.GetComponent<TorreIA>().enabled = false; }
        }
    }
    
    public void ComprarUpgradeVelocidade() { if (!upgradesDesbloqueados) return; int custo = 100 * (nivelUpgradeVelocidade + 1); if (GastarMoedas(custo)) { nivelUpgradeVelocidade++; AtualizarTextosDaLoja(); } }
    public void ComprarUpgradeVida() { if (!upgradesDesbloqueados) return; int custo = 150 * (nivelUpgradeVida + 1); if (GastarMoedas(custo)) { nivelUpgradeVida++; AtualizarTextosDaLoja(); } }
    public void ComprarUpgradeCapacidade()
    {
        if (!upgradesDesbloqueados) return;
        int custo = 1000 + (200 * nivelUpgradeCapacidade);
        if (GastarMoedas(custo))
        {
            nivelUpgradeCapacidade++;
            limiteMaximoDeMineradores++;
            if (minaPrincipal != null) { minaPrincipal.AumentarCapacidade(); }
            else { Debug.LogError("Referência para 'Mina Principal' não configurada no GameManager!"); }
            AtualizarTextosDaLoja();
            AtualizarContagemMineradores();
        }
    }

    public void RegistrarMorteDeMinerador() { mineradoresAtuais--; if (mineradoresAtuais < 0) mineradoresAtuais = 0; AtualizarContagemMineradores(); }
    public void DepositarMoedas(int quantidade) { moedasAzuis += quantidade; AtualizarTextoMoedas(); }
    private void PosicionarTorre()
    {
        TorreIA torreScript = torreParaPosicionar.GetComponent<TorreIA>();
        if (torreScript != null)
        {
            torreScript.enabled = true;
            OnTorreConstruida?.Invoke(torreScript);
        }
        torreParaPosicionar = null;
    }
    private void CancelarPosicionamento() { DepositarMoedas(custoTorre); Destroy(torreParaPosicionar); torreParaPosicionar = null; }
    private bool GastarMoedas(int quantidade) { if (moedasAzuis >= quantidade) { moedasAzuis -= quantidade; AtualizarTextoMoedas(); return true; } else { Debug.LogWarning("Moedas insuficientes!"); return false; } }
    private void AtualizarTextoMoedas() { if (textoMoedas != null) textoMoedas.text = "Moedas: " + moedasAzuis; }
    private void AtualizarContagemMineradores() { if (textoContagemMineradores != null) textoContagemMineradores.text = $"Mineradores: {mineradoresAtuais} / {limiteMaximoDeMineradores}"; }
    private void AtualizarTextosDaLoja()
    {
        if (textoCustoVelocidade != null) textoCustoVelocidade.text = $"Velocidade Nv.{nivelUpgradeVelocidade + 1}\nCusto: {100 * (nivelUpgradeVelocidade + 1)}";
        if (textoCustoVida != null) textoCustoVida.text = $"Vida Nv.{nivelUpgradeVida + 1}\nCusto: {150 * (nivelUpgradeVida + 1)}";
        if (textoCustoCapacidade != null) textoCustoCapacidade.text = $"Vagas Mina Nv.{nivelUpgradeCapacidade + 1}\nCusto: {1000 + (200 * nivelUpgradeCapacidade)}";
    }
}