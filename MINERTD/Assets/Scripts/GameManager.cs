using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

// --- ESTRUTURAS PARA ONDAS MISTAS (DEFINIDAS FORA DA CLASSE PRINCIPAL) ---
[System.Serializable]
public class GrupoDeMonstros
{
    public GameObject prefabDoMonstro;
    public int quantidade;
}

[System.Serializable]
public class Onda
{
    public string nomeDaOnda;
    public GrupoDeMonstros[] gruposDeMonstros;
    public float tempoEntreSpawns;
    public Transform pontoDeSpawnEspecifico; 
}

public class GameManager : MonoBehaviour
{
    public static event Action<TorreIA> OnTorreConstruida;


    [Header("Dados do Jogo")]
    public int moedasAzuis = 500;
    public int limiteMaximoDeMineradores = 5;

    public float limiteDireitoDoMapaX = 8.5f; 

    [Header("Sistema de Pontuação")]
    public TextMeshProUGUI textoPontuacao;
    public int pontosPorMonstro = 10;
    public int pontosPorMinerio = 1;
    private int pontuacaoTotal = 0;

    [Header("Referências da Cena (Arraste no Inspector)")]
    public GameObject painelGameplayUI;
    public TextMeshProUGUI textoMoedas;
    public TextMeshProUGUI textoContagemMineradores;
    public GameObject mineradorPrefab;
    public GameObject torrePrefab;
    public Transform pontoDeSpawn;
    public Transform localDeMineracao;
    public Transform localDaBase;
    public RecursoMineravel minaPrincipal;
    public GameObject painelGameOver;
    public GameObject painelVitoria;
    public TextMeshProUGUI textoPontuacaoFinal;

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
    public static GameManager Instance { get; private set; }

    // --- Variáveis Internas ---´

    private int mineradoresAtuais = 0;
    private int nivelUpgradeVelocidade = 0;
    private int nivelUpgradeVida = 0;
    private int nivelUpgradeCapacidade = 0;
    private GameObject torreParaPosicionar;
    private bool upgradesDesbloqueados = false;
    private int ondaAtualIndex = 0;
    private bool jogoAcabou = false;

    private int totalDeMinasNoNivel;
    private int minasEsgotadas = 0;
    private int custoMinerador = 150;
    private int custoTorre = 200;
    void Awake() 
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        jogoAcabou = false;
        Time.timeScale = 1f;
        if (painelGameOver != null) painelGameOver.SetActive(false);
        if (painelVitoria != null) painelVitoria.SetActive(false);

        minasEsgotadas = 0;

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

        pontuacaoTotal = 0;
        AtualizarTextoPontuacao();
    }


    private void AtualizarTextoPontuacao()
    {
        if (textoPontuacao != null)
        {
            textoPontuacao.text = "Pontos: " + pontuacaoTotal.ToString();
        }
    }
    public void AdicionarPontosPorMorteDeMonstro()
    {
        pontuacaoTotal += pontosPorMonstro;
        AtualizarTextoPontuacao();
        Debug.Log($"+{pontosPorMonstro} pontos! Total: {pontuacaoTotal}");
    }
    private void AdicionarPontosPorMineracao(int quantidadeMinerios)
    {
        pontuacaoTotal += quantidadeMinerios * pontosPorMinerio;
        AtualizarTextoPontuacao();
        Debug.Log($"+{quantidadeMinerios * pontosPorMinerio} pontos por mineração! Total: {pontuacaoTotal}");
    }
    public void RegistrarNovaMina()
    {
        totalDeMinasNoNivel++;
        Debug.Log($"NOVA MINA REGISTRADA! Total de minas agora é: {totalDeMinasNoNivel}");
        }

