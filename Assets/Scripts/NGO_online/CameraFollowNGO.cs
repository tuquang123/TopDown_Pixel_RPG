using Pattern;
 using Unity.Netcode;
 
 public class CameraFollowNGO : NetworkBehaviour
 {
     public override void OnNetworkSpawn()
     {
         if (!IsOwner) return;
 
         CameraFollow.Instance.target = transform;
     }
 }