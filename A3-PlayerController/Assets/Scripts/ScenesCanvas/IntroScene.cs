using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScenesCanvas
{
    public class IntroScene : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _intro;

        private PlayerInputManager inputManager;

        public void Init(PlayerInputManager manager)
        {
            inputManager = manager;
        }
        public IEnumerator Transition(System.Action onBeginFade, System.Action onDone)
        {
            gameObject.SetActive(true);
            _intro.alpha = 1;
            inputManager?.DisableJoining();
            yield return new WaitForSeconds(3); //time show intro

            onBeginFade?.Invoke();
            float timeTransparentIntro = 0.8f; //time for fade it

            float elapsedTime = 0f;
            while (elapsedTime < timeTransparentIntro)
            {
                _intro.alpha = Mathf.Lerp(1, 0, elapsedTime / timeTransparentIntro);
                yield return null;
                elapsedTime += Time.deltaTime;
            }
            gameObject.SetActive(false);
            inputManager.EnableJoining();
            onDone?.Invoke();
        }
    }
}