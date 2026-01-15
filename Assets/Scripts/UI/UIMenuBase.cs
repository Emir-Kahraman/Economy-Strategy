using UnityEngine;

public class UIMenuBase : MonoBehaviour
{
    [SerializeField] private MenuType type;

    public MenuType Type => type;
}
