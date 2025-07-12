using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorView : MonoBehaviour
{
    [SerializeField] private GameObject generationCompleteVisual = null;
    [SerializeField] private Image generatorIconImage = null;
    [SerializeField] private Image progressFill = null;
    [SerializeField] private TMP_Text nameText = null;
    [SerializeField] private TMP_Text numOwnedText = null;
    //private bool isViewActive = false;
    public void DisplayGenerationCompleteVisual()
    {
        if(generationCompleteVisual != null && !generationCompleteVisual.activeInHierarchy)
        {
            // this will be an object which begins an animation on enable and 
            generationCompleteVisual.SetActive(true);
        }
    }

    public void SetName(string generatorName)
    {
        if(nameText != null)
        {
            nameText.text = generatorName;
        }
    }

    public void PassGeneratorSprites(GeneratorSprites spriteData)
    {
        // TODO: Additional visuals. Include active/inactive states
        throw new NotImplementedException();
    }

    public void SetIconSprite(Sprite icon)
    {
        if(generatorIconImage != null)
        {
            generatorIconImage.sprite = icon;
        }
    }

    public void SetProgress(float progress)
    {
        if(progressFill != null)
        {
            progressFill.fillAmount = progress;
        }
    }

    public void SetNumOwned(uint numOwned)
    {
        //if(numOwned > 0 && !isViewActive)
        //{
        //    SetEnabled(true);
        //}
        //else if(isViewActive && numOwned <= 0)
        //{
        //    SetEnabled(false);
        //}


        if (numOwnedText != null)
        {
            numOwnedText.text = numOwned.ToString();
        }
    }

    public void SetEnabled(bool isEnabled)
    {
        // Logic for hiding panels for upcoming generators
        throw new NotImplementedException();
    }

    public void TransitionToInfiniteGeneration()
    {
        throw new NotImplementedException();
    }
}
