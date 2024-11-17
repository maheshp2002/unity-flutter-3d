using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using TMPro;
using Dummiesman;
using FlutterUnityIntegration;

public class SceneController : MonoBehaviour
{
    [SerializeField] private Material defaultMaterial; 
    public GameObject navigationPointPrefab;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Camera mainCamera;
    private GameObject selectedObject;
    private GameObject lastSelectedObject;
    private OBJLoader objLoader = new OBJLoader();
    public GameObject canvasUI;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleObjectSelection();
        HandleObjectManipulation();
        HandleCameraControls();
    }

    // Method to add a navigation point dynamically
    public void CreateNavigationPoint(string label, bool isSource, bool isDestination)
    {
        try
        {
            // Instantiate the navigation point prefab at the current camera position
            Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 2;
            GameObject newPoint = Instantiate(navigationPointPrefab, spawnPosition, Quaternion.identity);

            // Set metadata using the NavigationPoint script
            NavigationPoint navPoint = newPoint.GetComponent<NavigationPoint>();
            navPoint.SetData(label, isSource, isDestination);

            // Update the label text
            TextMeshPro labelComponent = newPoint.transform.Find("LabelText").GetComponent<TextMeshPro>();
            labelComponent.text = label;

            // Align the text above the navigation point
            labelComponent.transform.localPosition = new Vector3(0, 1.0f, 0);

            // Add to the list of spawned objects
            spawnedObjects.Add(newPoint);
            newPoint.name = label;

            Debug.Log($"Added Navigation Point: {label} (Source: {isSource}, Destination: {isDestination})");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create navigation point: {ex.Message}");
        }
    }

    public void ShowUnityUI()
    {
        if (canvasUI != null)
        {
            canvasUI.SetActive(true);
        }
    }

    private void HandleObjectSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (selectedObject != null)
                {
                    DeselectObject(selectedObject);
                }

                selectedObject = hit.collider.gameObject;

                if (selectedObject != lastSelectedObject)
                {
                    lastSelectedObject = selectedObject;
                }

                SelectObject(selectedObject);
            }
        }
    }

    private void SelectObject(GameObject obj)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow; // Highlight selected object
        }
    }

    private void DeselectObject(GameObject obj)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white; // Reset color
        }
    }

    private void HandleObjectManipulation()
    {
        if (selectedObject == null) return;

        float rotationSpeed = 100.0f;
        float scaleSpeed = 0.01f;
        float moveSpeed = 0.1f;

        // ----- Rotation -----
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Input.GetKey(KeyCode.Z))
        {
            // Rotate around Y-axis
            selectedObject.transform.Rotate(Vector3.up, -mouseX * rotationSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.X))
        {
            // Rotate around X-axis
            selectedObject.transform.Rotate(Vector3.right, mouseY * rotationSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.C))
        {
            // Rotate around Z-axis
            selectedObject.transform.Rotate(Vector3.forward, mouseX * rotationSpeed * Time.deltaTime, Space.World);
        }

        // ----- Flipping -----
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // Flip along X-axis
            selectedObject.transform.localScale = new Vector3(
                -selectedObject.transform.localScale.x,
                selectedObject.transform.localScale.y,
                selectedObject.transform.localScale.z
            );
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            // Flip along Y-axis
            selectedObject.transform.localScale = new Vector3(
                selectedObject.transform.localScale.x,
                -selectedObject.transform.localScale.y,
                selectedObject.transform.localScale.z
            );
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            // Flip along Z-axis
            selectedObject.transform.localScale = new Vector3(
                selectedObject.transform.localScale.x,
                selectedObject.transform.localScale.y,
                -selectedObject.transform.localScale.z
            );
        }


        // ----- Scaling -----
        // Uniform scale
        if (Input.GetKey(KeyCode.U))
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                selectedObject.transform.localScale += new Vector3(scaleSpeed, scaleSpeed, scaleSpeed);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                selectedObject.transform.localScale -= new Vector3(scaleSpeed, scaleSpeed, scaleSpeed);
            }
        }
        else
        {
            // Individual axis scaling
            // Scale X-axis
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.UpArrow))
            {
                selectedObject.transform.localScale += new Vector3(scaleSpeed, 0, 0);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.DownArrow))
            {
                selectedObject.transform.localScale -= new Vector3(scaleSpeed, 0, 0);
            }

            // Scale Y-axis
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.UpArrow))
            {
                selectedObject.transform.localScale += new Vector3(0, scaleSpeed, 0);
            }
            else if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.DownArrow))
            {
                selectedObject.transform.localScale -= new Vector3(0, scaleSpeed, 0);
            }

            // Scale Z-axis
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.RightArrow))
            {
                selectedObject.transform.localScale += new Vector3(0, 0, scaleSpeed);
            }
            else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftArrow))
            {
                selectedObject.transform.localScale -= new Vector3(0, 0, scaleSpeed);
            }
        }
        // Prevent negative or too small scaling
        selectedObject.transform.localScale = new Vector3(
            Mathf.Max(selectedObject.transform.localScale.x, 0.1f),
            Mathf.Max(selectedObject.transform.localScale.y, 0.1f),
            Mathf.Max(selectedObject.transform.localScale.z, 0.1f)
        );

        // ----- Movement -----
        if (Input.GetMouseButton(1))
        {
            float horizontal = Input.GetAxis("Mouse X") * moveSpeed;
            float vertical = Input.GetAxis("Mouse Y") * moveSpeed;
            float upDown = 0f;

            // Use Q and E keys to move up and down
            if (Input.GetKey(KeyCode.Q))
            {
                upDown = moveSpeed;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                upDown = -moveSpeed;
            }

            // Move object along X, Y, and Z axes
            if (Input.GetKey(KeyCode.M)) // Use M key to move up along Y-axis
            {
                selectedObject.transform.Translate(Vector3.up * moveSpeed, Space.World);
            }
            else if (Input.GetKey(KeyCode.N)) // Use N key to move down along Y-axis
            {
                selectedObject.transform.Translate(Vector3.down * moveSpeed, Space.World);
            }

            selectedObject.transform.Translate(new Vector3(horizontal, upDown, vertical), Space.World);
        }

        // ----- Delete Object -----
        if (Input.GetKeyDown(KeyCode.Delete) && selectedObject != null)
        {
            spawnedObjects.Remove(selectedObject); // Remove from list
            Destroy(selectedObject); // Remove from scene
            selectedObject = null; // Reset selected object
        }
    }

    private void HandleCameraControls()
    {
        // Camera view controls
        if (Input.GetKeyDown(KeyCode.T)) // Top view
        {
            mainCamera.transform.position = new Vector3(0, 10, 0);
            mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.B)) // Bottom view
        {
            mainCamera.transform.position = new Vector3(0, -10, 0);
            mainCamera.transform.rotation = Quaternion.Euler(-90, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.F)) // Front view
        {
            mainCamera.transform.position = new Vector3(0, 0, -10);
            mainCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.L)) // Left view
        {
            mainCamera.transform.position = new Vector3(-10, 0, 0);
            mainCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (Input.GetKeyDown(KeyCode.R)) // Right view
        {
            mainCamera.transform.position = new Vector3(10, 0, 0);
            mainCamera.transform.rotation = Quaternion.Euler(0, -90, 0);
        }

        // Camera movement controls
        if (Input.GetKey(KeyCode.W)) mainCamera.transform.Translate(Vector3.forward * Time.deltaTime);
        if (Input.GetKey(KeyCode.S)) mainCamera.transform.Translate(Vector3.back * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) mainCamera.transform.Translate(Vector3.left * Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) mainCamera.transform.Translate(Vector3.right * Time.deltaTime);
        if (Input.GetKey(KeyCode.Q)) mainCamera.transform.Rotate(Vector3.up, -1);
        if (Input.GetKey(KeyCode.E)) mainCamera.transform.Rotate(Vector3.up, 1);
        if (Input.GetKey(KeyCode.Space)) mainCamera.transform.Translate(Vector3.up * Time.deltaTime);
        if (Input.GetKey(KeyCode.LeftShift)) mainCamera.transform.Translate(Vector3.down * Time.deltaTime);
    }

    // Update the LoadModelFromUrl to handle base64 string
    public void LoadModelFromBase64(string base64String)
    {
        #if UNITY_WEBGL
        StartCoroutine(DownloadAndLoadModelFromBase64(base64String));
        #endif
    }

    private IEnumerator DownloadAndLoadModelFromBase64(string base64String)
    {
        byte[] modelData = Convert.FromBase64String(base64String);

        using (MemoryStream stream = new MemoryStream(modelData))
        {
            GameObject importedModel = objLoader.Load(stream);

            if (importedModel != null)
            {
                AssignDefaultShader(importedModel);
                importedModel.transform.position = Vector3.zero;

                // Send scale and rotation information back to Flutter
                string logMessages = $"Model Scale: {importedModel.transform.localScale}, Rotation: {importedModel.transform.rotation.eulerAngles}";
                UnityMessageManager.Instance.SendMessageToFlutter(logMessages); 

                #if UNITY_WEBGL
                    // Flip the model along the X-axis to correct the mirroring issue
                    importedModel.transform.localScale = new Vector3(
                        -importedModel.transform.localScale.x,
                        importedModel.transform.localScale.y,
                        importedModel.transform.localScale.z
                    );
                #endif

                #if UNITY_WEBGL
                    importedModel.transform.Rotate(0, 180, 0);
                #endif
                // Add BoxCollider or MeshCollider
                if (HasNegativeScale(importedModel))
                {
                    MeshCollider meshCollider = importedModel.AddComponent<MeshCollider>();
                    meshCollider.convex = true;
                }
                else
                {
                    BoxCollider boxCollider = importedModel.AddComponent<BoxCollider>();
                    boxCollider.center = importedModel.GetComponentInChildren<Renderer>().bounds.center - importedModel.transform.position;
                    boxCollider.size = CalculateBounds(importedModel);
                }

                importedModel.tag = "ImportedObject";
                spawnedObjects.Add(importedModel);

                // Send scale and rotation information back to Flutter
                string logMessage = $"Model Scale: {importedModel.transform.localScale}, Rotation: {importedModel.transform.rotation.eulerAngles}";
                UnityMessageManager.Instance.SendMessageToFlutter(logMessage); 
            }
            else
            {
                Debug.LogError("Failed to load the 3D model.");
            }
        }
        yield return null;
    }

    // Helper method to check if an object has any negative scale values
    private bool HasNegativeScale(GameObject obj)
    {
        Vector3 scale = obj.transform.localScale;
        return scale.x < 0 || scale.y < 0 || scale.z < 0;
    }

    // Helper method to calculate the bounds of the model for the BoxCollider
    private Vector3 CalculateBounds(GameObject model)
    {
        Bounds bounds = new Bounds(model.transform.position, Vector3.zero);
        MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // Use absolute values to avoid negative collider sizes
        return new Vector3(Mathf.Abs(bounds.size.x), Mathf.Abs(bounds.size.y), Mathf.Abs(bounds.size.z));
    }

    // Assigns a default white color shader to the imported model if it lacks one
    private void AssignDefaultShader(GameObject obj)
    {
        // Ensure default material is set
        if (defaultMaterial == null)
        {
            // Attempt to load the Standard shader if defaultMaterial is missing
            Shader standardShader = Shader.Find("Standard");
            if (standardShader != null)
            {
                defaultMaterial = new Material(standardShader);
            }
            else
            {
                Debug.LogError("Standard shader not found. Please assign a default material.");
                return;
            }
        }

        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            if (renderer.material == null)
            {
                renderer.material = defaultMaterial;
            }
        }
    }

    public void HideMeshRenderer()
    {
        if (selectedObject != null)
        {
            MeshRenderer renderer = selectedObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = !renderer.enabled;
            }
        }
    }

    public void ExportScene(string path)
    {
        SceneData sceneData = new SceneData();
        foreach (GameObject obj in spawnedObjects)
        {
            SceneObjectData objData = new SceneObjectData
            {
                position = obj.transform.position,
                rotation = obj.transform.rotation,
                scale = obj.transform.localScale,
                type = obj.tag
            };
            sceneData.objects.Add(objData);
        }

        string json = JsonUtility.ToJson(sceneData);

    #if UNITY_WEBGL
        string fileName = "sceneData.json";
        Application.ExternalEval($"var blob = new Blob([JSON.stringify({json})], {{ type: 'application/json' }}); " +
                                 $"var link = document.createElement('a'); " +
                                 $"link.href = URL.createObjectURL(blob); " +
                                 $"link.download = '{fileName}'; " +
                                 $"link.click();");
    #else
        System.IO.File.WriteAllText(path, json);
    #endif
    }

    [System.Serializable]
    public class PointData
    {
        public string label;
        public bool isSource;
        public bool isDestination;
        public float[] position;
    }

    [System.Serializable]
    public class SceneData
    {
        public List<SceneObjectData> objects = new List<SceneObjectData>();
    }

    [System.Serializable]
    public class SceneObjectData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public string type;
    }
}
