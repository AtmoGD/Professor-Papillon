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

    [field: SerializeField] public List<Dialogue> Dialogues { get; private set; } = new List<Dialogue>();

    private int currentDialogueIndex = 0;

    public void StartDialogue()
    {
        if (Dialogues.Count == 0)
            return;

        StartCoroutine(DialogueCoroutine(Dialogues[currentDialogueIndex], 1.5f));
    }


    IEnumerator DialogueCoroutine(Dialogue dialogue, float startDelay = 0f)
    {
        NextButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(startDelay);

        MoodImage.sprite = dialogue.MoodImage;

        string currentText = "";
        for (int i = 0; i < dialogue.Text.Length; i++)
        {
            currentText += dialogue.Text[i];
            DialogueText.text = currentText;
            yield return new WaitForSeconds(dialogue.CharacterTime);
        }

        if (dialogue.AutoSkip)
        {
            yield return new WaitForSeconds(dialogue.AutoSkipTime);
            NextDialogue(true);
        }
        else if (dialogue.Skippable)
        {
            NextButton.gameObject.SetActive(true);
        }
    }

    public void NextDialogue(bool forced = false)
    {
        if (!forced && !Dialogues[currentDialogueIndex].Skippable)
            return;

        currentDialogueIndex++;
        if (currentDialogueIndex >= Dialogues.Count)
        {
            currentDialogueIndex = 0;
            return;
        }

        StartCoroutine(DialogueCoroutine(Dialogues[currentDialogueIndex]));
    }
}
