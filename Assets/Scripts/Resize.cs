using UnityEngine;

public class Resize : MonoBehaviour
{
    [SerializeField] 
    private GameObject landscape;
    [SerializeField] 
    private GameObject portrait;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() => CheckResize();

    // Update is called once per frame
    void Update() => CheckResize();

    void CheckResize() {
        Vector2 viewSize = new Vector2(Screen.width, Screen.height);
        if (viewSize.x > viewSize.y) {
            // Landscape
            landscape.SetActive(true);
            portrait.SetActive(false);
        } else {
            // Portrait
            landscape.SetActive(false);
            portrait.SetActive(true);
        }
    }
}
