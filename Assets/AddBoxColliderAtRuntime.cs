using UnityEngine;
using System.Collections;
//using static Unity.VisualScripting.AnnotationUtility;
using System.Threading.Tasks;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Drawing;
using GLTFast.Schema;
using Camera = UnityEngine.Camera;
using System.Reflection;
using UnityEngine.UIElements;
using static System.Net.WebRequestMethods;
using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.Networking;
using TMPro;
using GLTFast;

/*[System.Serializable]
public class Annotation1
{
    public Vector3 position;

}*/
public class AddBoxColliderAtRuntime : MonoBehaviour
{
    //  public GameObject modelToCollide; // Reference the model GameObject in the Inspector

    [SerializeField]
    public List<Annotation> AnnotationsList;//{ get; private set; } // = new List<Annotation>();
    public GameObject model;
    public GameObject newObject;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private String baseUrl;
    private void Awake()
    {
        StartCoroutine(DownloadModelCoroutine("https://three-js-dashboard.onrender.com/uploads/modelFile-1712394564440-436215008.glb", newObject)); // watch 
        AddBoundingBox(model);
        annotationUrl = "https://three-js-dashboard.onrender.com/api/products/661111456378f1ef89a88754/annotations";
        StartCoroutine(GetProductAnnotations(annotationUrl)); //http://192.168.0.159/
    }
    String annotationUrl;
    private void Start()
    {

        lastPosition = Camera.main.transform.position;
        lastRotation = Camera.main.transform.rotation;
        description.text = " heya hello here is the description ";

    }

    private void LateUpdate()
    {
        //    float dist = Vector3.Distance(transformedPoint, Camera.main.transform.position);
        // Debug.Log(dist);
       /* if (Camera.main.transform.position != lastPosition || Camera.main.transform.rotation != lastRotation)
        {
            OnCameraMove();
            Debug.Log(" Camera has moved ");
            lastPosition = Camera.main.transform.position;
            lastRotation = Camera.main.transform.rotation;
        }*/
    }

/*    public bool TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
    {

        if (m_OnlySpawnInView)
        {
            var inViewMin = m_ViewportPeriphery;
            var inViewMax = 1f - m_ViewportPeriphery;
            var pointInViewportSpace = Camera.main.WorldToViewportPoint(spawnPoint);
            if (pointInViewportSpace.z < 0f || pointInViewportSpace.x > inViewMax || pointInViewportSpace.x < inViewMin ||
                pointInViewportSpace.y > inViewMax || pointInViewportSpace.y < inViewMin)
            {
                return false;
            }
        }


        if (newObject == null || objectIndex != m_SpawnOptionIndex)
        {
            if (newObject != null)
            {
                Debug.Log("not null ");
                Destroy(newObject);
                Destroy(SpawnedObject);
                objectIndex = -1;

                objectIndex = isSpawnOptionRandomized ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count) : m_SpawnOptionIndex;
            }

            if (newObject == null)
            {
                objectIndex = -1;
                objectIndex = isSpawnOptionRandomized ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count) : m_SpawnOptionIndex;

                newObject = Instantiate(baseModel);
                ButtonProductData objData = newObject.GetComponent<ButtonProductData>();
                objData = GetButtonProductDataAtIndex(objectIndex);

                string modelFilePath = objData.allProducts.modelFile;
                string productId = objData.allProducts._id;

                //     string url = "http://192.168.0.159:3001" + modelFilePath;
                string url = baseUrl + modelFilePath;
                Debug.Log("url " + url);
                StartCoroutine(DownloadModelCoroutine(url, newObject)); // watch 
               // string annotationUrl = baseUrl + "/api/products/661111456378f1ef89a88754/annotations"; // for Watch
                annotationUrl = "https://three-js-dashboard.onrender.com/api/products/661111456378f1ef89a88754/annotations";

                StartCoroutine(GetProductAnnotations(annotationUrl)); //http://192.168.0.159/
                Debug.Log("Annotation url " + annotationUrl);


                //  letTranslate = true;
            }
        }
    }*/
    public bool check;
    IEnumerator CheckCameraPosition()
    {
        // Continuously check the camera position
        bool check = true;
        while (check)
        {
            yield return new WaitForEndOfFrame();
            if (Camera.main.transform.position != lastPosition || Camera.main.transform.rotation != lastRotation)
            {
                OnCameraMove();
                Debug.Log(" Camera has moved ");
                lastPosition = Camera.main.transform.position;
                lastRotation = Camera.main.transform.rotation;
            }
            // yield return WaitForSeconds(0.5f);
        }
        //return null;
    }




