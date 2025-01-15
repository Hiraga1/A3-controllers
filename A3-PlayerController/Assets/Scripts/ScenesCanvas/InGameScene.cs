using TMPro;
using UnityEngine;

namespace ScenesCanvas
{
    public class InGameScene : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI roundNumberTxt;
        [SerializeField] private TextMeshProUGUI scroreBoardTxt;
        [SerializeField] private TextMeshProUGUI notifyTxt;

        [SerializeField] private GameObject[] chaserVisibility;
        [SerializeField] private TextMeshProUGUI _unlockChaserInSecondTxt;

        [field: SerializeField] public GameObject ControllerLayout { get; private set; }

        private void Awake()
        {
            notifyTxt.transform.parent.gameObject.SetActive(false);
            HideWhoIsChaser();
            ControllerLayout.gameObject.SetActive(false);
            _unlockChaserInSecondTxt.text = "";
        }

        public void ShowRoundInfo(GameManager.GameLog gameLog, int maxRound)
        {
            roundNumberTxt.text = $"Round: {gameLog.Round + 1}/{maxRound}";
            scroreBoardTxt.text = $"{gameLog.P1_Score}\t-\t{gameLog.P2_Score}";
        }

        public void ShowTimeUnlockChaser(float time)
        {
            _unlockChaserInSecondTxt.text = time <= 0 ? "" : $"Chaser can move in:\t{time.NiceFloat(1)}";
        }

        public void ShowTimer(string s)
        {
            timerText.text = s;
        }

        public void ShowNotify(string s)
        {
            notifyTxt.transform.parent.gameObject.SetActive(true);
            notifyTxt.text = s;
        }

        public void HideWhoIsChaser()
        {
            foreach (var i in chaserVisibility)
                i.gameObject.SetActive(false);
        }

        public void ShowWhoIsChaser(PlayerMovementAdvanced[] players)
        {
            HideWhoIsChaser();

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].IsChaser) chaserVisibility[i].gameObject.SetActive(true);
            }
        }
    }
}