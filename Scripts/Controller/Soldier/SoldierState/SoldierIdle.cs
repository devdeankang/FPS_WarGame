using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace WarGame
{
    public class SoldierIdle : IState<SoldierController>
    {
        float time;
        float minTime = 1f;
        float maxTime = 3f;
        float startTime;

        public void OperateEnter(SoldierController controller)
        {
            InitDestinations(controller);
            SetDestination(controller);
            InitStartTime();
            controller.isForcedRaderShutDown = true;
            controller.animator.SetBool("Idling", true);            
            controller.agent.isStopped = true;
        }

        public void OperateUpdate(SoldierController controller)
        {   
            time += Time.deltaTime;

            if (!controller.health.IsAlive)
                controller.ChangeState(SoldierController.SoldierState.Dead);

            else if (controller.health.IsAlive && time > startTime)
                controller.ChangeState(SoldierController.SoldierState.Move);
        }

        public void OperateFixedUpdate(SoldierController controller)
        {
            
        }

        public void OperateExit(SoldierController controller)
        {
            controller.isForcedRaderShutDown = false;
            controller.animator.SetBool("Idling", false);
            controller.agent.isStopped = false;
        }

        void InitStartTime()
        {
            time = 0f;
            startTime = Random.Range(minTime, maxTime);
        }

        void InitDestinations(SoldierController controller)
        {
            GameObject rootDestinations = null;

            if (controller.data.IsRuSoldier)
                rootDestinations = GameObject.FindGameObjectWithTag(controller.data.RU_Destinations);
            else
                rootDestinations = GameObject.FindGameObjectWithTag(controller.data.US_Destinations);

            if (controller.destinations.Count != 0) 
                controller.destinations = new List<Transform>();

            for (int i =0; i<rootDestinations.transform.childCount; i++)
            {
                controller.destinations.Add(rootDestinations.transform.GetChild(i));
            }            
        }

        void SetDestination(SoldierController controller)
        {
            int random = Random.Range(0, controller.destinations.Count);
            controller.destination = controller.destinations[random];
        }
    }
}