    private Bounds modelbounds;
    //void Add(GameObject model)
   void  AddBoundingBox(GameObject model)
    {
        
        Quaternion originalParentRotation = model.transform.parent.rotation;
        Vector3 originalParentPosition = model.transform.parent.position;
        Quaternion originalModelRotation = model.transform.rotation;

        model.transform.parent.rotation = Quaternion.identity;
        model.transform.parent.position = Vector3.zero;
        model.transform.rotation = Quaternion.identity;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds();

        bool hasBounds = false;
        // Calculate the combined bounds in the local space of the model
        foreach (Renderer renderer in renderers)
        {
            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        // Now the bounds are in world space, convert to local space of the model
        bounds.center = model.transform.InverseTransformPoint(bounds.center);
        bounds.size = model.transform.InverseTransformVector(bounds.size);

        // Now, apply the correct bounds to the BoxCollider
        BoxCollider collider = model.AddComponent<BoxCollider>();
        collider.center = bounds.center;
        collider.size = bounds.size;

        // Restore the original rotations and position
        model.transform.parent.rotation = originalParentRotation;
        model.transform.parent.position = originalParentPosition;
        model.transform.rotation = originalModelRotation;

        modelbounds = bounds;


      //  StartCoroutine(SpawnAndRotateObjects());
    }
    public List<string> savedModelPaths;


    public string savePath;
    public string modelName;
    IEnumerator DownloadModelCoroutine(string url, GameObject go)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            Debug.Log(url + " url is  calleed");
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error downloading model: {webRequest.error}");
            }
            else
            {
                modelName = Path.GetFileName(url);
                savePath =  Path.Combine(Application.persistentDataPath, modelName);

                if (!System.IO.File.Exists(savePath))
                {  // to check if file exits 
                    System.IO.File.WriteAllBytes(savePath, webRequest.downloadHandler.data);
                    savedModelPaths.Add(savePath);
                    Debug.Log($"Model saved to: {savePath}");
                }
                var addModelTask = AddDownloadedModelToPrefabsAsync(savePath, go);
                // if (go != null)
                //   {
                //  Debug.Log("IS TRANSparency called /");
                // ChangeTransparency(go, alphaValue);
                //  }
                // Wait for the async operation to complete
                yield return new WaitUntil(() => addModelTask.IsCompleted);
               // ChangeTransparency(go, alphaValue);
                StartCoroutine(SpawnAndRotateObjects());
            }
        }
    }
    public async Task AddDownloadedModelToPrefabsAsync(string modelPath, GameObject parentprefab)
    {
        //  Debug.Log("modelpath  loading from is" + modelPath);

        //  var gltf = new GLTFast.GltfImport();
        var gltf = new GltfImport();
        // Load the GLB file asynchronously
        if (await gltf.Load(modelPath))
        {

            // gltf.InstantiateMainScene(parentprefab.transform);
            await gltf.InstantiateMainSceneAsync(parentprefab.transform);
            GameObject instantiatedObject = parentprefab.transform.GetChild(parentprefab.transform.childCount - 1).gameObject;

            AddBoundingBox(instantiatedObject);

        }
        else
        {
            Debug.LogError("Failed to load the model.");
        }

    }
    IEnumerator GetProductAnnotations(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            Debug.Log("called get prd annotations" + url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
            }
            else
            {
                Debug.Log("Api is hit");
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"Response: {jsonResponse}");
                ProcessAnnotations(jsonResponse);
            }
        }
    }


    void ProcessAnnotations(string jsonResponse)
    {

        string jsonToParse = $"{{\"items\":{jsonResponse}}}";
        AnnotationArray annotationArray = JsonUtility.FromJson<AnnotationArray>(jsonToParse);


        AnnotationsList.Clear();
        foreach (var annotation in annotationArray.items)
        {
            AnnotationsList.Add(annotation);
            Debug.Log($"Annotation ID: {annotation.annotationID}, Title: {annotation.title}");
            Debug.Log($"Position - X: {annotation.position.x}, Y: {annotation.position.y}, Z: {annotation.position.z}");
        }

        foreach (var annotation in AnnotationsList)
        {
            //Instantiate(prefab, annotation.position, Quaternion.identity);
            Debug.Log("pos " + annotation.position + "title " + annotation.title + "Desc" + annotation.description);

        }

        // the testing part only prod ->> annotation is here 
        int currentIndex = 0;
        foreach (var annotation in AnnotationsList)
        {
            Debug.Log(" buttons are instantaietd");

            int index = currentIndex;
            var button = Instantiate(menuButtonPrefabContainer, Vector3.zero, Quaternion.identity);
            UnityEngine.UI.Button buttonClick = button.transform.GetComponent<UnityEngine.UI.Button>();
            // buttonClick.onClick.AddListener(() => CreateAnnotationUI(currentIndex));  // menuManager.SetObjectToSpawn(objectIndex));
            buttonClick.onClick.AddListener(() => InstantiateAnnotationPoint(index));  // menuManager.SetObjectToSpawn(objectIndex));
            // buttonClick.onClick.AddListener(() => menuManager.SetObjectToSpawn(objectIndex));
            button.GetComponentInChildren<TextMeshProUGUI>().text = AnnotationsList[index].title;// Annotationslist[index].title;
            // button.GetComponent<TextMeshProUGUI>().text = "Clicked!";
            button.transform.SetParent(uiAnnotationContent.transform, false);
            annotationButtonList.Add(button);

            Debug.Log("added call" + index);
            Debug.Log(currentIndex);
            currentIndex++;
        }
    }
    public TMP_Text title;
    public TMP_Text description;
    public void InstantiateAnnotationPoint(int currentIndex)
    {

       // StartCoroutine(CheckCameraPosition());
        Debug.Log("hello onclick" + currentIndex);
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        Debug.Log("the point fetched / " + AnnotationsList[currentIndex].position);
        Vector3 transformedPoint = parentobj.transform.TransformPoint(AnnotationsList[currentIndex].position);
        Vector3 pointdirection = transformedPoint - parentobj.transform.position;
        currentObject = Instantiate(prefab, transformedPoint, Quaternion.identity, parentobj.transform);
        Vector3 directionToCamera = Camera.main.transform.position - pointdirection; //transformedPoint; // parentobj.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
        float animationDuration = 6.00f;
        title.text = AnnotationsList[currentIndex].title;
        description.text = AnnotationsList[currentIndex].description;
        StartCoroutine(RotateObject(parentobj, currentObject.transform.position, Camera.main, animationDuration));
        StartCoroutine(CheckCameraPosition());

    }


    private GameObject currentObject;
    public GameObject prefab;
    public GameObject parentobj;

    public GameObject menuButtonPrefabContainer;
    public int objectIndex;

    Vector3 directionToCamera1;
    Vector3 directionToPoint1;
    Quaternion targetRotation1;
    public GameObject uiAnnotationContent;
    public List<GameObject> annotationButtonList;

    IEnumerator SpawnAndRotateObjects()
    {
  /*      int currentIndex = 0;
        foreach (Annotation annotation in AnnotationsList)
        {
            //   Vector3 transformedPoint = parentobj.transform.TransformPoint(annotation.position);

            // currentObject = Instantiate(prefab, transformedPoint, Quaternion.identity, parentobj.transform);
            var button = Instantiate(menuButtonPrefabContainer, Vector3.zero, Quaternion.identity);
            UnityEngine.UI.Button buttonClick = button.transform.GetComponent<UnityEngine.UI.Button>();
            button.transform.SetParent(uiAnnotationContent.transform, false);
            annotationButtonList.Add(button);
            Debug.Log("added call");
            buttonClick.onClick.AddListener(() => CreateAnnotationUI(currentIndex));//  menuManager.SetObjectToSpawn(objectIndex));
            currentIndex++;
        }
*/
        // foreach (Vector3 point in points) // *** used this for normal before only prod annotation particlRR
        /*     foreach (Annotation1 annotation in AnnotationsList)
             {
                 if (currentObject != null)
                 {
                     Destroy(currentObject);
                 }


                 // Instantiate a new object at the current point.
                 Vector3 transformedPoint = parentobj.transform.TransformPoint(annotation.position);
                 Vector3 pointdirection = transformedPoint - parentobj.transform.position;
                 currentObject = Instantiate(prefab, transformedPoint, Quaternion.identity, parentobj.transform);
                 Vector3 directionToCamera = Camera.main.transform.position - pointdirection; //transformedPoint; // parentobj.transform.position;
                 Quaternion targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
                 float animationDuration = 10f;
                 float duration = 6f;

                 yield return StartCoroutine(RotateObject(parentobj, currentObject.transform.position, Camera.main, animationDuration));


                 //  yield return null;
             }*/
        yield return null;

    }

    public void  CreateAnnotationUI(int currentIndex)
    {
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        Debug.Log("the point fetched / " + AnnotationsList[currentIndex].position);
        Vector3 transformedPoint = parentobj.transform.TransformPoint(AnnotationsList[currentIndex].position);
        Vector3 pointdirection = transformedPoint - parentobj.transform.position;
        currentObject = Instantiate(prefab, transformedPoint, Quaternion.identity, parentobj.transform);
        Vector3 directionToCamera = Camera.main.transform.position - pointdirection; //transformedPoint; // parentobj.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
        float animationDuration = 6.00f;

        StartCoroutine(RotateObject(parentobj, currentObject.transform.position, Camera.main, animationDuration));
    }

    public void CallSpawnRotation()
    {
        //  StartCoroutine(SpawnAndRotateObjects());
        CreateAnnotationUI(2);
    }
    private Vector3 point1;
  
    void OnDrawGizmos()
    {
        Vector3 pointOnCube = currentObject.transform.position; // the point on the cube's surface
        Vector3 PointCube = (pointOnCube - transform.position);
        Vector3 CamCube = (Camera.main.transform.position - transform.position);

        // Draw the vectors
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawLine(transform.position, transform.position + PointCube * 5); // scale for visibility

        Gizmos.color = UnityEngine.Color.green;
        Gizmos.DrawLine(transform.position, transform.position + CamCube * 3); // scale for visibility

        Gizmos.color = UnityEngine.Color.yellow;

        Gizmos.DrawLine(tryy.transform.position, Camera.main.transform.position);
    }

    // work parially decent need to complete the logic;
    /*   IEnumerator RotateObject(GameObject obj, Vector3 point, Camera camera, float duration)
       {
           // Calculate direction vector from the specified point to the camera
           Vector3 directionToCamera = (camera.transform.position - point).normalized;

           // Calculate the rotation to align the object's forward direction with the direction from the point to the camera
           Quaternion targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);

           // Quantize the rotation to multiples of 90 degrees
         //  targetRotation = QuantizeRotation(targetRotation);

           // Store the starting rotation of the object
           Quaternion startRotation = obj.transform.rotation;

           // Rotate the object gradually over time using Quaternion.Slerp
           for (float t = 0; t < 1.0f; t += Time.deltaTime / duration)
           {
               obj.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
               yield return null;
           }

           // Ensure the final rotation is exactly the target rotation
           obj.transform.rotation = targetRotation;

           yield return new WaitForSeconds(5);
       }*/

 

    Quaternion QuantizeRotation(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        //     if (euler.x > 90) { 
        euler.x = Mathf.Round(euler.x / 90) * 90;
        //    }
        //    if (euler.y > 90)
        //  {
        euler.y = Mathf.Round(euler.y / 90) * 90;

        //   }
        //  if (euler.z > 90)
        //  {
        euler.z = Mathf.Round(euler.z / 90) * 90;

        //   }
        return Quaternion.Euler(euler);
    }

    public GameObject tryy;
    public GameObject tryyprefab;

    Quaternion startRotation;
    Quaternion targetRotation;
    Vector3 directionToCamera;
    Vector3 ClosDir;
    Vector3 ClosestPoint;


    // angle method 

    public void callRotation()
    {
        StartCoroutine(RotateObject(parentobj, currentObject.transform.position, Camera.main, 6f));

    }

    public void OnCameraMove()
    {
        StartCoroutine(RotateObject(parentobj, currentObject.transform.position, Camera.main, 6f));

    }
    public IEnumerator RotateObject(GameObject obj, Vector3 pointOnSurface, UnityEngine.Camera camera, float duration)
    {
       

        Collider col = model.GetComponent<Collider>();
        ClosestPoint = col.ClosestPointOnBounds(camera.transform.position);
        Vector3 ClosestDirection = camera.transform.position - ClosestPoint;
      //  tryy = Instantiate(tryyprefab, ClosestPoint, Quaternion.identity);
        Vector3 PointCube = pointOnSurface - obj.transform.position;
        Vector3 CamCube = camera.transform.position - obj.transform.position;

        //  Debug.Log(" posurf " + pointOnSurface);
        Debug.Log(" PointCube " + PointCube);
        // Vector3 targetDir = PointCube - CamCube;
        Vector3 rotationAxis = Vector3.Cross(PointCube, CamCube);

        float angle = Vector3.Angle(PointCube, CamCube);
        Debug.Log(" float Angle value : " + angle);
        rotationAxis = rotationAxis.normalized;
        // rotationAxis = new Vector3 (0.00f, 1.00f, 0.00f);
        Debug.Log(rotationAxis);
        //   Quaternion targetRotation = Quaternion.AngleAxis(angle, rotationAxis); // previosuly used this
        //  Quaternion relativeRotation = targetRotation * Quaternion.Inverse(obj.transform.rotation);
        Quaternion CurrentRotation = obj.transform.rotation;
        float currentangle = Quaternion.Angle(CurrentRotation, Quaternion.identity);
        float angleDifference = angle - currentangle;
        // Debug.Log("Angle to ROtate: " + angleDifference);
        Quaternion objStartRotation = obj.transform.rotation;
        Quaternion relativeRotation = Quaternion.AngleAxis(angle, rotationAxis) * objStartRotation;

        // Quaternion relativeRotation = Quaternion.FromToRotation(CamCube, PointCube);

        PointCube = Quaternion.AngleAxis(angle, rotationAxis) * CamCube;
   
          float rotationDuration = 7.0f; // Duration in seconds
          float elapsedTime = 0f;
          while (elapsedTime < rotationDuration)
          {
              obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, relativeRotation, (elapsedTime / rotationDuration));
              elapsedTime += Time.deltaTime;
              yield return null;
          }
         // to Stop Rotation at exact point 
         //obj.transform.rotation = targetWorldRotation;
    }

    IEnumerator StartRotation(Vector3 RotationAxis, Quaternion targetWorldRotation, GameObject obj)
    {
        //  obj.transform.rotation = targetWorldRotation;
        //  obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetWorldRotation, 1f);
        float rotationDuration = 15.0f; // Duration in seconds
        float elapsedTime = 0f;
        Quaternion StartRotation = obj.transform.rotation;
        //  obj.transform.rotation = targetWorldRotation;

        while (elapsedTime < rotationDuration)
        {
            obj.transform.rotation = Quaternion.Slerp(StartRotation, targetWorldRotation, (elapsedTime / rotationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //   return null;

    }

}
// co routine // while 
// Rotate outside 