/* https://github.com/hww/noise_flow_field */

using UnityEngine;
using VARP.KochFractals;

namespace VARP.NoiseFields
{
    public class AudioFlowField : MonoBehaviour
    {
        public NoiseFlowField noiseFlowField;
        public AudioPeer audioPeer;
        [Header("Speed")] 
        public bool useSpeed = true;
        public Vector2 moveSpeedMinMax = new Vector2(0,50);
        public Vector2 rotationSpeedMinMax = new Vector2(50,250);
        [Header("Scale")] 
        public bool useScale = true;
        public Vector2 scaleMinMax = new Vector2(0,3);
        [Header("Material")] 
        public Material material;
        private Material[] audioMaterial;
        [Header("Color")] 
        public bool useColor1 = true;
        public string colorName1 = "_Color";
        public Gradient gradient1;
        private Color[] color1;
        [UnityEngine.Range(0,1)]
        public float colorThreshold1 = 0.2f;
        public float colorMultiplier1;
        [Header("Color 2")] 
        public bool useColor2 = true;
        public string colorName2 = "_EmissionColor";
        public Gradient gradient2;
        private Color[] color2;
        [UnityEngine.Range(0,1)]
        public float colorThreshold2 = 0.5f;
        public float colorMultiplier2;

        private const int COLORS_NUMBER = 8;

        private void Awake()
        {
            audioMaterial = new Material[COLORS_NUMBER];
            color1 = new Color[COLORS_NUMBER];
            color2 = new Color[COLORS_NUMBER];
            for (int i = 0; i < COLORS_NUMBER; i++)
            {
                color1[i] = gradient1.Evaluate(1f / COLORS_NUMBER * i);
                color2[i] = gradient2.Evaluate(1f / COLORS_NUMBER * i);
                audioMaterial[i] = new Material(material);
            }
            noiseFlowField = GetComponent<NoiseFlowField>();
            noiseFlowField.onParticlesGenerated = () =>
            {
                var bandCount = 0;
                for (var i = 0; i < noiseFlowField.amountOfParticles; i++)
                {
                    var band = bandCount % 8;
                    var particle = noiseFlowField.particles[i];
                    particle.meshRenderer.material = audioMaterial[band];
                    particle.audioBand = band;
                    bandCount++;
                }
            };
        }
        
        private void Update()
        {
            if (useSpeed)
            {
                noiseFlowField.particleMoveSpeed =
                    Mathf.Lerp(moveSpeedMinMax.x, moveSpeedMinMax.y, audioPeer.amplitudeBuffer);
                noiseFlowField.rotationSpeed =
                    Mathf.Lerp(rotationSpeedMinMax.x, rotationSpeedMinMax.y, audioPeer.amplitudeBuffer);
            }

            if (useScale)
            {
                for (var i = 0; i < noiseFlowField.amountOfParticles; i++)
                {
                    var particle = noiseFlowField.particles[i];
                    var ratio = audioPeer.audioBandBuffer[particle.audioBand];
                    if (!float.IsNaN(ratio))
                    {
                        var scale = Mathf.Lerp(scaleMinMax.x, scaleMinMax.y,
                            audioPeer.audioBandBuffer[particle.audioBand]);
                        particle.transform.localScale = new Vector3(scale, scale, scale);
                    }
                }
            }
            if (useColor1)
            {
                for (var i = 0; i < COLORS_NUMBER; i++)
                {
                    if (audioPeer.audioBandBuffer[i] > colorThreshold1)
                        audioMaterial[i].SetColor(colorName1, color1[i] * audioPeer.audioBandBuffer[i] * colorMultiplier1);
                    else
                        audioMaterial[i].SetColor(colorName1, color1[i] * 0f);
                }
            }
            if (useColor2)
            {
                for (var i = 0; i < COLORS_NUMBER; i++)
                {
                    if (audioPeer.audioBandBuffer[i] > colorThreshold2)
                        audioMaterial[i].SetColor(colorName2, color2[i] * audioPeer.audioBand[i] * colorMultiplier2);
                    else
                        audioMaterial[i].SetColor(colorName2, color2[i] * 0f);
                }
            }
        }
    }
}