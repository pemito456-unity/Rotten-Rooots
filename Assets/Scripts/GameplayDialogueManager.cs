using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Exibe falas e pensamentos em sequência durante a gameplay sem pausar o jogo.
/// Os objetos de interface ficam na hierarquia da cena MainGame.
/// </summary>
public class GameplayDialogueManager : MonoBehaviour
{
    private struct DialogueLine
    {
        public bool IsThought;
        public string Text;

        public DialogueLine(bool isThought, string text)
        {
            IsThought = isThought;
            Text = text;
        }
    }

    public static GameplayDialogueManager Instance { get; private set; }

    [Header("Balões da cena")]
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private CanvasGroup speechGroup;
    [SerializeField] private RectTransform speechRect;
    [SerializeField] private TextMeshProUGUI speechText;
    [SerializeField] private GameObject thoughtBubble;
    [SerializeField] private CanvasGroup thoughtGroup;
    [SerializeField] private RectTransform thoughtRect;
    [SerializeField] private TextMeshProUGUI thoughtText;

    [Header("Layout e leitura")]
    [SerializeField, Min(0f)] private float bubbleBottomOffset = 370f;
    [SerializeField, Min(200f)] private float maximumBubbleWidth = 1450f;
    [SerializeField, Min(0f)] private float horizontalPadding = 64f;
    [SerializeField, Min(0f)] private float verticalPadding = 40f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.18f;
    [SerializeField, Min(1f)] private float charactersPerSecond = 19f;
    [SerializeField, Min(0f)] private float extraReadingTime = 1.2f;
    [SerializeField, Min(0f)] private float minimumReadingTime = 2.8f;

    [Header("Ato 1 — entrada em A2")]
    [SerializeField, TextArea(2, 4)] private string a2Journalist =
        "De acordo com minhas fontes esse é o lugar de descarte, a aparência está pior do que eu pensei.";
    [SerializeField, TextArea(2, 4)] private string a2Thought =
        "Vou tirar algumas fotos e ver se consigo achar alguma evidência que liga esse lugar à Poison Tech.";

    [Header("Ato 1 — primeiro cadáver")]
    [SerializeField, TextArea(3, 6)] private string firstCorpseJournalist =
        "Meu Deus, que coisa horrível. Como pode haver cadáveres aqui? Parecem membros da equipe de segurança da Poison Tech, mas o que os matou não parece ser nenhum animal conhecido… Droga, o que está acontecendo aqui?";
    [SerializeField, TextArea(2, 5)] private string firstCorpseThought =
        "Ele ainda era humano. Ninguém deveria morrer passando por uma situação dessas. O medo e o desespero… pobre coitado.";

    [Header("Ato 1 — revólver")]
    [SerializeField, TextArea(3, 6)] private string revolverJournalist =
        "Uma arma. Parece que esse cara não vai mais precisar disso. Que descanse em paz. Posso não ter treinamento, mas, se aumentar minhas chances de sobrevivência, eu aceito.";

    [Header("Ato 1 — outros cadáveres")]
    [SerializeField, TextArea(3, 6)] private string otherCorpseThought =
        "Que desgraça é essa? Tantos corpos mortos de formas brutais… Que horrível. O que pode ter feito isso? Se acalme, não perca o controle. Deve haver uma explicação para isso… tem que haver.";

    [Header("Ato 1 — primeiro macaco")]
    [SerializeField, TextArea(2, 5)] private string monkeyJournalist =
        "Não… até os animais locais sofreram mutações. Isso não deveria ser possível. Aqueles malditos da Poison finalmente destruíram todo tipo de vida com o descarte.";
    [SerializeField, TextArea(2, 4)] private string monkeyThought =
        "Eles têm que acabar. Uma empresa assim… eles vão acabar.";

    [Header("Ato 1 — primeira árvore sinistra")]
    [SerializeField, TextArea(3, 6)] private string firstTreeJournalist =
        "Que desgraça é essa? Sabia que a situação era ruim, mas essa aparência é irreal. Nunca vi uma vegetação assim… mas, estranhamente, parece mais viva do que nunca. Isso não pode ser real.";
    [SerializeField, TextArea(2, 4)] private string firstTreeThought =
        "Que coisa horrenda. Não tocarei nisso. Que mutação bizarra.";

    [Header("Ato 1 — primeiro morcego")]
    [SerializeField, TextArea(2, 3)] private string firstBatThought = "Odeio morcegos.";

