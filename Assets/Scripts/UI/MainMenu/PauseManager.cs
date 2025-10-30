using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject settingsPanel;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;  //‘›Õ£”Œœ∑
        isPaused = true;

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;  //ª÷∏¥”Œœ∑
        isPaused = false;

        
    }
}
