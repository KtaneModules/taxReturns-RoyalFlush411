using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using taxReturns;

public class taxReturnsScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable pageLeft;
    public KMSelectable pageRight;
    public KMSelectable toggleSwitch;
    public Renderer toggleSwitchRend;
    public KMSelectable[] keypad;
    public KMSelectable submitBut;
    public GameObject page1Items;
    public GameObject page2Items;
    public TextMesh deadlineText;
    private int daysRemaining = 299;

    //Returns submission
    public TextMesh amount;
    private string enteredText;
    private bool moduleSolved = false;

    //Page info
    private bool page1 = true;
    private bool page2 = false;

    //P60
    public TextMesh surname;
    public TextMesh forename;
    public String[] surnameOptions;
    public String[] forenameOptions;
    private int surnameIndex = 0;
    public TextMesh niNumber;
    public TextMesh payrollNumber;
    private int payrollDigit = 0;
    public string[] niCodes;
    public string[] niLetters;
    private int niCodeIndex = 0;

    //Months & quarters
    public string[] quarters;
    public TextMesh quartersText;
    private int quarterIndex = 0;
    public string[] months;
    public TextMesh[] monthsText;
    private int monthIndex = 0;
    private int monthIncreaser = 0;

    //Turnover
    public string[] turnoverOptions;
    public int[] turnoverIntOptions;
    public TextMesh janTurnoverText;
    public TextMesh febTurnoverText;
    public TextMesh marTurnoverText;
    private List<string> janTurnoverChoices = new List<string>();
    private List<string> febTurnoverChoices = new List<string>();
    private List<string> marTurnoverChoices = new List<string>();
    private List<int> janTurnoverIntChoices = new List<int>();
    private List<int> febTurnoverIntChoices = new List<int>();
    private List<int> marTurnoverIntChoices = new List<int>();
    private int janSelections = 0;
    private int febSelections = 0;
    private int marSelections = 0;
    private int turnoverIndex = 0;

    //Expenses
    public string[] expensesOptions;
    public int[] expensesIntOptions;
    public TextMesh[] janExpensesText;
    public TextMesh[] febExpensesText;
    public TextMesh[] marExpensesText;
    private List<string> janExpensesChoices = new List<string>();
    private List<string> febExpensesChoices = new List<string>();
    private List<string> marExpensesChoices = new List<string>();
    private List<int> janExpensesIntChoices = new List<int>();
    private List<int> febExpensesIntChoices = new List<int>();
    private List<int> marExpensesIntChoices = new List<int>();
    private int janExSelections = 0;
    private int febExSelections = 0;
    private int marExSelections = 0;
    private int expensesIndex = 0;
    private int expensesIncreaser = 0;

    //Logic
    private int grossTurnover = 0;
    private int totalExpenses = 0;
    private int pensionContribution = 0;
    private string pensionLog = "";
    private int uniquePorts = 0;
    private int portfolio = 0;
    private int taxFreeInvestments = 0;
    private int grossProfit = 0;
    private int personalAllowance = 0;
    private int allowanceDeduction = 0;
    private int taxableIncome = 0;
    private int baseRateTax = 0;
    private int higherRateTax = 0;
    private int additionalRateTax = 0;
    private int taxableIncomeLessBase = 0;
    private int taxableIncomeLessHigher = 0;
    private int totalIncomeTax = 0;
    private int taxableNI = 0;
    private int additionalTaxableNI = 0;
    private int nationalInsurance = 0;
    private int additionalNI = 0;
    private int totalTaxBill = 0;
    private string correctAnswer = "";

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        pageRight.OnInteract += delegate () { OnpageRight(); return false; };
        pageLeft.OnInteract += delegate () { OnpageLeft(); return false; };
        toggleSwitch.OnInteract += delegate () { Onswitch(); return false; };
        foreach (KMSelectable button in keypad)
        {
            KMSelectable trueButton = button;
            button.OnInteract += delegate () { keypadPress(trueButton); return false; };
        }
        submitBut.OnInteract += delegate () { OnsubmitBut(); return false; };
    }

    void Start()
    {
        page1Items.SetActive(true);
        page2Items.SetActive(false);
        deadlineText.text = daysRemaining.ToString();
        int forenameIndex = UnityEngine.Random.Range(0,42);
        forename.text = forenameOptions[forenameIndex];
        surnameIndex = UnityEngine.Random.Range(0,42);
        surname.text = surnameOptions[surnameIndex];
        quartersText.text = quarters[0];
        while (payrollNumber.text.Count() < 6)
        {
            payrollDigit = UnityEngine.Random.Range(0,10);
            payrollNumber.text += payrollDigit;
        }
        niCodeIndex = UnityEngine.Random.Range(0,26);
        niNumber.text += niCodes[niCodeIndex];
        while (niNumber.text.Count() < 9)
        {
            int niNumberNumber = UnityEngine.Random.Range(0,10);
            niNumber.text += niNumberNumber;
        }
        niCodeIndex = UnityEngine.Random.Range(0,4);
        niNumber.text += niLetters[niCodeIndex];
        foreach (TextMesh month in monthsText)
        {
            month.text = months[monthIndex + monthIncreaser];
            monthIncreaser++;
        }
        monthIncreaser = 0;
        StartCoroutine(deadlineCoroutine());
        GetTurnover();
        GetExpenses();
        GetGrossTurnover();
        GetPensionContributions();
        GetTaxFreeInvestments();
        GetTotalExpenses();
        GetGrossProfit();
        GetPersonalAllowance();
        GetTaxRates();
        GetNationalInsurance();
        totalTaxBill = totalIncomeTax + nationalInsurance;
        Debug.LogFormat("[Tax Returns #{0}] YOUR TOTAL TAX BILL IS £{1}.", moduleId, totalTaxBill);
        correctAnswer = totalTaxBill.ToString();
    }

    private IEnumerator deadlineCoroutine()
    {
        while (daysRemaining > 0)
        {
            yield return new WaitForSeconds (3f);
            daysRemaining -= 1;
            deadlineText.text = daysRemaining.ToString();
            if (daysRemaining == 0 && moduleSolved == false)
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Tax Returns #{0}] Strike! Your tax returns are now late.", moduleId);
            }
        }
    }

    void GetTurnover()
    {
        while (janSelections != 4)
        {
            int index = UnityEngine.Random.Range(0,30);
            janTurnoverChoices.Add(turnoverOptions[index]);
            janTurnoverIntChoices.Add(turnoverIntOptions[index]);
            janSelections++;
        }
        while (febSelections != 4)
        {
            int index = UnityEngine.Random.Range(0,30);
            febTurnoverChoices.Add(turnoverOptions[index]);
            febTurnoverIntChoices.Add(turnoverIntOptions[index]);
            febSelections++;
        }
        while (marSelections != 4)
        {
            int index = UnityEngine.Random.Range(0,30);
            marTurnoverChoices.Add(turnoverOptions[index]);
            marTurnoverIntChoices.Add(turnoverIntOptions[index]);
            marSelections++;
        }
        janTurnoverText.text = janTurnoverChoices[0];
        febTurnoverText.text = febTurnoverChoices[0];
        marTurnoverText.text = marTurnoverChoices[0];
    }

    void GetExpenses()
    {
        while (janExSelections != 12)
        {
            int index = UnityEngine.Random.Range(0,22);
            janExpensesChoices.Add(expensesOptions[index]);
            janExpensesIntChoices.Add(expensesIntOptions[index]);
            janExSelections++;
        }
        while (febExSelections != 12)
        {
            int index = UnityEngine.Random.Range(0,22);
            febExpensesChoices.Add(expensesOptions[index]);
            febExpensesIntChoices.Add(expensesIntOptions[index]);
            febExSelections++;
        }
        while (marExSelections != 12)
        {
            int index = UnityEngine.Random.Range(0,22);
            marExpensesChoices.Add(expensesOptions[index]);
            marExpensesIntChoices.Add(expensesIntOptions[index]);
            marExSelections++;
        }
        foreach (TextMesh expense in janExpensesText)
        {
            expense.text = janExpensesChoices[expensesIndex];
            expensesIndex++;
        }
        expensesIndex = 0;
        foreach (TextMesh expense in febExpensesText)
        {
            expense.text = febExpensesChoices[expensesIndex];
            expensesIndex++;
        }
        expensesIndex = 0;
        foreach (TextMesh expense in marExpensesText)
        {
            expense.text = marExpensesChoices[expensesIndex];
            expensesIndex++;
        }
        expensesIndex = 0;
    }

    public void GetGrossTurnover()
    {
        foreach (int figure in janTurnoverIntChoices)
        {
            grossTurnover += figure;
        }
        foreach (int figure in febTurnoverIntChoices)
        {
            grossTurnover += figure;
        }
        foreach (int figure in marTurnoverIntChoices)
        {
            grossTurnover += figure;
        }
        Debug.LogFormat("[Tax Returns #{0}] Your gross turnover is £{1}.", moduleId, grossTurnover);
    }

    public void GetPensionContributions()
    {
        int offIndicators = Bomb.GetOffIndicators().Count();
        int onIndicators = Bomb.GetOnIndicators().Count();
        if (offIndicators + onIndicators == 0)
        {
            pensionContribution = 0;
            pensionLog = "0%";
        }
        else if (onIndicators > offIndicators)
        {
            pensionContribution = (grossTurnover * 5) / 100;
            pensionLog = "5%";
        }
        else if (offIndicators > onIndicators)
        {
            pensionContribution = (grossTurnover * 10) / 100;
            pensionLog = "10%";
        }
        else
        {
            pensionContribution = (grossTurnover * 15) / 100;
            pensionLog = "15%";
        }
        Debug.LogFormat("[Tax Returns #{0}] Your have contributed £{1} to a {2} pension.", moduleId, pensionContribution, pensionLog);
    }

    public void GetTaxFreeInvestments()
    {
        uniquePorts = Bomb.CountUniquePorts();
        if (niNumber.text[10] == 'A' || niNumber.text[10] == 'C' && surnameIndex < 26 && payrollDigit % 2 == 1)
        {
            portfolio = 932;
        }
        else if (niNumber.text[10] == 'A' || niNumber.text[10] == 'C' && surnameIndex < 26)
        {
            portfolio = 478;
        }
        else if (niNumber.text[10] == 'A' || niNumber.text[10] == 'C' && payrollDigit % 2 == 1)
        {
            portfolio = 736;
        }
        else if (surnameIndex < 26 && payrollDigit % 2 == 1)
        {
            portfolio = 81;
        }
        else if (surnameIndex < 26)
        {
            portfolio = 599;
        }
        else if (payrollDigit % 2 == 1)
        {
            portfolio = 1647;
        }
        else if (niNumber.text[10] == 'A' || niNumber.text[10] == 'C')
        {
            portfolio = 1241;
        }
        else
        {
            portfolio = 0;
        }
        taxFreeInvestments = (portfolio * uniquePorts);
        Debug.LogFormat("[Tax Returns #{0}] You have made {1} tax-free investments of £{2}, totalling £{3}.", moduleId, uniquePorts, portfolio, taxFreeInvestments);
    }

    public void GetTotalExpenses()
    {
        foreach (int figure in janExpensesIntChoices)
        {
            totalExpenses += figure;
        }
        foreach (int figure in febExpensesIntChoices)
        {
            totalExpenses += figure;
        }
        foreach (int figure in marExpensesIntChoices)
        {
            totalExpenses += figure;
        }
        Debug.LogFormat("[Tax Returns #{0}] Your total expenses are £{1}.", moduleId, totalExpenses);
    }

    public void GetGrossProfit()
    {
        grossProfit = grossTurnover - pensionContribution - taxFreeInvestments - totalExpenses;
        Debug.LogFormat("[Tax Returns #{0}] Your gross profit is £{1}.", moduleId, grossProfit);
    }

    public void GetPersonalAllowance()
    {
        personalAllowance = 11850;
        if (grossProfit > 100000)
        {
            allowanceDeduction = (grossProfit - 100000) / 2;
        }
        personalAllowance = 11850 - allowanceDeduction;
        if (personalAllowance < 1)
        {
            personalAllowance = 0;
        }
        Debug.LogFormat("[Tax Returns #{0}] Your personal tax-free allowance is £{1}.", moduleId, personalAllowance);
        taxableIncome = grossProfit - personalAllowance;
        Debug.LogFormat("[Tax Returns #{0}] Your taxable income is £{1}.", moduleId, taxableIncome);
    }

    public void GetTaxRates()
    {
        if (taxableIncome < 34501)
        {
            baseRateTax = (taxableIncome * 20) / 100;
        }
        else
        {
            baseRateTax = 6900;
            taxableIncomeLessBase = taxableIncome - 34500;
        }
        if (taxableIncomeLessBase < 103651)
        {
            higherRateTax = (taxableIncomeLessBase * 40) /100;
        }
        else
        {
            higherRateTax = 41460;
            taxableIncomeLessHigher = taxableIncomeLessBase - 103650;
        }
        additionalRateTax = (taxableIncomeLessHigher * 45) / 100;
        totalIncomeTax = baseRateTax + higherRateTax + additionalRateTax;
        Debug.LogFormat("[Tax Returns #{0}] Your must pay £{1} in Basic Rate tax at 20%.", moduleId, baseRateTax);
        Debug.LogFormat("[Tax Returns #{0}] Your must pay £{1} in Higher Rate tax at 40%.", moduleId, higherRateTax);
        Debug.LogFormat("[Tax Returns #{0}] Your must pay £{1} in Additional Rate tax at 45%.", moduleId, additionalRateTax);
        Debug.LogFormat("[Tax Returns #{0}] Your total income tax bill is £{1}.", moduleId, totalIncomeTax);
    }

    public void GetNationalInsurance()
    {
        taxableNI = grossTurnover - totalExpenses - 8423;
        if (taxableNI < 37927)
        {
            nationalInsurance = (taxableNI * 9) / 100;
        }
        else
        {
            nationalInsurance = 3413;
            additionalTaxableNI = taxableNI - 37926;
        }
        Debug.LogFormat("[Tax Returns #{0}] Your must pay £{1} in National Insurance at 9%.", moduleId, nationalInsurance);
        additionalNI = (additionalTaxableNI * 2) / 100;
        Debug.LogFormat("[Tax Returns #{0}] Your must pay £{1} in National Insurance at 2%.", moduleId, additionalNI);
        nationalInsurance += additionalNI;
        Debug.LogFormat("[Tax Returns #{0}] Your total National Insurance bill is £{1}.", moduleId, nationalInsurance);
    }

    public void OnpageRight()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pageRight.transform);
        pageRight.AddInteractionPunch(.5f);
        quarterIndex++;
        quarterIndex = quarterIndex % 4;
        quartersText.text = quarters[quarterIndex];
        monthIndex += 3;
        monthIndex = monthIndex % 12;
        foreach (TextMesh month in monthsText)
        {
            month.text = months[monthIndex + monthIncreaser];
            monthIncreaser++;
        }
        monthIncreaser = 0;
        turnoverIndex++;
        turnoverIndex = turnoverIndex % 4;
        janTurnoverText.text = janTurnoverChoices[turnoverIndex];
        febTurnoverText.text = febTurnoverChoices[turnoverIndex];
        marTurnoverText.text = marTurnoverChoices[turnoverIndex];
        expensesIndex += 3;
        expensesIndex = expensesIndex % 12;
        foreach (TextMesh expense in janExpensesText)
        {
            expense.text = janExpensesChoices[expensesIndex + expensesIncreaser];
            expensesIncreaser++;
        }
        expensesIncreaser = 0;
        foreach (TextMesh expense in febExpensesText)
        {
            expense.text = febExpensesChoices[expensesIndex + expensesIncreaser];
            expensesIncreaser++;
        }
        expensesIncreaser = 0;
        foreach (TextMesh expense in marExpensesText)
        {
            expense.text = marExpensesChoices[expensesIndex + expensesIncreaser];
            expensesIncreaser++;
        }
        expensesIncreaser = 0;
    }

    public void OnpageLeft()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pageLeft.transform);
        pageLeft.AddInteractionPunch(.5f);
        quarterIndex += 3;
        quarterIndex = quarterIndex % 4;
        quartersText.text = quarters[quarterIndex];
        monthIndex += 9;
        monthIndex = monthIndex % 12;
        foreach (TextMesh month in monthsText)
        {
            month.text = months[monthIndex + monthIncreaser];
            monthIncreaser++;
        }
        monthIncreaser = 0;
        turnoverIndex += 3;
        turnoverIndex = turnoverIndex % 4;
        janTurnoverText.text = janTurnoverChoices[turnoverIndex];
        febTurnoverText.text = febTurnoverChoices[turnoverIndex];
        marTurnoverText.text = marTurnoverChoices[turnoverIndex];
        expensesIndex += 9;
        expensesIndex = expensesIndex % 12;
        foreach (TextMesh expense in janExpensesText)
        {
            expense.text = janExpensesChoices[expensesIndex + expensesIncreaser];
            expensesIncreaser++;
        }
        expensesIncreaser = 0;
        foreach (TextMesh expense in febExpensesText)
        {
            expense.text = febExpensesChoices[expensesIndex + expensesIncreaser];
            expensesIncreaser++;
        }
        expensesIncreaser = 0;
        foreach (TextMesh expense in marExpensesText)
        {
            expense.text = marExpensesChoices[expensesIndex + expensesIncreaser];
            expensesIncreaser++;
        }
        expensesIncreaser = 0;
    }

    public void Onswitch()
    {
        Audio.PlaySoundAtTransform("click", transform);
        toggleSwitch.AddInteractionPunch(.5f);
        if (page1)
        {
            toggleSwitchRend.transform.localRotation = Quaternion.Euler(-20.0f, 0.0f, 0.0f) * toggleSwitchRend.transform.localRotation;;
            page1 = false;
            page1Items.SetActive(false);
            page2 = true;
            page2Items.SetActive(true);
        }
        else if (page2)
        {
            toggleSwitchRend.transform.localRotation = Quaternion.Euler(20.0f, 0.0f, 0.0f) * toggleSwitchRend.transform.localRotation;;
            page1 = true;
            page1Items.SetActive(true);
            page2 = false;
            page2Items.SetActive(false);
        }
    }
    public void keypadPress(KMSelectable button)
    {
        Audio.PlaySoundAtTransform("keystroke", transform);
        button.AddInteractionPunch(.5f);
        if (moduleSolved)
        {
            return;
        }
        else
        {
            if (amount.text.Length < 7)
            {
                if (button == keypad[0] && amount.text.Length > 0)
                {
                    enteredText += "0";
                }
                else if (button == keypad[1])
                {
                    enteredText += "1";
                }
                else if (button == keypad[2])
                {
                    enteredText += "2";
                }
                else if (button == keypad[3])
                {
                    enteredText += "3";
                }
                else if (button == keypad[4])
                {
                    enteredText += "4";
                }
                else if (button == keypad[5])
                {
                    enteredText += "5";
                }
                else if (button == keypad[6])
                {
                    enteredText += "6";
                }
                else if (button == keypad[7])
                {
                    enteredText += "7";
                }
                else if (button == keypad[8])
                {
                    enteredText += "8";
                }
                else if (button == keypad[9])
                {
                    enteredText += "9";
                }
            }
            if (button == keypad[10] && amount.text.Length > 0)
            {
                enteredText = enteredText.Substring(0, enteredText.Length - 1);
            }
            amount.text = enteredText;
        }
    }

    public void OnsubmitBut()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submitBut.transform);
        submitBut.AddInteractionPunch();
        if (moduleSolved)
        {
            return;
        }
        else
        {
            if (enteredText == correctAnswer)
            {
                Audio.PlaySoundAtTransform("correct", transform);
                GetComponent<KMBombModule>().HandlePass();
                Debug.LogFormat("[Tax Returns #{0}] You have paid £{1}. Thank you for using HMRC self assessment. Module disarmed.", moduleId, correctAnswer);
                moduleSolved = true;
            }
            else if (enteredText != correctAnswer)
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Tax Returns #{0}] Strike! You have tried to pay £{1}. I was expecting £{2}. Please check your figures and re-submit your return.", moduleId, enteredText, correctAnswer);
                enteredText = "";
                amount.text = enteredText;
            }
        }
    }

	#pragma warning disable 414
	private string TwitchHelpMessage = "Submit your taxes using !{0} submit <number>.";
	#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string inputCommand)
	{
		inputCommand = System.Text.RegularExpressions.Regex.Replace(inputCommand.ToLowerInvariant(), "^(submit|enter|give|return|pay) £?", "");

		int number; // Used to check that someone is submitting both a number and that it's not negitive.
		if (inputCommand.Length > 0 && inputCommand.Length <= 7 && int.TryParse(inputCommand, out number) && number > 0)
		{
			yield return null;

			toggleSwitch.OnInteract();
			yield return new WaitForSeconds(0.2f);

			foreach (KMSelectable button in inputCommand.TrimStart('0').ToCharArray().Select(digit => keypad[digit - '0']))
			{
				button.OnInteract();
				yield return new WaitForSeconds(0.2f);
			}

			submitBut.OnInteract();

			// TP stops execution on a strike but since we don't yield between submitting, we can turn the module back over.
			if (enteredText != correctAnswer) toggleSwitch.OnInteract();

			yield return new WaitForSeconds(0.2f);
		}
	}
}
