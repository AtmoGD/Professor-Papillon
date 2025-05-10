using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MushroomController : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public PlantData Data { get; private set; } = null;
    [field: SerializeField] public GameObject MushroomPRefab { get; private set; } = null;

    [SerializeField] private Volume volume;
    [SerializeField] private float timeMin = 5f;
    [SerializeField] private float timeMax = 25f;
    [SerializeField] private float activeLerpSpeed = 1f;
    [SerializeField] private float chromaticSpeed = 1f;
    [SerializeField] private float chromaticMin = 0.5f;
    [SerializeField] private float chromaticMax = 1f;
    [SerializeField] private float chromaticRandomnessMin = 0.5f;
    [SerializeField] private float chromaticRandomnessMax = 1f;
    [SerializeField] private float chromaticRandomness => UnityEngine.Random.Range(chromaticRandomnessMin, chromaticRandomnessMax);
    [SerializeField] private float lensDistortionSpeed = 1f;
    [SerializeField] private float lensDistortionMin = -0.5f;
    [SerializeField] private float lensDistortionMax = 1f;
    [SerializeField] private float lensDistortionRandomnessMin = 0.5f;
    [SerializeField] private float lensDistortionRandomnessMax = 1f;
    [SerializeField] private float lensDistortionRandomness => UnityEngine.Random.Range(lensDistortionRandomnessMin, lensDistortionRandomnessMax);
    [SerializeField] private bool active = false;
    private float timeLeft;
    private float activeMultiplier = 1f;

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        active = timeLeft > 0;

        activeMultiplier = Mathf.Lerp(activeMultiplier, active ? 1 : 0, activeLerpSpeed * Time.deltaTime);

        if (!active && activeMultiplier == 0)
            return;


        if (volume.profile.TryGet(out ChromaticAberration chromaticAberration))
        {
            float chromaticTarget = Remap01(Mathf.PingPong((Time.time + chromaticRandomness) * chromaticSpeed, 1), chromaticMin, chromaticMax) * activeMultiplier;
            chromaticAberration.intensity.value = chromaticTarget;
        }

        if (volume.profile.TryGet(out LensDistortion lensDistortion))
        {
            float lensDistortionTarget = Remap01(Mathf.PingPong((Time.time + lensDistortionRandomness) * lensDistortionSpeed, 1), lensDistortionMin, lensDistortionMax) * activeMultiplier;
            lensDistortion.intensity.value = lensDistortionTarget;
        }
    }

    public void SetEffectActive(bool active)
    {
        this.active = active;
        timeLeft = UnityEngine.Random.Range(timeMin, timeMax);
    }

    private float Remap01(float value, float from1, float to1)
    {
        return Remap(value, 0, 1, from1, to1);
    }

    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (active) return;

        Plant plant = other.GetComponent<Plant>();
        if (plant.Data == Data && !plant.IsPlacementIndicator)
        {
            // Spawn Mushrooms and get wonky wonkxy
            Debug.Log("Mushroom Spawned");
            plant.gameObject.SetActive(false);
        }
    }
}
