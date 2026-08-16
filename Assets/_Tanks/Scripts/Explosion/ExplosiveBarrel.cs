using UnityEngine;

namespace Tanks.Complete
{
    public class ExplosiveBarrel : MonoBehaviour
    {
        [Header("Explosion")]
        [SerializeField] private float m_ExplosionRadius = 5f;
        [SerializeField] private float m_MaxDamage = 100f;
        [SerializeField] private float m_ExplosionForce = 50f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem m_ExplosionParticles;
        [SerializeField] private AudioClip m_ExplosionClip;

        [Header("Layer")]
        [SerializeField] private LayerMask m_TankMask;


        private bool m_Exploded = false;

        public void Explode()
        {
            if (m_Exploded)
                return;

            m_Exploded = true;

            // 爆発範囲内のTankを取得
            Collider[] colliders = Physics.OverlapSphere(
                transform.position,
                m_ExplosionRadius,
                m_TankMask
            );

            foreach (Collider collider in colliders)
            {
                Rigidbody targetRigidbody = collider.GetComponent<Rigidbody>();

                if (!targetRigidbody)
                    continue;

                // 爆発の力
                TankMovement tankMovement =
                    targetRigidbody.GetComponent<TankMovement>();

                if (tankMovement != null)
                {
                    tankMovement.AddExplosionForce(
                        m_ExplosionForce,
                        transform.position,
                        m_ExplosionRadius
                    );
                }

                // ダメージ
                TankHealth targetHealth =
                    targetRigidbody.GetComponent<TankHealth>();

                if (targetHealth == null)
                    continue;

                float distance =
                    Vector3.Distance(targetRigidbody.position, transform.position);

                float relativeDistance =
                    (m_ExplosionRadius - distance) / m_ExplosionRadius;

                float damage =
                    Mathf.Max(0f, relativeDistance * m_MaxDamage);

                targetHealth.TakeDamage(damage);
            }

            // 爆発エフェクト
            if (m_ExplosionParticles != null)
            {
                ParticleSystem explosion = Instantiate(
                    m_ExplosionParticles,
                    transform.position,
                    Quaternion.identity
                );

                explosion.gameObject.SetActive(true);
                explosion.Play();

                Destroy(explosion.gameObject, 3f);
            }

            // 爆発音
            if (m_ExplosionClip != null)
            {
                GameObject audioObject = new GameObject("ExplosionAudio");

                audioObject.transform.position = transform.position;

                AudioSource audioSource = audioObject.AddComponent<AudioSource>();

                audioSource.clip = m_ExplosionClip;
                audioSource.volume = 1f;
                audioSource.spatialBlend = 0f;

                audioSource.Play();

                Destroy(audioObject, m_ExplosionClip.length);
            }

            // ドラム缶を削除
            Destroy(gameObject);
        }
        private void OnCollisionEnter(Collision collision)
        {
            // 戦車がぶつかった場合
            if (collision.gameObject.layer == LayerMask.NameToLayer("Players"))
            {
                Explode();
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            // 弾がぶつかった場合 
            if (other.GetComponent<ShellExplosion>() != null)
            {
                Explode();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                transform.position,
                m_ExplosionRadius
            );
        }
    }
}