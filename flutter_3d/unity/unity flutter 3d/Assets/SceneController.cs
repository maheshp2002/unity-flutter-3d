using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using Dummiesman;

public class SceneController : MonoBehaviour
{
    public GameObject startPrefab;
    public GameObject destinationPrefab;
    public GameObject navigationPrefab;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Camera mainCamera;
    private GameObject selectedObject;
    private GameObject lastSelectedObject;
    private OBJLoader objLoader = new OBJLoader();

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
        obj.GetComponent<Renderer>().material.color = Color.yellow; // Highlight selected object
    }

    private void DeselectObject(GameObject obj)
    {
        obj.GetComponent<Renderer>().material.color = Color.white; // Reset color
    }

    private void HandleObjectManipulation()
    {
        if (selectedObject == null) return;

        // Rotate object
        if (Input.GetKey(KeyCode.Z))
        {
            float rotationSpeed = 100.0f;
            float horizontal = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            selectedObject.transform.Rotate(Vector3.up, -horizontal, Space.World);
        }

        // Scale object
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.UpArrow))
        {
            float scaleSpeed = 0.01f;
            if (selectedObject.CompareTag("NavigationLine"))
            {
                selectedObject.transform.localScale += new Vector3(scaleSpeed, 0, 0); // Scale length (X-axis)
            }
            else
            {
                selectedObject.transform.localScale += new Vector3(scaleSpeed, scaleSpeed, scaleSpeed); // Uniform scaling
            }
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.DownArrow))
        {
            float scaleSpeed = 0.01f;
            if (selectedObject.CompareTag("NavigationLine"))
            {
                selectedObject.transform.localScale -= new Vector3(scaleSpeed, 0, 0); // Scale length (X-axis)
            }
            else
            {
                selectedObject.transform.localScale -= new Vector3(scaleSpeed, scaleSpeed, scaleSpeed); // Uniform scaling
            }
        }

        // Scale width (Z-axis) of Navigation Line
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.RightArrow))
        {
            float scaleSpeed = 0.01f;
            if (selectedObject.CompareTag("NavigationLine"))
            {
                selectedObject.transform.localScale += new Vector3(0, 0, scaleSpeed); // Scale width (Z-axis)
            }
        }
        else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftArrow))
        {
            float scaleSpeed = 0.01f;
            if (selectedObject.CompareTag("NavigationLine"))
            {
                selectedObject.transform.localScale -= new Vector3(0, 0, scaleSpeed); // Scale width (Z-axis)
            }
        }

        // Move object
        if (Input.GetMouseButton(1))
        {
            float moveSpeed = 0.1f;
            float horizontal = Input.GetAxis("Mouse X") * moveSpeed;
            float vertical = Input.GetAxis("Mouse Y") * moveSpeed;
            selectedObject.transform.Translate(new Vector3(horizontal, 0, vertical), Space.World);
        }

        // Delete object
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

    // Function to import 3D object from Flutter
    public void Import3DObject(string base64Model)
    {
        // Convert the base64 string to byte array
        byte[] modelData = System.Convert.FromBase64String(base64Model);

        // Save the data to a temporary .obj file
        string tempPath = System.IO.Path.Combine(Application.persistentDataPath, "tempModel.obj");
        System.IO.File.WriteAllBytes(tempPath, modelData);

        // Load the .obj file using OBJLoader
        GameObject importedModel = objLoader.Load(tempPath); // Correct usage of Load method

        // Check if the model was loaded correctly
        if (importedModel != null)
        {
            importedModel.transform.position = Vector3.zero;  // Set the initial position
            importedModel.AddComponent<BoxCollider>();        // Add collider for interaction if needed
            spawnedObjects.Add(importedModel);                // Add the object to your spawned objects list
        }
        else
        {
            Debug.LogError("Failed to load the 3D model.");
        }
    }

    public void AddStartPoint()
    {
        GameObject startPoint = Instantiate(startPrefab);
        spawnedObjects.Add(startPoint);
    }

    public void AddDestinationPoint()
    {
        GameObject destinationPoint = Instantiate(destinationPrefab);
        spawnedObjects.Add(destinationPoint);
    }

    public void AddNavigationLine(string json)
    {
        var data = JsonUtility.FromJson<NavigationLineData>(json);
        GameObject lineObject = Instantiate(navigationPrefab);

        lineObject.transform.position = new Vector3(data.start[0], data.start[1], data.start[2]);
        Vector3 direction = new Vector3(data.end[0], data.end[1], data.end[2]) - lineObject.transform.position;
        float length = direction.magnitude;
        lineObject.transform.localScale = new Vector3(length, 1, 1);
        lineObject.transform.LookAt(new Vector3(data.end[0], data.end[1], data.end[2]));
        lineObject.tag = "NavigationLine";
        lineObject.AddComponent<BoxCollider>();
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
        // Prepare the JSON data
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

        // Use JavaScript to trigger a file download in WebGL
        string fileName = "sceneData.json";
        Application.ExternalEval($"var blob = new Blob([JSON.stringify({json})], {{ type: 'application/json' }}); " +
                                 $"var link = document.createElement('a'); " +
                                 $"link.href = URL.createObjectURL(blob); " +
                                 $"link.download = '{fileName}'; " +
                                 $"link.click();");
    }

    [System.Serializable]
    public class NavigationLineData
    {
        public float[] start;
        public float[] end;
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
