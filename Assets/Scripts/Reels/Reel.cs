using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Reel : MonoBehaviour
{
    // ---------------- Serialized Fields ----------------
    [SerializeField]
    private RectTransform[] symbols;
    public Sprite[] symbolSprites;

    [SerializeField]
    private float symbolHeight = 100f;

    [SerializeField]
    private float symbolSpacing = 10f;

    [SerializeField]
    private int symbolsBeforeWrap = 3;

    // ---------------- Private Fields ----------------
    private bool isStopped = false;
    private float bounceDistance = 20f;
    private float bounceDuration = 0.1f;

    // ---------------- Awake Method ----------------
    // Inicializa los símbolos con sprites aleatorios al cargar.
    void Awake()
    {
        if (symbols == null)
            return;

        foreach (RectTransform symbol in symbols)
        {
            if (symbol != null)
            {
                SetSymbolOrRandom(symbol);
            }
        }
    }

    // ---------------- PrepareForSpin Method ----------------
    // Resetea el reel para permitir una nueva tirada.
    public void PrepareForSpin()
    {
        isStopped = false;
    }

    // ---------------- Spin Method ----------------
    // Mueve los símbolos hacia abajo a la velocidad indicada.
    public void Spin(float speed)
    {
        if (isStopped)
            return;

        MoveSymbols(speed);
    }

    // ---------------- Stop Method ----------------
    // Detiene el reel y aplica el resultado.
    public void Stop(int[] result)
    {
        isStopped = true;

        ApplyResult(result);
        StartCoroutine(StopCoroutine());
    }

    // ---------------- MoveSymbols Method ----------------
    // Desplaza los símbolos hacia abajo y los recoloca al superar el umbral.
    void MoveSymbols(float speed)
    {
        if (symbols == null)
            return;

        float wrapThreshold = -symbolHeight * symbolsBeforeWrap;

        foreach (RectTransform symbol in symbols)
        {
            if (symbol == null)
                continue;

            symbol.anchoredPosition -= new Vector2(0, speed * Time.deltaTime);

            if (symbol.anchoredPosition.y <= wrapThreshold)
            {
                MoveToTop(symbol);
            }
        }
    }

    // ---------------- MoveToTop Method ----------------
    // Coloca el símbolo en la parte superior del reel con un sprite aleatorio.
    void MoveToTop(RectTransform symbol)
    {
        float highestY = GetHighestSymbolY();

        symbol.anchoredPosition = new Vector2(
            symbol.anchoredPosition.x,
            highestY + symbolHeight + symbolSpacing
        );

        SetSymbolOrRandom(symbol);
    }

    // ---------------- GetHighestSymbolY Method ----------------
    // Obtiene la posición Y más alta entre todos los símbolos.
    float GetHighestSymbolY()
    {
        if (symbols == null || symbols.Length == 0)
            return 0f;

        float highest = float.MinValue;

        foreach (RectTransform symbol in symbols)
        {
            if (symbol != null && symbol.anchoredPosition.y > highest)
            {
                highest = symbol.anchoredPosition.y;
            }
        }

        return highest == float.MinValue ? 0f : highest;
    }

    // ---------------- ApplyResult Method ----------------
    // Aplica el resultado a los símbolos.
    void ApplyResult(int[] result)
    {
        if (result == null || result.Length == 0)
            return;

        for (int i = 0; i < result.Length; i++)
        {
            SetSymbolOrRandom(symbols[i + 1], result[i]);
        }
    }

    // ---------------- SetSymbolOrRandom Method ----------------
    // Asigna un sprite al símbolo: aleatorio si index es -1, o el índice especificado.
    void SetSymbolOrRandom(RectTransform symbol, int index = -1)
    {
        if (symbol == null || symbolSprites == null || symbolSprites.Length == 0)
            return;

        Image img = symbol.GetComponent<Image>();
        if (img == null)
            return;

        if (index == -1)
        {
            int randomIndex = Random.Range(0, symbolSprites.Length);
            img.sprite = symbolSprites[randomIndex];
        }
        else
        {
            img.sprite = symbolSprites[index];
        }
    }

    // ---------------------------------------
    // Bounce Methods
    // ---------------------------------------

    // ---------------- PlayStartBounce Method ----------------
    public void PlayStartBounce()
    {
        StartCoroutine(StartBounce());
    }

    // ---------------- StartBounce Coroutine ----------------
    // Mueve los símbolos hacia arriba y hacia abajo con un efecto de rebote.
    IEnumerator StartBounce()
    {
        float elapsed = 0f;

        Vector2[] startPos = new Vector2[symbols.Length];
        for (int i = 0; i < symbols.Length; i++)
            startPos[i] = symbols[i].anchoredPosition;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;

            float offset = Mathf.Lerp(0, bounceDistance, t);

            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i].anchoredPosition = startPos[i] + Vector2.up * offset;
            }

            yield return null;
        }
    }

    // ---------------- StopCoroutine Coroutine ----------------
    // Mueve los símbolos hacia abajo y hacia arriba con un efecto de rebote.
    IEnumerator StopCoroutine()
    {
        float elapsed = 0;

        Vector2[] basePos = new Vector2[symbols.Length];

        float step = symbolHeight + symbolSpacing;

        for (int i = 0; i < symbols.Length; i++)
        {
            float y = step * (2 - i);
            basePos[i] = new Vector2(symbols[i].anchoredPosition.x, y);
        }

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;

            float offset = Mathf.Lerp(0, bounceDistance, t);

            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i].anchoredPosition = basePos[i] + Vector2.down * offset;
            }

            yield return null;
        }

        elapsed = 0;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;

            float offset = Mathf.Lerp(-bounceDistance, 0, t);

            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i].anchoredPosition = basePos[i] + Vector2.up * offset;
            }

            yield return null;
        }

        for (int i = 0; i < symbols.Length; i++)
        {
            symbols[i].anchoredPosition = basePos[i];
        }
    }

    // ------------------------------------------------
    // ---------------- VISUAL METHODS ----------------
    // ------------------------------------------------

    // ---------------- Highlight Method ----------------
    // Resalta los símbolos que coinciden con el máscara.
    public void Highlight(int[] mask)
    {
        for (int i = 0; i < symbols.Length; i++)
        {
            Image img = symbols[i].GetComponent<Image>();
            if (img == null)
                continue;

            Color target;

            if (i >= 1 && i <= 3)
            {
                int visibleIndex = i - 1;

                target = mask[visibleIndex] != 0 ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else
            {
                target = new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            StartCoroutine(FadeToColor(img, target, 0.2f));
        }
    }

    // ---------------- ResetHighlight Method ----------------
    public void ResetHighlight()
    {
        foreach (var symbol in symbols)
        {
            Image img = symbol.GetComponent<Image>();
            if (img != null)
                img.color = Color.white;
        }
    }

    // ---------------- FadeToColor Coroutine ----------------
    public IEnumerator FadeToColor(Image img, Color target, float duration)
    {
        Color start = img.color;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            img.color = Color.Lerp(start, target, time / duration);
            yield return null;
        }

        img.color = target;
    }
}
