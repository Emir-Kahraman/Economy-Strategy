using UnityEngine;

public interface IUIWindow
{
    public void Initialize();
    public void Uninitialize();
    public void OpenWindow();
    public void CloseWindow();
}
