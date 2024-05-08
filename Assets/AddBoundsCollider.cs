using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBoundsCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AddBoundingBox(model);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public GameObject model;
    public void AddBoundingBox(GameObject model)
    {
        // code 1 adds bounding box well for model taking rots as 0 . 
        /*  Debug.Log(" bounding box called");
          Renderer[] childRenderers = model.GetComponentsInChildren<Renderer>();
          Bounds combinedBounds = new Bounds(model.transform.position, Vector3.zero);

          foreach (Renderer childRenderer in childRenderers)
          {
              Bounds childBounds = childRenderer.bounds;
              combinedBounds.Encapsulate(childBounds.min);
              combinedBounds.Encapsulate(childBounds.max);
          }



          BoxCollider boxCollider = model.AddComponent<BoxCollider>();
          boxCollider.center = combinedBounds.center;

          boxCollider.size = combinedBounds.size;*/

        // cc 2
        /*        Debug.Log("Bounding box called");
                Renderer[] childRenderers = model.GetComponentsInChildren<Renderer>();
                Bounds combinedBounds = new Bounds();

                bool boundsInitialized = false;

                foreach (Renderer childRenderer in childRenderers)
                {
                    if (!boundsInitialized)
                    {
                        // Initialize the combinedBounds with the first renderer's bounds
                        combinedBounds = new Bounds(childRenderer.bounds.center, childRenderer.bounds.size);
                        boundsInitialized = true;
                    }
                    else
                    {
                        // Encapsulate the current bounds of each child renderer
                        combinedBounds.Encapsulate(childRenderer.bounds.min);
                        combinedBounds.Encapsulate(childRenderer.bounds.max);
                    }
                }

                // Adding BoxCollider to the model
                BoxCollider boxCollider = model.AddComponent<BoxCollider>();
                boxCollider.center = model.transform.InverseTransformPoint(combinedBounds.center); // Convert bounds center from world space to local space
                boxCollider.size = model.transform.InverseTransformVector(combinedBounds.size); // Adjust the size according to the local transformation*/

        // cc 3
        /*
        Debug.Log("Bounding box called");
        Renderer[] childRenderers = model.GetComponentsInChildren<Renderer>();
        Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;

        // Calculate bounds in local space of the model
        foreach (Renderer renderer in childRenderers)
        {
            if (!hasBounds)
            {
                combinedBounds = TransformBoundsToLocal(renderer, model.transform);
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(TransformBoundsToLocal(renderer, model.transform));
            }
        }

        // Add or update the BoxCollider
        BoxCollider boxCollider = model.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = model.AddComponent<BoxCollider>();
        }
        boxCollider.center = combinedBounds.center;
        boxCollider.size = combinedBounds.size;

        // Helper method to transform renderer bounds to local space of the model
        Bounds TransformBoundsToLocal(Renderer renderer, Transform transform)
        {
            Bounds bounds = renderer.bounds;
            bounds.SetMinMax(transform.InverseTransformPoint(bounds.min), transform.InverseTransformPoint(bounds.max));
            return bounds;
        }
*/

        /*     Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
             Bounds bounds = new Bounds(model.transform.position, Vector3.zero);

             // Calculate the combined bounds
             foreach (Renderer renderer in renderers)
             {
                 bounds.Encapsulate(renderer.bounds);
             }

             // Now the bounds are in world space, convert to local space
             bounds.center = model.transform.InverseTransformPoint(bounds.center);
             bounds.size = model.transform.InverseTransformVector(bounds.size);

             // Apply to the collider
             BoxCollider collider = model.AddComponent<BoxCollider>();
             collider.center = bounds.center;
             collider.size = bounds.size;*/


        // try 4 // does not work when parent has rots
        /*
        Quaternion originalRotation = model.transform.rotation;
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

        // Convert bounds to local space
        Vector3 localCenter = bounds.center - model.transform.position;
        bounds.center = localCenter;

        // Now, apply the correct bounds to the BoxCollider
        BoxCollider collider = model.AddComponent<BoxCollider>();
        collider.center = model.transform.InverseTransformPoint(bounds.center);
        collider.size = model.transform.InverseTransformVector(bounds.size);

        // Reapply the original rotation to the model
        model.transform.rotation = originalRotation;
*/

        Quaternion originalParentRotation = model.transform.parent.rotation;
        Vector3 originalParentPosition = model.transform.parent.position;

        // Save the original rotation of the model
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
        BoxCollider collider =  model.AddComponent<BoxCollider>();
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
}