    [Header("Ato 1 — primeiras fotos")]
    [SerializeField, TextArea(2, 5)] private string firstCorpsePhotoJournalist =
        "Esses desgraçados não se importam com suas ações. Olha o estado dessa vegetação. Está mais do que contaminada; está, ao que parece… mutando?";
    [SerializeField, TextArea(2, 5)] private string firstAnimalPhotoJournalist =
        "Esses desgraçados não se importam com suas ações. Olha o estado desses bichos. Estão mais do que contaminados; estão, ao que parece… mutando?";

    [Header("Ato 2 — cinco evidências")]
    [SerializeField, TextArea(3, 6)] private string halfEvidenceLine1 =
        "Como é possível essas mutações surgirem? Lixo tóxico mata a fauna e a flora, mas parece que eles acabaram dando uma nova vida a essa floresta… Não pode ser. Isso é impossível.";
    [SerializeField, TextArea(2, 5)] private string halfEvidenceLine2 =
        "Mesmo sendo da Poison Tech, esses seguranças ainda eram pessoas. Ninguém deveria morrer assim.";
    [SerializeField, TextArea(3, 6)] private string halfEvidenceLine3 =
        "Meu Deus, esses agentes da Poison Tech… Se descobrirem que o lixo é capaz de criar essas mutações, o que poderiam fazer com essa informação? Criar novos tipos de armas químicas ou até biológicas. Tenho que descobrir mais antes que eu acabe como parte dessa floresta maldita.";
    [SerializeField, TextArea(2, 5)] private string halfEvidenceLine4 =
        "Tenho que achar mais provas e dar o fora daqui antes que seja tarde demais.";
    [SerializeField, TextArea(2, 5)] private string leavingHalfEvidenceChunk =
        "Isso não é um conto de terror. Nada disso deveria ser possível. Não… não pode ser real. Tenho que sair daqui.";

    [Header("Ato 3 — penúltimo cadáver")]
    [SerializeField, TextArea(3, 6)] private string penultimateCorpseJournalist =
        "Tantos corpos, mais vidas perdidas do que as que já teriam com a contaminação. Isso parece um pesadelo vindo de um livro cruel. Mas deve haver como sair daqui, caso eu não queira me tornar parte da flora.";

    [Header("Ato 3 — quarto cadáver")]
    [SerializeField, TextArea(2, 5)] private string fourthCorpseLine1 =
        "Desgraçados! Parece que vão queimar tudo e encobrir esse acidente, e tudo vai se tornar ‘perda colateral’. Parece que os verdadeiros monstros são eles, não as criaturas daqui… Malditos.";
    [SerializeField, TextArea(3, 6)] private string fourthCorpseLine2 =
        "Então tinha outras pessoas vivas aqui comigo, e parece que a Poison Tech estava me vigiando mesmo depois de todo esse caos. Não sei como, mas tenho que tomar cuidado para não me tornar uma perda aceitável como esse pobre bastardo. Parece que ele e os outros que faleceram tinham encontrado uma saída, no C-1 ao norte daqui.";
    [SerializeField, TextArea(2, 4)] private string fourthCorpseLine3 =
        "Tenho que me apressar para não queimar junto com esse inferno.";

