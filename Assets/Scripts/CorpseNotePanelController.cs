using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla o painel de anotações que já existe na cena.
/// </summary>
public class CorpseNotePanelController : MonoBehaviour
{
    public static CorpseNotePanelController Instance { get; private set; }

    [Header("Objetos da interface na hierarquia")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI pageCounterText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    private CorpseInvestigation currentCorpse;
    private int currentPage;
    private float timeScaleBeforeOpen = 1f;
    private GameObject previousSelection;

    public bool IsOpen
    {
        get
        {
            if (panelRoot == null)
                return false;

            return panelRoot.activeSelf;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void Open(CorpseInvestigation corpse)
    {
        if (corpse == null || panelRoot == null)
        {
            Debug.LogError("CorpseNotePanelController: atribua o painel no Inspector.");
            return;
        }

        if (corpse.pages == null || corpse.pages.Count == 0)
        {
            Debug.LogWarning($"{corpse.corpseName} não possui páginas cadastradas.", corpse);
            return;
        }

        if (EventSystem.current != null)
            previousSelection = EventSystem.current.currentSelectedGameObject;

        timeScaleBeforeOpen = Time.timeScale;
        Time.timeScale = 0f;
        currentCorpse = corpse;
        currentPage = 0;
        panelRoot.SetActive(true);
        RefreshPage();
        Button buttonToSelect = closeButton;
        if (nextButton != null && nextButton.interactable)
            buttonToSelect = nextButton;

        if (buttonToSelect != null)
            SelectGameObject(buttonToSelect.gameObject);
    }

    public void PreviousPage()
    {
        if (!IsOpen || currentCorpse == null || currentPage <= 0)
            return;

        currentPage--;
        RefreshPage();
    }

    public void NextPage()
    {
        if (!IsOpen || currentCorpse == null || currentPage >= currentCorpse.pages.Count - 1)
            return;

        currentPage++;
        RefreshPage();
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        panelRoot.SetActive(false);
        currentCorpse = null;
        Time.timeScale = timeScaleBeforeOpen;

        GameObject selection = null;
        if (previousSelection != null && previousSelection.activeInHierarchy)
            selection = previousSelection;

        SelectGameObject(selection);
        previousSelection = null;
    }

    private void RefreshPage()
    {
        if (currentCorpse == null || currentCorpse.pages.Count == 0)
            return;

        CorpseInvestigation.NotePage page = currentCorpse.pages[currentPage];
        if (titleText != null)
            titleText.text = string.IsNullOrWhiteSpace(page.title)
                ? currentCorpse.corpseName
                : page.title;
        if (bodyText != null)
        {
            if (page.text == null)
                bodyText.text = string.Empty;
            else
                bodyText.text = page.text;
        }
        if (pageCounterText != null)
            pageCounterText.text = $"{currentPage + 1} / {currentCorpse.pages.Count}";

        if (previousButton != null)
            previousButton.interactable = currentPage > 0;
        if (nextButton != null)
            nextButton.interactable = currentPage < currentCorpse.pages.Count - 1;
    }

    private void SelectGameObject(GameObject target)
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        if (target != null && target.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(target);
    }

    private void OnDestroy()
    {
        if (previousButton != null)
            previousButton.onClick.RemoveListener(PreviousPage);
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextPage);
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        if (Instance == this)
            Instance = null;

        if (IsOpen)
            Time.timeScale = timeScaleBeforeOpen;
    }
}
