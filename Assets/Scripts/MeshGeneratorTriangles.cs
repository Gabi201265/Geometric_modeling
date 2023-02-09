using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorTriangles : MonoBehaviour
{
    MeshFilter m_Mf;

    void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
        //m_Mf.mesh = CreateTriangle();
        //m_Mf.mesh = CreateStrip(7, new Vector3(4, 1, 3));
        m_Mf.mesh = CreateGridXZ(7, 8, new Vector3(4, 1, 3));
    }

    Mesh CreateTriangle()
    {
        Mesh mesh = new Mesh();
        mesh.name = "triangle";

        Vector3[] vertices = new Vector3[3];
        int[] triangles = new int[1 * 3];

        vertices[0] = Vector3.right; // (1,0,0)
        vertices[1] = Vector3.up; // (0,1,0)
        vertices[2] = Vector3.forward; // (0,0,1)

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    Mesh CreateQuad(Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "quad";

        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[2 * 3];

        vertices[0] = new Vector3(-halfSize.x, 0, -halfSize.z);
        vertices[1] = new Vector3(-halfSize.x, 0, halfSize.z);
        vertices[2] = new Vector3(halfSize.x, 0, halfSize.z);
        vertices[3] = new Vector3(halfSize.x, 0, -halfSize.z);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    Mesh CreateStrip(int nSegments, Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "strip";

        Vector3[] vertices = new Vector3[(nSegments + 1) * 2];
        int[] triangles = new int[nSegments * 2 * 3];

        int index = 0;
        Vector3 leftTopPos = new Vector3(-halfSize.x, 0, halfSize.z);
        Vector3 rightTopPos = new Vector3(halfSize.x, 0, halfSize.z);

        // 1 boucle for pour remplir vertices
        for (int i = 0; i < nSegments + 1; i++)
        {
            float k = (float)i / nSegments;

            Vector3 tmpPos = Vector3.Lerp(leftTopPos, rightTopPos, k);
            vertices[index++] = tmpPos; // vertice du haut
            vertices[index++] = tmpPos - 2 * halfSize.z * Vector3.forward; // vertice du bas
        }
        // 1 boucle for pour remplir triangles
        index = 0;
        for (int i = 0; i < nSegments; i++)
        {
            triangles[index++] = 2 * i;
            triangles[index++] = 2 * i + 2;
            triangles[index++] = 2 * i + 1;

            triangles[index++] = 2 * i + 1;
            triangles[index++] = 2 * i + 2;
            triangles[index++] = 2 * i + 3;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }
    
    Mesh CreateGridXZ(int nSegmentsX,int nSegmentsZ, Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "grid";

        Vector3[] vertices = new Vector3[(nSegmentsX+1)*(nSegmentsZ+1)];
        int[] triangles = new int[nSegmentsX * nSegmentsZ * 2 * 3];


        Vector3 leftBotPos = new Vector3(-halfSize.x, 0, -halfSize.z);
        Vector3 leftTopPos = new Vector3(-halfSize.x, 0, halfSize.z);
        //Vector3 rightTopPos = new Vector3(halfSize.x, 0, halfSize.z);

        int index = 0;
        // 2 boucle for pour remplir vertices
        for (int i = 0; i < (nSegmentsZ+1); i++)
        {
            float k = (float)i / (nSegmentsZ + 1);
            Vector3 tmpLeftPos = Vector3.Lerp(leftTopPos, leftBotPos, k);
            Vector3 tmpRightPos = tmpLeftPos + 2 * halfSize.z * Vector3.right;

            for (int j=0; j< (nSegmentsX+1); j++)
            {
                float b = (float)j / (nSegmentsX + 1);
                vertices[index++] = Vector3.Lerp(tmpLeftPos, tmpRightPos, b);
            }
        }
        index = 0;
        // 2 boucle for pour remplir triangles
        for (int i = 0; i < (nSegmentsZ); i++)
        {
            for (int j = 0; j < (nSegmentsX); j++)
            {
                triangles[index++] = j+(i* (nSegmentsX+1));
                triangles[index++] = (j+1) + (i*(nSegmentsX+1));
                triangles[index++] = (j + nSegmentsX+1) + (i * (nSegmentsX + 1));

                triangles[index++] = (j + nSegmentsX + 1) + (i * (nSegmentsX + 1));
                triangles[index++] = (j + 1) + (i * (nSegmentsX + 1));
                triangles[index++] = (j + nSegmentsX + 2) + (i * (nSegmentsX + 1));
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }
    
}
