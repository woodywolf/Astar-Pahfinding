using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Unit : MonoBehaviour
{
    private const float minPathUpdateTime = .2f;
    private const float PathUpdateMoveThreshold = .5f;
    
    public Transform target;
    public float speed = 5f;
    public float turnDistance = 5f;
    public float turnSpeed = 3f;
    public float stoppingDistance = 10f;
    
    private Path path;

    private void Start()
    {
        StartCoroutine(UpdatePath());
    }

    private void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new Path(waypoints, transform.position, turnDistance, stoppingDistance);
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
    }

    private IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < .3f)
            yield return new WaitForSeconds(.3f);
        
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
        
        float sqrMoveThreshold = PathUpdateMoveThreshold * PathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;
        
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                targetPosOld = target.position;
            }
        }
    }

    private IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;
        
        while (followingPath)
        {
            Vector2 position2D = new Vector2(transform.position.x, transform.position.z);

            while (path.turnBoundaries[pathIndex].HasCrossedLine(position2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            if (followingPath)
            {
                if (pathIndex >= path.slowDownIndex && stoppingDistance > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].
                        DistanceFromPoint(position2D) / stoppingDistance);

                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                    }
                }

                Quaternion targetRotation =
                    Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = 
                    Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                    
                transform.Translate(Vector3.forward * (Time.deltaTime * speed * speedPercent), Space.Self);
            }
            
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        path?.DrawWithGizmos();
    }
}
