using UnityEngine;

public class OrderData
{
    public string id;
    public ResourceData resourceData;
    public float existenceTime;
    public float completionTime;
    public int resourceAmount;
    public int reward;

    public OrderData(string id, float existenceTime, float completionTime, ResourceData resourceData, int resourceAmount, int reward)
    {
        this.id = id;
        this.existenceTime = existenceTime;
        this.completionTime = completionTime;
        this.resourceData = resourceData;
        this.resourceAmount = resourceAmount;
        this.reward = reward;
    }
}
