using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Collections;

namespace WarGame
{
    public class Grenade : MonoBehaviour
    {
        #region Variables

        public float Timer = 3f;
        public RaycastBlowSettings BlowSettings;
        [SerializeField] GameObject Blow;
        [SerializeField] Rigidbody grenadeBody;
        [SerializeField] float throwForce = 100f;
        [SerializeField] Rigidbody grenadePart;
        [SerializeField] Transform grenadePartTorqueDirection;
        [SerializeField] float grenadePartTorque;
        [SerializeField] Transform grenadePartForceDirection;
        [SerializeField] float grenadePartForce;

        [SerializeField] float blowRadius;
        [SerializeField] AnimationCurve blowDamageCurve;
        [SerializeField] float blowDamage;

        [SerializeField] LayerMask TargetsMask;
        [SerializeField] LayerMask CollisionMask;
        [SerializeField] AudioSource landingSource;

        SphereRaycastCommand grenadeSphereRaycastCommand;
        bool isWaitCasts = false;

        #endregion

        [System.Serializable]
        public class RaycastBlowSettings
        {
            public float Radius;
            public int CircleResolution = 25;
            public int SphereResolution = 25;
            public int RaysNum
            {
                get
                {
                    return CircleResolution * SphereResolution;
                }
            }
            public Color GizmosColor;
            public LayerMask Layer;
            public int MaxHits = 2;
            public float DamageDecreasingFactor = 0.7f;
        }
                
        private void Awake()
        {
            if (grenadeSphereRaycastCommand == null)
            {
                grenadeSphereRaycastCommand = new SphereRaycastCommand();
                grenadeSphereRaycastCommand.Radius = BlowSettings.Radius;
                grenadeSphereRaycastCommand.CircleResolution = BlowSettings.CircleResolution;
                grenadeSphereRaycastCommand.SphereResolution = BlowSettings.SphereResolution;
                grenadeSphereRaycastCommand.Center = transform.position;
                grenadeSphereRaycastCommand.Layer = BlowSettings.Layer;
                grenadeSphereRaycastCommand.MaxHits = BlowSettings.MaxHits;
                grenadeSphereRaycastCommand.Prepare();
            }
        }

        public void Throw(Vector3 direction)
        {
            transform.SetParent(null);
            grenadeBody.isKinematic = false;
            grenadeBody.AddForce(direction * throwForce);

            Physics.IgnoreCollision(GetComponent<Collider>(), grenadePart.GetComponent<Collider>());

            grenadePart.transform.SetParent(null);
            grenadePart.isKinematic = false;
            grenadePart.AddTorque(grenadePartTorqueDirection.forward * grenadePartTorque);
            grenadePart.AddForce(grenadePartForceDirection.forward * grenadePartForce);
            grenadePart.maxAngularVelocity = 25;
            Destroy(grenadePart.gameObject, 5f);
            StartCoroutine(Blowing());
        }

        private void Update()
        {
            if(isWaitCasts)
                grenadeSphereRaycastCommand.IsWaitJobComplete();
        }

        IEnumerator Blowing()
        {
            yield return new WaitForSeconds(Timer);
            grenadeSphereRaycastCommand.Center = transform.position;
            grenadeSphereRaycastCommand.Cast(Raycasted);
            isWaitCasts = true;

            while (isWaitCasts)
                yield return null;

            gameObject.SetActive(false);
            Blow.transform.SetParent(null);
            Blow.SetActive(true);
            Blow.GetComponent<TransformStateSetupper>().LoadSavedState();
            Destroy(gameObject, 5f);
            Destroy(Blow, 5f);
        }

        private void OnDestroy()
        {
            grenadeSphereRaycastCommand.Dispose();
        }

