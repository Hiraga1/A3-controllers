using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private PlayerMovementAdvanced[] players;
    [SerializeField] private PlayerLogState[] uiLogStates;

    //[SerializeField] private GameObject Player1;
    //[SerializeField] private GameObject Player2;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float countDownTime;
    private float currentCountdown;

    [Header("Trans")]
    [SerializeField] private Transform _bg;
    [SerializeField] private CanvasGroup _loadingSim;
    [SerializeField] private AnimationCurve _loadingCurve;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        inputManager.onPlayerJoined += input =>
        {
            var index = inputManager.playerCount - 1;
            players[index].InputHandler.SetPlayerInput(input);
            players[index].InputHandler.SetEnable(false);

            //manage gameobject spawned
            input.gameObject.transform.parent = transform;
            input.name = $"player ({index + 1}) input";

            //can make a countdown here if player full

            if (inputManager.playerCount == players.Length)
            {
                StartCoroutine(transition());
            }
        };

        for (int i = 0; i < players.Length; i++)
        {
            uiLogStates[i].Init(this, players[i]);
            uiLogStates[i].SetActiveReadyState(true);
        }

        IEnumerator transition()
        {
            float t = 0;
            var lastT = _loadingCurve.keys[_loadingCurve.keys.Length - 1].time;
            //add some delay
            yield return new WaitForSeconds(0.5f);
            while (t < lastT)
            {
                _loadingSim.alpha = _loadingCurve.Evaluate(t);
                if (_loadingSim.alpha >= 0.9f)
                {
                    _bg.gameObject.SetActive(false);
                    for (int i = 0; i < players.Length; i++)
                    {
                        uiLogStates[i].SetActiveReadyState(false);
                    }
                }
                yield return null;
                t += Time.deltaTime;
            }

            currentCountdown = countDownTime;
        }
    }

    private void startGame()
    {
        var chaserIndex = Random.Range(0, players.Length);

        for (int i = 0; i < players.Length; i++)
        {
            players[i].SetChaser(i == chaserIndex);

            if (i != chaserIndex) players[i].InputHandler.SetEnable(true);
            else players[i].freeze = true;
        }

        StartCoroutine(turnOnChaser());

        IEnumerator turnOnChaser()
        {
            yield return new WaitForSeconds(5);
            players[chaserIndex].InputHandler.SetEnable(true);
            players[chaserIndex].freeze = false;
        }
    }

    private void Update()
    {
        if (currentCountdown > 0)
        {
            currentCountdown -= Time.deltaTime;
            if (currentCountdown < 0)
            {
                IsPlaying = true;
                startGame();
            }
        }
        timerText.gameObject.SetActive(currentCountdown > 0);
        timerText.text = currentCountdown.NiceFloat(1);
    }

    //private void PlayerRandomizer()
    //{
    //    RandomPicker = Random.Range(0, 100);

    //    if (RandomPicker < 50)
    //    {
    //        Player1.tag = "Runner";

    //        Player2.tag = "Chaser";
    //    }
    //    else if (RandomPicker > 50)
    //    {
    //        Player1.tag = "Chaser";

    //        Player2.tag = "Runner";
    //    }
    //}
}