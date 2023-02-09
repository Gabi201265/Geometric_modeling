using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace HalfEdge
{
    public class HalfEdge
    {
        public int index;
        public Vertex sourceVertex;
        public Face face;
        public HalfEdge prevEdge;
        public HalfEdge nextEdge;
        public HalfEdge twinEdge;
        public HalfEdge(int index, Vertex sourceVertex, Face face, HalfEdge prevEdge = null, HalfEdge nextEdge = null, HalfEdge twinEdge = null)
        {
            this.index = index;
            this.sourceVertex = sourceVertex;
            this.face = face;
            this.prevEdge = prevEdge;
            this.nextEdge = nextEdge;
            this.twinEdge = twinEdge;
        }
    }
    public class Vertex
    {
        public int index;
        public Vector3 position;
        public HalfEdge outgoingEdge;
        public Vertex(int index, Vector3 position)
        {
            this.index = index;
            this.position = position;
        }
    }
    public class Face
    {
        public int index;
        public HalfEdge edge;
        public Face(int index)
        {
            this.index = index;
        }
        public List<HalfEdge> GetEdges()
        {
            List<HalfEdge> faceEdges = new List<HalfEdge>();
            HalfEdge currEdge = null;
            HalfEdge startEdge = this.edge;
            faceEdges.Add(startEdge);
            //Edge CW

            currEdge = edge.nextEdge;
            while (currEdge != startEdge)
            {
                faceEdges.Add(currEdge);
                currEdge = currEdge.nextEdge;

            }
            return faceEdges;
        }
        public List<Vertex> GetVertex()
        {
            List<HalfEdge> faceEdges = GetEdges();
            List<Vertex> faceVertices = new List<Vertex>();
            //Vertice CW
            HalfEdge mon_Edge = null;
            for (int i = 0; i < faceEdges.Count; i++)
            {
                mon_Edge = faceEdges[i];
                faceVertices.Add(mon_Edge.sourceVertex);
            }
            return faceVertices;
        }
    }
    public class HalfEdgeMesh
    {
        public List<Vertex> vertices;
        public List<HalfEdge> edges;
        public List<Face> faces;

        public bool isValid = false;
        public HalfEdgeMesh(Mesh mesh)
        { // constructeur prenant un mesh Vertex-Face en param�tre
            int nSides;
            switch (mesh.GetTopology(0))
            {
                case MeshTopology.Quads:
                    nSides = 4;
                    break;
                case MeshTopology.Triangles:
                    nSides = 3;
                    break;
                default:
                    isValid = false;
                    return;
            }
            isValid = true;
            vertices = new List<Vertex>();
            edges = new List<HalfEdge>();
            faces = new List<Face>();

            //Oncompl�te les vertices de la m�me mani�re que WingedEdge
            Vector3[] tmpVertices = mesh.vertices;
            for (int i = 0; i < tmpVertices.Length; i++)
            {
                vertices.Add(new Vertex(i, tmpVertices[i]));
            }

            //On r�cup�re les quads et on les parcourt pour trouver les meshs
            int[] indexes = mesh.GetIndices(0);
            Dictionary<ulong, HalfEdge> dicoHalfEdges = new Dictionary<ulong, HalfEdge>();
            //List<HalfEdge> faceHalfEdges = new List<HalfEdge>();

            for (int i = 0; i < indexes.Length / nSides; i++)
            {
                int[] faceIndexes = new int[nSides];
                for (int k = 0; k < nSides; k++)
                {
                    faceIndexes[k] = indexes[nSides * i + k];
                }

                Face newFace = new Face(faces.Count);
                //On complete la liste de face 
                faces.Add(newFace);

                //On compl�te d'abord toutes les edges de la face (sourceVertex face twinEdge)
                for (int j = 0; j < faceIndexes.Length; j++)
                {
                    int startIndex = faceIndexes[j];
                    int endIndex = faceIndexes[(j + 1) % faceIndexes.Length];

                    Vertex startVertex = vertices[startIndex];
                    Vertex endVertex = vertices[endIndex];
                    // creer un dictionnaire cle a laquell est associ� une seule edge utiliser la m�thode try get Value
                    ulong key = ((ulong)Mathf.Min(startIndex, endIndex)) + (((ulong)Mathf.Max(startIndex, endIndex)) << 32);

                    HalfEdge maEdge = null;
                    HalfEdge halfEdge = null;
                    if (!dicoHalfEdges.TryGetValue(key, out halfEdge))
                    {
                        maEdge = new HalfEdge(edges.Count, startVertex, newFace);
                        edges.Add(maEdge);
                        dicoHalfEdges.Add(key, maEdge);
                    }
                    else
                    {
                        //Recr�er une nouvelle Halfedge, qui sera cette fois une twinEdge
                        maEdge = new HalfEdge(edges.Count, startVertex, newFace, null, null, halfEdge);
                        edges.Add(maEdge);
                        halfEdge.twinEdge = maEdge;
                        
                    }
                    //On ajoute la edge associ�e � la face.
                    if (newFace.edge == null) newFace.edge = maEdge;

                    //On compl�te le outgoingEdge du vertice
                    if (startVertex.outgoingEdge == null) startVertex.outgoingEdge = maEdge;

                    //On compl�te maintenant les prevEdges et les nextEdges
                    if (j != 0)
                    {
                        maEdge.prevEdge = edges[edges.Count - 2];
                        edges[edges.Count - 2].nextEdge = maEdge;
                    }
                    //C'est la derni�re HalfEdge de la face donc la suivante sera la premi�re
                    if (j == 3)
                    {
                        maEdge.nextEdge = edges[edges.Count - 4];
                        edges[edges.Count - 4].prevEdge = maEdge;
                    }
                }
            }
            GUIUtility.systemCopyBuffer = ConvertToCSVFormat("\t");
        }
        public Mesh ConvertToFaceVertexMesh()
        {
            Mesh faceVertexMesh = new Mesh();

            Vector3[] m_vertices = new Vector3[vertices.Count];
            int[] m_quads = new int[faces.Count * 4];

            //Vertices
            for (int i = 0; i < vertices.Count; i++)
            {
                m_vertices[i] = vertices[i].position;
            }

            int index = 0;

            //Quads
            for (int j = 0; j < faces.Count; j++)
            {
                List<Vertex> maListeVertexFace = faces[j].GetVertex();
                for (int w = 0; w < maListeVertexFace.Count; w++)
                {
                    m_quads[index++] = maListeVertexFace[w].index;
                }
            }
            faceVertexMesh.vertices = m_vertices;
            faceVertexMesh.SetIndices(m_quads, MeshTopology.Quads, 0);
            return faceVertexMesh;
        }
        public string ConvertToCSVFormat(string separator = "\t")
        {
            List<string> strings = new List<string>();

            //Vertices

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 pos = vertices[i].position;
                strings.Add(vertices[i].index + separator
                    + pos.x.ToString("N03") + " "
                    + pos.y.ToString("N03") + " "
                    + pos.z.ToString("N03") + separator
                    + vertices[i].outgoingEdge.index
                    + separator + separator);
            }

            for (int i = vertices.Count; i < edges.Count; i++)
            {
                strings.Add(separator + separator + separator + separator);
            }

            //Edges

            for (int i = 0; i < edges.Count; i++)
            {
                strings[i] += edges[i].index + separator
                    + edges[i].sourceVertex.index + separator
                    + edges[i].face.index + separator
                    + edges[i].prevEdge.index + separator
                    + edges[i].nextEdge.index + separator
                    + edges[i].twinEdge.index + separator + separator;
            }

            //Faces

            for (int i = 0; i < faces.Count; i++)
            {
                List<HalfEdge> faceEdges = faces[i].GetEdges();
                List<Vertex> faceVertex = faces[i].GetVertex();

                List<int> edgesIndex = new List<int>();
                List<int> vertexIndex = new List<int>();
                //Edge CW
                for(int w=0; w < faceEdges.Count; w++)
                {
                    edgesIndex.Add(faceEdges[w].index);
                }
                //Vertice CW
                for (int w = 0; w < faceVertex.Count; w++)
                {
                    vertexIndex.Add(faceVertex[w].index);
                }


                strings[i] += faces[i].index + separator
                    + faces[i].edge.index + separator
                    + string.Join(" ", edgesIndex) + separator
                    + string.Join(" ", vertexIndex) + separator + separator;
            }

            //Pr�sentation CSV

            string str = "Vertex" + separator + separator + separator + separator + "HalfEges" + separator + separator + separator + separator + separator + separator + separator + "Faces\n"
                + "Index" + separator + "Position" + separator + "outgoingEdge" + separator + separator +
                "Index" + separator + "sourceVertex" + separator + "Face" + separator + "prevEdge" + separator + "nextEdge" + separator + "twinEdge" + separator + separator +
                "Index" + separator + "Edge" + separator + "CW Edges" + separator + "CW Vertices\n"
                + string.Join("\n", strings);
            Debug.Log(str);
            return str;
        }
        public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces, Transform transform)
        {
            Gizmos.color = Color.black;
            GUIStyle style = new GUIStyle();
            style.fontSize = 12;

            //vertices
            if (drawVertices)
            {
                style.normal.textColor = Color.red;
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vector3 worldPos = transform.TransformPoint(vertices[i].position);
                    Handles.Label(worldPos, "V" + vertices[i].index, style);
                }
            }
            //faces
            
            for (int i = 0; i < faces.Count; i++)
            {
                style.normal.textColor = Color.green;

                List<Vertex> vertices_une_face = faces[i].GetVertex();
                Vector3 total = new Vector3(0, 0, 0);
                for (int w = 0; w < vertices_une_face.Count; w++)
                {
                    Vector3 pt = transform.TransformPoint(vertices_une_face[w].position);
                    Vector3 pt2 = transform.TransformPoint(vertices_une_face[(w + 1) % vertices_une_face.Count].position);
                    Gizmos.DrawLine(pt,pt2);
                    total += pt;
                }
                if (drawFaces)
                {
                    Handles.Label((total) / 4.0f, "F" + faces[i].index, style);
                }

                //edges
                //Il est important de r�cup�rer le centre par soucis de visibilit� afin que les deux halfeds ne se mettent pas les une sur les autres. 
                //On les Lerp vers le centre de leur face respective.
                if (drawEdges)
                {
                    List<HalfEdge> halfedges_une_face = faces[i].GetEdges();
                    style.normal.textColor = Color.blue;
                    for (int j = 0; j < halfedges_une_face.Count; j++)
                    {
                        Vector3 start = halfedges_une_face[j].sourceVertex.position;
                        Vector3 end = halfedges_une_face[j].nextEdge.sourceVertex.position;
                        Vector3 pos = Vector3.Lerp(Vector3.Lerp(start, end, 0.5f), (total) / 4.0f, 0.1f);
                        Handles.Label(pos, "e" + halfedges_une_face[j].index, style);
                    }

                }
            }
        }
    }
}
