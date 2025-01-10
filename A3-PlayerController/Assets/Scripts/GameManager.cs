using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private InputHandler[] players;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float remainingTime;
    private void Awake()
    {
        inputManager.onPlayerJoined += input =>
        {
            var index = inputManager.playerCount - 1;
            players[index].SetPlayerInput(input);

            //manage gameobject spawned
            input.gameObject.transform.parent = transform;
            input.name = $"player ({index + 1}) input";

            //can make a countdown here if player full

            /*if(inputManager.playerCount == maxPlayer)
            { 
            }
            */
        };
    }
    private void Update()
    {
        remainingTime -= Time.deltaTime;
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        float milliSeconds = (remainingTime % 1) * 1000;
        timerText.text = string.Format("{0:00}:{1:00}", seconds, milliSeconds);
    }
}
