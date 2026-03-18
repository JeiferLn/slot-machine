using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Reel : MonoBehaviour
{
    // ---------------- Serialized Fields ----------------
    [SerializeField]
    private RectTransform[] symbols;

    [SerializeField]
    private Sprite[] symbolSprites;

    [SerializeField]
    private float symbolHeight = 100f;

    [SerializeField]
    private float symbolSpacing = 10f;

    [SerializeField]
    private int symbolsBeforeWrap = 3;

    // ---------------- Private Fields ----------------
    private const int VisibleSymbolsOffset = 1;
    private const int VisibleSymbolsCount = 3;
    private bool isStopped = false;
    private float bounceDistance = 20f;
    private float bounceDuration = 0.1f;
    private Dictionary<Image, Tween> activeTweens = new Dictionary<Image, Tween>();

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
        StartCoroutine(StopBounce());
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
    // Aplica el resultado a los símbolos visibles (índices 1-3 del array).
    void ApplyResult(int[] result)
    {
        if (result == null || result.Length == 0)
            return;

        for (int i = 0; i < result.Length; i++)
        {
            SetSymbolOrRandom(symbols[i + VisibleSymbolsOffset], result[i]);
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

        img.sprite =
            index == -1
                ? symbolSprites[Random.Range(0, symbolSprites.Length)]
                : symbolSprites[index];
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

    // ---------------- StopBounce Coroutine ----------------
    // Mueve los símbolos hacia abajo y hacia arriba con un efecto de rebote.
    IEnumerator StopBounce()
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

            if (i >= VisibleSymbolsOffset && i < VisibleSymbolsOffset + VisibleSymbolsCount)
            {
                int visibleIndex = i - VisibleSymbolsOffset;
                bool isWinner = mask[visibleIndex] != 0;

                target = isWinner ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);

                if (isWinner)
                {
                    PlayWinAnimation(img);
                }
                else
                {
                    StopWinAnimation(img);
                }
            }
            else
            {
                target = new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            StartCoroutine(FadeToColor(img, target, 0.2f));
        }
    }

    // ---------------- PlayWinAnimation Method ----------------
    // Reproduce el efecto de animación de ganancia del símbolo.
    void PlayWinAnimation(Image img)
    {
        if (img == null)
            return;

        if (activeTweens.ContainsKey(img))
        {
            activeTweens[img].Kill();
            activeTweens.Remove(img);
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(img.transform.DOScale(1.15f, 0.3f).SetEase(Ease.OutQuad));
        seq.Append(img.transform.DOScale(1f, 0.3f).SetEase(Ease.InOutQuad));
        seq.SetLoops(-1);

        seq.Join(
            img.transform.DORotate(new Vector3(0, 0, 5f), 0.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
        );

        activeTweens.Add(img, seq);
    }

    // ---------------- ResetHighlight Method ----------------
    // Resetea el color de los símbolos.
    public void ResetHighlight()
    {
        foreach (var symbol in symbols)
        {
            Image img = symbol.GetComponent<Image>();
            if (img != null)
            {
                StopWinAnimation(img);
                img.color = Color.white;
            }
        }
    }

    // ---------------- StopWinAnimation Method ----------------
    // Detiene el efecto de animación de ganancia del símbolo.
    void StopWinAnimation(Image img)
    {
        if (img == null)
            return;

        if (activeTweens.ContainsKey(img))
        {
            activeTweens[img].Kill();
            activeTweens.Remove(img);
        }

        img.transform.DOKill();

        img.transform.localScale = Vector3.one;
        img.transform.localRotation = Quaternion.identity;
    }

    // --------------------------------------------
    // ---------------- COROUTINES ----------------
    // --------------------------------------------

    // ---------------- FadeToColor Coroutine ----------------
    // Fadea el color del símbolo a un color objetivo.
    IEnumerator FadeToColor(Image img, Color target, float duration)
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
