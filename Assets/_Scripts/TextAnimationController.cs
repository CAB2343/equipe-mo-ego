using TMPro;
using UnityEngine;

#region Settings Structs

[System.Serializable]
public struct WaveSettings
{
    public bool enabled;
    public float amplitude;
    public float frequency;
    public float speed;
}

[System.Serializable]
public struct ShakeSettings
{
    public bool enabled;
    public float magnitude;
    public float speed;
}

[System.Serializable]
public struct BounceSettings
{
    public bool enabled;
    public float height;
    public float speed;
}

[System.Serializable]
public struct FadeSettings
{
    public bool enabled;
    public float speed;
}

[System.Serializable]
public struct ScaleSettings
{
    public bool enabled;
    public float scaleMultiplier;
    public float speed;
}

[System.Serializable]
public struct RotateSettings
{
    public bool enabled;
    public float angle;
    public float speed;
}

[System.Serializable]
public struct JitterSettings
{
    public bool enabled;
    public float magnitude;
    public float speed;
}

[System.Serializable]
public struct ImpactSettings
{
    public bool enabled;
    public bool loop;                  // <- novo
    public float impactDistance;
    public float impactDuration;
    public AnimationCurve impactCurve;
}


[System.Serializable]
public struct TypewriterSettings
{
    public bool enabled;
    public float charsPerSecond;
    public bool randomDelay;
    public Vector2 delayRange;
}

#endregion

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextAnimationController : MonoBehaviour
{
    [Header("Global")]
    public bool animateText = true;
    public bool skipWithKey = true;
    public KeyCode skipKey = KeyCode.Space;

    [Header("Typewriter")]
    public TypewriterSettings typewriter = new TypewriterSettings
    {
        enabled = true,
        charsPerSecond = 30f,
        randomDelay = false,
        delayRange = new Vector2(0.01f, 0.05f)
    };

    [Header("Effects")]
    public WaveSettings wave;
    public ShakeSettings shake;
    public BounceSettings bounce;
    public FadeSettings fade;
    public ScaleSettings scale;
    public RotateSettings rotate;
    public JitterSettings jitter;
    public ImpactSettings impact;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip charSound;
    public float charSoundPitchVariation = 0.1f;

    private TextMeshProUGUI tmpText;
    private TMP_TextInfo textInfo;

    private string originalText;
    private float typeTimer;
    private int visibleCharacterCount;
    private float[] charTimers;
    private float[] charImpactTimers;

    private bool skipped;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        originalText = tmpText.text;
        tmpText.ForceMeshUpdate();
    }

    private void Start()
    {
        SetText(originalText);
    }

    public void SetText(string newText)
    {
        originalText = newText;
        tmpText.text = newText;
        typeTimer = 0f;
        visibleCharacterCount = 0;
        skipped = false;
        tmpText.maxVisibleCharacters = 0;
        tmpText.ForceMeshUpdate();

        int len = newText.Length;
        charTimers = new float[len];
        charImpactTimers = new float[len];

        if (typewriter.randomDelay)
        {
            for (int i = 0; i < len; i++)
            {
                charTimers[i] = Random.Range(typewriter.delayRange.x, typewriter.delayRange.y);
            }
        }
    }

    void Update()
    {
        if (!animateText) return;

        if (skipWithKey && !skipped && Input.GetKeyDown(skipKey))
        {
            skipped = true;
            visibleCharacterCount = originalText.Length;
            tmpText.maxVisibleCharacters = visibleCharacterCount;
        }

        // Typewriter logic
        if (typewriter.enabled && !skipped)
        {
            if (typewriter.randomDelay)
            {
                while (visibleCharacterCount < originalText.Length &&
                       typeTimer >= charTimers[visibleCharacterCount])
                {
                    PlayCharSound();
                    typeTimer -= charTimers[visibleCharacterCount];
                    charImpactTimers[visibleCharacterCount] = Time.time; // Start impact
                    visibleCharacterCount++;
                }

                typeTimer += Time.deltaTime;
            }
            else
            {
                typeTimer += Time.deltaTime * typewriter.charsPerSecond;
                int newVisible = Mathf.FloorToInt(typeTimer);

                while (visibleCharacterCount < newVisible && visibleCharacterCount < originalText.Length)
                {
                    PlayCharSound();
                    charImpactTimers[visibleCharacterCount] = Time.time; // Start impact
                    visibleCharacterCount++;
                }
            }

            tmpText.maxVisibleCharacters = visibleCharacterCount;
        }
        else
        {
            tmpText.maxVisibleCharacters = int.MaxValue;
        }

        tmpText.ForceMeshUpdate();
        textInfo = tmpText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (typewriter.enabled && i >= visibleCharacterCount)
                continue;

            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] verts = textInfo.meshInfo[matIndex].vertices;

            Vector3 offset = Vector3.zero;

            // Impact
            if (impact.enabled)
            {
                float eval = 0f;

                if (impact.loop)
                {
                    float loopTime = (Time.time + i * 0.1f) % impact.impactDuration;
                    float t = loopTime / impact.impactDuration;
                    eval = impact.impactCurve.Evaluate(t);
                }
                else if (charImpactTimers.Length > i)
                {
                    float t = (Time.time - charImpactTimers[i]) / impact.impactDuration;
                    if (t <= 1f)
                    {
                        eval = impact.impactCurve.Evaluate(t);
                    }
                }

                offset.y -= eval * impact.impactDistance;
            }


            // Wave
            if (wave.enabled)
            {
                offset.y += Mathf.Sin(Time.time * wave.frequency + i * wave.speed) * wave.amplitude;
            }

            // Shake
            if (shake.enabled)
            {
                offset += new Vector3(
                    Mathf.PerlinNoise(Time.time * shake.speed + i, 0) - 0.5f,
                    Mathf.PerlinNoise(0, Time.time * shake.speed + i) - 0.5f,
                    0f
                ) * shake.magnitude;
            }

            // Bounce
            if (bounce.enabled)
            {
                float b = Mathf.Abs(Mathf.Sin((Time.time + i * 0.1f) * bounce.speed)) * bounce.height;
                offset.y += b;
            }

            // Jitter (X only)
            if (jitter.enabled)
            {
                offset.x += Mathf.Sin(Time.time * jitter.speed + i) * jitter.magnitude;
            }

            // Apply offset
            for (int j = 0; j < 4; j++)
            {
                verts[vertexIndex + j] += offset;
            }

            // Scale
            if (scale.enabled)
            {
                float s = 1 + Mathf.Sin(Time.time * scale.speed + i * 0.2f) * scale.scaleMultiplier;
                Vector3 center = (verts[vertexIndex] + verts[vertexIndex + 2]) / 2;
                for (int j = 0; j < 4; j++)
                {
                    verts[vertexIndex + j] = center + (verts[vertexIndex + j] - center) * s;
                }
            }

            // Rotate
            if (rotate.enabled)
            {
                float angle = Mathf.Sin(Time.time * rotate.speed + i) * rotate.angle;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 center = (verts[vertexIndex] + verts[vertexIndex + 2]) / 2;
                Quaternion rot = Quaternion.Euler(0, 0, angle);
                for (int j = 0; j < 4; j++)
                {
                    verts[vertexIndex + j] = center + rot * (verts[vertexIndex + j] - center);
                }
            }

            // Optional: fade logic (can be added using vertex colors per character)
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            tmpText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    private void PlayCharSound()
    {
        if (audioSource != null && charSound != null)
        {
            audioSource.pitch = 1f + Random.Range(-charSoundPitchVariation, charSoundPitchVariation);
            audioSource.PlayOneShot(charSound);
        }
    }
}
