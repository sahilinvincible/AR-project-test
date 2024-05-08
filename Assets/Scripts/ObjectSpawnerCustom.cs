using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using System.Collections;
using UnityEditor.Rendering;/*
using System.Collections.Generic;
using UnityEngine;
using System.Collections;*/
using System.Drawing;
using TMPro;
using UnityEngine.Networking;
using static Unity.VisualScripting.AnnotationUtility;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;
using GLTFast;
using System.IO;
using System.Threading.Tasks;
using Image = UnityEngine.UI.Image;
using static Unity.Tutorials.Core.Editor.TutorialWelcomePage;
using Unity.Android.Types;



// Trial 2      //////                     ///////                /////                         /////                       //////                     /////

[System.Serializable]
public class Annotation
{
    public Vector3 position;
    public string annotationID;
    public string title;
    public string description;
    public string _id;
}

[System.Serializable]
public class AnnotationArray
{
    public Annotation[] items; // This will be used to match the JSON structure for deserialization
}

[System.Serializable]
public class AllProducts
{

    public string _id;
    public string user;
    public string name;
    public string description;
    public List<string> images;
    public string modelFile;

}


[System.Serializable]
public class ProductsArray
{
    public AllProducts[] items; // This will be used to match the JSON structure for deserialization
}


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
        //  string allProductsUrl = "http://192.168.0.159:3001/api/products/public/660ba5d05e45ee5c1a47801a";
        string allProductsUrl = baseUrl + "/api/products/public/660ba5d05e45ee5c1a47801a";

        StartCoroutine(GetProducts(allProductsUrl));
        //   string annotationUrl = "http://192.168.0.124:3001/api/products/661112256378f1ef89a88790/annotations";
        //  StartCoroutine(GetProductAnnotations(annotationUrl));
        //   Debug.Log("Annotation url " + annotationUrl);
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


    public Vector3 targetEulerAngles; //
    public float animationDuration; // Adjust as needed
    public GameObject prefab; // Prefab to instantiate.
                              //  public List<Vector3> points = new List<Vector3>(); // List of points to spawn and rotate objects at.
    public List<string> title = new List<string>();
    //public List<Annotation> annotations; //= new List<Annotation>();

    [SerializeField]
    public List<Annotation> AnnotationsList;//{ get; private set; } // = new List<Annotation>();

    [SerializeField]
    public List<AllProducts> AllProductsList; // = new List<AllProducts>();
    private GameObject currentObject; // Currently active object.
    public GameObject model;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public string productId = "660bcdbb96d5eaf0d310dfa8";

    public GameObject prefabContainer;
    public GameObject menuButtonPrefabContainer;
    public GameObject annotationsMenuButtonPrefabContainer;
    public GameObject uiMenuContent;
    public GameObject baseModel;
    public ARSampleMenuManagerCustom menuManager;
    [SerializeField]
    private List<GameObject> datalist;
    private List<GameObject> annotationButtonList;
    public GameObject spawnObjectdata;
    private string baseUrl = "http://192.168.0.124:3001";
    // private string baseUrl =  "https://three-js-dashboard.onrender.com";
    public GameObject uiAnnotationContent;


    private void Start()
    {

        lastPosition = Camera.main.transform.position;
        lastRotation = Camera.main.transform.rotation;

    }
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

        // if (newObject != null) { Debug.Log(newObject.transform.position + " transPos"); }

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

                //  newObject = Instantiate(m_ObjectPrefabs[objectIndex]);
                newObject = Instantiate(baseModel);
                //  AddBoundingBox(newObject);
                //  ButtonProductData Prefabdata = m_ObjectPrefabs[objectIndex].GetComponent<ButtonProductData>();
                ButtonProductData objData = newObject.GetComponent<ButtonProductData>();
                objData = GetButtonProductDataAtIndex(objectIndex);
                //     GameObject buttonData = spawnObjectdata;

                //  objData = GetButtonProductDataButton(buttonData);

                // AllProducts allProductsData = data.allProducts;
                // string modelFilePath = Prefabdata.allProducts.modelFile;
                string modelFilePath = objData.allProducts.modelFile;
                string productId = objData.allProducts._id;
                //   Debug.Log("modelFile" + buttonData.allProducts.modelFile);
                //   AllProducts allProductsData = Prefabdata.allProducts;

                // Get the modelFile value
                //   string modelFilePath = allProductsData.modelFile;

                /*ButtonProductData buttonData = prefabcontainer.GetComponent<ButtonProductData>();
                buttonData.allProducts = data.AllProducts;



                ButtonProductData buttonData1 = button.GetComponent<ButtonProductData>();
                Debug.Log(buttonData1.name + " button name");
                buttonData1.allProducts = AllProducts;

                string modelFilePath = allProductsData.modelFile;*/
                //  Debug.Log(modelFilePath + " modelfilepath");
                //     string url = "http://192.168.0.159:3001" + modelFilePath;
                string url = baseUrl + modelFilePath;
                Debug.Log("url " + url);
                //   StartCoroutine(DownloadModelCoroutine( "http://192.168.0.124:3001/uploads/modelFile-1712049595230-836896281.glb" , newObject)); // iphone
                //  StartCoroutine(DownloadModelCoroutine("http://192.168.0.124:3001/uploads/modelFile-1712394564440-436215008.glb", newObject)); // watch 
                StartCoroutine(DownloadModelCoroutine(url, newObject)); // watch 
                string annotationUrl = baseUrl + "/api/products/" + productId + "/annotations";
                // string annotationUrl = "http://192.168.0.159:3001/api/products/661112256378f1ef89a88790/annotations";
              //  string annotationUrl = baseUrl + "/api/products/661111456378f1ef89a88754/annotations"; // for Watch
                StartCoroutine(GetProductAnnotations(annotationUrl)); //http://192.168.0.159/
                Debug.Log("Annotation url " + annotationUrl);

                /*  if (newObject != null)
                  {
                      Debug.Log("IS TRANSparency called /");
                      ChangeTransparency(newObject, alphaValue);
                  }*/
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
        Debug.Log("yes called transparency");
    }

    public void SpawnObject()
    {

        if (SpawnedObject == null)
        {
            // Debug.Log(newObject.transform.position + " Initial pos");
            // SpawnedObject = Instantiate(m_ObjectPrefabs[objectIndex]);
            SpawnedObject = Instantiate(baseModel);
            _ = AddDownloadedModelToPrefabsAsync(savePath, SpawnedObject);
            SpawnedObject.transform.position = newObject.transform.position;
            SpawnedObject.transform.rotation = newObject.transform.rotation;
            Destroy(newObject);
            Debug.Log(" newobject destroyed");
            newObject = SpawnedObject;
            Debug.Log(newObject.transform.position + " pos before");
            letTranslate = false;
            // StartCoroutine(Rotateobjects()); // JUS ADDED 
            //  StartCoroutine(SpawnAndRotateObjects());
        }

    }

    public List<Vector3> points = new List<Vector3>(); // List of points to spawn and rotate objects at.
                                                       //   private GameObject currentObject; // Currently active object.



    IEnumerator GetProducts(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
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
                ProcessProducts(jsonResponse);
            }
        }
    }
    public GameObject addedcube;

    void ProcessProducts(string jsonResponse)
    {
        Debug.Log("iscalled");
        string jsonToParse = $"{{\"items\":{jsonResponse}}}";
        ProductsArray productsArray = JsonUtility.FromJson<ProductsArray>(jsonToParse);
        Debug.Log("now?");
        // AnnotationsList.Clear();

        int currentIndex = 0;
        foreach (var AllProducts in productsArray.items)
        {

            int objectIndex = currentIndex;
            // Debug.Log("is added ?");
            AllProductsList.Add(AllProducts);

            //GameObject prefabcontainer = Instantiate(prefabContainer, Vector3.zero, Quaternion.identity); // check spelling of prefab conatainer . changed it 
            // objectPrefabs.Add(prefabContainer);
            // datalist.Add(prefabcontainer);

            // ButtonProductData buttonData = prefabcontainer.GetComponent<ButtonProductData>();
            //  ButtonProductData buttonData = prefabcontainer.GetComponent<ButtonProductData>();
            //  buttonData.allProducts = AllProducts;
            //  Debug.Log("modelFile" + buttonData.allProducts.modelFile);


            var button = Instantiate(menuButtonPrefabContainer, Vector3.zero, Quaternion.identity);
            button.transform.SetParent(uiMenuContent.transform, false);
            datalist.Add(button);
            objectPrefabs.Add(button);

            ButtonProductData buttonData1 = button.GetComponent<ButtonProductData>();
            Debug.Log(buttonData1.name + " button name");
            buttonData1.allProducts = AllProducts;
            button.name = buttonData1.allProducts.name;
            UnityEngine.UI.Button buttonClick = button.transform.GetComponent<UnityEngine.UI.Button>();
            // Debug.Log(currentIndex + "currentInndex");
            // Add a click event listener to each button
            buttonClick.onClick.AddListener(() => menuManager.SetObjectToSpawn(objectIndex));
            // buttonClick.onClick.AddListener(() => menuManager.SetObjectToSpawnObj(button));
            currentIndex++;

            Transform objectIconTransform = button.transform.GetChild(0);
            GameObject objectIcon = objectIconTransform.gameObject;
            //  Debug.Log("images" + AllProducts.images);

            if (AllProducts.images != null && AllProducts.images.Count > 0)
            {
                // string imageUrl = "http://192.168.0.159:3001" + AllProducts.images[0]; // Use the first image in the list
                string imageUrl = baseUrl + AllProducts.images[0]; // Use the first image in the list
                Debug.Log("IMAGEURL " + imageUrl);
                StartCoroutine(DownloadImage(imageUrl, objectIcon)); //http://192.168.0.159/

            }
        }



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
                savePath = Path.Combine(Application.persistentDataPath, modelName);

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
                ChangeTransparency(go, alphaValue);
                // StartCoroutine(SpawnAndRotateObjects());
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


    // public GameObject button1;
    // private Image targetImage;
    IEnumerator DownloadImage(string url, GameObject button)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            // Send the request and wait for the desired page
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error while downloading image: {webRequest.error}");
            }
            else
            {
                Image targetImage = button.GetComponent<Image>();
                //  Debug.Log("targetImage" + targetImage);
                if (targetImage != null)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                    Sprite spriteToAssign = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    targetImage.sprite = spriteToAssign;
                }
                else
                {
                    Debug.LogError("No Image component found on buttonGameObject.");
                }
            }
        }
    }

    private Bounds combinedBounds;

    public void AddBoundingBox(GameObject model)
    {
        Quaternion originalParentRotation = model.transform.parent.rotation;
        Vector3 originalParentPosition = model.transform.parent.position;
        Quaternion originalModelRotation = model.transform.rotation;

        // Reset the parent's and model's rotation to identity to calculate the bounds
        model.transform.parent.rotation = Quaternion.identity;
        model.transform.parent.position = Vector3.zero;
        model.transform.rotation = Quaternion.identity;
        Debug.Log(model.transform.parent.rotation);
        Debug.Log(model.transform.parent.position);
        Debug.Log(model.transform.rotation);
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

        Debug.Log(model.transform.parent.rotation);
        Debug.Log(model.transform.parent.position);
        Debug.Log(model.transform.rotation);



    }



    public ButtonProductData GetButtonProductDataAtIndex(int index)
    {
        if (index >= 0 && index < datalist.Count)
        {
            ButtonProductData data = datalist[index].GetComponent<ButtonProductData>();
            if (data != null)
            {
                // Here you can use or return the data
                return data;
            }
            else
            {
                Debug.LogError("ButtonProductData component not found on the object at index " + index);
            }
        }
        else
        {
            Debug.LogError("Index " + index + "is out of range.");
        }
        return null;
    }


    public ButtonProductData GetButtonProductDataButton(GameObject button)
    {

        ButtonProductData data = button.GetComponent<ButtonProductData>();
        if (data != null)
        {
            // Here you can use or return the data
            return data;
        }
        else
        {
            Debug.LogError("ButtonProductData component not found on the object at index ");
        }

        return null;
    }

    private void OnDrawGizmos()
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
        Gizmos.DrawLine(annotationIndicator.transform.position, Camera.main.transform.position);
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

        /*foreach (var annotation in AnnotationsList)
        {
            //Instantiate(prefab, annotation.position, Quaternion.identity);
            Debug.Log("pos " + annotation.position + "title " + annotation.title + "Desc" + annotation.description);
         //   var button = Instantiate(menuButtonPrefabContainer, Vector3.zero, Quaternion.identity);
         //   UnityEngine.UI.Button buttonClick = button.transform.GetComponent<UnityEngine.UI.Button>();
            //  buttonClick.onClick.AddListener(() => menuManager.SetObjectToSpawn(objectIndex));
        }
*/
        int currentIndex = 0;
        foreach (var annotation in AnnotationsList)
        {
            Debug.Log(" buttons are instantiated");
            int index = currentIndex;
            var button = Instantiate(annotationsMenuButtonPrefabContainer, Vector3.zero, Quaternion.identity);
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


   /* IEnumerator SpawnAndRotateObjects()
    {

        foreach (var annotation in AnnotationsList)
        {
            var instantiatedObject = newObject.transform.GetChild(newObject.transform.childCount - 1).gameObject;
            newObject.transform.rotation = Quaternion.identity;
            Vector3 transformedPoint = newObject.transform.TransformPoint(annotation.position);

            GameObject obj = Instantiate(prefab, transformedPoint, newObject.transform.rotation);
            // Debug.Log("pos " + annotation.position + "title " + annotation.title + "Desc" + annotation.description);
            obj.transform.SetParent(newObject.transform, true);
        }
        *//*  foreach (var annotation in AnnotationsList)
       {
           GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity); // Instantiate at world origin
           obj.transform.SetParent(newObject.transform, true); // Set the parent, false to keep local position, rotation, and scale
           obj.transform.localPosition = annotation.position; // Now set the local position directly
          //  obj.transform.localRotation = Quaternion.Euler(annotation.position.rotation); // If you have rotation data, apply it here
         // Additional setup for obj based on annotation details...
       }*//*



        // foreach (Vector3 point in points)
        foreach (Annotation annotation in AnnotationsList)
        {
            if (currentObject != null)
            {
                Destroy(currentObject);
            }

            Vector3 transformedPoint = newObject.transform.TransformPoint(annotation.position);
            Vector3 pointdirection = transformedPoint - newObject.transform.position;
            currentObject = Instantiate(prefab, transformedPoint, Quaternion.identity, newObject.transform);
            Vector3 directionToCamera = Camera.main.transform.position - pointdirection; //transformedPoint; // newObject.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            float animationDuration = 10f;
            float duration = 6f;

            // yield return null;
            yield return StartCoroutine(RotateObject(newObject, currentObject.transform.position, Camera.main, animationDuration));

        }
    }*/

    public void InstantiateAnnotationPoint(int currentIndex)
    {

        // StartCoroutine(CheckCameraPosition());
        Debug.Log("hello onclick InstantiateAnnotationPoint" + currentIndex);
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        Debug.Log("the point fetched / " + AnnotationsList[currentIndex].position);
        Vector3 transformedPoint = newObject.transform.TransformPoint(AnnotationsList[currentIndex].position);
        Vector3 pointdirection = transformedPoint - newObject.transform.position;
        currentObject = Instantiate(prefab, transformedPoint, Quaternion.identity, newObject.transform);
        Vector3 directionToCamera = Camera.main.transform.position - pointdirection; //transformedPoint; // newObject.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
        float animationDuration = 6.00f;
        titleText.text = AnnotationsList[currentIndex].title;
        descriptionText.text = AnnotationsList[currentIndex].description;
        StartCoroutine(RotateObject(newObject, currentObject.transform.position, Camera.main, animationDuration));
        StartCoroutine(CheckCameraPosition());

    }


    public void callRotation()
    {
        StartCoroutine(RotateObject(newObject, currentObject.transform.position, Camera.main, 6f));

    }

    public void OnCameraMove()
    {
        StartCoroutine(RotateObject(newObject, currentObject.transform.position, Camera.main, 6f));

    }

    public GameObject box;
    Vector3 ClosestPoint;
    GameObject annotationPrefab;
    public GameObject annotationIndicator;

    IEnumerator RotateObject(GameObject obj, Vector3 buttonWorldPosition, Camera camera, float duration)
    {
        Collider col = model.GetComponent<Collider>();
        ClosestPoint = col.ClosestPointOnBounds(camera.transform.position);
        Vector3 ClosestDirection = camera.transform.position - ClosestPoint;
        annotationIndicator = Instantiate(annotationPrefab, ClosestPoint, Quaternion.identity);
        Vector3 PointCube = buttonWorldPosition - obj.transform.position;
        Vector3 CamCube = camera.transform.position - obj.transform.position;

        Debug.Log(" PointCube " + PointCube);
        Vector3 rotationAxis = Vector3.Cross(PointCube, CamCube);

        float angle = Vector3.Angle(PointCube, CamCube);
        Debug.Log(" float Angle value : " + angle);
        rotationAxis = rotationAxis.normalized;
        Debug.Log(rotationAxis);
        Quaternion CurrentRotation = obj.transform.rotation;
        float currentangle = Quaternion.Angle(CurrentRotation, Quaternion.identity);
        float angleDifference = angle - currentangle;
        Quaternion objStartRotation = obj.transform.rotation;
        Quaternion relativeRotation = Quaternion.AngleAxis(angle, rotationAxis) * objStartRotation;


        PointCube = Quaternion.AngleAxis(angle, rotationAxis) * CamCube;
        yield return StartCoroutine(StartRotation(rotationAxis, relativeRotation, obj));
    }

    IEnumerator StartRotation(Vector3 RotationAxis, Quaternion targetWorldRotation, GameObject obj)
    {
        float rotationDuration = 6.0f; // Duration in seconds
        float elapsedTime = 0f;
        Quaternion StartRotation = obj.transform.rotation;
        while (elapsedTime < rotationDuration)
        {
            obj.transform.rotation = Quaternion.Slerp(StartRotation, targetWorldRotation, (elapsedTime / rotationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public bool check;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
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



}


