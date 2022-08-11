using UnityEngine;

public class Key : MonoBehaviour
{
    public string keyCode = "";
    [ContextMenu("Destroy")]

    public void Collect()
    {
        Destroy(gameObject);
    }
}
