using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDetectTest : MonoBehaviour
{
    [SerializeField] private Transform[] _cameraPositions;
    [SerializeField] private bool _canCheck = false;
    [SerializeField] private MeshFilter m_Filter;

    [SerializeField] private MeshSlicer _slicer;
    [SerializeField] private Material _nonWireframeMaterial;

    [SerializeField] private Camera _checkMeshCamera;
    [SerializeField] private Transform _cameraParent;
    [SerializeField] private int _currentIndex = 0;

    private List<GameObject> _slicedMeshes;
    private void Start()
    {
        _checkMeshCamera.transform.parent = _cameraParent;
    }

    [ContextMenu("Slice")]
    public void Slice()
    {
        _slicedMeshes = _slicer.SliceMesh(m_Filter, _nonWireframeMaterial);

    }
    [ContextMenu("Next")]
    public void NextPoint()
    {
        var pos = _cameraPositions[_currentIndex].localPosition;
        var rot = _cameraPositions[_currentIndex].localRotation;
        ++_currentIndex;

        for (int i = 0; i < _slicedMeshes.Count; ++i)
        {

            var mf = _slicedMeshes[i].GetComponent<MeshFilter>();
            var render = mf.GetComponent<MeshRenderer>();
            render.material = _nonWireframeMaterial;

            if (IsMeshInCamera(mf, pos, rot))
            {
                //mf.GenerateUV(_checkMeshCamera);
                //var render = mf.GetComponent<MeshRenderer>();
                //render.material = _nonWireframeMaterial;
                //render.material.color = Color.white;
                //render.material.SetTexture("_BaseMap", camData.Texture);

                //mf.name = $"Handled_{mf.name}";
                //++handledCount;


                render.material.color = Color.green;
            }
            else
            {
                render.material.color = Color.red;
            }
        }
    }


    private bool IsMeshInCamera(MeshFilter mFilter, Vector3 position, Quaternion rotation)
    {
        _checkMeshCamera.transform.position = position;
        _checkMeshCamera.transform.rotation = rotation;

        var camPlanes = GeometryUtility.CalculateFrustumPlanes(_checkMeshCamera);

        var bounds = mFilter.GetComponent<MeshRenderer>().localBounds;
        Vector3[] points = new Vector3[8];
        points[0] = bounds.center + new Vector3(-bounds.size.x / 2, -bounds.size.y / 2, -bounds.size.z / 2);
        points[1] = bounds.center + new Vector3(-bounds.size.x / 2, bounds.size.y / 2, -bounds.size.z / 2);
        points[2] = bounds.center + new Vector3(bounds.size.x / 2, bounds.size.y / 2, -bounds.size.z / 2);
        points[3] = bounds.center + new Vector3(bounds.size.x / 2, -bounds.size.y / 2, -bounds.size.z / 2);
        points[4] = bounds.center + new Vector3(-bounds.size.x / 2, -bounds.size.y / 2, bounds.size.z / 2);
        points[5] = bounds.center + new Vector3(-bounds.size.x / 2, bounds.size.y / 2, bounds.size.z / 2);
        points[6] = bounds.center + new Vector3(bounds.size.x / 2, bounds.size.y / 2, bounds.size.z / 2);
        points[7] = bounds.center + new Vector3(bounds.size.x / 2, -bounds.size.y / 2, bounds.size.z / 2);

        var listcolliders = new List<SphereCollider>();
        foreach (var point in points)
        {
            var go = new GameObject("point");

            var tr = go.transform;

            tr.parent = mFilter.transform;
            tr.localPosition = point;
            var col = go.AddComponent<SphereCollider>();
            listcolliders.Add(col);

            col.radius = 0.01f;
        }

        int countCollidersInFrustrum = 0;
        foreach (var col in listcolliders)
        {
            if (GeometryUtility.TestPlanesAABB(camPlanes, col.bounds))
                countCollidersInFrustrum++;

            Destroy(col.gameObject);
        }

        if (countCollidersInFrustrum == 8)
            return true;
        else
            return false;
    }
}
