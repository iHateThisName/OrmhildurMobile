using UnityEngine;
using UnityEngine.UI;

public class PageController : MonoBehaviour {

    [SerializeField] private Image pageImage;

    private bool isRightPage = true;
    private EnumBiomes biome = EnumBiomes.None;

    private void Awake() {
        if (this.biome == EnumBiomes.None) {
            Debug.LogError("Current biome is not set. Please set the current biome in the inspector.");
            return;
        }


        EnumBiomes currentBiomeSelected = GameManager.Instance.CurrentBiomeSelected;
        EnumBiomes nextBiome = GameManager.Instance.CurrentBiomeSelected + 1;
        //EnumBiomes previousBiome = GameManager.Instance.CurrentBiomeSelected - 1 == EnumBiomes.None ? 
        if (this.biome != currentBiomeSelected) {
            this.pageImage.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update() {

    }
}

public enum EnumBiomes : int {
    None = 0,
    Cliffs = 1,
    JaggedMountains = 2,
    Sea = 3,
}
