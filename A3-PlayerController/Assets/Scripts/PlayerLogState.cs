using UnityEngine;

public class PlayerLogState : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI velocityTxt;
    [SerializeField] private TMPro.TextMeshProUGUI isSprintTxt;
    [SerializeField] private TMPro.TextMeshProUGUI isReadyTxt;

    private GameManager manager;
    private PlayerMovementAdvanced player;

    private void Awake()
    {
        velocityTxt.text = "";
        isSprintTxt.text = "";
        isReadyTxt.text = "";
    }

    public void Init(GameManager manager, PlayerMovementAdvanced player)
    {
        this.manager = manager;
        this.player = player;
    }

    private void Update()
    {
        if (manager != null && player != null)
        {
            isReadyTxt.text = player.IsRegister ? "Ready" : "Not Ready";
            isReadyTxt.transform.parent.gameObject.SetActive(!manager.IsPlaying);
            if (manager.IsPlaying)
            {
                var velocity = player.Velocity;
                velocityTxt.text = $"{velocity.magnitude.NiceFloat(1)}-{velocity}";
                isSprintTxt.text = $"state: {player.state}";
            }
        }
    }
}