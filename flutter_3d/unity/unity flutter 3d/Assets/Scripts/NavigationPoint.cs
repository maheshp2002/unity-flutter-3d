using UnityEngine;

public class NavigationPoint : MonoBehaviour
{
    public string label;
    public bool isSource;
    public bool isDestination;

    public void SetData(string label, bool isSource, bool isDestination)
    {
        this.label = label;
        this.isSource = isSource;
        this.isDestination = isDestination;
    }
}
