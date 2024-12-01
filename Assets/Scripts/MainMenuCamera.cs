using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    [SerializeField] private Animator cameraAnim;

    public void PlayCameraTransition(string anim)
    {
        cameraAnim.Play(anim);
    }

    public void ChangeUI() => MenuManager.Instance.ChangeUI();
}
