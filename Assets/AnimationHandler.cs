using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    public PlayerController playerController;

    public void JumpingComplete()
    {
        playerController.playerState = PlayerState.Falling;
    }
}
