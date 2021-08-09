using UnityEngine;

[CreateAssetMenu(fileName = "SimulationSetting")]
public class SimulationSetting : ScriptableObject
{
    public int resolation = 1024;
    public Texture2D fromTexture;
    public Texture2D toTexture;

    [Tooltip("Runtime'da değiştirilmesi bir anlam ifade etmez")]
    public float difusionSpeed = 21;
    public float damping = .5f;
    public bool autoDiffusion;
    [Range(.1f, .4f)] public float brushSize = .15f;
}