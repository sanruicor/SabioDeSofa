using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class TriviaManager : MonoBehaviour
{
    public ApiResponse apiResponse;

    // ── URL base de la API ───────────────────────────────────────────
    private const string BASE_URL = "https://opentdb.com/api.php?amount=10";

    // ── Estado del juego ─────────────────────────────────────────────
    private List<TriviaQuestion> questions;
    private int currentIndex = 0;
    private int score = 0;
    private List<string> shuffledAnswers = new List<string>();

    // ── Paneles UI ───────────────────────────────────────────────────
    [Header("Panels")]
    public GameObject categoryPanel;
    public GameObject questionPanel;
    public GameObject resultPanel;

    // ── Category Panel ───────────────────────────────────────────────
    [Header("Category Panel")]
    public TMP_Dropdown categoryDropdown;
    public Button startButton;

    // ── Question Panel ───────────────────────────────────────────────
    [Header("Question Panel")]
    public TextMeshProUGUI questionNumberText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;          // 4 botones de respuesta
    public TextMeshProUGUI feedbackText;
    public Button nextButton;

    // ── Result Panel ─────────────────────────────────────────────────
    [Header("Result Panel")]
    public TextMeshProUGUI scoreText;
    public Button restartButton;

    // ── Categorías disponibles (id → nombre) ─────────────────────────
    private readonly Dictionary<int, string> categories = new Dictionary<int, string>
    {
        { 0,  "Any Category" },
        { 9,  "General Knowledge" },
        { 10, "Entertainment: Books" },
        { 11, "Entertainment: Film" },
        { 12, "Entertainment: Music" },
        { 14, "Entertainment: Television" },
        { 15, "Entertainment: Video Games" },
        { 17, "Science & Nature" },
        { 18, "Science: Computers" },
        { 19, "Science: Mathematics" },
        { 20, "Mythology" },
        { 21, "Sports" },
        { 22, "Geography" },
        { 23, "History" },
        { 24, "Politics" },
        { 25, "Art" },
        { 27, "Animals" },
        { 28, "Vehicles" },
        { 29, "Entertaiment: Comics" },
        { 30, "Science: Gadgets" },
        { 31, "Entertainment: Japanese Anime & Manga" },
        { 32, "Entertainment: Cartoon & Animations" }
    };


    void Start()
    {
        // Rellenar dropdown
        categoryDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var kv in categories)
            options.Add(kv.Value);
        categoryDropdown.AddOptions(options);

        startButton.onClick.AddListener(OnStartClicked);
        nextButton.onClick.AddListener(OnNextClicked);
        restartButton.onClick.AddListener(OnRestartClicked);

        ShowPanel(categoryPanel);
    }

    // ── Botón Start ──────────────────────────────────────────────────
    void OnStartClicked()
    {
        int selectedIndex = categoryDropdown.value;
        int categoryId = GetCategoryIdByIndex(selectedIndex);
        string url = BASE_URL + (categoryId != 0 ? $"&category={categoryId}" : "");
        StartCoroutine(FetchQuestions(url));
    }

    int GetCategoryIdByIndex(int dropdownIndex)
    {
        int i = 0;
        foreach (var kv in categories)
        {
            if (i == dropdownIndex) return kv.Key;
            i++;
        }
        return 0;
    }

    // ── Corrutina: llamada a la API ──────────────────────────────────
    IEnumerator FetchQuestions(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error en la petición: " + request.error);
                yield break;
            }

            // Deserializar JSON
            apiResponse = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);

            // ── Log requerido por el enunciado ───────────────────────
            Debug.Log($"Número de preguntas solicitadas: {apiResponse.results.Count}");
            if (apiResponse.results.Count > 0)
            {
                TriviaQuestion first = apiResponse.results[0];
                Debug.Log($"[Pregunta 1] Categoría: {first.category}");
                Debug.Log($"[Pregunta 1] Dificultad: {first.difficulty}");
                Debug.Log($"[Pregunta 1] Pregunta: {first.question}");
                Debug.Log($"[Pregunta 1] Respuesta correcta: {first.correct_answer}");
            }

            questions = apiResponse.results;
            currentIndex = 0;
            score = 0;
            ShowPanel(questionPanel);
            LoadQuestion();
        }
    }

    // ── Cargar pregunta actual ───────────────────────────────────────
    void LoadQuestion()
    {
        feedbackText.text = "";
        nextButton.gameObject.SetActive(false);

        TriviaQuestion q = questions[currentIndex];

        questionNumberText.text = $"Pregunta {currentIndex + 1} / {questions.Count}";
        categoryText.text = DecodeHtml(q.category);
        difficultyText.text = $"Dificultad: {TranslateDifficulty(q.difficulty)}";
        questionText.text = DecodeHtml(q.question);

        // Mezclar respuestas
        shuffledAnswers.Clear();
        shuffledAnswers.Add(q.correct_answer);
        shuffledAnswers.AddRange(q.incorrect_answers);
        Shuffle(shuffledAnswers);

        // Primero resetear TODOS los botones
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].gameObject.SetActive(false);
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
            answerButtons[i].onClick.RemoveAllListeners();
        }

        // Luego activar solo los que correspondan
        for (int i = 0; i < shuffledAnswers.Count; i++)
        {
            int idx = i;
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text =
                DecodeHtml(shuffledAnswers[i]);
            answerButtons[i].interactable = true;
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(idx));
        }
    }

    // ── Respuesta seleccionada ───────────────────────────────────────
    void OnAnswerSelected(int index)
    {
        // Bloquear botones
        foreach (var btn in answerButtons)
            btn.interactable = false;

        TriviaQuestion q = questions[currentIndex];
        bool correct = shuffledAnswers[index] == q.correct_answer;

        if (correct)
        {
            int points = GetPoints(q.difficulty);
            score += points;
            feedbackText.text = $"✔ ¡Correcto! +{points} punto{(points > 1 ? "s" : "")}";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = $"✘ Incorrecto. Era: {DecodeHtml(q.correct_answer)}";
            feedbackText.color = Color.red;
        }

        nextButton.gameObject.SetActive(true);
        nextButton.GetComponentInChildren<TextMeshProUGUI>().text =
            currentIndex < questions.Count - 1 ? "Siguiente →" : "Ver resultado";
    }

    // ── Botón Siguiente ──────────────────────────────────────────────
    void OnNextClicked()
    {
        currentIndex++;
        if (currentIndex < questions.Count)
        {
            LoadQuestion();
        }
        else
        {
            ShowResults();
        }
    }

    // ── Mostrar resultados ───────────────────────────────────────────
    void ShowResults()
    {
        int maxScore = 0;
        foreach (var q in questions)
            maxScore += GetPoints(q.difficulty);

        scoreText.text = $"Puntuación final\n{score} / {maxScore}";
        ShowPanel(resultPanel);
    }

    // ── Reiniciar ────────────────────────────────────────────────────
    void OnRestartClicked()
    {
        ShowPanel(categoryPanel);
    }

    // ── Helpers ──────────────────────────────────────────────────────
    void ShowPanel(GameObject panel)
    {
        categoryPanel.SetActive(panel == categoryPanel);
        questionPanel.SetActive(panel == questionPanel);
        resultPanel.SetActive(panel == resultPanel);
    }

    int GetPoints(string difficulty)
    {
        return difficulty switch
        {
            "easy" => 1,
            "medium" => 2,
            "hard" => 3,
            _ => 1
        };
    }

    string TranslateDifficulty(string d)
    {
        return d switch
        {
            "easy" => "Fácil",
            "medium" => "Media",
            "hard" => "Difícil",
            _ => d
        };
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Decodifica entidades HTML básicas que devuelve la API
    string DecodeHtml(string text)
    {
        return text
            .Replace("&quot;", "\"")
            .Replace("&#039;", "'")
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&ldquo;", "\u201C")
            .Replace("&rdquo;", "\u201D")
            .Replace("&ntilde;", "ñ")
            .Replace("&eacute;", "é")
            .Replace("&oacute;", "ó");
    }
}