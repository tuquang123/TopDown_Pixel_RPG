using UnityEngine;

public class ArcherController : PlayerController
{
    protected static readonly int ShotTrigger = Animator.StringToHash("7_Shoot");
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private float arrowArcHeight = 5f; 

    protected override void AttackEnemy()
    {
        if (targetEnemy == null) return;

        lastAttackTime = Time.time;
        anim.SetTrigger(ShotTrigger);
        //Invoke("PlayAnimation",.5f);

        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
        arrow.GetComponent<Arrow>()?.SetTarget(targetEnemy, attackDamage);
    }

    public void PlayAnimation()
    {
        anim.SetTrigger(ShotTrigger);
    }
    

}