        void Raycasted(NativeArray<RaycastHit> raycastHits)
        {
            RaycastHit[] hits;
            hits = raycastHits.ToArray();
            float damagePerHit = blowDamage / raycastHits.Length;
            List<IHittableObject> damagedTargets = new List<IHittableObject>();
            int raysNum = BlowSettings.RaysNum;
            for (int i = 0; i < raysNum; i++)
            {
                for (int j = 0; j < BlowSettings.MaxHits; j++)
                {
                    RaycastHit hit = hits[(i * BlowSettings.MaxHits) + j];
                    if (hit.collider == null)
                    {
                        break;
                    }
                    else
                    {
                        IHittableObject hittable = hit.collider.GetComponent<IHittableObject>();

                        if (hittable == null)
                            continue;

                        float damageFactor = 1f;
                        for (int k = 1; k < j; k++)
                        {
                            damageFactor *= BlowSettings.DamageDecreasingFactor;
                        }

                        float distance = hit.distance;
                        float value = blowDamageCurve.Evaluate(distance / blowRadius);
                        DamageData damage = new DamageData();
                        damage.DamageAmount = blowDamage * value * damageFactor;
                        damage.HitDirection = grenadeSphereRaycastCommand.GetRayDirectionByIndex(i);
                        damage.HitPosition = hit.point;
                        damage.HitType = DamageData.DamageType.Explosion;
                        hittable.TakeDamage(damage);
                        damagedTargets.Add(hittable);
                    }
                }
            }

            grenadeSphereRaycastCommand.Dispose();
            isWaitCasts = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            AudioSource landingClone = Instantiate(landingSource, landingSource.transform.parent);

            landingClone.Play();

            Destroy(landingClone.gameObject, 10f);
        }

        void CalculateBlow()
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, blowRadius, TargetsMask);

            List<IHittableObject> damagedTargets = new List<IHittableObject>();
            foreach(Collider c in targets)
            {
                IHittableObject hittable = c.GetComponent<IHittableObject>();

                if(hittable != null && !damagedTargets.Contains(hittable))
                {
                    Vector3 targetPoint = c.bounds.center;
                    Vector3 direction = (targetPoint - transform.position);
                    Ray r = new Ray(transform.position, direction.normalized);
                    RaycastHit hitInfo;
                    if(c.bounds.Contains(transform.position) || !Physics.Raycast(r, out hitInfo, direction.magnitude, CollisionMask))
                    {
                        DamageData damage = new DamageData();
                        float distance = direction.magnitude;
                        float value = blowDamageCurve.Evaluate(distance / blowRadius);
                        damage.DamageAmount = blowDamage * value;
                        damage.HitDirection = r.direction;
                        damage.HitPosition = targetPoint;
                        damage.HitType = DamageData.DamageType.Explosion;

                        hittable.TakeDamage(damage);
                    }
                    else
                    {
                        if (hitInfo.collider == c)
                        {
                            DamageData damage = new DamageData();
                            float distance = hitInfo.distance;
                            float value = blowDamageCurve.Evaluate(distance / blowRadius);
                            damage.DamageAmount = blowDamage * value;
                            damage.HitDirection = r.direction;
                            damage.HitPosition = targetPoint;
                            damage.HitType = DamageData.DamageType.Explosion;

                            hittable.TakeDamage(damage);
                        }
                    }
                    damagedTargets.Add(hittable);
                }
            }
        }
      
        private void OnDrawGizmos()
        {
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(transform.position, blowRadius);

            //if (grenadePart != null)
            //{
            //    Gizmos.DrawWireSphere(grenadePart.worldCenterOfMass, 0.02f);
            //}

            //if (grenadePartTorqueDirection != null)
            //{
            //    Gizmos.color = Color.red;
            //    Gizmos.DrawLine(grenadePartTorqueDirection.position, grenadePartTorqueDirection.position + grenadePartTorqueDirection.forward);
            //}

            //if (grenadePartForceDirection != null)
            //{
            //    Gizmos.color = Color.green;
            //    Gizmos.DrawLine(grenadePartForceDirection.position, grenadePartForceDirection.position + grenadePartForceDirection.forward);
            //}
            //////Gizmos.color = BlowSettings.GizmosColor;
            //////grenadeSphereRaycastCommand.DrawGizmosLines();
        }
    }
}