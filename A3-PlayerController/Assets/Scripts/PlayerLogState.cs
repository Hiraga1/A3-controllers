using UnityEngine;
using UnityEngine.UI;
public class PlayerLogState : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI velocityTxt;
    [SerializeField] private TMPro.TextMeshProUGUI internalLogTxt;
    [SerializeField] private TMPro.TextMeshProUGUI isReadyTxt;

    private GameManager manager;
    private PlayerMovementAdvanced player;

    private void Awake()
    {
        velocityTxt.text = "";
        internalLogTxt.text = "";
        isReadyTxt.text = "";
    }

    public void SetActiveReadyState(bool value)
    {
        isReadyTxt.transform.parent.gameObject.SetActive(value);
    }

    public void Init(GameManager manager, PlayerMovementAdvanced player)
    {
        this.manager = manager;
        this.player = player;
    }

    private void Update()
    {
        if (manager != null)
        {
            velocityTxt.transform.parent.gameObject.SetActive(manager.IsPlaying);
            internalLogTxt.transform.parent.gameObject.SetActive(manager.IsPlaying);
            if (player != null)
            {
                isReadyTxt.text = player.IsRegister ? "Ready" : "Press start to ready";
                if (manager.IsPlaying)
                {
                    var velocity = player.Velocity;
                    velocityTxt.text = $"{velocity.magnitude.NiceFloat(1)}-{velocity}";
                    internalLogTxt.text = $"isChaser: {player.IsChaser} state: {player.state}";
                }
            }
        }
    }
}