using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // VARIABLES
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject victoryText;
    [SerializeField] private TMPro.TextMeshProUGUI heroScoreText;

    [SerializeField] private int prisonersInScene;
    [SerializeField] private int guardiansInScene;
    [SerializeField] private int prisonersRescued;
    [SerializeField] private int guardiansKilled;



    // METHODS

    // Built in.
    void Start()
    {
        // Make sure the game over text is disabled.
        if (gameOverText)
            gameOverText.SetActive(false);
        if (victoryText)
            victoryText.SetActive(false);

        // Register event listeners.
        GuardianCaptureZone.OnCaptureZoneEnter += DoGameOver;
        FortressKillzone.OnKillZoneEnter += UpdateGuardiansKilled;
        Fortress.OnFortressEnter += UpdatePrisonersRescued;

        // Count items in scene.
        GameObject[] guardians = GameObject.FindGameObjectsWithTag("Guardian");
        guardiansInScene = guardians.Length;
        GameObject[] prisoners = GameObject.FindGameObjectsWithTag("Prisoner");
        prisonersInScene = prisoners.Length;

        // Set text;
        UpdateScoreText();
    }


    // Management.
    void UpdateGuardiansKilled(GameObject dummy)
    {
        guardiansKilled += 1;
        UpdateScoreText();
    }
    void UpdatePrisonersRescued(GameObject dummy)
    {
        prisonersRescued += 1;
        UpdateScoreText();

        if (prisonersRescued == prisonersInScene)
        {
            DoVictory();
        }
    }
    void UpdateScoreText()
    {
        if (heroScoreText != null)
        {
            heroScoreText.text = (string.Format("Hero's Score:\n{0:D2}/{1:D2} Guadians Killed\n{2:D2}/{3:D2} Prisoners Rescued", guardiansKilled, guardiansInScene, prisonersRescued, prisonersInScene));
        }
    }

    void DoGameOver(GameObject go)
    {
        gameOverText.SetActive(true);
        Time.timeScale = 0;
    }
    void DoVictory()
    {
        victoryText.SetActive(true);
        Time.timeScale = 0;
    }
}
