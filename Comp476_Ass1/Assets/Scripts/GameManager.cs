using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // VARIABLES
    [SerializeField] private GameObject guardianVictoryText;
    [SerializeField] private GameObject heroVictoryText;
    [SerializeField] private TMPro.TextMeshProUGUI heroScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI guardianScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI timerText;

    [SerializeField] private int prisonersInScene;
    [SerializeField] private int guardiansInScene;
    [SerializeField] private int heroesInScene;
    [SerializeField] private int prisonersRescued;
    [SerializeField] private int guardiansKilled;
    [SerializeField] private int heroesKilled;
    [SerializeField] private int timeElapsed = 0;


    // METHODS

    // Built in.
    void Start()
    {
        // Make sure the game over text is disabled.
        if (guardianVictoryText)
            guardianVictoryText.SetActive(false);
        if (heroVictoryText)
            heroVictoryText.SetActive(false);

        // Register event listeners.
        HeroKillZone.OnCaptureZoneEnter += UpdateHeroesKilled;
        GuardianKillZone.OnKillZoneEnter += UpdateGuardiansKilled;
        Fortress.OnPrisonerFortressEnter += UpdatePrisonersRescued;

        // Count items in scene.
        GameObject[] guardians = GameObject.FindGameObjectsWithTag("Guardian");
        guardiansInScene = guardians.Length;
        GameObject[] prisoners = GameObject.FindGameObjectsWithTag("Prisoner");
        prisonersInScene = prisoners.Length;
        GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
        heroesInScene = heroes.Length;

        // Set text;
        UpdateScoreText();

        // Start timer;
        StartCoroutine(TimerTick());
    }


    // Management.
    void UpdateGuardiansKilled(GameObject dummy)
    {
        guardiansKilled += 1;
        UpdateScoreText();

        // check for victory if needed
    }
    void UpdateHeroesKilled(GameObject dummy)
    {
        heroesKilled += 1;
        UpdateScoreText();

        if (heroesKilled == heroesInScene)
        {
            DoGuardianVictory();
        }
    }
    void UpdatePrisonersRescued(GameObject dummy)
    {
        prisonersRescued += 1;
        UpdateScoreText();

        if (prisonersRescued == prisonersInScene)
        {
            DoHeroVictory();
        }
    }
    void UpdateScoreText()
    {
        if (heroScoreText != null)
        {
            heroScoreText.text = (string.Format("Heroes' Score:\n{0:D2}/{1:D2} Guadians Killed\n{2:D2}/{3:D2} Prisoners Rescued\n({4:D3} Points)", guardiansKilled, guardiansInScene, prisonersRescued, prisonersInScene, prisonersRescued * 25));
        }
        if (guardianScoreText != null)
        {
            guardianScoreText.text = (string.Format("Guardians' Score:\n{0:D2}/{1:D2} Heroes Killed", heroesKilled, heroesInScene));
            // To do: add untouchable prisoner count.
        }
    }

    void DoGuardianVictory()
    {
        guardianVictoryText.SetActive(true);

        Time.timeScale = 0;
        StopAllCoroutines();
    }
    void DoHeroVictory()
    {
        heroVictoryText.SetActive(true);

        Time.timeScale = 0;
        StopAllCoroutines();
    }

    // Timer manager.
    IEnumerator TimerTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            timeElapsed++;
            if (timerText != null)
            {
                timerText.text = string.Format("Time Elapsed:\n{0:D4} Seconds", timeElapsed);
            }
        }
    }


    // TO DO: hero gets +25 points when a prisoner is brought to the base.

}
