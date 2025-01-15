using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ScenesCanvas
{
    public class EndGameScene : MonoBehaviour
    {
        [SerializeField] TMPro.TextMeshProUGUI _message;
        [SerializeField] Button _newGameBtn;

        public void ShowResult(bool isPlayer1Win, System.Action onNewGameClick)
        {
            _message.text = isPlayer1Win ? "Player 1 win" : "Player 2 win";
            _newGameBtn.onClick.AddListener(() => onNewGameClick());
        }
    }
}