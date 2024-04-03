using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using System.Collections;
using UnityEditor.Rendering;

/// <summary>
/// Behavior with an API for spawning objects from a given set of prefabs.
/// </summary>
/*
public class ObjectSpawnerCustom : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The camera that objects will face when spawned. If not set, defaults to the main camera.")]
    Camera m_CameraToFace;

    /// <summary>
    /// The camera that objects will face when spawned. If not set, defaults to the <see cref="Camera.main"/> camera.
    /// </summary>
    public Camera cameraToFace
    {
        get
        {
            EnsureFacingCamera();
            return m_CameraToFace;
        }
        set => m_CameraToFace = value;
    }

    [SerializeField]
    [Tooltip("The list of prefabs available to spawn.")]
    List<GameObject> m_ObjectPrefabs = new List<GameObject>();

    /// <summary>
    /// The list of prefabs available to spawn.
    /// </summary>
    public List<GameObject> objectPrefabs
    {
        get => m_ObjectPrefabs;
        set => m_ObjectPrefabs = value;
    }

    [SerializeField]
    [Tooltip("Optional prefab to spawn for each spawned object. Use a prefab with the Destroy Self component to make " +
        "sure the visualization only lives temporarily.")]
    GameObject m_SpawnVisualizationPrefab;

    /// <summary>
    /// Optional prefab to spawn for each spawned object.
    /// </summary>
    /// <remarks>Use a prefab with <see cref="DestroySelf"/> to make sure the visualization only lives temporarily.</remarks>
    public GameObject spawnVisualizationPrefab
    {
        get => m_SpawnVisualizationPrefab;
        set => m_SpawnVisualizationPrefab = value;
    }

    [SerializeField]
    [Tooltip("The index of the prefab to spawn. If outside the range of the list, this behavior will select " +
        "a random object each time it spawns.")]
    int m_SpawnOptionIndex = -1;

    /// <summary>
    /// The index of the prefab to spawn. If outside the range of <see cref="objectPrefabs"/>, this behavior will
    /// select a random object each time it spawns.
    /// </summary>
    /// <seealso cref="isSpawnOptionRandomized"/>
    public int spawnOptionIndex
    {
        get => m_SpawnOptionIndex;
        set => m_SpawnOptionIndex = value;
    }

    /// <summary>
    /// Whether this behavior will select a random object from <see cref="objectPrefabs"/> each time it spawns.
    /// </summary>
    /// <seealso cref="spawnOptionIndex"/>
    /// <seealso cref="RandomizeSpawnOption"/>
    public bool isSpawnOptionRandomized => m_SpawnOptionIndex < 0 || m_SpawnOptionIndex >= m_ObjectPrefabs.Count;

    [SerializeField]
    [Tooltip("Whether to only spawn an object if the spawn point is within view of the camera.")]
    bool m_OnlySpawnInView = true;

    /// <summary>
    /// Whether to only spawn an object if the spawn point is within view of the <see cref="cameraToFace"/>.
    /// </summary>
    public bool onlySpawnInView
    {
        get => m_OnlySpawnInView;
        set => m_OnlySpawnInView = value;
    }

    [SerializeField]
    [Tooltip("The size, in viewport units, of the periphery inside the viewport that will not be considered in view.")]
    float m_ViewportPeriphery = 0.15f;

    /// <summary>
    /// The size, in viewport units, of the periphery inside the viewport that will not be considered in view.
    /// </summary>
    public float viewportPeriphery
    {
        get => m_ViewportPeriphery;
        set => m_ViewportPeriphery = value;
    }

    [SerializeField]
    [Tooltip("When enabled, the object will be rotated about the y-axis when spawned by Spawn Angle Range, " +
        "in relation to the direction of the spawn point to the camera.")]
    bool m_ApplyRandomAngleAtSpawn = true;

    /// <summary>
    /// When enabled, the object will be rotated about the y-axis when spawned by <see cref="spawnAngleRange"/>
    /// in relation to the direction of the spawn point to the camera.
    /// </summary>
    public bool applyRandomAngleAtSpawn
    {
        get => m_ApplyRandomAngleAtSpawn;
        set => m_ApplyRandomAngleAtSpawn = value;
    }

    [SerializeField]
    [Tooltip("The range in degrees that the object will randomly be rotated about the y axis when spawned, " +
        "in relation to the direction of the spawn point to the camera.")]
    float m_SpawnAngleRange = 45f;

    /// <summary>
    /// The range in degrees that the object will randomly be rotated about the y axis when spawned, in relation
    /// to the direction of the spawn point to the camera.
    /// </summary>
    public float spawnAngleRange
    {
        get => m_SpawnAngleRange;
        set => m_SpawnAngleRange = value;
    }

    [SerializeField]
    [Tooltip("Whether to spawn each object as a child of this object.")]
    bool m_SpawnAsChildren;

    /// <summary>
    /// Whether to spawn each object as a child of this object.
    /// </summary>
    public bool spawnAsChildren
    {
        get => m_SpawnAsChildren;
        set => m_SpawnAsChildren = value;
    }

    /// <summary>
    /// Event invoked after an object is spawned.
    /// </summary>
    /// <seealso cref="TrySpawnObject"/>
    public event Action<GameObject> objectSpawned;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void Awake()
    {
        // EnsureFacingCamera();
    }

    void EnsureFacingCamera()
    {
        if (m_CameraToFace == null)
            m_CameraToFace = Camera.main;
    }

    /// <summary>
    /// Sets this behavior to select a random object from <see cref="objectPrefabs"/> each time it spawns.
    /// </summary>
    /// <seealso cref="spawnOptionIndex"/>
    /// <seealso cref="isSpawnOptionRandomized"/>
    public void RandomizeSpawnOption()
    {
        m_SpawnOptionIndex = -1;
    }

    /// <summary>
    /// Attempts to spawn an object from <see cref="objectPrefabs"/> at the given position. The object will have a
    /// yaw rotation that faces <see cref="cameraToFace"/>, plus or minus a random angle within <see cref="spawnAngleRange"/>.
    /// </summary>
    /// <param name="spawnPoint">The world space position at which to spawn the object.</param>
    /// <param name="spawnNormal">The world space normal of the spawn surface.</param>
    /// <returns>Returns <see langword="true"/> if the spawner successfully spawned an object. Otherwise returns
    /// <see langword="false"/>, for instance if the spawn point is out of view of the camera.</returns>
    /// <remarks>
    /// The object selected to spawn is based on <see cref="spawnOptionIndex"/>. If the index is outside
    /// the range of <see cref="objectPrefabs"/>, this method will select a random prefab from the list to spawn.
    /// Otherwise, it will spawn the prefab at the index.
    /// </remarks>
    /// <seealso cref="objectSpawned"/>

    public GameObject SpawnedObject;
    public GameObject newObject;
    public int previousObjectIndex = -1;
    public int objectIndex = -1;
    public float alphaValue;
    public Material previewMaterial;
    bool letTranslate = true;


    public bool TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
    {

        if (m_OnlySpawnInView)
        {
            var inViewMin = m_ViewportPeriphery;
            var inViewMax = 1f - m_ViewportPeriphery;
            //  var pointInViewportSpace = cameraToFace.WorldToViewportPoint(spawnPoint);
            var pointInViewportSpace = Camera.main.WorldToViewportPoint(spawnPoint);
            if (pointInViewportSpace.z < 0f || pointInViewportSpace.x > inViewMax || pointInViewportSpace.x < inViewMin ||
                pointInViewportSpace.y > inViewMax || pointInViewportSpace.y < inViewMin)
            {
                return false;
            }
        }

        if (newObject != null) { Debug.Log(newObject.transform.position + " where its pos ?"); }

        if (newObject == null || objectIndex != m_SpawnOptionIndex)
        {
            if (newObject != null)
            {
                Destroy(newObject);
                Destroy(SpawnedObject);
                objectIndex = -1;

                objectIndex = isSpawnOptionRandomized ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count) : m_SpawnOptionIndex;
            }

            if (newObject == null)
            {
                objectIndex = -1;
                objectIndex = isSpawnOptionRandomized ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count) : m_SpawnOptionIndex;

                newObject = Instantiate(m_ObjectPrefabs[objectIndex]);
                if (newObject != null)
                {
                    ChangeTransparency(newObject, alphaValue);
                }
                letTranslate = true;
            }
        }

        if (letTranslate)
        {
            newObject.transform.position = spawnPoint;
        }

        if (m_SpawnAsChildren)
        {
            newObject.transform.parent = transform;
        }

        // EnsureFacingmera();
        var facePosition = m_CameraToFace.transform.position;
        var forward = facePosition - spawnPoint;

        BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
        newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);



        if (m_ApplyRandomAngleAtSpawn)
        {
            var randomRotation = UnityEngine.Random.Range(-m_SpawnAngleRange, m_SpawnAngleRange);
            newObject.transform.Rotate(Vector3.up, randomRotation);
        }

        if (m_SpawnVisualizationPrefab != null)
        {
            var visualizationTrans = Instantiate(m_SpawnVisualizationPrefab).transform;
            visualizationTrans.position = spawnPoint;
            visualizationTrans.rotation = newObject.transform.rotation;
        }

        objectSpawned?.Invoke(newObject);
        return true;
    }
    public void ChangeTransparency(GameObject obj, float alpha)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = previewMaterial;
        }
    }

    public void SpawnObject()
    {

        if (SpawnedObject == null)
        {
            Debug.Log(newObject.transform.position + " Initial pos");
            SpawnedObject = Instantiate(m_ObjectPrefabs[objectIndex]);
            SpawnedObject.transform.position = newObject.transform.position;
            SpawnedObject.transform.rotation = newObject.transform.rotation;
            Destroy(newObject);
            newObject = SpawnedObject;
             Debug.Log(newObject.transform.position + " pos before");
            letTranslate = false;
             StartCoroutine(Rotateobjects()); // JUS ADDED 
        }

    }

    public  Vector3 targetEulerAngles; //
    public float animationDuration = 5.0f; // Adjust as needed
    IEnumerator Rotateobjects() // (GameObject obj)
    {
        if (newObject != null)
        {
               var obj = newObject;
               var startRotation = obj.transform.rotation;
               var camera = Camera.main;
               var targetPosition = camera.transform.position + new Vector3(-0.2f, 0f, 0f);
               var endRotation = Quaternion.LookRotation(targetPosition - obj.transform.position);
               Debug.Log(endRotation + " end rotation");
               obj.transform.rotation = Quaternion.Lerp(startRotation, endRotation, animationDuration);
               Debug.Log(newObject.transform.position + " pos aFTER"); 


            
               Quaternion startRotation = newObject.transform.rotation;
               Quaternion endRotation = Quaternion.Euler(targetEulerAngles);
               for (float t = 0; t < 1.0f; t += Time.deltaTime / animationDuration)
               {
                   newObject.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
                   yield return null;
               }
               newObject.transform.rotation = endRotation;


            // try 3
              var mainObjectTransform = newObject.transform;
               //  var  buttonTransform = new Vector3 ( 0.05f , 0.09f , 0 ); // Replace with your button gameobject name
               // Option A: Fixed offset from the camera's position
               var targetbuttonTransformtPosition = Camera.main.transform.position + new Vector3(10f, 0.09f, -0.01f);
               // Calculate the direction vector from the main object to the button
               var direction = targetbuttonTransformtPosition - mainObjectTransform.position;
               // Derive the rotation angles using LookRotation
               var targetRotation = Quaternion.LookRotation(direction);
               var targetEulerAngles = targetRotation.eulerAngles;
               // Perform the rotation animation (assuming you have this code elsewhere)
               Quaternion startRotation = mainObjectTransform.rotation;
               for (float t = 0; t < 2.0f; t += Time.deltaTime / animationDuration)
               {
                   mainObjectTransform.rotation = Quaternion.Slerp(startRotation, Quaternion.Euler(targetEulerAngles), t);
                   yield return null;
               }
              mainObjectTransform.rotation = Quaternion.Euler(targetEulerAngles);


            
              // try 4 
              // Assume buttonPoint is the local position of the button on the smartphone
              Vector3 buttonPoint = new Vector3(0.05f, 0.09f, -0.01f);

              // Assume targetDirection is the world direction we want the button point to face
              // This could be a vector like Vector3.forward, which represents facing the user directly
              Vector3 targetDirection = Vector3.forward;

              // Calculate the current world position of the button point
              //   Vector3 buttonPointWorld = newObject.transform.TransformPoint(buttonPoint);
              Vector3 buttonPointWorld = new Vector3(0.05f, 0.09f, -0.01f);
           
              // Calculate the current direction vector of the button point from the center of the object
              Vector3 currentButtonDirection = (buttonPointWorld - newObject.transform.position).normalized;

              // Calculate the rotation needed to align the current button direction with the target direction
              Quaternion rotationToTarget = Quaternion.FromToRotation(currentButtonDirection, targetDirection);

              // Apply this rotation to the object
              newObject.transform.rotation = rotationToTarget * newObject.transform.rotation;
              yield return null;


            // This assumes you have the local position of the button relative to the phone
            Vector3 buttonWorldPosition = new Vector3(0.05f, 0.09f, -0.01f);

            // Calculate the world position of the button
          //  Vector3 buttonWorldPosition = newObject.TransformPoint(buttonLocalPosition);

            // Determine the direction from the button to the camera
            Vector3 directionToCamera = (Camera.main.transform.position - buttonWorldPosition).normalized;

            // Rotate the phone to face the camera, aligning the button with the direction to the camera
            // This rotation keeps the up direction of the phone aligned with the global up direction
            newObject.transform.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up);

            // Now the phone is facing the camera with the button positioned as the VR controller button in your example.
            // You can then place a UI element over the button to indicate its interactivity.
            yield return null;

        }

    }
}*/



