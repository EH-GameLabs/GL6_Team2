using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
    public string BoolName;
    public bool ExpectedValue;

    public ShowIfAttribute(string boolName, bool expectedValue = true)
    {
        this.BoolName = boolName;
        this.ExpectedValue = expectedValue;
    }
}
