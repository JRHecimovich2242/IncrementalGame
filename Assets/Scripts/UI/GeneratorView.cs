using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorView : MonoBehaviour
{
    private readonly Color CAN_AFFORD_COLOR = Color.green;
    private readonly Color CANT_AFFORD_COLOR = Color.red;
    [SerializeField] private GameObject obscureCover = null;
    [SerializeField] private GameObject generationCompleteVisual = null;
    [SerializeField] private Image generatorIconImage = null;
    [SerializeField] private Image progressFill = null;
    [SerializeField] private TMP_Text nameText = null;
    [SerializeField] private TMP_Text costText = null;
    [SerializeField] private TMP_Text numOwnedText = null;
    [SerializeField] private TMP_Text generationRateText = null;
    [SerializeField] private TMP_Text generatorDescriptionText = null;
    [SerializeField] private GameObject activationButtonCover = null;
	[SerializeField] private GameObject normalProgressBarParent = null;
	[SerializeField] private GameObject rapidGenerationProgressBarParent = null;

	public bool Obscured { get { return obscureCover != null && obscureCover.activeSelf; } }

    public bool RapidDisplay { get { return rapidGenerationProgressBarParent != null && rapidGenerationProgressBarParent.activeSelf; } }

    private void Awake()
    {
        SetRapidGeneration(false);
    }

    public void SetDescription(string descriptionText)
    {
        if(generatorDescriptionText != null)
        {
            generatorDescriptionText.text = descriptionText;
        }
    }

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

    public void SetNumOwned(HugeInt numOwned)
    {
        if (activationButtonCover != null)
        {
            activationButtonCover.SetActive(numOwned <= HugeInt.Zero);
        }

        if (numOwned > HugeInt.Zero && !gameObject.activeSelf)
        {
            SetViewActive(true);
        }
        else if (gameObject.activeSelf && numOwned <= HugeInt.Zero)
        {
            //SetEnabled(false);
        }

        if (numOwnedText != null)
        {
            numOwnedText.text = numOwned.ToString();
        }
    }

    public void SetViewActive(bool isEnabled)
    {
        // Logic for hiding panels for upcoming generators
        gameObject.SetActive(isEnabled);
    }

    public void TransitionToInfiniteGeneration()
    {
        throw new NotImplementedException();
    }

    public void SetCostText(HugeInt cost, bool canAfford = true)
    {
        if(costText != null)
        {
            costText.text = CurrencyManager.ConvertHugeIntCurrencyToString(cost);

            costText.color = canAfford ? CAN_AFFORD_COLOR : CANT_AFFORD_COLOR;
        }
    }

    public void UpdateAmountGeneratedText(HugeInt amountGenerated)
    {
        if(generationRateText != null)
        {
            generationRateText.text = CurrencyManager.ConvertHugeIntCurrencyToString(amountGenerated);
        }
    }

    public void SetObscured(bool shouldObscure)
    {
        if(obscureCover != null)
        {
            obscureCover.SetActive(shouldObscure);
        }
    }

    public void SetRapidGeneration(bool isRapid)
    {
		if (normalProgressBarParent != null)
		{
			normalProgressBarParent.SetActive(!isRapid);
		}

		if (rapidGenerationProgressBarParent != null)
		{
			rapidGenerationProgressBarParent.SetActive(isRapid);
		}
	}
}
