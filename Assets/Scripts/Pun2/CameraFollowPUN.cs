using Pattern;
using Photon.Pun;

public class CameraFollowPUN : MonoBehaviourPun 
{
    void Start()
    {
        // Chỉ player local mới set camera follow
        if (!photonView.IsMine) return;

        CameraFollow.Instance.target = transform;
    }
}