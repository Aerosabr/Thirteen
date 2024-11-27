using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanup : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);

        if (ThirteenMultiplayer.Instance != null)
            Destroy(ThirteenMultiplayer.Instance.gameObject);
    }
}
