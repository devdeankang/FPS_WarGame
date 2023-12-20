using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace WarGame
{
    public class PlayerAction : MonoBehaviour
    {
        public Transform RaycastStartPoint;
        public LayerMask Layer;
        public float ActionDistance = 2f;
        public PlayerInventory PlayerInventory;

        GameObject hittedTarget = null;
        IPlayerAction currentAction;

        public bool NeedShowHand { get { return (currentAction != null); } }

        void Update()
        {
            Ray r = new Ray();
            r.origin = RaycastStartPoint.position;
            r.direction = RaycastStartPoint.forward;

            RaycastHit hit;

            hittedTarget = null;
            if (Physics.Raycast(r, out hit, ActionDistance, Layer, QueryTriggerInteraction.Collide))
            {
                hittedTarget = hit.collider.gameObject;
            }

            if (hittedTarget != null)
            {
                if (currentAction == null || !Input.GetKey(KeyCode.E))
                {
                    IPlayerAction useAction = hit.collider.GetComponent<IPlayerAction>();
                    if (useAction != null)
                    {

                        if (Input.GetKeyUp(KeyCode.E) && currentAction != null)
                        {
                            currentAction.UseEnd();
                            currentAction = null;
                        }
                        currentAction = useAction;
                    }
                    else
                    {
                        if (currentAction != null)
                        {
                            currentAction.UseEnd();
                            currentAction = null;
                        }
                    }
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.E))
                {
                    if (currentAction != null)
                    {
                        currentAction.UseEnd();
                        currentAction = null;
                    }
                }
            }

            if (currentAction != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickupableItem pickupableItem = currentAction as PickupableItem;
                    if(pickupableItem != null)
                    {
                        Type itemType = pickupableItem.GetItemType();
                        BaseItem item = PlayerInventory.GetItem(itemType);

                        if (item == null || item.IsCanAdd())
                        {
                            PlayerInventory.AddItem(pickupableItem.PickupItem());
                        }
                    }
                    else
                    {
                        currentAction.UseStart();
                    }
                }
            }

            if (currentAction != null)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    currentAction.UseUpdate();
                }
            }

            if (currentAction != null)
            {
                if (Input.GetKeyUp(KeyCode.E))
                {
                    currentAction.UseEnd();
                    currentAction = null;
                }
            }
        }
    }
}

public interface IPlayerAction
{
    void UseStart();
    void UseUpdate();
    void UseEnd();
}