using UnityEngine;
using UnityEngine.UI;

// 게임 점수를 관리하고 화면 왼쪽 위에 표시한다.
// TargetDestroy가 타겟을 파괴할 때 AddScore를 호출한다.
public class ScoreManager : MonoBehaviour
{
    const string ManagerName = "Score Manager";
    const string CanvasName = "Score Canvas";

    static ScoreManager instance;

    int score;
    Text scoreText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindFirstObjectByType<ScoreManager>() == null)
        {
            new GameObject(ManagerName).AddComponent<ScoreManager>();
        }
    }

    public static void AddScore(int points)
    {
        if (points <= 0)
        {
            return;
        }

        if (instance == null)
        {
            Bootstrap();
        }

        if (instance == null)
        {
            return;
        }

        instance.score += points;
        instance.RefreshText();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildScoreUi();
        RefreshText();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void BuildScoreUi()
    {
        if (scoreText != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject(CanvasName);
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("Score Text");
        textObject.transform.SetParent(canvasObject.transform, false);

        scoreText = textObject.AddComponent<Text>();
        scoreText.font = LoadUiFont();
        scoreText.fontSize = 34;
        scoreText.fontStyle = FontStyle.Bold;
        scoreText.alignment = TextAnchor.MiddleLeft;
        scoreText.color = Color.white;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(2f, -2f);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(24f, -20f);
        rect.sizeDelta = new Vector2(260f, 56f);
    }

    void RefreshText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    static Font LoadUiFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }
}
