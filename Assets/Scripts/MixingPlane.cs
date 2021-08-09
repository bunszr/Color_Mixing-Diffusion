using UnityEngine;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;

public class MixingPlane : MonoBehaviour
{
    static readonly int
        uvMousePosId = Shader.PropertyToID("uvMousePos"),
        deltaTimeId = Shader.PropertyToID("deltaTime"),
        dampingId = Shader.PropertyToID("damping"),
        brushSizeId = Shader.PropertyToID("brushSize"),
        autoDiffusionId = Shader.PropertyToID("autoDiffusion");

    int kernelIndex;

    public ComputeShader computeShader;
    [SerializeField, HideInInspector] SimulationSetting currSetting;
    [HideInInspector] public int currSettingIndex;
    public SimulationSetting[] settings;

    ComputeBuffer buffer;

    RenderTexture renderTexture;
    Material material;

    private void Start()
    {
        material = GetComponent<Renderer>().sharedMaterial;
        PixelData[] pixelDatas = new PixelData[currSetting.resolation * currSetting.resolation];
        Color[] fromColors = currSetting.fromTexture.GetPixels();
        Color[] toColors = currSetting.toTexture.GetPixels();

        for (int i = 0; i < pixelDatas.Length; i++)
        {
            pixelDatas[i] = new PixelData(fromColors[i], toColors[i], fromColors[i].a > .1f ? 1 : 0,
                currSetting.autoDiffusion ? currSetting.difusionSpeed : 0,
                currSetting.difusionSpeed);
        }
        buffer = new ComputeBuffer(pixelDatas.Length, 15 * 4);
        buffer.SetData(pixelDatas);

        renderTexture = new RenderTexture(currSetting.resolation, currSetting.resolation, 24, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        Graphics.Blit(currSetting.fromTexture, renderTexture);

        kernelIndex = computeShader.FindKernel("CSMain");

        computeShader.SetBuffer(kernelIndex, "PixelDatas", buffer);
        computeShader.SetTexture(kernelIndex, "Result", renderTexture);
        computeShader.SetFloat("resolation", (float)currSetting.resolation);
        computeShader.Dispatch(kernelIndex, currSetting.resolation / 8, currSetting.resolation / 8, 1);
        material.SetTexture("_BaseMap", renderTexture);
    }

    private void OnDisable()
    {
        if (buffer != null)
        {
            buffer.Release(); // Ekran kartında ayırdığımız belleği salıyoruz(yok editoruz).
            buffer = null;
        }
    }

    private void Update()
    {
        Vector4 uvMousePos = Vector4.zero;
        if (Input.GetMouseButton(0))
            uvMousePos = GetUvCoordinate();

        computeShader.SetVector(uvMousePosId, uvMousePos);
        computeShader.SetFloat(deltaTimeId, Time.deltaTime);
        computeShader.SetFloat(brushSizeId, currSetting.brushSize);
        computeShader.SetFloat(dampingId, currSetting.damping);
        computeShader.SetBool(autoDiffusionId, currSetting.autoDiffusion);
        computeShader.Dispatch(kernelIndex, currSetting.resolation / 8, currSetting.resolation / 8, 1); //CSMain foksiyonumuzu çalıştır

        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    Vector4 GetUvCoordinate()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit);
        return new Vector4(hit.textureCoord.x, hit.textureCoord.y);
    }

    public void UpdateSetting()
    {
        currSetting = settings[currSettingIndex];
        if (material == null)
            material = GetComponent<MeshRenderer>().material;
        material.SetTexture("_BaseMap", currSetting.fromTexture);
    }

    public struct PixelData
    {
        public Color currentColor;
        public Color fromColor;
        public Color toColor;
        public float time;
        public float speed;
        public float defaultspeed;

        public PixelData(Color fromColor, Color toColor, float time, float speed, float defaultspeed)
        {
            this.currentColor = fromColor;
            this.fromColor = fromColor;
            this.toColor = toColor;
            this.time = time;
            this.speed = speed;
            this.defaultspeed = defaultspeed;
        }
    }
}