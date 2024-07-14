public class StatusFlags
{
    public bool IsReadyToPlayCard { get; set; }
    public bool IsReadyToBid { get; set; }
    public bool IsReadyToDiscard { get; set; }
    public bool IsReadyToDeclareGame { get; set; }
    public bool IsReadyToContinue { get; set; }
    public bool IsReadyToDecideTakeSkat { get; set; }
    
    public void Reset()
    {
        IsReadyToPlayCard = false;
        IsReadyToBid = false;
        IsReadyToDiscard = false;
        IsReadyToDeclareGame = false;
        IsReadyToContinue = false;
        IsReadyToDecideTakeSkat = false;
    }
}