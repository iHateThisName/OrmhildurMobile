using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGridGame : MonoBehaviour {

    //[SerializeField] private TMP_Text gameStateText;
    //[SerializeField] private Button endTurnButton;

    [Header("General UI")]
    [SerializeField] private TMP_Text currentToolText;
    [SerializeField] private Button restartButton;

    [Header("Tool Charge UI")]
    [SerializeField] private TMP_Text icePickChargeText;
    [SerializeField] private TMP_Text hammerChargeText;
    [SerializeField] private TMP_Text magnifyingGlassChargeText;

    public void OnMainMenuButton()
    {
        GameSceneManager.Instance.LoadScene(EnumScene.MainMenuScene);
    }

    public void OnEnable() {
        //GridGameManager.OnGameStateChanged += HandleGameStateChanged;
        GridGameManager.OnToolChanged += HandleToolChanged;

        // Listen to the inventory manager
        InventoryManager.OnToolChargeChanged += HandleToolChargeChanged;
    }
    public void OnDisable() {
        //GridGameManager.OnGameStateChanged -= HandleGameStateChanged;
        GridGameManager.OnToolChanged -= HandleToolChanged;

        // Always unsubscribe to prevent memory leaks
        InventoryManager.OnToolChargeChanged -= HandleToolChargeChanged;
    }

    private void Start() {
        //this.endTurnButton.onClick.AddListener(() => {
        //    GridGameManager.Instance.ChangeGameState(EnumGridGameState.SimulatingPlayerEnd);
        //});

        this.restartButton.onClick.AddListener(() => {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    private void HandleToolChanged(EnumGridTool tool)
    {
        if (currentToolText != null)
        {
            this.currentToolText.text = $"{tool}";
        }
    }

    private void HandleToolChargeChanged(EnumGridTool tool, int newChargeCount)
    {
        switch (tool)
        {
            case EnumGridTool.IcePick:
                if (icePickChargeText != null) icePickChargeText.text = newChargeCount.ToString();
                break;
            case EnumGridTool.Hammer:
                if (hammerChargeText != null) hammerChargeText.text = newChargeCount.ToString();
                break;
            case EnumGridTool.MagnifyingGlass:
                if (magnifyingGlassChargeText != null) magnifyingGlassChargeText.text = newChargeCount.ToString();
                break;
        }
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
