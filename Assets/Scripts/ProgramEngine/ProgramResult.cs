using UnityEngine;

public class ProgramResult : CustomYieldInstruction
{
    public bool isDone = false;

    public override bool keepWaiting
    {
        get
        {
            return !isDone;
        }
    }

    public bool IsSucceeded { get; set; }
    public string ErrorMessage { get; set; }

    public ProgramResult()
    {
        this.isDone = false;
    }
    public ProgramResult(bool isSucceeded, string errorMessage)
    {
        this.isDone = false;
        this.IsSucceeded = false;
        this.ErrorMessage = errorMessage;
    }

    public ProgramResult(bool isDone, bool isSucceeded, string errorMessage)
    {
        this.isDone = isDone;
        this.IsSucceeded = false;
        this.ErrorMessage = errorMessage;
    }
}
