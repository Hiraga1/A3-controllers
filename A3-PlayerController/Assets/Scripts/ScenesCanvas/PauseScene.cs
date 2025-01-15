using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

namespace ScenesCanvas
{
    public class PauseScene : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _messagext;

        public bool IsPausing { get; private set; }

        private const float pauseTime = 3;
        private float currentPauseTime;
        private bool isCoundtingDown = false;

        public void Pop()
        {
            gameObject.SetActive(true);
            _messagext.text = "PAUSE";
            IsPausing = true;
        }

        private System.Action onDone;
        public void UnPause(System.Action onDoneCountdown)
        {
            if (isCoundtingDown) return;
            //countdown();
            onDone = onDoneCountdown;
            isCoundtingDown = true;
            currentPauseTime = pauseTime;       
        }

        private void Update()
        {
            if (currentPauseTime > 0 && isCoundtingDown)
            {
                currentPauseTime -= Time.unscaledDeltaTime;
                _messagext.text = $"{currentPauseTime.NiceFloat(1)}";
                if (currentPauseTime < 0)
                {
                    gameObject.SetActive(false);
                    isCoundtingDown = false;
                    IsPausing = false;
                    onDone?.Invoke();
                }
            }
        }
    }
}