using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

// 問題データを格納するためのクラス
public class Question
{
    public string displayText;      // A列: 表示用テキスト (例: 「花火」)
    public string baseRomajiText;   // B列: 基本のローマ字 (例: 「HANABI」)
    public List<string> answerPatterns = new List<string>(); // 生成された正解パターン (例: 「ANAHIB」)

    public Question(string display, string answer)
    {
        displayText = display;
        baseRomajiText = answer;
    }
}


public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject StartPanel;
    [SerializeField] GameObject GamePanel;
    [SerializeField] GameObject ResultPanel;
    [SerializeField] RectTransform Obj1;
    [SerializeField] RectTransform Obj2;
    [SerializeField] RectTransform Obj3;
    [SerializeField] GameObject Easy_Waku;
    [SerializeField] TextMeshProUGUI Easy_Moji;
    [SerializeField] GameObject Normal_Waku;
    [SerializeField] TextMeshProUGUI Normal_Moji;
    [SerializeField] GameObject Hard_Waku;
    [SerializeField] TextMeshProUGUI Hard_Moji;
    private TextMeshProUGUI correctAnswerText;

    private string currentInput = ""; // プレイヤーの現在の入力文字列
    private bool isInputEnabled = true;

    private List<Question> questions = new List<Question>();
    private int currentQuestionIndex = 0;

    [SerializeField] TextMeshProUGUI gameTimerText;
    [SerializeField] TextMeshProUGUI gameOverText;
    public float gameTime = 60f;
    private float currentGameTime = 0f;
    private bool isGameActive = false;

    private int currentObjectIndex = 0;
    private List<RectTransform> objects = new List<RectTransform>();
    private Vector2 startPos;
    private Vector2 endPos;
    public float moveDuration = 10f;
    private float moveTimer = 0f;
    private bool isMoving = false;
    private float objectSpacing = 200f;

    [Header("Result UI")]
    [SerializeField] TextMeshProUGUI totalTypesText;
    [SerializeField] TextMeshProUGUI missTypesText;
    [SerializeField] TextMeshProUGUI accuracyText;

    private int totalTypes = 0;
    private int missTypes = 0;

    [Header("Game UI")]
    [SerializeField] TextMeshProUGUI correctCountText;
    [SerializeField] TextMeshProUGUI resultCorrectCountText;
    [SerializeField] private TextMeshProUGUI timeBonusText;

    [SerializeField] GameObject EasyButton;
    [SerializeField] GameObject NormalButton;
    [SerializeField] GameObject HardButton;

    private int correctCount = 0;
    private int consecutiveCorrectTypes = 0;

    private BGMManager bgmManager;
    private Boolean isSelectDifficulty = false;

    // --- ADDED --- ▼▼▼
    // 難易度を管理するためのenum（列挙型）
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }
    private Difficulty selectedDifficulty;
    // --- ADDED --- ▲▲▲

    // ▼▼▼ ローマ字の音節を「子音」「母音」に分解するための対応表 ▼▼▼
    // キー: ローマ字表記, 値: (子音部, 母音部) のタプル
    // ここを編集すれば、例外処理を自由に追加できます。
    private static readonly Dictionary<string, (string consonant, string vowel)> romajiSyllables = new Dictionary<string, (string, string)>
    {
        // 母音
        {"A", ("", "A")}, {"I", ("", "I")}, {"U", ("", "U")}, {"E", ("", "E")}, {"O", ("", "O")},
        // K
        {"KA", ("K", "A")}, {"KI", ("K", "I")}, {"KU", ("K", "U")}, {"KE", ("K", "E")}, {"KO", ("K", "O")},
        // S
        {"SA", ("S", "A")}, {"SI", ("S", "I")}, {"SU", ("S", "U")}, {"SE", ("S", "E")}, {"SO", ("S", "O")},
        // T
        {"TA", ("T", "A")}, {"TI", ("T", "I")}, {"TU", ("T", "U")}, {"TE", ("T", "E")}, {"TO", ("T", "O")},
        // N
        {"NA", ("N", "A")}, {"NI", ("N", "I")}, {"NU", ("N", "U")}, {"NE", ("N", "E")}, {"NO", ("N", "O")},
        // H
        {"HA", ("H", "A")}, {"HI", ("H", "I")}, {"HU", ("H", "U")}, {"HE", ("H", "E")}, {"HO", ("H", "O")},
        // M
        {"MA", ("M", "A")}, {"MI", ("M", "I")}, {"MU", ("M", "U")}, {"ME", ("M", "E")}, {"MO", ("M", "O")},
        // Y
        {"YA", ("Y", "A")}, {"YU", ("Y", "U")}, {"YO", ("Y", "O")},
        // R
        {"RA", ("R", "A")}, {"RI", ("R", "I")}, {"RU", ("R", "U")}, {"RE", ("R", "E")}, {"RO", ("R", "O")},
        // W
        {"WA", ("W", "A")}, {"WO", ("W", "O")},
        // ん
        {"N", ("N", "")}, {"NN", ("NN", "")},
        // G
        {"GA", ("G", "A")}, {"GI", ("G", "I")}, {"GU", ("G", "U")}, {"GE", ("G", "E")}, {"GO", ("G", "O")},
        // Z
        {"ZA", ("Z", "A")}, {"ZI", ("Z", "I")}, {"ZU", ("Z", "U")}, {"ZE", ("Z", "E")}, {"ZO", ("Z", "O")},
        // D
        {"DA", ("D", "A")}, {"DI", ("D", "I")}, {"DU", ("D", "U")}, {"DE", ("D", "E")}, {"DO", ("D", "O")},
        // B
        {"BA", ("B", "A")}, {"BI", ("B", "I")}, {"BU", ("B", "U")}, {"BE", ("B", "E")}, {"BO", ("B", "O")},
        // P
        {"PA", ("P", "A")}, {"PI", ("P", "I")}, {"PU", ("P", "U")}, {"PE", ("P", "E")}, {"PO", ("P", "O")},
        // -
        {"-", ("", "-")},

        // --- 入力のゆれ（例外）定義 ---
        {"CA", ("C", "A")}, // か
        {"CU", ("C", "U")}, // く
        {"QU", ("Q", "U")}, // く
        {"CO", ("C", "O")}, // こ
        {"SHI", ("SH", "I")}, // し
        {"CI", ("C", "I")}, // し
        {"CE", ("C", "E")}, // せ
        {"CHI", ("CH", "I")}, // ち
        {"TSU", ("TS", "U")}, // つ
        {"FU", ("F", "U")}, // ふ
        {"JI", ("J", "I")}, // じ
        // 必要に応じて {"TI", ("T", "I")} -> {"CHI", ("CH", "I")} のように基本形をCHIにするなど調整してください
    };

    // --- ここから追加 --- ▼▼▼
    // ▼▼▼ 同じ音とみなすローマ字のグループを定義 ▼▼▼
    // ここにグループを追加・編集することで、入力のゆれに柔軟に対応できます。
    private static readonly List<List<string>> equivalentRomajiGroups = new List<List<string>>
    {
        new List<string> { "KA", "CA" }, // 「か」 のグループ
        new List<string> { "KU", "CU", "QU" }, // 「く」 のグループ
        new List<string> { "KO", "CO" }, // 「こ」 のグループ
        new List<string> { "SI", "SHI", "CI" }, // 「し」のグループ
        new List<string> { "SE", "CE" }, // 「せ」 のグループ
        new List<string> { "TI", "CHI" },      // 「ち」のグループ
        new List<string> { "TU", "TSU" },      // 「つ」のグループ
        new List<string> { "HU", "FU" },       // 「ふ」のグループ
        new List<string> { "ZI", "JI" },        // 「じ」のグループ
        new List<string> { "N", "NN" }        // 「ん」のグループ
        // 必要に応じて他のグループも追加してください
    };
    // --- ここまで追加 --- ▲▲▲

    // 音節のキーを長い順にソートしたもの (前方一致検索用)
    private static List<string> sortedSyllableKeys;


    enum Scene { Start, Game, Result, }
    Scene Current = Scene.Start;

    private void Awake()
    {
        // 処理を高速化するため、キーを長い順に一度だけソートしておく
        sortedSyllableKeys = romajiSyllables.Keys.OrderByDescending(k => k.Length).ToList();
        // LoadQuestionsFromCSV();
    }

    private void Start()
    {
        bgmManager = UnityEngine.Object.FindFirstObjectByType<BGMManager>();
        ShowScene(StartPanel);
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }

    void LoadQuestionsFromCSV(string csvFileName)
    {
        questions.Clear();
        TextAsset csvFile = Resources.Load(csvFileName) as TextAsset;
        if (csvFile == null)
        {
            Debug.LogError($"Resourcesフォルダに {csvFileName} が見つかりません。");
            return;
        }

        StringReader reader = new StringReader(csvFile.text);

        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');
            if (values.Length >= 2)
            {
                // CSVから読み込んだ基本ローマ字を元に、正解パターンを生成する
                var question = new Question(values[0], values[1].ToUpper());
                GenerateAnswerPatterns(question);
                if (question.answerPatterns.Any())
                {
                    questions.Add(question);
                }
            }
        }
        Debug.Log(questions.Count + $"件の問題を {csvFileName}.csv から読み込み、正解パターンを生成しました。");
    }

    // ▼▼▼ 新しいゲームルールのための最重要メソッド ▼▼▼
    void GenerateAnswerPatterns(Question question)
    {
        // 再帰的にすべてのパターンを探索
        GeneratePatternsRecursive(question.baseRomajiText, "", question.answerPatterns);
    }

    // このメソッドの中身をまるごと入れ替えてください
    void GeneratePatternsRecursive(string remainingRomaji, string currentPath, List<string> solutions)
    {
        // 残りのローマ字がなくなったら、完成したパターンをソリューションに追加
        if (string.IsNullOrEmpty(remainingRomaji))
        {
            // 重複を避ける
            if (!solutions.Contains(currentPath))
            {
                solutions.Add(currentPath);
            }
            return;
        }

        // remainingRomajiの先頭に一致する最長の音節を探す
        string matchedKey = sortedSyllableKeys.FirstOrDefault(key => remainingRomaji.StartsWith(key));

        // 一致する音節が見つかった場合
        if (matchedKey != null)
        {
            // このキーが同音グループに属するかチェック
            var group = equivalentRomajiGroups.FirstOrDefault(g => g.Contains(matchedKey));

            // グループに属する場合、グループ内のすべての表記でパターンを生成
            if (group != null)
            {
                foreach (var equivalentKey in group)
                {
                    if (romajiSyllables.TryGetValue(equivalentKey, out var syllable))
                    {
                        string swapped = syllable.vowel + syllable.consonant;
                        GeneratePatternsRecursive(
                            remainingRomaji.Substring(matchedKey.Length), // 元のキーの長さで切り取る
                            currentPath + swapped,
                            solutions);
                    }
                }
            }
            else // グループに属さない場合、通常通り処理
            {
                var syllable = romajiSyllables[matchedKey];
                string swapped = syllable.vowel + syllable.consonant;
                GeneratePatternsRecursive(
                    remainingRomaji.Substring(matchedKey.Length),
                    currentPath + swapped,
                    solutions);
            }
        }
    }

    private void InitializeObjects()
    {
        Obj1.gameObject.SetActive(false);
        Obj2.gameObject.SetActive(false);
        Obj3.gameObject.SetActive(false);
        objects.Clear();
        //if (Obj1 != null) objects.Add(Obj1);
        //if (Obj2 != null) objects.Add(Obj2);
        //if (Obj3 != null) objects.Add(Obj3);

        switch (selectedDifficulty)
        {
            case Difficulty.Easy:
                if (Obj1 != null) objects.Add(Obj1); // EasyならObj1をリストに追加
                break;
            case Difficulty.Normal:
                if (Obj2 != null) objects.Add(Obj2); // NormalならObj2をリストに追加
                break;
            case Difficulty.Hard:
                if (Obj3 != null) objects.Add(Obj3); // HardならObj3をリストに追加
                break;
        }
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] != null)
            {
                Vector2 initialPos = new Vector2(-477f - (i * objectSpacing), objects[i].anchoredPosition.y);
                objects[i].anchoredPosition = initialPos;
                objects[i].gameObject.SetActive(false);
            }
        }
    }

    private void InitializeCorrectAnswer()
    {
        if (questions.Count > 0)
        {
            for (int i = questions.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                var temp = questions[i];
                questions[i] = questions[randomIndex];
                questions[randomIndex] = temp;
            }

            currentQuestionIndex = 0;
            currentInput = "";
            UpdateCorrectAnswerDisplay();
        }
    }

    private void UpdateCorrectAnswerDisplay()
    {
        if (correctAnswerText != null && questions.Count > 0)
        {
            var currentQuestion = questions[currentQuestionIndex];
            string display = currentQuestion.displayText;
            // 正解パターンのうち、基本となるものを一つ表示
            string answerGuide = currentQuestion.answerPatterns.First();

            // 入力済みの部分を緑色で表示
            string coloredAnswer = $"<color=green>{currentInput}</color>";

            correctAnswerText.text = $"{display}\n{coloredAnswer}";
            bgmManager.PlaySound(bgmManager.TypingSound);
        }
    }

    // --- ADDED --- ▼▼▼
    // ボタンから呼び出すためのpublicメソッド
    // int (0=Easy, 1=Normal, 2=Hard) を受け取り、難易度を設定してゲームを開始します。
    public void SelectDifficultyAndStart(int difficulty)
    {
        selectedDifficulty = (Difficulty)difficulty;
        switch (difficulty)
        {
            case 0:
                SetButtonColor(EasyButton);
                SetWakuMoji("Easy");
                break;
            case 1:
                SetButtonColor(NormalButton);
                SetWakuMoji("Normal");
                break;
            case 2:
                SetButtonColor(HardButton);
                SetWakuMoji("Hard");
                break;
        }
    }

    private void SetWakuMoji(string str)
    {
        switch (str)
        {
            case "Easy":
                Easy_Waku.SetActive(true);
                Normal_Waku.SetActive(false);
                Hard_Waku.SetActive(false);
                correctAnswerText = Easy_Moji;
                break;
            case "Normal":
                Easy_Waku.SetActive(false);
                Normal_Waku.SetActive(true);
                Hard_Waku.SetActive(false);
                correctAnswerText = Normal_Moji;
                break;
            case "Hard":
                Easy_Waku.SetActive(false);
                Normal_Waku.SetActive(false);
                Hard_Waku.SetActive(true);
                correctAnswerText = Hard_Moji;
                break;
        }
    }
    private void SetButtonColor(GameObject ButtonName)
    {
        EasyButton.GetComponent<Image>().color= Color.white;
        NormalButton.GetComponent<Image>().color= Color.white;
        HardButton.GetComponent<Image>().color= Color.white;

        EasyButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        NormalButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        HardButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;

        ButtonName.GetComponent<Image>().color= Color.yellow;
        ButtonName.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        isSelectDifficulty = true;
    }
    // --- ADDED --- ▲▲▲

    public void MoveScene()
    {
        switch (Current)
        {
            case Scene.Start:
                if (isSelectDifficulty)
                {
                    Current = Scene.Game;
                    ShowScene(GamePanel); StartGame();
                }
                break;
            case Scene.Game:
                Current = Scene.Result;
                ShowScene(ResultPanel);
                break;
            case Scene.Result:
                Current = Scene.Start;
                ShowScene(StartPanel);
                bgmManager.PlaySound(bgmManager.RestartSound);
                break;
        }
    }

    private void StartGame()
    {
        // --- ADDED --- ▼▼▼
        // 選択された難易度に応じてCSVファイルを決定
        string csvFileName = "";
        switch (selectedDifficulty)
        {
            case Difficulty.Easy:
                csvFileName = "questions_easy";
                break;
            case Difficulty.Normal:
                csvFileName = "questions_normal";
                break;
            case Difficulty.Hard:
                csvFileName = "questions_hard";
                break;
        }
        // CSVをロード
        LoadQuestionsFromCSV(csvFileName);
        // --- ADDED --- ▲▲▲

        bgmManager.PlaySound(bgmManager.StartSound);
        isGameActive = true;
        currentGameTime = 0f;
        isInputEnabled = true;

        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        if (timeBonusText != null) timeBonusText.gameObject.SetActive(false);

        InitializeCorrectAnswer();

        totalTypes = 0;
        missTypes = 0;
        correctCount = 0;
        UpdateCorrectCountDisplay();
        consecutiveCorrectTypes = 0;

        InitializeObjects();
        StartObjectMovement();
    }

    private void Update()
    {
        HandleKeyboardInput();
        UpdateObjectMovement();

        if (isGameActive && Current == Scene.Game)
        {
            currentGameTime += Time.deltaTime;
            if (gameTimerText != null)
                gameTimerText.text = "残り時間: " + Mathf.CeilToInt(gameTime - currentGameTime).ToString() + "秒";
            if (currentGameTime >= gameTime)
                GameOver();
        }
    }

    private void HandleKeyboardInput()
    {
        if (!isInputEnabled || questions.Count == 0) return;

        if (Input.anyKeyDown)
        {
            string pressedKeyStr = Input.inputString.ToUpper();
            if (string.IsNullOrEmpty(pressedKeyStr)) return;

            totalTypes++;
            string potentialNextInput = currentInput + pressedKeyStr;
            var currentPatterns = questions[currentQuestionIndex].answerPatterns;

            // いずれかの正解パターンの前方一致とマッチするかチェック
            if (currentPatterns.Any(pattern => pattern.StartsWith(potentialNextInput)))
            {
                currentInput = potentialNextInput;
                consecutiveCorrectTypes++;
                UpdateCorrectAnswerDisplay();

                // タイムボーナス
                if (consecutiveCorrectTypes >= 15)
                {
                    currentGameTime -= 3f;
                    if (currentGameTime < 0) currentGameTime = 0f;
                    StartCoroutine(ShowTimeBonusText());
                    consecutiveCorrectTypes = 0;
                }

                // いずれかの正解パターンと完全一致したかチェック
                if (currentPatterns.Contains(currentInput))
                {
                    correctCount++;
                    UpdateCorrectCountDisplay();
                    MoveToNextCorrectAnswer();
                    bgmManager.PlaySound(bgmManager.CorrectSound);
                }
            }
            else
            {
                // ミスタイプ
                missTypes++;
                consecutiveCorrectTypes = 0;
                // ここでミス音を鳴らすなどの演出を追加可能
            }
        }
    }

    private IEnumerator ShowTimeBonusText()
    {
        if (timeBonusText != null)
        {
            timeBonusText.text = "タイムボーナス！";
            timeBonusText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            timeBonusText.gameObject.SetActive(false);
        }
    }

    private void MoveToNextCorrectAnswer()
    {
        isMoving = false;
        currentQuestionIndex++;
        if (currentQuestionIndex >= questions.Count)
        {
            currentQuestionIndex = 0;
        }

        currentInput = "";
        UpdateCorrectAnswerDisplay();
        MoveToNextObject();
        Debug.Log("正解！次の問題: " + questions[currentQuestionIndex].displayText);
    }

    private void GameOver()
    {
        bgmManager.PlaySound(bgmManager.ResultSound);
        isGameActive = false;
        isInputEnabled = false;
        if (gameOverText != null) gameOverText.gameObject.SetActive(true);
        Invoke("MoveToResult", 2f);
    }

    private void MoveToResult()
    {
        UpdateResultPanel();
        Current = Scene.Result;
        ShowScene(ResultPanel);
    }

    private void UpdateResultPanel()
    {
        int correctTypes = totalTypes - missTypes;
        if (correctTypes < 0) correctTypes = 0;
        float accuracy = 0f;
        if (totalTypes > 0) accuracy = ((float)correctTypes / totalTypes) * 100f;

        if (totalTypesText != null) totalTypesText.text = "総タイプ数: " + totalTypes.ToString();
        if (missTypesText != null) missTypesText.text = "ミスタイプ数: " + missTypes.ToString();
        if (accuracyText != null) accuracyText.text = "正解率: " + accuracy.ToString("F2") + "%";

        if (resultCorrectCountText != null)
        {
            resultCorrectCountText.text = correctCount.ToString() + "杯完食！";
        }
    }

    private void UpdateCorrectCountDisplay()
    {
        if (correctCountText != null)
        {
            correctCountText.text = "正解数: " + correctCount.ToString();
        }
    }

    private void ShowScene(GameObject sceneName)
    {
        StartPanel.SetActive(false);
        GamePanel.SetActive(false);
        ResultPanel.SetActive(false);
        sceneName.SetActive(true);
    }

    private void StartObjectMovement()
    {
        if (objects.Count > 0)
        {
            currentObjectIndex = 0;
            StartNextObjectMovement();
        }
    }

    private void StartNextObjectMovement()
    {
        if (currentObjectIndex < objects.Count && objects[currentObjectIndex] != null)
        {
            UpdateObjectVisibility();
            // 親オブジェクトのサイズを取得
            Vector2 parentSize = objects[0].parent.parent.GetComponent<RectTransform>().rect.size;

            // 親オブジェクトの幅と高さを基準にして相対位置を設定
            // startPos: 親の幅の 50%（中央）、高さの 10%（画面下部）に設定
            startPos = new Vector2(parentSize.x * 0.25f, parentSize.y * -0.48f);

            // endPos: 親の幅の 10%（画面左側）、高さの 50%（中央）に設定
            endPos = new Vector2(parentSize.x * -0.48f, parentSize.y * -0.07f); 
            objects[currentObjectIndex].anchoredPosition = startPos;
            moveTimer = 0f;
            isMoving = true;
        }
    }

    private void UpdateObjectMovement()
    {
        if (isMoving && currentObjectIndex < objects.Count && objects[currentObjectIndex] != null)
        {
            moveTimer += Time.deltaTime;
            float t = Mathf.Clamp01(moveTimer / moveDuration);
            objects[currentObjectIndex].anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            if (t >= 1f)
            {
                isMoving = false;
                if (!questions[currentQuestionIndex].answerPatterns.Contains(currentInput))
                {
                    MoveToNextCorrectAnswer();
                }
                else
                {
                    MoveToNextObject();
                }
            }
        }
    }

    private void MoveToNextObject()
    {
        currentObjectIndex++;
        if (currentObjectIndex >= objects.Count) currentObjectIndex = 0;
        StartNextObjectMovement();
    }

    private void UpdateObjectVisibility()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] == null) continue;
            bool shouldBeActive = (i == currentObjectIndex);
            if (objects[i].gameObject.activeSelf != shouldBeActive)
            {
                objects[i].gameObject.SetActive(shouldBeActive);
            }
        }
    }
}