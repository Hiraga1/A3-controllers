using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private PlayerMovementAdvanced[] players;
    [SerializeField] private PlayerLogState[] uiLogStates;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float countDownTime;
    private float currentCountdown;

    public bool IsPlaying { get; private set; }

    private List<PlayerInput> storedInput = new List<PlayerInput>();

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
                currentCountdown = countDownTime;
            }

        };

        for (int i = 0; i < players.Length; i++)
        {
            uiLogStates[i].Init(this, players[i]);
        }
    }

    private void startGame()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].InputHandler.SetEnable(true);
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
}
