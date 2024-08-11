using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WarGame
{
    public class SoldierMove : IState<SoldierController>
    {
        GameObject nearestTarget;

        public void OperateEnter(SoldierController controller)
        {
            controller.agent.isStopped = false;
            controller.animator.SetBool("Idling", false);
            controller.animator.SetBool("Walk", false);

            controller.agent.SetDestination(controller.destination.transform.position);
        }

        public void OperateUpdate(SoldierController controller)
        {            
            DetectTarget(controller);
            ChangeState(controller);
        }

        public void OperateFixedUpdate(SoldierController controller)
        {
            controller.SetNaviAgent(controller.data.runSpeed);            
        }

        public void OperateExit(SoldierController controller)
        {

        }

        void DetectTarget(SoldierController controller)
        {
            if (controller.NeedRaderSystem)
                nearestTarget = controller.rader.GetNearestTarget(controller.rader.detectedTargets);
            else
                nearestTarget = null;
        }

        void ChangeState(SoldierController controller)
        {
            if (!controller.health.IsAlive) 
                controller.ChangeState(SoldierController.SoldierState.Dead);

            else if (controller.IsOccupied) 
                controller.ChangeState(SoldierController.SoldierState.Occupied);

            else if (controller.IsAttackableTarget(nearestTarget))
            {
                if (controller.rader.GetSqrMagnitude(nearestTarget) <= controller.ShootDistance)
                    controller.ChangeState(SoldierController.SoldierState.Attack);
                else
                    controller.ChangeState(SoldierController.SoldierState.Track);
            }
        }
    }
}