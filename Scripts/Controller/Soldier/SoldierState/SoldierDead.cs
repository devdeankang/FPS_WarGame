using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class SoldierDead : IState<SoldierController>
    {

        public void OperateEnter(SoldierController controller)
        {
            controller.isForcedRaderShutDown = true;

            controller.animator.SetBool("Idling", true);

            controller.agent.destination = controller.gameObject.transform.position;
            controller.agent.isStopped = true;

            DeathAnim(controller);
        }

        public void OperateUpdate(SoldierController controller)
        {            
            Destroy(controller);
        }

        public void OperateFixedUpdate(SoldierController controller)
        {

        }

        public void OperateExit(SoldierController controller)
        {

        }

        void Destroy(SoldierController controller)
        {
            if (controller.animator.GetCurrentAnimatorStateInfo(0).IsTag("Death") &&
                controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                PoolManager.Instance.BackToPool(controller.gameObject, controller.data.soldier.GroupName);                
            }
        }

        void DeathAnim(SoldierController controller)
        {
            controller.animator.SetInteger("Death", Random.Range(1,4));
        }
    }
}