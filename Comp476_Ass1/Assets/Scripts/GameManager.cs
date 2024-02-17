using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // VARIABLES
    [Header("References")]
    [SerializeField] private GameObject guardianVictoryText;
    [SerializeField] private GameObject heroVictoryText;
    [SerializeField] private TMPro.TextMeshProUGUI heroScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI guardianScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI timerText;
    [SerializeField] private GameObject guardianPrefab;

    [Header("Level Param")]
    [SerializeField] private bool vsPlayer;

    [Header("Counters")]
    [SerializeField] private int prisonersInScene;
    [SerializeField] private int guardiansInScene;
    [SerializeField] private int heroesInScene;
    [SerializeField] private int prisonersRescued;
    [SerializeField] private int guardiansKilled;
    [SerializeField] private int heroesKilled;
    [SerializeField] private int timeElapsed = 0;
    [SerializeField] private int score = 0;
    [SerializeField] private int prisonerNeutralized = 0;


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
        Player.OnPlayerCaughtHero += UpdateHeroesKilled;
        HeroKillZone.OnHeroKillZoneEnter += UpdateHeroesKilled;
        GuardianKillZone.OnGuardianKillZoneEnter += UpdateGuardiansKilled;
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

        CheckForVictory();
    }
    void UpdateHeroesKilled(GameObject potentialHeroSO, bool killForever)
    {
        //Debug.Log(string.Format("The player hit a hero, will they be killed forever? {0}", killForever));

        Hero potentialHero = potentialHeroSO.GetComponent<Hero>();
        if (potentialHero)
        {
            if (killForever)
            {
                UpdateHeroesKilled();
                prisonerNeutralized++;
            }
            else
            {
                // Spawn a new guardian in the scene. Recalculate their numbers.
                Instantiate(guardianPrefab, potentialHeroSO.transform.position, potentialHeroSO.transform.rotation);
                guardiansInScene++;

                score -= 10;
            }
            UpdateScoreText();
        }

        CheckForVictory();
    }
    void UpdateHeroesKilled(GameObject potentialHeroSO)
    {
        if (potentialHeroSO.CompareTag("Hero"))
        {
            Hero potentialHero = potentialHeroSO.GetComponent<Hero>();
            if (potentialHero && !potentialHero.Respawns)
            {
                UpdateHeroesKilled();
            }
        }

        CheckForVictory();
    }
    void UpdateHeroesKilled()
    {
        heroesKilled += 1;
        UpdateScoreText();

        CheckForVictory();
    }
    void UpdatePrisonersRescued(GameObject dummy)
    {
        prisonersRescued += 1;
        score += 25;
        UpdateScoreText();

        CheckForVictory();
    }
    void UpdateScoreText()
    {
        if (heroScoreText != null)
        {
            heroScoreText.text = (string.Format("Heroes' Score:\n{0:D2}/{1:D2} Guadians Killed\n{2:D2}/{3:D2} Prisoners Rescued\n({4:D3} Points)", guardiansKilled, guardiansInScene, prisonersRescued, prisonersInScene, score));
        }
        if (guardianScoreText != null)
        {
            guardianScoreText.text = (string.Format("Guardians' Score:\n{0:D2}/{1:D2} Heroes Killed\n{2:D2}/{3:D2} Prisoners Locked", heroesKilled, heroesInScene, prisonerNeutralized, prisonersInScene));
            // To do: add untouchable prisoner count.
        }
    }

    void CheckForVictory()
    {
        Debug.Log(string.Format("Prisoners rescued = {0}", prisonersRescued)) ;
        Debug.Log(string.Format("Prisoners locked = {0}", prisonerNeutralized)) ;

        if (heroesKilled == heroesInScene)
        {
            DoGuardianVictory();
        }

        if (vsPlayer)
        {
            if (prisonerNeutralized >= 3)
            {
                DoGuardianVictory();
            }
            else if (prisonersRescued >= 3)
            {
                DoHeroVictory();
            }
        }
        else
        {
            if (prisonersRescued == prisonersInScene)
            {
                DoHeroVictory();
            }
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
}
