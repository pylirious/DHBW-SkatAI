using System;
using Code.Scripts;
using UnityEngine;

public class HumanUiManager : MonoBehaviour
{
    public static HumanUiManager instance;
    
    public GameObject readyToContinuePanel;
    public GameObject bidPanel;
    public TMPro.TMP_InputField bidInputField;
    public GameObject takeSkatPanel;
    public GameObject discardCardsPanel;
    public GameObject callGameTypePanel;
    public TMPro.TMP_Text winnerText;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(this);
        }
    
    }


    public void ReadyToContinue()
    {
        readyToContinuePanel.SetActive(false);
        GameManager.instance.currentMainPlayer.Flags.IsReadyToContinue = true;
    }

    public void DisplayReadyToContinue()
    {
        readyToContinuePanel.SetActive(true);
    }
    
    public void DisplayBidPanel()
    {
        bidInputField.text = "";
        bidPanel.SetActive(true);
    }

    public void Bid()
    {
        int bid = 0;
        if (bidInputField.text != "")
        {
            bid = int.Parse(bidInputField.text);
        }
        
        if(GameManager.instance.currentMainPlayer.CurrentBid >= bid)
        {
            bid = GameManager.instance.currentMainPlayer.CurrentBid;
        }
        
        // Setting Flag
        GameManager.instance.currentMainPlayer.CurrentBid = bid;
        bidPanel.SetActive(false);
    }
    
    public void DisplayTakeSkatPanel()
    {
        takeSkatPanel.SetActive(true);
    }
    
    public void DisplayDiscardCardsPanel()
    {
        discardCardsPanel.SetActive(true);
    }
    
    public void TakeSkat(bool takeSkat)
    {
        GameManager.instance.currentMainPlayer.TakesSkat = takeSkat;
        takeSkatPanel.SetActive(false);
    }
    
    public void DisplayCallGameTypePanel()
    {
        callGameTypePanel.SetActive(true);
    }

    public void CallGameType(int gameType)
    {
        GameManager.instance.currentMainPlayer.GameType = (GameType)gameType;
        callGameTypePanel.SetActive(false);
    }
    
    public void EnableWinnerText(int winner = 0)
    {
        winnerText.text = "Player " + winner + " won the Game!";
        winnerText.transform.parent.gameObject.SetActive(true);
    }
    
    public void DisableWinnerText()
    {
        winnerText.transform.parent.gameObject.SetActive(false);
    }
    
    
}