void Update()
{
    if (jogoAcabou) return;

    if (torreParaPosicionar != null)
{

    ValidadorDePosicaoTorre validadorTorre = torreParaPosicionar.GetComponent<ValidadorDePosicaoTorre>();


    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    torreParaPosicionar.transform.position = new Vector3(mousePos.x, mousePos.y, 0);


    bool dentroDoLimite = mousePos.x < limiteDireitoDoMapaX;
    

    bool foraDeZonaProibida = validadorTorre.podeSerPosicionada;


    Debug.Log($"Pode construir? {dentroDoLimite && foraDeZonaProibida} | Dentro do Limite do Mapa? {dentroDoLimite} | Fora de Zona Proibida? {foraDeZonaProibida}");
    

    validadorTorre.DefinirValidade(dentroDoLimite); 

   
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
        if (dentroDoLimite && foraDeZonaProibida)
        {
            PosicionarTorre();
        }
        else
        {
            Debug.Log("Não é possível construir aqui!");
        }
    }
    else if (Mouse.current.rightButton.wasPressedThisFrame)
    {
        CancelarPosicionamento();
    }
}
        if (upgradesDesbloqueados && mineradoresAtuais <= 0 && moedasAzuis < custoMinerador)
        {
            GameOver("Você ficou sem mineradores e sem moedas!");
        }
    }
    
    IEnumerator CicloDeOndas()
    {
        yield return new WaitForSeconds(tempoParaIniciarPrimeiraOnda);
        while (ondaAtualIndex < ondas.Length)
        {
            yield return StartCoroutine(SpawnOnda(ondas[ondaAtualIndex]));
            ondaAtualIndex++;
            if (ondaAtualIndex < ondas.Length)
            {
                yield return new WaitForSeconds(tempoEntreOndas);
            }
        }
    }

    IEnumerator SpawnOnda(Onda onda)
    {
        foreach (GrupoDeMonstros grupo in onda.gruposDeMonstros)
        {
            if (grupo.prefabDoMonstro == null)
            {
                Debug.LogError("Um grupo na onda '" + onda.nomeDaOnda + "' está sem um prefab de monstro!");
                continue;
            }
            for (int i = 0; i < grupo.quantidade; i++)
            {
                SpawnMonstro(onda, grupo.prefabDoMonstro);
                yield return new WaitForSeconds(onda.tempoEntreSpawns);
            }
        }
    }

    void SpawnMonstro(Onda onda, GameObject monstroPrefab)
    {
        Transform pontoDeSpawnFinal = null;
        if (onda.pontoDeSpawnEspecifico != null)
        {
            pontoDeSpawnFinal = onda.pontoDeSpawnEspecifico;
        }
        else if (pontosDeSpawnDosMonstros.Length > 0)
        {
            pontoDeSpawnFinal = pontosDeSpawnDosMonstros[UnityEngine.Random.Range(0, pontosDeSpawnDosMonstros.Length)];
        }
        if (pontoDeSpawnFinal != null)
        {
            Instantiate(monstroPrefab, pontoDeSpawnFinal.position, Quaternion.identity);
        }
    }

    public void TentarSpawnarMinerador()
    
    {
         if (jogoAcabou) return;
        if (mineradoresAtuais >= limiteMaximoDeMineradores) { return; }
        if (GastarMoedas(custoMinerador))
        {
            mineradoresAtuais++;
            AtualizarContagemMineradores();
            float vBonus = nivelUpgradeVelocidade * 0.2f;
            float hBonus = nivelUpgradeVida * 20f;
            GameObject novoGO = Instantiate(mineradorPrefab, pontoDeSpawn.position, Quaternion.identity);
            SeguirCaminho sc = novoGO.GetComponent<SeguirCaminho>();
            if (sc != null) { sc.AplicarUpgrades(vBonus, hBonus); sc.Inicializar(localDeMineracao, localDaBase); }
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
    
private void PosicionarTorre()
{
    // Restaura a cor original da torre
    torreParaPosicionar.GetComponent<ValidadorDePosicaoTorre>()?.RestaurarCor();

    TorreIA torreScript = torreParaPosicionar.GetComponent<TorreIA>();
    if (torreScript != null)
    {
        torreScript.enabled = true;
        OnTorreConstruida?.Invoke(torreScript);
    }
    torreParaPosicionar = null;
}

    public void RegistrarMinaEsgotada()
    {
        minasEsgotadas++;
         Debug.Log($"MINA ESGOTADA! Contagem atual: {minasEsgotadas} de {totalDeMinasNoNivel}");
    VerificarCondicaoDeVitoria();
    }
private void VerificarCondicaoDeVitoria()
{

    if (minasEsgotadas >= totalDeMinasNoNivel)
    {

        if (jogoAcabou) return;
        jogoAcabou = true;


        Time.timeScale = 0f;
    if (painelGameplayUI != null) painelGameplayUI.SetActive(false);

        if (textoPontuacaoFinal != null)
        {
            textoPontuacaoFinal.text = "Pontuação Final: " + pontuacaoTotal.ToString();
        }

        if (painelVitoria != null)
        {
            painelVitoria.SetActive(true);
        }
    }
}

    private void GameOver(string motivo)
    {
        if (jogoAcabou) return;
        jogoAcabou = true;
        Time.timeScale = 0f;
        if (painelGameplayUI != null) painelGameplayUI.SetActive(false);
        Debug.Log("FIM DE JOGO! Motivo: " + motivo);
        if (painelGameOver != null) { painelGameOver.SetActive(true); }
    }

    public void ReiniciarJogo() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void ToggleLoja() { if (painelLoja != null) { painelLoja.SetActive(!painelLoja.activeSelf); } }
    public void TentarComprarTorre() {  if (jogoAcabou) return; if (!upgradesDesbloqueados) return; if (torreParaPosicionar != null) return; if (GastarMoedas(custoTorre)) { torreParaPosicionar = Instantiate(torrePrefab); if(torreParaPosicionar.GetComponent<TorreIA>() != null) { torreParaPosicionar.GetComponent<TorreIA>().enabled = false; } } }
    public void ComprarUpgradeVelocidade() { if (jogoAcabou) return; if (!upgradesDesbloqueados) return; int custo = 100 * (nivelUpgradeVelocidade + 1); if (GastarMoedas(custo)) { nivelUpgradeVelocidade++; AtualizarTextosDaLoja(); } }
    public void ComprarUpgradeVida() { if (jogoAcabou) return; if (!upgradesDesbloqueados) return; int custo = 100 * (nivelUpgradeVida + 1); if (GastarMoedas(custo)) { nivelUpgradeVida++; AtualizarTextosDaLoja(); } }
    public void ComprarUpgradeCapacidade() { if (jogoAcabou) return; if (!upgradesDesbloqueados) return; int custo = 400 + (200 * nivelUpgradeCapacidade); if (GastarMoedas(custo)) { nivelUpgradeCapacidade++; limiteMaximoDeMineradores++; if (minaPrincipal != null) { minaPrincipal.AumentarCapacidade(); } else { Debug.LogError("Referência para 'Mina Principal' não configurada no GameManager!"); } AtualizarTextosDaLoja(); AtualizarContagemMineradores(); } }
    public void RegistrarMorteDeMinerador() { mineradoresAtuais--; if (mineradoresAtuais < 0) mineradoresAtuais = 0; AtualizarContagemMineradores(); }
    public void DepositarMoedas(int quantidade)
{
    moedasAzuis += quantidade;
    AtualizarTextoMoedas();
    

    AdicionarPontosPorMineracao(quantidade);
 
}
    private void CancelarPosicionamento() { DepositarMoedas(custoTorre); Destroy(torreParaPosicionar); torreParaPosicionar = null; }
    private bool GastarMoedas(int quantidade) { if (moedasAzuis >= quantidade) { moedasAzuis -= quantidade; AtualizarTextoMoedas(); return true; } else { Debug.LogWarning("Moedas insuficientes!"); return false; } }
    private void AtualizarTextoMoedas() { if (textoMoedas != null) textoMoedas.text = "Moedas: " + moedasAzuis; }
    private void AtualizarContagemMineradores() { if (textoContagemMineradores != null) textoContagemMineradores.text = $"Mineradores: {mineradoresAtuais} / {limiteMaximoDeMineradores}"; }
    private void AtualizarTextosDaLoja() { if (textoCustoVelocidade != null) textoCustoVelocidade.text = $"Velocidade Nv.{nivelUpgradeVelocidade + 1}\nCusto: {100 * (nivelUpgradeVelocidade + 1)}"; if (textoCustoVida != null) textoCustoVida.text = $"Vida Nv.{nivelUpgradeVida + 1}\nCusto: {150 * (nivelUpgradeVida + 1)}"; if (textoCustoCapacidade != null) textoCustoCapacidade.text = $"Vagas Mina Nv.{nivelUpgradeCapacidade + 1}\nCusto: {400 + (200 * nivelUpgradeCapacidade)}"; }
}