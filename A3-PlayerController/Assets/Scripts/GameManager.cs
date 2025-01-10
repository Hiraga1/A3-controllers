using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private InputHandler[] players;

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
}