    private readonly Queue<DialogueLine> pendingLines = new Queue<DialogueLine>();
    private readonly HashSet<string> triggeredEvents = new HashSet<string>();
    private ChunkManager chunkManager;
    private PhotoEvidenceGallery evidenceGallery;
    private PhotoEvidenceCamera photoCamera;
    private string lastChunkID;
    private string fiveEvidenceChunkID;
    private float spawnCheckTimer;
    private bool fiveEvidenceReached;
    private bool fiveEvidenceExitPlayed;
    private bool firstTreePlayed;
    private bool firstBatPlayed;
    private bool firstMonkeyDetected;
    private bool firstCorpsePhotoPlayed;
    private bool firstAnimalPhotoPlayed;
    private Coroutine queueRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideBubbles();
    }

    private void OnEnable()
    {
        CorpseNotePanelController.NotesClosed += OnCorpseNotesClosed;
        ItemPickup2D.ItemCollected += OnItemCollected;
        MonkeyEnemy.PlayerDetected += OnMonkeyDetected;
    }

    private void Start()
    {
        chunkManager = ChunkManager.Instance;
        evidenceGallery = PhotoEvidenceGallery.Instance;
        BindPhotoCamera();
    }

    private void Update()
    {
        if (chunkManager == null)
            chunkManager = ChunkManager.Instance;
        if (evidenceGallery == null)
            evidenceGallery = PhotoEvidenceGallery.Instance;

        BindPhotoCamera();
        ObserveChunkChanges();
        ObserveEnvironmentAppearances();
        ObserveEvidenceMilestone();

        if (queueRoutine == null && pendingLines.Count > 0)
            queueRoutine = StartCoroutine(PlayPendingLines());
    }

    private void BindPhotoCamera()
    {
        if (photoCamera != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        photoCamera = player.GetComponent<PhotoEvidenceCamera>();
        if (photoCamera != null)
            photoCamera.EvidenceRegistered += OnEvidenceRegistered;
    }

    private void ObserveChunkChanges()
    {
        if (chunkManager == null || string.IsNullOrEmpty(chunkManager.CurrentChunkID))
            return;

        string currentChunkID = chunkManager.CurrentChunkID;
        if (lastChunkID == null)
        {
            lastChunkID = currentChunkID;
            OnChunkEntered(currentChunkID);
            return;
        }

        if (currentChunkID == lastChunkID)
            return;

        if (fiveEvidenceReached && !fiveEvidenceExitPlayed &&
            lastChunkID == fiveEvidenceChunkID)
        {
            fiveEvidenceExitPlayed = true;
            QueueEvent("five_evidence_exit", Journalist(leavingHalfEvidenceChunk));
        }

        lastChunkID = currentChunkID;
        OnChunkEntered(currentChunkID);
    }

    private void OnChunkEntered(string chunkID)
    {
        if (chunkID == "A2")
        {
            QueueEvent("first_enter_a2", Journalist(a2Journalist), Thought(a2Thought));
        }
    }

    private void ObserveEnvironmentAppearances()
    {
        if (chunkManager == null || string.IsNullOrEmpty(chunkManager.CurrentChunkID))
            return;

        spawnCheckTimer += Time.unscaledDeltaTime;
        if (spawnCheckTimer < 0.2f)
            return;
        spawnCheckTimer = 0f;

        if (!firstTreePlayed)
        {
            foreach (ArvoreSinistra tree in FindObjectsOfType<ArvoreSinistra>())
            {
                if (tree == null || !chunkManager.IsInCurrentChunk(tree.transform))
                    continue;

                firstTreePlayed = true;
                QueueEvent("first_hostile_tree", Journalist(firstTreeJournalist), Thought(firstTreeThought));
                break;
            }
        }

        if (!firstBatPlayed)
        {
            foreach (BatEnemy bat in FindObjectsOfType<BatEnemy>())
            {
                if (bat == null || !chunkManager.IsInCurrentChunk(bat.transform))
                    continue;

                firstBatPlayed = true;
                QueueEvent("first_bat", Thought(firstBatThought));
                break;
            }
        }
    }

    private void ObserveEvidenceMilestone()
    {
        if (fiveEvidenceReached || evidenceGallery == null || evidenceGallery.CollectedCount < 5)
            return;

        QueueFiveEvidenceSequence();
    }

    private void QueueFiveEvidenceSequence()
    {
        if (fiveEvidenceReached)
            return;

        fiveEvidenceReached = true;
        fiveEvidenceChunkID = chunkManager != null ? chunkManager.CurrentChunkID : lastChunkID;
        QueueEvent(
            "five_evidence",
            Journalist(halfEvidenceLine1),
            Journalist(halfEvidenceLine2),
            Journalist(halfEvidenceLine3),
            Thought(halfEvidenceLine4)
        );
    }

    private void OnCorpseNotesClosed(CorpseInvestigation corpse)
    {
        QueueCorpseStory(corpse);
    }

    private void OnEvidenceRegistered(EvidencePhotoTarget target)
    {
        if (target == null)
            return;

        CorpseInvestigation corpse = target.GetComponent<CorpseInvestigation>();
        if (corpse != null)
        {
            QueueCorpseStory(corpse);
            if (!firstCorpsePhotoPlayed)
            {
                firstCorpsePhotoPlayed = true;
                QueueEvent("first_corpse_photo", Journalist(firstCorpsePhotoJournalist));
            }

        }
        else if (target.category == EvidencePhotoTarget.EvidenceCategory.MutantAnimal &&
                 !firstAnimalPhotoPlayed)
        {
            firstAnimalPhotoPlayed = true;
            QueueEvent("first_animal_photo", Journalist(firstAnimalPhotoJournalist));
        }

        ObserveEvidenceMilestone();
    }

    private void QueueCorpseStory(CorpseInvestigation corpse)
    {
        if (corpse == null)
            return;

        if (corpse.storyOrder == 1)
        {
            QueueEvent("first_corpse", Journalist(firstCorpseJournalist), Thought(firstCorpseThought));
        }
        else if (corpse.storyOrder >= 2)
        {
            QueueEvent("other_corpse", Thought(otherCorpseThought));
        }

        if (corpse.storyOrder == 3)
        {
            QueueEvent("penultimate_corpse", Journalist(penultimateCorpseJournalist));
        }
        else if (corpse.storyOrder == 4)
        {
            QueueEvent(
                "fourth_corpse",
                Journalist(fourthCorpseLine1),
                Journalist(fourthCorpseLine2),
                Journalist(fourthCorpseLine3)
            );
        }
    }

    private void OnItemCollected(ItemPickup2D.ItemType itemType)
    {
        if (itemType == ItemPickup2D.ItemType.Gun)
            QueueEvent("revolver", Journalist(revolverJournalist));
    }

    private void OnMonkeyDetected(MonkeyEnemy monkey)
    {
        if (firstMonkeyDetected)
            return;

        firstMonkeyDetected = true;
        QueueEvent("first_monkey", Journalist(monkeyJournalist), Thought(monkeyThought));
    }

    private void QueueEvent(string eventID, params DialogueLine[] lines)
    {
        if (string.IsNullOrEmpty(eventID) || !triggeredEvents.Add(eventID))
            return;

        foreach (DialogueLine line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line.Text))
                pendingLines.Enqueue(line);
        }
    }

    private IEnumerator PlayPendingLines()
    {
        while (pendingLines.Count > 0)
        {
            DialogueLine line = pendingLines.Dequeue();
            yield return DisplayLine(line);
        }

        HideBubbles();
        queueRoutine = null;
    }

    private IEnumerator DisplayLine(DialogueLine line)
    {
        GameObject root = line.IsThought ? thoughtBubble : speechBubble;
        CanvasGroup group = line.IsThought ? thoughtGroup : speechGroup;
        RectTransform panel = line.IsThought ? thoughtRect : speechRect;
        TextMeshProUGUI label = line.IsThought ? thoughtText : speechText;

        GameObject otherRoot = line.IsThought ? speechBubble : thoughtBubble;
        if (otherRoot != null)
            otherRoot.SetActive(false);

        if (root == null || group == null || panel == null || label == null)
        {
            Debug.LogError("GameplayDialogueManager: confira as referências dos balões no Inspector.", this);
            yield break;
        }

        label.text = line.Text;
        ResizeBubble(panel, label, line.Text);
        root.SetActive(true);
        group.alpha = 0f;
        yield return Fade(group, 1f);

        float readingTime = Mathf.Max(
            minimumReadingTime,
            line.Text.Length / charactersPerSecond + extraReadingTime
        );
        yield return new WaitForSeconds(readingTime);
        yield return Fade(group, 0f);
        root.SetActive(false);
    }

    private void ResizeBubble(RectTransform panel, TextMeshProUGUI label, string text)
    {
        float availableTextWidth = Mathf.Max(100f, maximumBubbleWidth - horizontalPadding);
        Vector2 unconstrained = label.GetPreferredValues(text);
        float textWidth = Mathf.Min(unconstrained.x, availableTextWidth);
        Vector2 preferred = label.GetPreferredValues(text, textWidth, 0f);
        float textHeight = preferred.y;

        RectTransform textRect = label.rectTransform;
        textRect.sizeDelta = new Vector2(textWidth, textHeight);
        panel.sizeDelta = new Vector2(
            textWidth + horizontalPadding,
            textHeight + verticalPadding
        );
        panel.anchoredPosition = new Vector2(0f, bubbleBottomOffset);
    }

    private IEnumerator Fade(CanvasGroup group, float targetAlpha)
    {
        if (fadeDuration <= 0f)
        {
            group.alpha = targetAlpha;
            yield break;
        }

        float startAlpha = group.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        group.alpha = targetAlpha;
    }

    private void HideBubbles()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);
        if (thoughtBubble != null)
            thoughtBubble.SetActive(false);
    }

    private static DialogueLine Journalist(string text) => new DialogueLine(false, text);
    private static DialogueLine Thought(string text) => new DialogueLine(true, text);

    private void OnDisable()
    {
        CorpseNotePanelController.NotesClosed -= OnCorpseNotesClosed;
        ItemPickup2D.ItemCollected -= OnItemCollected;
        MonkeyEnemy.PlayerDetected -= OnMonkeyDetected;
        if (photoCamera != null)
            photoCamera.EvidenceRegistered -= OnEvidenceRegistered;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
