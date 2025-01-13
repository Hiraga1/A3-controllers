using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private PlayerMovementAdvanced[] players;
    [SerializeField] private PlayerLogState[] uiLogStates;

    //[SerializeField] private GameObject Player1;
    //[SerializeField] private GameObject Player2;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI roundNumberTxt;
    [SerializeField] private TextMeshProUGUI scroreBoardTxt;
    [SerializeField] private CanvasGroup inGameGroup;

    [Space]
    [Header("Time Config")]
    [SerializeField] private float countDownTime = 2;

    [SerializeField] private float roundTime = 30;
    [SerializeField] private float freezeChaserTime = 5;
    [SerializeField] private float nextRoundTime = 5;

    [Header("Trans")]
    [SerializeField] private Transform _bg;

    [SerializeField] private CanvasGroup _loadingSim;
    [SerializeField] private AnimationCurve _loadingCurve;

    [SerializeField] private TextMeshProUGUI _notifyTxt;

    private static GameObject storeInput;
    private static GameLog gameLog = new GameLog();
    public static GameManager Instance; //Singleton pattern

    public bool IsPlaying { get; private set; }

    private int chaserIndex;

    private static List<PlayerInput> _storedPlayerInput = new List<PlayerInput>();

    //private bool checkReadyPhase = false;

    private void Awake()
    {
        Instance = this;
        inGameGroup.alpha = 0;

        if (_storedPlayerInput.Count == 0)
        {
            storeInput = new GameObject();
            DontDestroyOnLoad(storeInput);
        }

        inputManager.onPlayerJoined += input =>
        {
            var index = inputManager.playerCount - 1;
            setPlayerReady(players[index], input);

            //var map = input.actions.FindActionMap("Player");
            //map.FindAction("Join").performed += _ =>
            //{
            //    onPlyerPressJoin(index);
            //};

            //manage gameobject spawned
            input.gameObject.transform.parent = storeInput.transform;
            input.name = $"player ({index + 1}) input";

            _storedPlayerInput.Add(input);

            if (inputManager.playerCount == players.Length)
            {
                StartCoroutine(openGameWhenFullPlayer());
            }
        };

        //new roud, already have full players
        if (_storedPlayerInput.Count > 0)
        {
            _bg.gameObject.SetActive(false);
            _loadingSim.alpha = 1;

            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                var input = _storedPlayerInput[i];

                setPlayerReady(player, input);
            }
            StartCoroutine(openGameWhenFullPlayer());
            //checkReadyPhase = true;
        }

        for (int i = 0; i < players.Length; i++)
        {
            uiLogStates[i].Init(this, players[i]);
            uiLogStates[i].SetActiveReadyState(true);
        }

        IEnumerator openGameWhenFullPlayer()
        {
            yield return transition();
            for (int i = 0; i < players.Length; i++)
            {
                uiLogStates[i].SetActiveReadyState(false);
            }
            yield return countDownToStartGame(countDownTime);
        }
    }

    private void setPlayerReady(PlayerMovementAdvanced player, PlayerInput input)
    {
        player.InputHandler.SetPlayerInput(input);
        player.InputHandler.SetEnable(false);
    }

    //private void onPlyerPressJoin(int index)
    //{
    //    if (checkReadyPhase && !IsPlaying)
    //    {
    //        setPlayerReady(players[index], _storedPlayerInput[index]);
    //    }

    //    foreach (var i in players)
    //    {
    //        if (!i.IsRegister)
    //        {
    //            return;
    //        }
    //    }

    //    StartCoroutine(countDownToStartGame(countDownTime));
    //}

    private IEnumerator transition()
    {
        float t = 0;
        var lastT = _loadingCurve.keys[_loadingCurve.keys.Length - 1].time;
        //add some delay
        yield return new WaitForSeconds(0.5f);
        while (t < lastT)
        {
            _loadingSim.alpha = _loadingCurve.Evaluate(t);
            if (_loadingSim.alpha > 0.8f)
                _bg.gameObject.SetActive(false);
            yield return null;
            t += Time.deltaTime;
        }
    }

    private IEnumerator startGame()
    {
        IsPlaying = true;
        currentRoundTime = roundTime;
        chaserIndex = Random.Range(0, players.Length);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].SetChaser(i == chaserIndex);

            //not unlock input if it is chaser
            if (i != chaserIndex) players[i].InputHandler.SetEnable(true);
        }

        var chaserBehaviour = players[chaserIndex];
        //not nesscessary but set for log and old logic
        chaserBehaviour.freeze = true;

        //wait to unlock chaser
        yield return new WaitForSeconds(freezeChaserTime);
        players[chaserIndex].InputHandler.SetEnable(true);
        players[chaserIndex].freeze = false;
    }

    private IEnumerator countDownToStartGame(float t)
    {
        inGameGroup.alpha = 1;
        updateRoundInfo();
        while (t >= 0)
        {
            t -= Time.deltaTime;
            timerText.text = $"Game start in: {t.NiceFloat(1)}";
            timerText.gameObject.SetActive(t > 0);
            yield return null;
        }
        yield return startGame();
    }

    private bool isEndRound = false;

    private void handleEndGame(bool chaserWin)
    {
        if (isEndRound) return;

        currentRoundTime = 0; //break point value for not run in Update
        isEndRound = true;

        if (chaserWin)
        {
            if (chaserIndex == 0) gameLog.P1_Score++;
            else gameLog.P2_Score++;
        }
        else
        {
            if (chaserIndex == 0) gameLog.P2_Score++;
            else gameLog.P1_Score++;
        }
        gameLog.Round++;

        //lock all input
        foreach (var i in players)
        {
            i.InputHandler.SetEnable(false);
            i.InputHandler.ClearCallbackInput();
        }

        updateRoundInfo();

        StartCoroutine(gameEndTransition(chaserWin));
    }

    private void updateRoundInfo()
    {
        roundNumberTxt.text = $"Round: {gameLog.Round + 1}";
        scroreBoardTxt.text = $"{gameLog.P1_Score}\t-\t{gameLog.P2_Score}";
    }

    public void NofifyChaserTouchRunner()
    {
        handleEndGame(true);
    }

    private IEnumerator gameEndTransition(bool chaserWin)
    {
        _notifyTxt.transform.parent.gameObject.SetActive(true);
        _notifyTxt.text = chaserWin ? "Chaser win" : "Runner win";

        float t = nextRoundTime;
        while (t >= 0)
        {
            t -= Time.deltaTime;
            timerText.text = $"New round start in: {t.NiceFloat(1)}";
            timerText.gameObject.SetActive(t > 0);
            yield return null;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private float currentRoundTime;

    private void Update()
    {
        if (currentRoundTime > 0)
        {
            currentRoundTime -= Time.deltaTime;
            timerText.gameObject.SetActive(true);
            timerText.text = $"Round end in: {currentRoundTime.NiceFloat(1)}";
            if (currentRoundTime < 0)
            {
                handleEndGame(false);
            }
        }
    }

    private class GameLog
    {
        public int Round;
        public int P1_Score;
        public int P2_Score;
    }
}