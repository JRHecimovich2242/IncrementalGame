using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;

public class TutorialObserver : MonoBehaviour
{
    enum Step
    {
        Click,
        BuyGenerator,
        ActivateGenerator,
        HighlightGeneratorProgress,
        RepeatActivateGenerator,
        HighlightUpgrade,
        Completed
    }

    private Step _step;

    [Header("Step 1: Click the Cookie")]
    [SerializeField] private GameObject clickPromptUI = null;

    [Header("Step 2: Buy a Generator")]
    [SerializeField] private GameObject buyPromptUI = null;
    [SerializeField] private GeneratorData dataOfTutorialGenerator = null;

    [Header("Step 3: Activate Generator")]
    [SerializeField] private GameObject activatePromptUI = null;

    [Header("Step 4: Show Generator Progress")]
    [SerializeField] private GameObject indicateProgressUI = null;

    [Header("Step 5: Activate Generator Again")]
    [SerializeField] private GameObject activateRepeatPromptUI = null;

    [Header("Step 6: Highlight Upgrade")]
    [SerializeField] private GameObject upgradesPanel = null;
    [SerializeField] private GameObject upgradePromptUI;
    [SerializeField] private UpgradeData tutorialUpgradeData;

    public void TryBeginTutorial(bool gameLoadedFromSave)
    {
        if (!gameLoadedFromSave)
        {
            StartStep(Step.Click);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if(_step == Step.Click && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HideAllPrompts();
            Destroy(gameObject);
        }
    }

    private void StartStep(Step next)
    {
        _step = next;
        HideAllPrompts();

        switch (next)
        {
            case Step.Click:
                {
                    clickPromptUI.SetActive(true);
                    CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyGenerated_ClickerStep;
                    break;
                }

            case Step.BuyGenerator:
                {
                    // wait until they can afford one
                    CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChangedForBuy;
                    break;
                }

            case Step.ActivateGenerator:
                {
                    activatePromptUI.SetActive(true);
                    GameManager.Instance.GetGeneratorEntityMatchingData(dataOfTutorialGenerator).OnDirty += OnTutorialGeneratorDirty;
                    break;
                }
            case Step.RepeatActivateGenerator:
                {
                    activateRepeatPromptUI.SetActive(true);
                    GameManager.Instance.GetGeneratorEntityMatchingData(dataOfTutorialGenerator).OnDirty += OnTutorialGeneratorDirty;
                    break;
                }

            case Step.HighlightGeneratorProgress:
                {
                    Invoke(nameof(HighlightGeneratorProgress), (float)dataOfTutorialGenerator.BaseRate / 2f);
                    break;
                }

            case Step.HighlightUpgrade:
                {
                    CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChangedForUpgrade;
                    break;
            }

            case Step.Completed:
                {
                    Debug.Log("Tutorial complete!");
                    Destroy(gameObject);
                    break;
                }
        }
    }

    private void OnCurrencyGenerated_ClickerStep(CurrencyType type, HugeInt amount)
    {
        if(amount <= HugeInt.Zero) { return; }

        CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyGenerated_ClickerStep;
        clickPromptUI.SetActive(false);
        StartStep(Step.BuyGenerator);
    }

    private void OnCurrencyChangedForBuy(CurrencyType type, HugeInt newBalance)
    {
        if (type != CurrencyType.Basic)
        {
            return;
        }

        // check if they can afford exactly one
        if (newBalance >= (HugeInt)dataOfTutorialGenerator.BaseCost)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChangedForBuy;
            buyPromptUI.SetActive(true);
            GameState.Instance.OnGeneratorPurchasedAction += OnBuyGenerator;
        }
    }

    private void OnBuyGenerator(GeneratorData data)
    {
        GameState.Instance.OnGeneratorPurchasedAction -= OnBuyGenerator;
        buyPromptUI.SetActive(false);
        StartStep(Step.ActivateGenerator);
    }

    private void OnTutorialGeneratorDirty(GeneratorDirtyFlags flags)
    {
        if(flags.HasFlag(GeneratorDirtyFlags.Progress))
        {
            if(_step == Step.ActivateGenerator)
            {
                OnActivateGenerator();
            }
            else if(_step == Step.RepeatActivateGenerator)
            {
                OnActivateGeneratorRepeat();
            }
        }
    }
    private void OnActivateGenerator()
    {
        GameManager.Instance.GetGeneratorEntityMatchingData(dataOfTutorialGenerator).OnDirty -= OnTutorialGeneratorDirty;
        activatePromptUI.SetActive(false);
        StartStep(Step.HighlightGeneratorProgress);
    }

    private void HighlightGeneratorProgress()
    {
        Time.timeScale = 0f;
        indicateProgressUI.SetActive(true);
    }

    public void EndGeneratorProgressHighlight()
    {
        Time.timeScale = 1f;
        indicateProgressUI.SetActive(false);
        Invoke(nameof(TriggerRepeatActivationStep), (float)(dataOfTutorialGenerator.BaseRate / 2f) + .01f);
    }

    private void TriggerRepeatActivationStep()
    {
        StartStep(Step.RepeatActivateGenerator);
    }

    private void OnActivateGeneratorRepeat()
    {
        GameManager.Instance.GetGeneratorEntityMatchingData(dataOfTutorialGenerator).OnDirty -= OnTutorialGeneratorDirty;
        activateRepeatPromptUI.SetActive(false);
        StartStep(Step.HighlightUpgrade);
    }
    private void OnCurrencyChangedForUpgrade(CurrencyType type, HugeInt newBalance)
    {
        if (type != CurrencyType.Basic)
        {
            return;
        }

        // check if they can afford your chosen tutorial upgrade
        if (newBalance >= tutorialUpgradeData.Cost)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChangedForUpgrade;
            upgradePromptUI.SetActive(true);
            transform.SetSiblingIndex(upgradesPanel.transform.GetSiblingIndex() - 1);
            GameState.Instance.OnUpgradePurchasedAction += OnUpgrade;
        }
    }

    private void OnUpgrade(UpgradeData data)
    {
        GameState.Instance.OnUpgradePurchasedAction -= OnUpgrade;
        upgradePromptUI.SetActive(false);
        StartStep(Step.Completed);
    }

    private void HideAllPrompts()
    {
        clickPromptUI.SetActive(false);
        buyPromptUI.SetActive(false);
        activatePromptUI.SetActive(false);
    }

    private void OnDestroy()
    {
        // clean up any lingering subscriptions
        if(CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyGenerated_ClickerStep;
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChangedForBuy;
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChangedForUpgrade;
        }

        if(GameManager.Instance != null)
        {
            GameManager.Instance.GetGeneratorEntityMatchingData(dataOfTutorialGenerator).OnDirty -= OnTutorialGeneratorDirty;
        }
        
        if(GameState.Instance != null)
        {
            GameState.Instance.OnUpgradePurchasedAction -= OnUpgrade;
            GameState.Instance.OnGeneratorPurchasedAction -= OnBuyGenerator;
        }
    }
}
