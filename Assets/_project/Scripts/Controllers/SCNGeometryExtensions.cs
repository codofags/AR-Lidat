//using UnityEngine;

//public static class SCNGeometryExtensions
//{
//    public static Mesh ToMesh(this ARMeshGeometry geometry, Camera camera, Matrix4x4 modelMatrix, bool needTexture = false)
//    {
//        Mesh mesh = new Mesh();

//        Vector3[] vertices = geometry.vertices.ToArray();
//        Vector3[] normals = geometry.normals.ToArray();
//        int[] faces = geometry.faces.ToArray();

//        mesh.vertices = vertices;
//        mesh.normals = normals;
//        mesh.triangles = faces;

//        if (needTexture)
//        {
//            Vector2[] textureCoordinates = CalcTextureCoordinates(geometry, camera, modelMatrix);
//            mesh.uv = textureCoordinates;
//        }

//        return mesh;
//    }

//    private static Vector2[] CalcTextureCoordinates(ARMeshGeometry geometry, Camera camera, Matrix4x4 modelMatrix)
//    {
//        Vector2[] textureCoordinates = new Vector2[geometry.vertices.Length];
//        Vector2 screenSize = new Vector2(camera.pixelWidth, camera.pixelHeight);

//        for (int i = 0; i < geometry.vertices.Length; i++)
//        {
//            Vector3 vertex = geometry.vertices[i];
//            Vector4 vertex4 = new Vector4(vertex.x, vertex.y, vertex.z, 1f);
//            Vector4 worldVertex4 = modelMatrix * vertex4;
//            Vector3 worldVector3 = new Vector3(worldVertex4.x, worldVertex4.y, worldVertex4.z);
//            Vector3 screenPoint = camera.WorldToScreenPoint(worldVector3);
//            float u = screenPoint.x / screenSize.x;
//            float v = 1f - (screenPoint.y / screenSize.y);
//            textureCoordinates[i] = new Vector2(u, v);
//        }

//        return textureCoordinates;
//    }
//}