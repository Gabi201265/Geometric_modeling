using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WingedEdge;

public class Subdivide : MonoBehaviour
{
    MeshFilter m_Mf;
    Mesh m_Mesh_init;
    WingedEdgeMesh m_WingedEdgeMesh;
    // Start is called before the first frame update
    void Start()
    {
        this.m_Mf = GetComponent<MeshFilter>();
        this.m_Mesh_init = m_Mf.mesh;
        this.m_WingedEdgeMesh = new WingedEdgeMesh(m_Mf.mesh);
        this.m_Mf.mesh = this.m_WingedEdgeMesh.ConvertToFaceVertexMesh();
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        int compteur = 0;
        //Boucle infinie
        while(true)
        {
            yield return new WaitForSeconds(2f);
            if (compteur % 4 == 0)
            {
                m_WingedEdgeMesh = new WingedEdgeMesh(m_Mesh_init);
                this.m_Mf.mesh = this.m_WingedEdgeMesh.ConvertToFaceVertexMesh();
            }
            else
            {
                m_WingedEdgeMesh.SubdivideCatmullClark(1);
                this.m_Mf.mesh = this.m_WingedEdgeMesh.ConvertToFaceVertexMesh();
            }
            compteur++;
        }
    }
}