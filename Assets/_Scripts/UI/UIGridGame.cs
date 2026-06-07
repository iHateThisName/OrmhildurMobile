using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGridGame : MonoBehaviour {

    [SerializeField] private TMP_Text gameStateText;
    [SerializeField] private Button endTurnButton;

    public void OnEnable() {
        GridGameManager.OnGameStateChanged += HandleGameStateChanged;
    }
    public void OnDisable() {
        GridGameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start() {
        this.endTurnButton.onClick.AddListener(() => {
            GridGameManager.Instance.ChangeGameState(EnumGridGameState.SimulatingPlayerEnd);
        });
    }

    private void HandleGameStateChanged(EnumGridGameState state) {
        StringBuilder builder = new StringBuilder();
        builder.Append("Game State: ");
        builder.Append(state.ToString());
        this.gameStateText.text = builder.ToString();

        if (state == EnumGridGameState.PlayerTurn) {
            this.endTurnButton.interactable = true;
        } else {
            this.endTurnButton.interactable = false;
        }
    }
}
