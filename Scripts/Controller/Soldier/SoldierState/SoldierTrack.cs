using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WarGame
{
    public class SoldierTrack : IState<SoldierController>
    {
        GameObject nearestTarget = null;

        public void OperateEnter(SoldierController controller)
        {
            controller.agent.isStopped = false;
            controller.animator.SetBool("Idling", false);
            controller.animator.SetBool("Walk", true);

            controller.SetNaviAgent(controller.data.speed);
        }

        public void OperateUpdate(SoldierController controller) 
        {
            DetectTarget(controller);
            
            if(nearestTarget != null) controller.agent.SetDestination(nearestTarget.transform.position);

            ChangeState(controller);
        }

        public void OperateFixedUpdate(SoldierController controller)
        {
            
        }

        public void OperateExit(SoldierController controller)
        {
            controller.animator.SetBool("Walk", false);
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

            else if (!controller.IsAttackableTarget(nearestTarget))
                controller.ChangeState(SoldierController.SoldierState.Move);

            else if (controller.IsAttackableTarget(nearestTarget) && controller.IsShootDistance(nearestTarget))
                controller.ChangeState(SoldierController.SoldierState.Attack);                
        }
    }
}