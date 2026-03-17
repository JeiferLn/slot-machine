using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReelController : MonoBehaviour
{
    // ---------------- Serialized Fields ----------------
    [SerializeField]
    private Image boardImage;

    [SerializeField]
    private Reel[] reels;

    [SerializeField]
    private float spinSpeed = 100f;

    [SerializeField]
    private float spinDelay = 0.5f;

    // ---------------- Private Fields ----------------
    private ReelState currentState;
    private float spinTime = 0f;
    private float stopTime = 0f;
    private int[][] resultBoard;
    private bool[] reelsStarted;
    private bool[] reelsStopped;
    private int[][][] pendingStreaks;

    // control coroutines
    private Coroutine winRoutine;
    private Coroutine fadeRoutine;
    private bool isShowingWin = false;

    // ---------------- Awake Method ----------------
    void Awake()
    {
        currentState = ReelState.STOPPED;
    }

    // ---------------- GetReelState Method ----------------
    public ReelState GetReelState() => currentState;

    // ---------------- IsBusy Method ----------------
    public bool IsBusy() => isShowingWin;

    // ---------------- SetReelState Method ----------------
    public void SetReelState(ReelState state)
    {
        if (state == ReelState.SPINNING)
        {
            spinTime = 0f;
            reelsStarted = new bool[reels.Length];
            PrepareAllReelsForSpin();
            ResetVisuals();
        }
        currentState = state;
    }

    // ---------------- PrepareAllReelsForSpin Method ----------------
    void PrepareAllReelsForSpin()
    {
        if (!HasValidReels())
            return;

        foreach (Reel reel in reels)
        {
            if (reel != null)
                reel.PrepareForSpin();
        }
    }

    // ---------------- HasValidReels Method ----------------
    bool HasValidReels() => reels != null && reels.Length > 0;

    // ---------------- Update Method ----------------
    void Update()
    {
        if (currentState == ReelState.SPINNING || currentState == ReelState.STOPPING)
        {
            UpdateSpinning();

            if (currentState == ReelState.STOPPING)
                UpdateStop();
        }
    }

    // ---------------- UpdateSpinning Method ----------------
    void UpdateSpinning()
    {
        if (!HasValidReels() || reelsStarted == null)
            return;

        spinTime += Time.deltaTime;

        for (int i = 0; i < reels.Length; i++)
        {
            if (spinTime >= spinDelay * i)
            {
                if (!reelsStarted[i])
                {
                    reels[i].PlayStartBounce();
                    reelsStarted[i] = true;
                }

                reels[i].Spin(spinSpeed);
            }
        }
    }

    // ---------------- UpdateStop Method ----------------
    void UpdateStop()
    {
        if (!HasValidReels() || reelsStopped == null)
            return;

        stopTime += Time.deltaTime;

        for (int i = 0; i < reels.Length; i++)
        {
            if (!reelsStopped[i] && stopTime >= spinDelay * i)
            {
                reels[i].Stop(resultBoard[i]);
                reelsStopped[i] = true;
            }
        }

        if (AllReelsStopped())
        {
            currentState = ReelState.STOPPED;
            ShowWin(pendingStreaks);
        }
    }

    // ---------------- AllReelsStopped Method ----------------
    bool AllReelsStopped()
    {
        if (reelsStopped == null)
            return false;

        foreach (bool stopped in reelsStopped)
        {
            if (!stopped)
                return false;
        }
        return true;
    }

    // ---------------- Show Win Method ----------------
    public void ShowWin(int[][][] streaks)
    {
        if (streaks == null || streaks.Length == 0)
            return;

        if (winRoutine != null)
            StopCoroutine(winRoutine);

        winRoutine = StartCoroutine(ShowWinRoutine(streaks));
    }

    // ---------------- ResetVisuals Method ----------------
    void ResetVisuals()
    {
        StartFadeBoard(1f, 0.2f);

        foreach (Reel reel in reels)
        {
            reel.ResetHighlight();
        }
    }

    // -----------------------------------------------
    // ------------- Coroutines Methods --------------
    // -----------------------------------------------

    // ---------------- StartFadeBoard Method ----------------
    void StartFadeBoard(float target, float duration)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeBoard(target, duration));
    }

    // ---------------- FadeBoard Coroutine ----------------
    IEnumerator FadeBoard(float target, float duration)
    {
        float start = boardImage.color.r;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float v = Mathf.Lerp(start, target, time / duration);
            boardImage.color = new Color(v, v, v, 1f);
            yield return null;
        }
    }

    // ---------------- ShowWinRoutine Coroutine ----------------
    IEnumerator ShowWinRoutine(int[][][] streaks)
    {
        isShowingWin = true;

        int[][] streak = streaks[0];

        StartFadeBoard(0.5f, 0.2f);

        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].Highlight(streak[i]);
        }

        yield return new WaitForSeconds(2f);

        ResetVisuals();

        isShowingWin = false;
    }

    // -----------------------------------------------
    // ---------------- Stops Methods ----------------
    // -----------------------------------------------

    // ---------------- Normal Stop Method ----------------
    public void Stop(int[][] board, int[][][] streaks)
    {
        if (!HasValidReels() || board == null || board.Length != reels.Length)
            return;

        SetReelState(ReelState.STOPPING);
        stopTime = 0f;
        resultBoard = board;
        reelsStopped = new bool[reels.Length];
        pendingStreaks = streaks;
    }

    // ---------------- Force Stop Method ----------------
    public void ForceStop(int[][] board, int[][][] streaks)
    {
        if (!HasValidReels() || board == null || board.Length != reels.Length)
            return;

        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].Stop(board[i]);
        }

        currentState = ReelState.STOPPED;

        ShowWin(streaks);
    }
}
