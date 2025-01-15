using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private PlayerMovementAdvanced[] players;
    [SerializeField] private PlayerLogState[] uiLogStates;

    [Space]
    [Header("Time Config")]
    [SerializeField] private float countDownTime = 5;

    [SerializeField] private float roundTime = 30;
    [SerializeField] private float freezeChaserTime = 5;
    [SerializeField] private float nextRoundTime = 5;

    [SerializeField] private int maxRound = 3;

    [Header("Trans")]
    [SerializeField] private ScenesCanvas.IntroScene _introScene;

    [SerializeField] private ScenesCanvas.ReadyScene _readyScene;
    [SerializeField] private ScenesCanvas.InGameScene _ingameScene;
    [SerializeField] private ScenesCanvas.EndGameScene _endGameScene;
    [SerializeField] private ScenesCanvas.PauseScene _pauseScene;

    [SerializeField] private CanvasGroup _loadingSim;
    [SerializeField] private AnimationCurve _loadingCurve;

    [SerializeField] private TextMeshProUGUI _notifyTxt;

    private static GameObject storeInput;
    private static GameLog gameLog = new GameLog();
    public static GameManager Instance; //Singleton pattern
    private static bool needIntro = true;

    public bool IsPlaying { get; private set; }

    private int chaserIndex;

    private static List<PlayerInput> _storedPlayerInput = new List<PlayerInput>();

    //private bool checkReadyPhase = false;

    private void Awake()
    {
        Instance = this;

        //hide all scene
        _introScene.gameObject.SetActive(false);
        _readyScene.gameObject.SetActive(false);
        _ingameScene.gameObject.SetActive(false);
        _endGameScene.gameObject.SetActive(false);
        _pauseScene.gameObject.SetActive(false);

        if (needIntro)
        {
            needIntro = false;
            _introScene.Init(inputManager);

            StartCoroutine(_introScene.Transition(
                onBeginFade: () =>
                {
                    showReadyScreen();
                },
                onDone: null));
        }

        if (_storedPlayerInput.Count == 0)
        {
            storeInput = new GameObject();
            DontDestroyOnLoad(storeInput);
        }

        inputManager.onPlayerJoined += input =>
        {
            var index = inputManager.playerCount - 1;
            setPlayerReady(players[index], input, autoSetStarted: true);

            input.gameObject.transform.parent = storeInput.transform;
            input.name = $"player ({index + 1}) input";

            _storedPlayerInput.Add(input);

            if (inputManager.playerCount == players.Length)
            {
                StartCoroutine(openGameWhenFullPlayer());
            }
        };

        //new roud or new game, already have full players
        if (_storedPlayerInput.Count > 0)
        {
            _loadingSim.alpha = 1;

            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                player.DidPressStart = false;
                var input = _storedPlayerInput[i];

                setPlayerReady(player, input, autoSetStarted: gameLog.Round > 0);
            }
            if (gameLog.Round > 0) StartCoroutine(openGameWhenFullPlayer());
            else
            {
                StartCoroutine(transition(showReadyScreen));
            }
        }

        void showReadyScreen()
        {
            _readyScene.Init(players);
            _readyScene.gameObject.SetActive(true);
        }

        for (int i = 0; i < players.Length; i++)
        {
            uiLogStates[i].Init(this, players[i]);
            uiLogStates[i].SetActiveReadyState(true);
        }
    }

    private IEnumerator openGameWhenFullPlayer()
    {
        if (_readyScene.gameObject.activeSelf)
        {
            yield return new WaitForSeconds(0.5f); //wait a little bit for better smooth transition
            _readyScene.LockUpdate();
        }

        yield return transition(onFullBlack: () =>
        {
            _readyScene.gameObject.SetActive(false);
            _ingameScene.gameObject.SetActive(true);
        });
        //just for log
        for (int i = 0; i < players.Length; i++)
        {
            uiLogStates[i].SetActiveReadyState(false);
        }
        //end for log

        yield return countDownToStartGame(countDownTime);
    }

    private void setPlayerReady(PlayerMovementAdvanced player, PlayerInput input, bool autoSetStarted)
    {
        player.InputHandler.SetPlayerInput(input);
        player.InputHandler.SetEnable(false);
        if (autoSetStarted)
        {
            player.DidPressStart = true;
        }
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

    private IEnumerator transition(System.Action onFullBlack)
    {
        float t = 0;
        var lastT = _loadingCurve.keys[_loadingCurve.keys.Length - 1].time;
        //add some delay
        yield return new WaitForSeconds(0.5f);
        while (t < lastT)
        {
            _loadingSim.alpha = _loadingCurve.Evaluate(t);
            if (_loadingSim.alpha > 0.9f)
                onFullBlack?.Invoke();
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

        _ingameScene.ShowWhoIsChaser(players);

        var chaserBehaviour = players[chaserIndex];
        //not nesscessary but set for log and old logic
        chaserBehaviour.freeze = true;

        //wait to unlock chaser
        var t = freezeChaserTime;
        while (t >= 0)
        {
            t -= Time.deltaTime;
            _ingameScene.ShowTimeUnlockChaser(t);
            yield return null;
        }
        chaserBehaviour.InputHandler.SetEnable(true);
        chaserBehaviour.freeze = false;
    }

    private IEnumerator countDownToStartGame(float duration)
    {
        _ingameScene.ShowRoundInfo(gameLog, maxRound);
        //check if need show controller layout
        if (gameLog.NewGameC == 0 && gameLog.Round == 0)
        {
            _ingameScene.ControllerLayout.gameObject.SetActive(true);
        }
        float t = duration;
        while (t >= 0)
        {
            t -= Time.deltaTime;
            _ingameScene.ShowTimer($"Game start in: {t.NiceFloat(1)}");
            //20% of time
            if (t / duration < 0.2f) _ingameScene.ControllerLayout.gameObject.SetActive(false);
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
        IsPlaying = false;

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

        _ingameScene.ShowRoundInfo(gameLog, maxRound);

        gameLog.Round++;

        //lock all input
        foreach (var i in players)
        {
            i.InputHandler.SetEnable(false);
            i.InputHandler.ClearCallbackInput();
        }

        _ingameScene.ShowNotify(chaserWin ? "Chaser win" : "Runner win");
        //can call declere winner here but i will call in gameEndtransition for some animation

        StartCoroutine(gameEndTransition(chaserWin));
    }

    //private void updateRoundInfo()
    //{
    //    roundNumberTxt.text = $"Round: {gameLog.Round + 1}";
    //    scroreBoardTxt.text = $"{gameLog.P1_Score}\t-\t{gameLog.P2_Score}";
    //}

    public void NofifyChaserTouchRunner()
    {
        handleEndGame(true);
    }

    public void NotifyPlayerPressPause(InputHandler input)
    {
        if (IsPlaying && !isEndRound) //pause Game
        {
            Debug.Log("Pause press");
            if (_pauseScene.IsPausing) _pauseScene.UnPause(() => Time.timeScale = 1);
            else
            {
                Time.timeScale = 0;
                _pauseScene.Pop();
            }
        }
        else if (gameLog.Round == 0 && gameLog.NewGameC > 0) //set Ready
        {
            int readyCount = 0;
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if (player.InputHandler == input)
                {
                    player.DidPressStart = true;
                }
                if (player.IsReady) readyCount++;
            }
            if (readyCount == players.Length)
            {
                StartCoroutine(openGameWhenFullPlayer());
            }
        }
    }

    private IEnumerator gameEndTransition(bool chaserWin)
    {
        _notifyTxt.transform.parent.gameObject.SetActive(true);
        _notifyTxt.text = chaserWin ? "Chaser win" : "Runner win";

        if (gameLog.Round == maxRound)
        {
            bool isPlayer1Win;
            declareWinner(out isPlayer1Win);
            //trasition to result scene
            //float t = 2;
            //while (t >= 0)
            //{
            //    t -= Time.deltaTime;
            //    _ingameScene.ShowTimer($"Game end in: {t.NiceFloat(1)}");

            //    yield return null;
            //}
            yield return new WaitForSeconds(1);
            yield return transition(() =>
            {
                _endGameScene.gameObject.SetActive(true);
                _endGameScene.ShowResult(isPlayer1Win, openNewGame);
            });

            void openNewGame()
            {
                gameLog.NewGame();
                SceneManager.LoadScene(0);
            }
        }
        else //transition to loop game logic
        {
            float t = nextRoundTime;
            while (t >= 0)
            {
                t -= Time.deltaTime;
                _ingameScene.ShowTimer($"New round start in: {t.NiceFloat(1)}");
                //timerText.gameObject.SetActive(t > 0);
                yield return null;
            }
            SceneManager.LoadScene(0);
        }
    }

    private float currentRoundTime;

    private void Update()
    {
        if (currentRoundTime > 0)
        {
            currentRoundTime -= Time.deltaTime;
            _ingameScene.ShowTimer($"Round end in: {currentRoundTime.NiceFloat(1)}");
            if (currentRoundTime < 0)
            {
                handleEndGame(false);
            }
        }
    }

    public class GameLog
    {
        public int NewGameC;
        public int Round;
        public int P1_Score;
        public int P2_Score;

        public void NewGame()
        {
            NewGameC++;
            Round = P1_Score = P2_Score = 0;
        }
    }

    private void declareWinner(out bool isPlayer1Win)
    {
        if (gameLog.P1_Score > gameLog.P2_Score)
        {
            isPlayer1Win = true;
        }
        else
        {
            isPlayer1Win = false;
        }
    }
}