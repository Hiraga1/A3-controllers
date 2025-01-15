using System.Collections;
using UnityEngine;

namespace ScenesCanvas
{
    public class ReadyScene : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI[] _playersState;

        private PlayerMovementAdvanced[] players;
        bool lockingUpdate = true;

        public void Init(PlayerMovementAdvanced[] players)
        {
            this.players = players;
            lockingUpdate = false;
        }

        public void LockUpdate()
        {
            lockingUpdate = true;
        }

        private void Update()
        {
            if (lockingUpdate) return;
            for (int i = 0; i < players.Length; i++)
            {
                _playersState[i].text = players[i].IsRegister ? "Ready" : "Press start to ready";
            }
        }
    }
}