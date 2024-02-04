using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // VARIABLES
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private TMPro.TextMeshProUGUI heroScoreText;

    [SerializeField] private int heroScore;




    // METHODS

    // Built in.

    void Start()
    {
        // Make sure the game over text is disabled.
        if (gameOverText)
        {
            gameOverText.SetActive(false);
        }

        // Register event listeners.
        GuardianCaptureZone.OnCaptureZoneEnter += DoGameOver;
        FortressKillzone.OnKillZoneEnter += UpdateHeroScore;

    }


    // Management.
    
    void UpdateHeroScore(GameObject dummy)
    {
        if (heroScoreText != null)
        {
            heroScore += 1;
            heroScoreText.text = (string.Format("Hero's Score: {0:D2}", heroScore));
        }
    }

    void DoGameOver(GameObject go)
    {
        gameOverText.SetActive(true);
        Time.timeScale = 0;
    }
}
