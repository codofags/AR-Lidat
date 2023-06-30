using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System.Threading.Tasks;

public class MeshSlicer : MonoBehaviour
{
    [SerializeField] private MeshFilter _baseMesh;

    [SerializeField] private float _voxelSizeInMeters = 1;

    [ContextMenu("Slice")]
    public List<GameObject> SliceMesh(MeshFilter meshForSlice, Material material)
    {
        _baseMesh = meshForSlice;

        var transform = _baseMesh.transform;
        var renderer = _baseMesh.GetComponent<MeshRenderer>();
        var bounds = renderer.bounds;

        var right = bounds.center + transform.right * bounds.size.x / 2;
        var left = bounds.center + transform.right * -1 * bounds.size.x / 2;

        var top = bounds.center + transform.up * bounds.size.y / 2;
        var bottom = bounds.center + transform.up * -1 * bounds.size.y / 2;

        var forward = bounds.center + transform.forward * bounds.size.z / 2;
        var backward = bounds.center + transform.forward * -1 * bounds.size.z / 2;


        //var lS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //lS.transform.position = left;
        //lS.transform.localScale = Vector3.one * 0.05f;
        //lS.GetComponent<MeshRenderer>().material.color = Color.blue;

        //var rS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //rS.transform.position = right;
        //rS.transform.localScale = Vector3.one * 0.05f;
        //rS.GetComponent<MeshRenderer>().material.color = Color.red;


        //var tS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //tS.transform.position = top;
        //tS.transform.localScale = Vector3.one * 0.1f;
        //tS.GetComponent<MeshRenderer>().material.color = Color.red;

        //var bS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //bS.transform.position = bottom;
        //bS.transform.localScale = Vector3.one * 0.1f;
        //bS.GetComponent<MeshRenderer>().material.color = Color.blue;


        //var fS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //fS.transform.position = forward;
        //fS.transform.localScale = Vector3.one * 0.2f;
        //fS.GetComponent<MeshRenderer>().material.color = Color.red;


        //var baS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //baS.transform.position = backward;
        //baS.transform.localScale = Vector3.one * 0.2f;
        //baS.GetComponent<MeshRenderer>().material.color = Color.blue;

        GameObject nextObjectForSlice = _baseMesh.gameObject;

        #region X
        int xAxisPlaneCounts = Mathf.Abs((int)(Vector3.Distance(left, right) / _voxelSizeInMeters)) + 1;
        Debug.Log($"X parts: {xAxisPlaneCounts}");

        List<GameObject> xSlicedMeshes = new List<GameObject>();

        for (int panelIndex = 1; panelIndex < xAxisPlaneCounts; panelIndex++)
        {
            var position = new Vector3(right.x - _voxelSizeInMeters * panelIndex, right.y, right.z);
            //var posObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //posObj.transform.position = position;
            //posObj.transform.localScale = Vector3.one * 0.1f;

            var sh = nextObjectForSlice.Slice(position, transform.right * -1);
            Debug.Log($"SH {sh != null}");
            var neededPart = sh.CreateLowerHull();
            Destroy(nextObjectForSlice);

            Debug.Log($"neededPart {neededPart != null}");

            if (neededPart != null)
            {
                neededPart.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
                neededPart.name = $"X_{panelIndex - 1}";
                xSlicedMeshes.Add(neededPart);


                if (panelIndex + 1 == xAxisPlaneCounts)
                {

                    neededPart = sh.CreateUpperHull();
                    neededPart.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
                    neededPart.name = $"X_{panelIndex}";
                    xSlicedMeshes.Add(neededPart);
                }
                else
                {
                    nextObjectForSlice = sh.CreateUpperHull();
                }
            }
            else
            {
                neededPart = sh.CreateUpperHull();
                neededPart.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
                neededPart.name = $"X_{panelIndex}";
                xSlicedMeshes.Add(neededPart);

                if (panelIndex + 1 != xAxisPlaneCounts)
                    nextObjectForSlice = neededPart;
            }

        }

        #endregion

        #region Z

        int zAxisPlaneCounts = Mathf.Abs((int)(Vector3.Distance(forward, backward) / _voxelSizeInMeters)) + 1;
        Debug.Log($"Z parts: {zAxisPlaneCounts}");


        List<GameObject> zSlicedMeshes = new List<GameObject>();

        foreach (var xSliced in xSlicedMeshes)
        {
            nextObjectForSlice = xSliced;

            for (int panelIndex = 1; panelIndex < zAxisPlaneCounts; panelIndex++)
            {
                var position = new Vector3(forward.x, forward.y, forward.z - panelIndex * _voxelSizeInMeters);
                //var posObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //posObj.transform.position = position;
                //posObj.transform.localScale = Vector3.one * 0.1f;


                var sh = nextObjectForSlice.Slice(position, transform.forward * -1);
                if (sh == null)
                {
                    if (panelIndex + 1 == zAxisPlaneCounts)
                    {
                        nextObjectForSlice.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
                        nextObjectForSlice.name = $"{xSliced.name}_Z_{panelIndex - 1}";
                        zSlicedMeshes.Add(nextObjectForSlice);
                        break;
                    }
                    else
                        continue;
                }
                else
                {
                    var neededPart = sh.CreateLowerHull();
                    neededPart.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
                    neededPart.name = $"{xSliced.name}_Z_{panelIndex - 1}";
                    zSlicedMeshes.Add(neededPart);

                    Destroy(nextObjectForSlice);

                    if (panelIndex + 1 == zAxisPlaneCounts)
                    {
                        neededPart = sh.CreateUpperHull();
                        neededPart.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
                        neededPart.name = $"{xSliced.name}_Z_{panelIndex}";
                        zSlicedMeshes.Add(neededPart);
                    }
                    else
                    {
                        nextObjectForSlice = sh.CreateUpperHull();
                    }
                }

            }
        }

        #endregion

        #region Y
        
        int yAxisPlaneCounts = Mathf.Abs((int)(Vector3.Distance(bottom, top) / _voxelSizeInMeters)) + 1;
        Debug.Log($"Y parts: {yAxisPlaneCounts}");


        List<GameObject> ySlicedMeshes = new List<GameObject>();

        foreach (var zSliced in zSlicedMeshes)
        {
            nextObjectForSlice = zSliced;

            for (int panelIndex = 1; panelIndex < yAxisPlaneCounts; panelIndex++)
            {
                var position = new Vector3(top.x, top.y - _voxelSizeInMeters * panelIndex, top.z);
                //var posObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //posObj.transform.position = position;
                //posObj.transform.localScale = Vector3.one * 0.1f;


                var sh = nextObjectForSlice.Slice(position, transform.up * -1);
                if (sh == null)
                {
                    if (panelIndex + 1 == yAxisPlaneCounts)
                    {
                        var rend = nextObjectForSlice.GetComponent<MeshRenderer>();
                        rend.material = material;
                        rend.material.color = Random.ColorHSV();
                        nextObjectForSlice.name = $"{zSliced.name}_Y_{panelIndex - 1}";
                        ySlicedMeshes.Add(nextObjectForSlice);
                    }
                    else
                        continue;
                }
                else
                {
                    var neededPart = sh.CreateLowerHull();
                    var rend = neededPart.GetComponent<MeshRenderer>();
                    rend.material = material;
                    rend.material.color = Random.ColorHSV();
                    neededPart.name = $"{zSliced.name}_Y_{panelIndex - 1}";
                    ySlicedMeshes.Add(neededPart);

                    Destroy(nextObjectForSlice);

                    if (panelIndex + 1 == yAxisPlaneCounts)
                    {
                        neededPart = sh.CreateUpperHull();
                        var rend1 = neededPart.GetComponent<MeshRenderer>();
                        rend1.material = material;
                        rend1.material.color = Random.ColorHSV(); 
                        neededPart.name = $"{zSliced.name}_Y_{panelIndex}";
                        ySlicedMeshes.Add(neededPart);
                    }
                    else
                    {
                        nextObjectForSlice = sh.CreateUpperHull();
                    }
                }

            }
        }


        
        #endregion


        Debug.Log("DONE-1");

        foreach (var m in ySlicedMeshes)
        {
            var mf = m.GetComponent<MeshFilter>();
            var o_mesh = mf.mesh;
            var mesh = o_mesh.Extract(0);

            mf.mesh = mesh;
        }

        Debug.Log("DONE-2");

        return ySlicedMeshes;
    }


}
