using Fusion;
using UnityEngine;

public class BombBehavior : NetworkBehaviour
{
    public float explosionRadius = 5f;
    public float damage = 50f;
    public float fuseTime = 3f;

    public void Initialize()
    {
        LeanTween.delayedCall(fuseTime, () =>
        {
            RPC_Explode();
        });
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Explode()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hitPlayers)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                //player.TakeDamage(damage);
            }
        }

        Runner.Despawn(Object);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
