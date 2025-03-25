using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    [System.Serializable]
    public class PositionSettings
    {
        public Vector3 targetPosOffset = new Vector3(0, 3.4f, 0);
        public float distanceFromTarget = -8;
        public float smooth = 0.05f;
        public bool smoothFollow = true;
        public float adjustmentDistance = -8;
    }


    [System.Serializable]
    public class DebugSettings
    {
        public bool drawDesiredCollisionLine = true;
        public bool drwaAdjustedCollisionLines = true;
    }
    [System.Serializable]
    public class OrbitSettings
    {
        public float xRotation = -20;
        public float yRotation = -180;
    }
    public Transform target;
    public PositionSettings position = new PositionSettings();
    public OrbitSettings Orbit = new OrbitSettings();
    public DebugSettings debug = new DebugSettings();
    public ColliderHandler collision = new ColliderHandler();
    Vector3 adjustedDestination = Vector3.zero;
    Vector3 camVel = Vector3.zero;
    Vector3 targetPos = Vector3.zero;
    Vector3 destination = Vector3.zero;

    private void Start()
    {
        moveTotarget();
        collision.Initialize(Camera.main);
        collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints);
    }
    private void LateUpdate()
    {
        moveTotarget();
        collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints);

        for (int i = 0; i < 5; i++)
        {
            if (debug.drawDesiredCollisionLine)
            {
                Debug.DrawLine(targetPos, collision.desiredCameraClipPoints[i], Color.white);
            }
            if (debug.drwaAdjustedCollisionLines)
            {
                Debug.DrawLine(targetPos, collision.adjustedCameraClipPoints[i], Color.red);
            }
        }
        collision.CheckColliding(targetPos);
        position.adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(targetPos);
    }
    void moveTotarget()
    {
        targetPos = target.position + Vector3.up * position.targetPosOffset.y + Vector3.forward * position.targetPosOffset.z + transform.TransformDirection(Vector3.right * position.targetPosOffset.x);

        // Use the parent's rotation on the X-axis and the target's rotation on the Y-axis
        destination = Quaternion.Euler(Orbit.xRotation + transform.parent.eulerAngles.x, Orbit.yRotation + target.eulerAngles.y, 0) * -Vector3.forward * position.distanceFromTarget;
        destination += targetPos;

        if (collision.colliding)
        {
            adjustedDestination = Quaternion.Euler(Orbit.xRotation + transform.parent.eulerAngles.x, Orbit.yRotation + target.eulerAngles.y, 0) *
                                  Vector3.forward * position.adjustmentDistance;
            adjustedDestination += targetPos;
            if (position.smoothFollow)
            {
                transform.position = Vector3.SmoothDamp(transform.position, adjustedDestination, ref camVel, position.smooth);
            }
            else
            {
                transform.position = adjustedDestination;
            }
        }
        else
        {
            if (position.smoothFollow)
            {
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVel, position.smooth);
            }
            else
            {
                transform.position = destination;
            }
        }
        transform.LookAt(targetPos);
    }
    [System.Serializable]
    public class ColliderHandler
    {
        public LayerMask collisionLayer;
        [HideInInspector]
        public bool colliding = false;
        [HideInInspector]
        public Vector3[] adjustedCameraClipPoints;
        [HideInInspector]
        public Vector3[] desiredCameraClipPoints;

        Camera _camera;
        [HideInInspector]
        public Vector3 collisionCheckPos;

        /// <summary>
        /// Initializes the camera member and the clip point arrays.
        /// </summary>
        public void Initialize(Camera cam)
        {
            _camera = cam;
            adjustedCameraClipPoints = new Vector3[5];
            desiredCameraClipPoints = new Vector3[5];
        }
        /// <summary>
        /// Calculates the clip points based on the camera's near clip plane, field of view and aspect ratio.
        /// Places the calculated clip points in intoArray
        /// </summary>
        public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
        {
            if (!_camera)
                return;

            //clear the contents of intoArray
            intoArray = new Vector3[5];

            float z = _camera.nearClipPlane;
            float x = Mathf.Tan(_camera.fieldOfView / 3.41f) * z;
            float y = x / _camera.aspect;

            //top left
            intoArray[0] = (atRotation * new Vector3(-x, y, z)) + cameraPosition; //added and rotated the point relative to camera
                                                                                  //top right
            intoArray[1] = (atRotation * new Vector3(x, y, z)) + cameraPosition; //added and rotated the point relative to camera
                                                                                 //bottom left
            intoArray[2] = (atRotation * new Vector3(-x, -y, z)) + cameraPosition; //added and rotated the point relative to camera
                                                                                   //bottom right
            intoArray[3] = (atRotation * new Vector3(x, -y, z)) + cameraPosition; //added and rotated the point relative to camera
                                                                                  //camera's position
            intoArray[4] = cameraPosition - _camera.transform.forward;
        }

        /// <summary>
        /// Returns true if any of the clip point rays return a hit.
        /// </summary>
        bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
        {
            for (int i = 0; i < clipPoints.Length; i++)
            {
                Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
                float distance = Vector3.Distance(clipPoints[i], fromPosition);
                if (Physics.Raycast(ray, distance, collisionLayer))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the distance closest to the target which is derived from one of the 5 rays that returns.. 
        /// ..a hit casted from the camera's clip points and center.
        /// </summary>
        public float GetAdjustedDistanceWithRayFrom(Vector3 from)
        {
            float distance = -1;

            for (int i = 0; i < desiredCameraClipPoints.Length; i++)
            {
                Ray ray = new Ray(from, desiredCameraClipPoints[i] - from);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (distance == -1)
                        distance = hit.distance;
                    else
                    {
                        if (hit.distance < distance)
                            distance = hit.distance;
                    }
                }
            }

            if (distance == -1)
                return 0;
            else
                return distance;
        }

        /// <summary>
        /// Returns true when a collision is detected from one of the 5 rays casted from the desired clip points and center
        /// </summary>
        public void CheckColliding(Vector3 targetPosition)
        {
            if (CollisionDetectedAtClipPoints(desiredCameraClipPoints, targetPosition))
            {
                colliding = true;
            }
            else
            {
                colliding = false;
            }
        }
    }
}

