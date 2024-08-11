using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WarGame
{
    public class SoldierOccupied : IState<SoldierController>
    {
        Transform currDestination = null;

        public void OperateEnter(SoldierController controller)
        {
            controller.isForcedRaderShutDown = true;
            controller.agent.isStopped = true;

            currDestination = controller.destination;   // Init currDest

            SetNextDestination(controller);
            controller.ChangeState(SoldierController.SoldierState.Move);
        }

        public void OperateUpdate(SoldierController controller)
        {

        }

        public void OperateFixedUpdate(SoldierController controller)
        {

        }

        public void OperateExit(SoldierController controller)
        {
            controller.isForcedRaderShutDown = false;
            controller.agent.isStopped = false;
        }

        void SetNextDestination(SoldierController controller)
        {
            if (controller.destinations.Count <= 1)
            {
                Debug.LogError($" ## {controller.gameObject.name} agent didn't have more than one destination.");
                return;
            }

            while(currDestination == controller.destination)
            {
                int nextDest = Random.Range(0, controller.destinations.Count);
                currDestination =  controller.destinations[nextDest];
            }

            controller.destination = currDestination;
        }
    }
}