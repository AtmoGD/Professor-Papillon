using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Dialogue
{
    [field: SerializeField] public Sprite MoodImage { get; set; }
    [field: SerializeField] public string Text { get; set; }
    [field: SerializeField] public float CharacterTime { get; set; }
    [field: SerializeField] public bool AutoSkip { get; set; } = true;
    [field: SerializeField] public float AutoSkipTime { get; set; } = 2f;
    [field: SerializeField] public bool Skippable { get; set; } = true;
}

public class DialogueSystem : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public Image MoodImage { get; private set; } = null;
    [field: SerializeField] public TMP_Text DialogueText { get; private set; } = null;
    [field: SerializeField] public Button NextButton { get; private set; } = null;
    [field: SerializeField] public FMODUnity.StudioEventEmitter MumblingEmitter { get; private set; } = null;

    [SerializeField] public List<Dialogue> Dialogues = new List<Dialogue>();

    [SerializeField] public ButterflyData ButterflyDataA = null;
    [SerializeField] public List<Dialogue> SpawnSingleButterflyA = new List<Dialogue>();
    [SerializeField] public ButterflyData ButterflyDataB = null;
    [SerializeField] public List<Dialogue> SpawnSingleButterflyB = new List<Dialogue>();
    [SerializeField] public ButterflyData ButterflyDataC = null;
    [SerializeField] public List<Dialogue> SpawnSingleButterflyC = new List<Dialogue>();

    [SerializeField] public List<Dialogue> SpawnMulti = new List<Dialogue>();

    [SerializeField] public List<Dialogue> PlantDie = new List<Dialogue>();
    [SerializeField] public float DialogueChance = 0.5f;

    IEnumerator currentDialogueCoroutine = null;

    private bool tutorialDone = false;



    private int currentDialogueIndex = 0;

    public void StartDialogue()
    {
        if (Dialogues.Count == 0)
            return;

        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        currentDialogueCoroutine = DialogueCoroutine(Dialogues[currentDialogueIndex], 1.5f);

        StartCoroutine(currentDialogueCoroutine);
    }


    IEnumerator DialogueCoroutine(Dialogue dialogue, float startDelay = 0f)
    {
        NextButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(startDelay);

        MoodImage.sprite = dialogue.MoodImage;

        MumblingEmitter.Play();

        string currentText = "";
        for (int i = 0; i < dialogue.Text.Length; i++)
        {
            currentText += dialogue.Text[i];
            DialogueText.text = currentText;
            yield return new WaitForSeconds(dialogue.CharacterTime);
        }

        MumblingEmitter.Stop();

        if (dialogue.AutoSkip)
        {
            yield return new WaitForSeconds(dialogue.AutoSkipTime);
            NextDialogue(true);
        }
        else if (dialogue.Skippable)
        {
            NextButton.gameObject.SetActive(true);
        }

        currentDialogueCoroutine = null;
    }

    public void NextDialogue(bool forced = false)
    {
        if (!forced && !Dialogues[currentDialogueIndex].Skippable)
            return;

        currentDialogueIndex++;
        if (currentDialogueIndex >= Dialogues.Count - 1)
        {
            tutorialDone = true;
            return;
        }

        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        currentDialogueCoroutine = DialogueCoroutine(Dialogues[currentDialogueIndex], 0.5f);

        StartCoroutine(currentDialogueCoroutine);
    }


    public void SingleButterflySpawn(Butterfly butterfly)
    {
        if (currentDialogueCoroutine != null || !tutorialDone)
        {
            return;
        }

        if (Random.Range(0f, 1f) > DialogueChance)
        {
            return;
        }

        if (butterfly.Data == ButterflyDataA)
        {
            Dialogue dialogue = SpawnSingleButterflyA[Random.Range(0, SpawnSingleButterflyA.Count)];
            StartCoroutine(DialogueCoroutine(dialogue, 0.5f));
        }
        else if (butterfly.Data == ButterflyDataB)
        {
            Dialogue dialogue = SpawnSingleButterflyB[Random.Range(0, SpawnSingleButterflyB.Count)];
            StartCoroutine(DialogueCoroutine(dialogue, 0.5f));
        }
        else if (butterfly.Data == ButterflyDataC)
        {
            Dialogue dialogue = SpawnSingleButterflyC[Random.Range(0, SpawnSingleButterflyC.Count)];
            StartCoroutine(DialogueCoroutine(dialogue, 0.5f));
        }
    }

    public void MultiButterflySpawn()
    {
        if (currentDialogueCoroutine != null || !tutorialDone)
        {
            return;
        }

        if (Random.Range(0f, 1f) > DialogueChance)
        {
            return;
        }

        Dialogue dialogue = SpawnMulti[Random.Range(0, SpawnMulti.Count)];

        currentDialogueCoroutine = DialogueCoroutine(dialogue, 0.5f);

        StartCoroutine(currentDialogueCoroutine);
    }

    public void PlantDying()
    {
        if (currentDialogueCoroutine != null || !tutorialDone)
        {
            return;
        }

        if (Random.Range(0f, 1f) > DialogueChance)
        {
            return;
        }


        Dialogue dialogue = PlantDie[Random.Range(0, PlantDie.Count)];

        currentDialogueCoroutine = DialogueCoroutine(dialogue, 0.5f);

        StartCoroutine(currentDialogueCoroutine);
    }
}
