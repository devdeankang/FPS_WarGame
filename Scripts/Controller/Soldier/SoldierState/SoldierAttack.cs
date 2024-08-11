using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WarGame
{
    public class SoldierAttack : IState<SoldierController>
    {
        GameObject nearestTarget = null;
        float nextFireTime = 0f;

        public void OperateEnter(SoldierController controller)
        {            
            controller.agent.isStopped = true;
            controller.animator.SetBool("Idling", true);
        }

        public void OperateUpdate(SoldierController controller)
        {
            controller.agent.destination = controller.transform.position;

            DetectTarget(controller);
            ChangeState(controller);

            nextFireTime += Time.deltaTime;
            if (nearestTarget != null)
            {
                controller.transform.LookAt(nearestTarget.transform);
                if (nextFireTime >= controller.data.autoFireRate)
                {                    
                    controller.StartCoroutine(MuzzleEffect(controller));
                    controller.data.GunSound.GetComponent<AudioSource>().Play();
                    Fire(controller, nearestTarget);
                }                
            }
        }

        public void OperateFixedUpdate(SoldierController controller)
        {            

        }

        public void OperateExit(SoldierController controller)
        {
            controller.agent.isStopped = false;
            controller.animator.SetBool("Idling", false);
        }        

        void ChangeState(SoldierController controller)
        {
            if (!controller.health.IsAlive) 
                controller.ChangeState(SoldierController.SoldierState.Dead);

            else if (controller.IsOccupied) 
                controller.ChangeState(SoldierController.SoldierState.Occupied);

            else if (!controller.IsAttackableTarget(nearestTarget)) 
                controller.ChangeState(SoldierController.SoldierState.Move);

            else if (controller.rader.GetSqrMagnitude(nearestTarget) > controller.ShootDistance)
                controller.ChangeState(SoldierController.SoldierState.Track);
        }

        void DetectTarget(SoldierController controller)
        {
            if (controller.NeedRaderSystem)
                nearestTarget = controller.rader.GetNearestTarget(controller.rader.detectedTargets);
            else
                nearestTarget = null;
        }

        void Fire(SoldierController controller, GameObject nearestTarget = null)
        {
            ShootAnim(controller);
            EmitHitEffect(controller, nearestTarget);
            nextFireTime = 0f;
        }

        void ShootAnim(SoldierController controller)
        {            
            controller.animator.SetTrigger("Use");
        }


        void EmitHitEffect(SoldierController controller, GameObject nearestTarget)
        {
            GameObject hands = controller.data.handsObject;
            Vector3 rayDirection = controller.transform.forward;
            if (nearestTarget != null)
            {
                rayDirection = (nearestTarget.transform.position - hands.transform.position).normalized;

                for(int i =0; i<nearestTarget.transform.childCount; i++)
                {
                    if(nearestTarget.transform.GetChild(i).CompareTag(controller.data.targetPoint))
                    {
                        rayDirection = (nearestTarget.transform.GetChild(i).transform.position - hands.transform.position).normalized;
                        break;
                    }
                }
            }
            
            Ray r = new Ray(hands.transform.position, rayDirection);
            RaycastHit hitInfo;

            if (Physics.Raycast(r, out hitInfo, controller.ShootDistance, controller.FireCollisionLayer, QueryTriggerInteraction.Ignore))
            {
                IHittableObject hittable = hitInfo.collider.GetComponent<IHittableObject>();

                if (hittable == null)
                {
                    hitInfo.collider.GetComponentInParent<IHittableObject>();
                }

                if (hittable != null)
                {
                    DamageData damage = new DamageData();
                    damage.DamageAmount = controller.data.damageAmount;
                    damage.HitDirection = r.direction;
                    damage.HitPosition = hitInfo.point;
                    hittable.TakeDamage(damage);
                }
                                
            }
        }

        IEnumerator MuzzleEffect(SoldierController controller)
        {
            controller.data.MuzzleFX.SetActive(true);
            controller.data.MuzzleFX.GetComponent<ParticleSystem>().Play();
            
            yield return new WaitForSeconds(0.25f);
            
            controller.data.MuzzleFX.GetComponent<ParticleSystem>().Stop();
            controller.data.MuzzleFX.SetActive(false);
        }
    }
}