using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpinButton : MonoBehaviour
{
    // ---------------- Serialized Fields ----------------
    [SerializeField]
    private ReelController reelController;

    // ---------------- Private Fields ----------------
    private Button button;
    private Coroutine stopCoroutine;

    // ---------------- Start Method ----------------
    void Start()
    {
        button = GetComponent<Button>();
    }

    // ---------------- Update Method ----------------
    void Update()
    {
        if (button == null || reelController == null)
            return;

        bool isInactive =
            reelController.IsBusy() || reelController.GetReelState() == ReelState.FORCE_STOPPING;

        button.interactable = !isInactive;
    }

    // ---------------- OnClick Method ----------------
    public void OnClick()
    {
        if (reelController == null || reelController.IsBusy())
            return;

        ReelState state = reelController.GetReelState();

        if (state == ReelState.SPINNING || state == ReelState.STOPPING)
        {
            if (stopCoroutine != null)
                StopCoroutine(stopCoroutine);

            reelController.SetReelState(ReelState.FORCE_STOPPING);
            reelController.ForceStop(GetTestBoard(), GetTestStreaks());
            return;
        }

        if (state == ReelState.STOPPED)
        {
            reelController.SetReelState(ReelState.SPINNING);
            stopCoroutine = StartCoroutine(DelayedStop());
        }
    }

    // ---------------- DelayedStop Method ----------------
    private IEnumerator DelayedStop()
    {
        yield return new WaitForSeconds(3);
        reelController.Stop(GetTestBoard(), GetTestStreaks());
    }

    // ---------------- Test Data ----------------
    private int[][] GetTestBoard()
    {
        return new int[][]
        {
            new int[] { 0, 4, 4 },
            new int[] { 2, 3, 4 },
            new int[] { 10, 2, 4 },
            new int[] { 11, 10, 4 },
            new int[] { 4, 13, 8 },
        };
    }

    private int[][][] GetTestStreaks()
    {
        return new int[][][]
        {
            new int[][]
            {
                new int[] { 0, 0, 1 },
                new int[] { 0, 0, 1 },
                new int[] { 0, 0, 1 },
                new int[] { 0, 0, 1 },
                new int[] { 0, 0, 0 },
            },
        };
    }
}
