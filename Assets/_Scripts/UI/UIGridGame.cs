using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGridGame : MonoBehaviour {

    //[SerializeField] private TMP_Text gameStateText;
    //[SerializeField] private Button endTurnButton;

    [SerializeField] private TMP_Text currentToolText;
    [SerializeField] private Button restartButton;

    public void OnMainMenuButton()
    {
        GameSceneManager.Instance.LoadScene(EnumScene.MainMenuScene);
    }

    public void OnEnable() {
        //GridGameManager.OnGameStateChanged += HandleGameStateChanged;
        GridGameManager.OnToolChanged += HandleToolChanged;
    }
    public void OnDisable() {
        //GridGameManager.OnGameStateChanged -= HandleGameStateChanged;
        GridGameManager.OnToolChanged -= HandleToolChanged;
    }

    private void Start() {
        //this.endTurnButton.onClick.AddListener(() => {
        //    GridGameManager.Instance.ChangeGameState(EnumGridGameState.SimulatingPlayerEnd);
        //});

        this.restartButton.onClick.AddListener(() => {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    private void HandleToolChanged(EnumGridTool tool) {
        this.currentToolText.text = $"{tool}";
    }

    public void OnHandButton() => OnChangeTool(EnumGridTool.Hand);
    public void OnIcePickButton() => OnChangeTool(EnumGridTool.IcePick);
    public void OnHammerButton() => OnChangeTool(EnumGridTool.Hammer);
    public void OnMagnifyingGlassButton() => OnChangeTool(EnumGridTool.MagnifyingGlass);
    private void OnChangeTool(EnumGridTool tool) => GridGameManager.Instance.CurrentTool = tool;

    //private void HandleGameStateChanged(EnumGridGameState state) {
    //    StringBuilder builder = new StringBuilder();
    //    builder.Append("Game State: ");
    //    builder.Append(state.ToString());
    //    this.gameStateText.text = builder.ToString();

    //    if (state == EnumGridGameState.PlayerTurn) {
    //        this.endTurnButton.interactable = true;
    //    } else {
    //        this.endTurnButton.interactable = false;
    //    }
    //}
}
