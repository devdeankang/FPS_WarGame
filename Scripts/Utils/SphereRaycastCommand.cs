using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SphereRaycastCommand
{
    public float Radius;
    public int CircleResolution = 8;
    public int SphereResolution = 4;
    public Vector3 Center;
    public LayerMask Layer;
    public int MaxHits;

    RaycastsResultEvent resultEvent = new RaycastsResultEvent();

    NativeArray<RaycastCommand> commands;
    NativeArray<RaycastHit> results;
    JobHandle jobHandle;

    bool isjobStarted = false;

    public void DrawGizmosLines()
    {
        float circleStep = 360f / CircleResolution;
        float sphereStep = 180f / SphereResolution;

        for (int i = 0; i < SphereResolution; i++)
        {
            float sphereAngle = sphereStep * i;
            for (int j = 0; j < CircleResolution; j++)
            {
                float circleAngle = circleStep * j;

                Vector3 direction = GetRayDirection(circleAngle, sphereAngle) * Radius;

                Gizmos.DrawLine(Center, Center + direction);
            }
        }
    }

    public void Prepare()
    {
        commands = new NativeArray<RaycastCommand>(SphereResolution * CircleResolution, Allocator.Persistent);
        results = new NativeArray<RaycastHit>(SphereResolution * CircleResolution * MaxHits, Allocator.Persistent);

        RaycastCommand[] commandsArray = new RaycastCommand[commands.Length];
        float circleStep = 360f / CircleResolution;
        float sphereStep = 180f / SphereResolution;
        int rayIndex = 0;
        for (int i = 0; i < SphereResolution; i++)
        {
            float sphereAngle = sphereStep * i;
            for (int j = 0; j < CircleResolution; j++)
            {
                float circleAngle = circleStep * j;

                Vector3 direction = GetRayDirection(circleAngle, sphereAngle);
                direction = direction.normalized;

                commandsArray[rayIndex] = new RaycastCommand(Center, direction, Radius, Layer, MaxHits);

                rayIndex++;
            }
        }

        commands.CopyFrom(commandsArray);
    }

    public void Cast(UnityAction<NativeArray<RaycastHit>> callback)
    {
        resultEvent.RemoveAllListeners();
        resultEvent.AddListener(callback);

        RaycastsCreator raycastsCreator = new RaycastsCreator();
        raycastsCreator.Center = Center;
        raycastsCreator.Commands = commands;

        jobHandle = raycastsCreator.Schedule();

        isjobStarted = true;
        jobHandle = RaycastCommand.ScheduleBatch(commands, results, 1, jobHandle);
    }

    public RaycastCommand GetCommandByIndex(int index)
    {
        return commands[index];
    }

    public Vector3 GetRayDirectionByIndex(int index)
    {
        return commands[index].direction;
    }

    public Vector3 GetRayCenterByIndex(int index)
    {
        return commands[index].from;
    }

    public void Dispose()
    {
        jobHandle.Complete();

        if (results.IsCreated)
            results.Dispose();

        if (commands.IsCreated)
            commands.Dispose();
        isjobStarted = false;
    }

    public bool IsWaitJobComplete()
    {
        if (!isjobStarted)
            return true;

        if (jobHandle.IsCompleted)
        {
            jobHandle.Complete();

            resultEvent.Invoke(results);
            isjobStarted = false;
        }

        return jobHandle.IsCompleted;
    }

    Vector3 GetRayDirection(float angleCircle, float angleSphere)
    {
        float x = Mathf.Sin(angleSphere) * Mathf.Cos(angleCircle);
        float y = Mathf.Sin(angleSphere) * Mathf.Sin(angleCircle);
        float z = Mathf.Cos(angleSphere);

        return new Vector3(x, y, z);
    }

    public struct RaycastsCreator : IJob
    {
        public Vector3 Center;

        public NativeArray<RaycastCommand> Commands;

        public void Execute()
        {
            for (int i = 0; i < Commands.Length; i++)
            {
                RaycastCommand raycastCommand = Commands[i];
                raycastCommand.from = Center;
                Commands[i] = raycastCommand;
            }
        }
    }

    public class RaycastsResultEvent : UnityEvent<NativeArray<RaycastHit>>
    {

    }
}