// Trial 2      //////                     ///////                /////                         /////                       //////                     /////


public class ObjectSpawnerCustom : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The camera that objects will face when spawned. If not set, defaults to the main camera.")]
    Camera m_CameraToFace;

    /// <summary>
    /// The camera that objects will face when spawned. If not set, defaults to the <see cref="Camera.main"/> camera.
    /// </summary>
    public Camera cameraToFace
    {
        get
        {
            EnsureFacingCamera();
            return m_CameraToFace;
        }
        set => m_CameraToFace = value;
    }

    [SerializeField]
    [Tooltip("The list of prefabs available to spawn.")]
    List<GameObject> m_ObjectPrefabs = new List<GameObject>();

    /// <summary>
    /// The list of prefabs available to spawn.
    /// </summary>
    public List<GameObject> objectPrefabs
    {
        get => m_ObjectPrefabs;
        set => m_ObjectPrefabs = value;
    }

    [SerializeField]
    [Tooltip("Optional prefab to spawn for each spawned object. Use a prefab with the Destroy Self component to make " +
        "sure the visualization only lives temporarily.")]
    GameObject m_SpawnVisualizationPrefab;

    /// <summary>
    /// Optional prefab to spawn for each spawned object.
    /// </summary>
    /// <remarks>Use a prefab with <see cref="DestroySelf"/> to make sure the visualization only lives temporarily.</remarks>
    public GameObject spawnVisualizationPrefab
    {
        get => m_SpawnVisualizationPrefab;
        set => m_SpawnVisualizationPrefab = value;
    }

    [SerializeField]
    [Tooltip("The index of the prefab to spawn. If outside the range of the list, this behavior will select " +
        "a random object each time it spawns.")]
    int m_SpawnOptionIndex = -1;

    /// <summary>
    /// The index of the prefab to spawn. If outside the range of <see cref="objectPrefabs"/>, this behavior will
    /// select a random object each time it spawns.
    /// </summary>
    /// <seealso cref="isSpawnOptionRandomized"/>
    public int spawnOptionIndex
    {
        get => m_SpawnOptionIndex;
        set => m_SpawnOptionIndex = value;
    }

    /// <summary>
    /// Whether this behavior will select a random object from <see cref="objectPrefabs"/> each time it spawns.
    /// </summary>
    /// <seealso cref="spawnOptionIndex"/>
    /// <seealso cref="RandomizeSpawnOption"/>
    public bool isSpawnOptionRandomized => m_SpawnOptionIndex < 0 || m_SpawnOptionIndex >= m_ObjectPrefabs.Count;

    [SerializeField]
    [Tooltip("Whether to only spawn an object if the spawn point is within view of the camera.")]
    bool m_OnlySpawnInView = true;

    /// <summary>
    /// Whether to only spawn an object if the spawn point is within view of the <see cref="cameraToFace"/>.
    /// </summary>
    public bool onlySpawnInView
    {
        get => m_OnlySpawnInView;
        set => m_OnlySpawnInView = value;
    }

    [SerializeField]
    [Tooltip("The size, in viewport units, of the periphery inside the viewport that will not be considered in view.")]
    float m_ViewportPeriphery = 0.15f;

    /// <summary>
    /// The size, in viewport units, of the periphery inside the viewport that will not be considered in view.
    /// </summary>
    public float viewportPeriphery
    {
        get => m_ViewportPeriphery;
        set => m_ViewportPeriphery = value;
    }

    [SerializeField]
    [Tooltip("When enabled, the object will be rotated about the y-axis when spawned by Spawn Angle Range, " +
        "in relation to the direction of the spawn point to the camera.")]
    bool m_ApplyRandomAngleAtSpawn = true;

    /// <summary>
    /// When enabled, the object will be rotated about the y-axis when spawned by <see cref="spawnAngleRange"/>
    /// in relation to the direction of the spawn point to the camera.
    /// </summary>
    public bool applyRandomAngleAtSpawn
    {
        get => m_ApplyRandomAngleAtSpawn;
        set => m_ApplyRandomAngleAtSpawn = value;
    }

    [SerializeField]
    [Tooltip("The range in degrees that the object will randomly be rotated about the y axis when spawned, " +
        "in relation to the direction of the spawn point to the camera.")]
    float m_SpawnAngleRange = 45f;

    /// <summary>
    /// The range in degrees that the object will randomly be rotated about the y axis when spawned, in relation
    /// to the direction of the spawn point to the camera.
    /// </summary>
    public float spawnAngleRange
    {
        get => m_SpawnAngleRange;
        set => m_SpawnAngleRange = value;
    }

    [SerializeField]
    [Tooltip("Whether to spawn each object as a child of this object.")]
    bool m_SpawnAsChildren;

    /// <summary>
    /// Whether to spawn each object as a child of this object.
    /// </summary>
    public bool spawnAsChildren
    {
        get => m_SpawnAsChildren;
        set => m_SpawnAsChildren = value;
    }

    /// <summary>
    /// Event invoked after an object is spawned.
    /// </summary>
    /// <seealso cref="TrySpawnObject"/>
    public event Action<GameObject> objectSpawned;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void Awake()
    {
        // EnsureFacingCamera();
    }

    void EnsureFacingCamera()
    {
        if (m_CameraToFace == null)
            m_CameraToFace = Camera.main;
    }

    /// <summary>
    /// Sets this behavior to select a random object from <see cref="objectPrefabs"/> each time it spawns.
    /// </summary>
    /// <seealso cref="spawnOptionIndex"/>
    /// <seealso cref="isSpawnOptionRandomized"/>
    public void RandomizeSpawnOption()
    {
        m_SpawnOptionIndex = -1;
    }

    /// <summary>
    /// Attempts to spawn an object from <see cref="objectPrefabs"/> at the given position. The object will have a
    /// yaw rotation that faces <see cref="cameraToFace"/>, plus or minus a random angle within <see cref="spawnAngleRange"/>.
    /// </summary>
    /// <param name="spawnPoint">The world space position at which to spawn the object.</param>
    /// <param name="spawnNormal">The world space normal of the spawn surface.</param>
    /// <returns>Returns <see langword="true"/> if the spawner successfully spawned an object. Otherwise returns
    /// <see langword="false"/>, for instance if the spawn point is out of view of the camera.</returns>
    /// <remarks>
    /// The object selected to spawn is based on <see cref="spawnOptionIndex"/>. If the index is outside
    /// the range of <see cref="objectPrefabs"/>, this method will select a random prefab from the list to spawn.
    /// Otherwise, it will spawn the prefab at the index.
    /// </remarks>
    /// <seealso cref="objectSpawned"/>

    public GameObject SpawnedObject;
    public GameObject newObject;
    public int previousObjectIndex = -1;
    public int objectIndex = -1;
    public float alphaValue;
    public Material previewMaterial;
    bool letTranslate = true;


    public bool TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
    {
       
        if (m_OnlySpawnInView)
        {
            var inViewMin = m_ViewportPeriphery;
            var inViewMax = 1f - m_ViewportPeriphery;
            //  var pointInViewportSpace = cameraToFace.WorldToViewportPoint(spawnPoint);
            var pointInViewportSpace = Camera.main.WorldToViewportPoint(spawnPoint);
            if (pointInViewportSpace.z < 0f || pointInViewportSpace.x > inViewMax || pointInViewportSpace.x < inViewMin ||
                pointInViewportSpace.y > inViewMax || pointInViewportSpace.y < inViewMin)
            {
                return false;
            }
        }

        if (newObject != null) { Debug.Log(newObject.transform.position + " transPos"); }

        if (newObject == null || objectIndex != m_SpawnOptionIndex)
        {
            if (newObject != null)
            {
                Destroy(newObject);
                Destroy(SpawnedObject);
                objectIndex = -1;

                objectIndex = isSpawnOptionRandomized ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count) : m_SpawnOptionIndex;
            }

            if (newObject == null)
            {
                objectIndex = -1;
                objectIndex = isSpawnOptionRandomized ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count) : m_SpawnOptionIndex;

                newObject = Instantiate(m_ObjectPrefabs[objectIndex]);
                if (newObject != null)
                {
                    ChangeTransparency(newObject, alphaValue);
                }
                letTranslate = true;
            }
        }

        if (letTranslate)
        {
            newObject.transform.position = spawnPoint;
        }

        if (m_SpawnAsChildren)
        {
            newObject.transform.parent = transform;
        }

        // EnsureFacingmera();
        var facePosition = m_CameraToFace.transform.position;
        var forward = facePosition - spawnPoint;

        BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
        newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);



        if (m_ApplyRandomAngleAtSpawn)
        {
            var randomRotation = UnityEngine.Random.Range(-m_SpawnAngleRange, m_SpawnAngleRange);
            newObject.transform.Rotate(Vector3.up, randomRotation);
        }

        if (m_SpawnVisualizationPrefab != null)
        {
            var visualizationTrans = Instantiate(m_SpawnVisualizationPrefab).transform;
            visualizationTrans.position = spawnPoint;
            visualizationTrans.rotation = newObject.transform.rotation;
        }

        objectSpawned?.Invoke(newObject);
        return true;
    }
    public void ChangeTransparency(GameObject obj, float alpha)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = previewMaterial;
        }
    }

    public void SpawnObject()
    {

        if (SpawnedObject == null)
        {
            Debug.Log(newObject.transform.position + " Initial pos");
            SpawnedObject = Instantiate(m_ObjectPrefabs[objectIndex]);
            SpawnedObject.transform.position = newObject.transform.position;
            SpawnedObject.transform.rotation = newObject.transform.rotation;
            Destroy(newObject);
            newObject = SpawnedObject;
            Debug.Log(newObject.transform.position + " pos before");
            letTranslate = false;
            // StartCoroutine(Rotateobjects()); // JUS ADDED 
            StartCoroutine(SpawnAndRotateObjects());
        }

    }

    public Vector3 targetEulerAngles; //
    public float animationDuration = 5.0f; // Adjust as needed
    public GameObject prefab; // Prefab to instantiate.
    public List<Vector3> points = new List<Vector3>(); // List of points to spawn and rotate objects at.
    private GameObject currentObject; // Currently active object.


    /*   IEnumerator Rotateobjects() // (GameObject obj)
       {
           if (newObject != null)
           {
               Quaternion startRotation = newObject.transform.rotation;
               Quaternion endRotation = Quaternion.Euler(targetEulerAngles);
               for (float t = 0; t < 1.0f; t += Time.deltaTime / animationDuration)
               {
                   newObject.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
                   yield return null;
               }
               newObject.transform.rotation = endRotation;
           }
       }

   */

    IEnumerator SpawnAndRotateObjects()
    {
        foreach (Vector3 point in points)
        {
            if (currentObject != null)
            {
                Destroy(currentObject); // Optionally destroy the previous object.
            }

            // Instantiate a new object at the current point.
            currentObject = Instantiate(prefab, point, Quaternion.identity);

            // Start rotating the newly instantiated object.
            yield return StartCoroutine(RotateObject(currentObject, Quaternion.Euler(targetEulerAngles), animationDuration));
        }
    }

    IEnumerator RotateObject(GameObject obj, Quaternion endRotation, float duration)
    {
        Quaternion startRotation = obj.transform.rotation;
        for (float t = 0; t < 1.0f; t += Time.deltaTime / duration)
        {
            obj.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
        obj.transform.rotation = endRotation;
    }
}























