using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace WingedEdge
{
    public class WingedEdge
    {
        public int index;
        public Vertex startVertex;
        public Vertex endVertex;
        public Face leftFace;
        public Face rightFace;
        public WingedEdge startCWEdge;
        public WingedEdge startCCWEdge;
        public WingedEdge endCWEdge;
        public WingedEdge endCCWEdge;

        public WingedEdge(int index, Vertex startVertex, Vertex endVertex, Face rightFace, Face leftFace, WingedEdge startCWEdge = null, WingedEdge startCCWEdge = null, WingedEdge endCWEdge = null, WingedEdge endCCWEdge = null)
        {
            this.index = index;
            this.startVertex = startVertex;
            this.endVertex = endVertex;
            this.rightFace = rightFace;
            this.leftFace = leftFace;
            this.startCWEdge = startCWEdge;
            this.startCCWEdge = startCCWEdge;
            this.endCWEdge = endCWEdge;
            this.endCCWEdge = endCCWEdge;
            
        }
        public WingedEdge FindEndCWBorder()
        {
            //Nous avons trois cas diff�rents, dont deux o� il n'y a pas de leftface pour la edge courante
            //Le troisi�me cas est d�j� trait�e par la premi�re boucle du cosntructeur

            //Par cons�quent si pas de LeftFace, on skip
            if (this.leftFace != null)
            {
                return null;
            }
            WingedEdge endCW = this.endCCWEdge;
            while(endCW.leftFace!=null)
            {
                if(endCW.endVertex == this.endVertex)
                {
                    endCW = endCW.endCCWEdge;
                }
                else
                {
                    endCW = endCW.startCCWEdge;
                }
            }
            return endCW;

        }
        public WingedEdge FindStartCCWBorder()
        {
            //De m�me que FindEndCWBorder en parcourant dans l'autre sens
            if (this.leftFace != null)
            {
                return null;
            }
            WingedEdge startCCW = this.startCWEdge;
            while (startCCW.leftFace != null)
            {
                if (startCCW.startVertex == this.startVertex)
                {
                    startCCW = startCCW.startCCWEdge;
                }
                else
                {
                    startCCW = startCCW.endCWEdge;
                }
            }
            return startCCW;
        }
    }
    public class Vertex
    {
        public int index;
        public Vector3 position;
        public WingedEdge edge;
        public Vertex(int index, Vector3 position)
        {
            this.index = index;
            this.position = position;
        }
        public Vector3 GetPosition()
        {
            return this.position;
        }
        public List<WingedEdge> GetAdjEdges() 
        {
            WingedEdge currentEdge = edge;
            List<WingedEdge> adjEdges = new List<WingedEdge>();
            do
            {
                adjEdges.Add(currentEdge);
                currentEdge = (this == currentEdge.startVertex) ? currentEdge.startCWEdge : currentEdge.endCWEdge;
            } while (currentEdge != edge);

            return adjEdges;
        }
        public List<Face> GetAdjFaces()
        {
            List<Face> adjFaces = new List<Face>();
            List<WingedEdge> adjEdges = GetAdjEdges();
            for (int i = 0; i < adjEdges.Count(); i++)
            {
                //ignore les edges en bordure dirigés vers la vertice courante
                if ( !(adjEdges[i].leftFace == null && this == adjEdges[i].endVertex ))
                {
                    //ajoute la bonne face en fonction de la direction de l'edge par rapport à la vertice courante
                    adjFaces.Add((this == adjEdges[i].startVertex) ? adjEdges[i].rightFace : adjEdges[i].leftFace);
                }
            }
            return adjFaces;
        }
        public List<WingedEdge> GetBorderEdges() 
        {
            List<WingedEdge> borderEdges = new List<WingedEdge>();
            List<WingedEdge> adjEdges = GetAdjEdges();
            for (int i = 0; i < adjEdges.Count(); i++)
            {
                if (adjEdges[i].leftFace == null) 
                { 
                    borderEdges.Add(adjEdges[i]); 
                }
            }

            return borderEdges;
        }
    }
    public class Face
    {
        public int index;
        public WingedEdge edge;
        public Face(int index)
        {
            this.index = index;
        }
        public List<WingedEdge> GetEdges()
        {
            List<WingedEdge> faceEdges = new List<WingedEdge>();
            WingedEdge currEdge = null;
            WingedEdge startEdge = this.edge;
            faceEdges.Add(startEdge);
            if (this == edge.rightFace) {currEdge = edge.endCCWEdge;}
            else {currEdge = edge.startCCWEdge;}
            while (currEdge != startEdge)
            {
                faceEdges.Add(currEdge);
                if (this == currEdge.rightFace) {currEdge = currEdge.endCCWEdge;}
                else {currEdge = currEdge.startCCWEdge;}
            }
            return faceEdges;
        }

        public List<Vertex> GetVertex()
        {
            List<WingedEdge> faceEdges = GetEdges();
            List<Vertex> faceVertices = new List<Vertex>();
            WingedEdge mon_Edge = null;
            for (int i = 0; i < faceEdges.Count(); i++)
            {
                mon_Edge = faceEdges[i];
                if (mon_Edge.rightFace == this) {faceVertices.Add(mon_Edge.startVertex);}
                else {faceVertices.Add(mon_Edge.endVertex);}
            }
            return faceVertices;
        }

        
    }

    public class WingedEdgeMesh
    {
        public List<Vertex> vertices = null;
        public List<WingedEdge> edges = null;
        public List<Face> faces = null;

        public bool isValid = false;
        public WingedEdgeMesh(Mesh mesh)
        {// constructeur prenant un mesh Vertex-Face en paramètre
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
            edges = new List<WingedEdge>();
            faces = new List<Face>();
            //On complète la liste de vertices.
            Vector3[] tmpVertices = mesh.vertices;
            for (int i = 0; i < tmpVertices.Length; i++)
            {
                vertices.Add(new Vertex(i, tmpVertices[i]));
            }

            //On récupère les quads et on les parcourt pour trouver les meshs
            int[] indexes = mesh.GetIndices(0);
            Dictionary<ulong, WingedEdge> dicoWingedEdges = new Dictionary<ulong, WingedEdge>();
            List<WingedEdge> faceEdges = new List<WingedEdge>();

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

                //On complete startVertex, endVertex, leftFace et rightFace
                for (int j = 0; j < faceIndexes.Length; j++)
                {
                    int startIndex = faceIndexes[j];
                    int endIndex = faceIndexes[(j + 1) % faceIndexes.Length];
                    Vertex startVertex = vertices[startIndex];
                    Vertex endVertex = vertices[endIndex];

                    // creer un dictionnaire cle a laquell est associé une seule edge utiliser la méthode try get Value
                    ulong key = ((ulong)Mathf.Min(startIndex, endIndex)) + (((ulong)Mathf.Max(startIndex, endIndex)) << 32);

                    WingedEdge edge = null;

                    if (!dicoWingedEdges.TryGetValue(key, out edge))
                    {
                        edge = new WingedEdge(edges.Count, startVertex, endVertex, newFace, null);
                        edges.Add(edge);
                        dicoWingedEdges.Add(key, edge);
                        faceEdges.Add(edge);

                        //On ajoute les edges associées à la face et aux vertices.
                        if (newFace.edge == null) newFace.edge = edge;
                        if (startVertex.edge == null) startVertex.edge = edge;
                    }
                    else
                    {
                        //Complète les info manquante la leftFace 
                        edge.leftFace = newFace;
                        if (newFace.edge == null) newFace.edge = edge;
                        faceEdges.Add(edge);
                    }

                    //Il manque plus qu'à compléter les CCW et CW
                }
                for (int w = 0; w < faceEdges.Count; w++)
                {
                    if (newFace == faceEdges[w].rightFace)
                    {
                        faceEdges[w].startCWEdge = faceEdges[(w - 1 + faceEdges.Count) % faceEdges.Count];
                        faceEdges[w].endCCWEdge = faceEdges[(w + 1) % faceEdges.Count];
                    }
                    else if (newFace == faceEdges[w].leftFace)
                    {
                        faceEdges[w].endCWEdge = faceEdges[(w - 1 + faceEdges.Count) % faceEdges.Count];
                        faceEdges[w].startCCWEdge = faceEdges[(w + 1) % faceEdges.Count];
                    }
                }
                faceEdges.Clear();

                //MAJ CCW and CW Edge pour les borderEdges
                for (int w = 0; w < edges.Count; w++)
                {
                    if (edges[w].leftFace == null)
                    {
                        edges[w].startCCWEdge = edges[w].FindStartCCWBorder();
                        edges[w].endCWEdge = edges[w].FindEndCWBorder();
                    }
                }
            }
            //////vertices
            string str = "Vertex - edges : \n";
            foreach (Vertex v in vertices)
            {
                str += "V" + v.index.ToString() + ": " + v.position.ToString() + " | e" + v.edge.index + " \n";
            }
            Debug.Log(str);
            //////faces
            str = "Faces - edges : \n";
            foreach (Face f in faces)
            {
                str += f.index.ToString() + ": F" + f.index.ToString() + " - e "+ f.edge.index + " \n";
            }
            Debug.Log(str);

            //////wingedEdge

            //str = "WingedEdges : \n";
            foreach (WingedEdge e in edges)
            {
                str += $"e{e.index} : V{e.startVertex.index} - V{e.endVertex.index}| {(e.leftFace == null ? "NoLeftFace" : $"F{e.leftFace.index}")} | {(e.rightFace == null ? "NoRightFace" : $"F{e.rightFace.index}")}\n"; //| SCCW : e{e.startCCWEdge.index} - SCW : e{e.startCWEdge.index} - ECW : e{e.endCWEdge.index} - ECCW : e{e.endCCWEdge.index}\n";
            }
            Debug.Log(str);

            GUIUtility.systemCopyBuffer = ConvertToCSV("\t");

        }
        
        //MY CODE
        public void SubdivideCatmullClark(int NBSUB)
        {
            List<Vector3> facePoints = new List<Vector3>();
            List<Vector3> edgePoints = new List<Vector3>();
            List<Vector3> vertexPoints = new List<Vector3>();
            
            for(int s = 0; s<NBSUB; ++s){        
                //Calcul des nouveaux points : faces Points, edges Points, vertex Points
                CatmullClarkCreateNewPoints(out facePoints,out edgePoints,out vertexPoints);
                
                //Split de chaque edge pour en créer 2 nouveaux à partir de la liste de nouveaux edges
                for(int i = 0; i<edgePoints.Count(); ++i){
                    SplitEdge(edges[i],edgePoints[i]);
                }

                //Split de chaque face pour en créer 4 nouvelles à partir de la liste de nouvelles faces
                for(int i = 0; i<facePoints.Count(); ++i){
                    SplitFace(faces[i],facePoints[i]);
                }

                //Mise à jour les postions des vertices grace à la liste de nouveaux points
                for(int i = 0; i<vertexPoints.Count(); ++i){
                    //Debug.Log(vertexPoints[i]);
                    vertices[i].position = vertexPoints[i];
                }
            }
        }

        public void CatmullClarkCreateNewPoints(out List<Vector3> facePoints, out List<Vector3> edgePoints, out List<Vector3> vertexPoints)
        {
            facePoints = new List<Vector3>();
            ////STEP 1/////
            //parcours de toutes les faces de WindegEdgeMesh
            for(int f = 0; f<faces.Count(); ++f){    

                //Recupération des points de la face courante
                List<Vertex> vertexCurrentFace = faces[f].GetVertex();
                
                //Recuperation des coordonnées de la liste de points de la face courante
                List<Vector3> positionsVertexCurrentFace = new List<Vector3>();
                for(int v = 0; v<vertexCurrentFace.Count(); ++v){
                    positionsVertexCurrentFace.Add(vertexCurrentFace[v].GetPosition())   ;
                }
            
                //C0 = Somme de tous les points de la face courante/nb points
                float sumX = 0,sumY = 0,sumZ = 0,nbPoints = 0;
                for(int p = 0; p<positionsVertexCurrentFace.Count(); ++p){
                    sumX += positionsVertexCurrentFace[p].x;
                    sumY += positionsVertexCurrentFace[p].y;
                    sumZ += positionsVertexCurrentFace[p].z;
                    nbPoints += 1;
                }
                Vector3 C0 = new Vector3(   sumX/nbPoints,
                                            sumY/nbPoints,
                                            sumZ/nbPoints);
                
                //Ajout du point C0 (Face point) de la face courante dans la liste  
                facePoints.Add(C0);       
            }
            ///FIN STEP 1////

            List<Vector3> midPoints = new List<Vector3>();
            ///STEP 2////
            //parcours de tous les edges de WindegEdgeMesh
            for(int e = 0; e<edges.Count(); ++e){
                
                //Calcul mid points
                float midX = 0,midY = 0,midZ = 0;
                midX = (edges[e].startVertex.GetPosition().x + edges[e].endVertex.GetPosition().x)/2;
                midY = (edges[e].startVertex.GetPosition().y + edges[e].endVertex.GetPosition().y)/2;
                midZ = (edges[e].startVertex.GetPosition().z + edges[e].endVertex.GetPosition().z)/2;
                Vector3 m0 = new Vector3(   midX,
                                            midY,
                                            midZ);
                //Ajout du point m0 (Mid point) de l'edge courant dans la liste
                midPoints.Add(m0);
            }

            edgePoints = new List<Vector3>();
            //parcours de tous les edges de WindegEdgeMesh
            for(int e = 0; e<edges.Count(); ++e){
                
                //E0 = Somme des points de l'edge et des facepoints des faces adjacentes
                float sumX = 0,sumY = 0,sumZ = 0;
                if(edges[e].leftFace != null && edges[e].rightFace != null){
                    sumX = (edges[e].startVertex.GetPosition().x + edges[e].endVertex.GetPosition().x + facePoints[edges[e].leftFace.index].x + facePoints[edges[e].rightFace.index].x)/4;
                    sumY = (edges[e].startVertex.GetPosition().y + edges[e].endVertex.GetPosition().y + facePoints[edges[e].leftFace.index].y + facePoints[edges[e].rightFace.index].y)/4;
                    sumZ = (edges[e].startVertex.GetPosition().z + edges[e].endVertex.GetPosition().z + facePoints[edges[e].leftFace.index].z + facePoints[edges[e].rightFace.index].z)/4;
                }
                else{//edge en bordure
                    sumX = midPoints[e].x;
                    sumY = midPoints[e].y;
                    sumZ = midPoints[e].z;
                }
                Vector3 E0 = new Vector3(   sumX,
                                            sumY,
                                            sumZ);
                //Ajout du point E0 (Edge point) de l'edge courant dans la liste
                edgePoints.Add(E0);
            }
            ///FIN STEP 2////           

            vertexPoints = new List<Vector3>();                       
            ///STEP 3////
            //parcours de tous les vertices de WindegEdgeMesh
            for(int v = 0; v<vertices.Count(); ++v){
                //On stocke la position courante de la vertice
                Vector3 V = vertices[v].position;

                //On récupère notre liste d'edges adjacents à notre vertex
                List<WingedEdge> adjEdges = new List<WingedEdge>();
                adjEdges = vertices[v].GetAdjEdges();
                //On ajoute les positions des mid points de ces edges pour en trouver la moyenne
                float sumMidX = 0,sumMidY = 0,sumMidZ = 0;
                for(int a = 0; a<adjEdges.Count(); ++a){
                    sumMidX += midPoints[adjEdges[a].index].x;
                    sumMidY += midPoints[adjEdges[a].index].y;
                    sumMidZ += midPoints[adjEdges[a].index].z;
                }
                //On récupère notre liste de faces adjacentes à notre vertex eq. nombre de edges inc. à la vertice
                List<Face> adjFaces = new List<Face>();
                adjFaces = vertices[v].GetAdjFaces();

                sumMidX = sumMidX/adjFaces.Count();
                sumMidY = sumMidY/adjFaces.Count();
                sumMidZ = sumMidZ/adjFaces.Count();
                //On stocke la moyenne des "mid-points" des edges adj. à la vertice dans R
                Vector3 R = new Vector3(sumMidX,
                                        sumMidY,
                                        sumMidZ);
                //On ajoute les positions des face points de ces faces pour en trouver la moyenne
                float sumFacePX = 0,sumFacePY = 0,sumFacePZ = 0;
                for(int a = 0; a<adjFaces.Count(); ++a){
                    sumFacePX += facePoints[adjFaces[a].index].x;
                    sumFacePY += facePoints[adjFaces[a].index].y;
                    sumFacePZ += facePoints[adjFaces[a].index].z;
                }
                sumFacePX = sumFacePX/adjFaces.Count();
                sumFacePY = sumFacePY/adjFaces.Count();
                sumFacePZ = sumFacePZ/adjFaces.Count();
                //On stocke la moyenne des "face points" des faces adj. à la vertice dans Q
                Vector3 Q = new Vector3(sumFacePX,
                                        sumFacePY,
                                        sumFacePZ);

                //On stocke le nombre d'edges incidents de la vertice eq. nombre de faces adj. à la vertice
                float n = adjFaces.Count();
                
                List<WingedEdge> borderEdges = new List<WingedEdge>();
                borderEdges = vertices[v].GetBorderEdges();
                //On calcul la nouvelle position de la vertice
                //Cas Bordure :
                if(borderEdges.Count() != 0){
                    float sumBX = 0,sumBY = 0,sumBZ = 0;
                    for(int b = 0; b<borderEdges.Count(); ++b){
                        sumBX += midPoints[borderEdges[b].index].x;
                        sumBY += midPoints[borderEdges[b].index].y;
                        sumBZ += midPoints[borderEdges[b].index].z;
                    }
                    sumBX += V.x;
                    sumBY += V.y;
                    sumBZ += V.z;
                    V = new Vector3(sumBX/3,sumBY/3,sumBZ/3);
                }
                //Cas Non Bordure :
                else{
                    V = (Q/n) + (2f*R/n) + ((n-3f)*V)/n;
                }
                //Ajout du point V (position) de la vertice courante dans la liste
                vertexPoints.Add(V);
            }
        }   

        public void SplitEdge(WingedEdge edge, Vector3 splittingPoint)
        {                 
            //Création d'un nouveau Vertex avec les coordonnées de splittingPoint
            Vertex V0 = new Vertex(vertices.Count(), splittingPoint);
            //Ajout dans la liste de vertices 
            vertices.Add(V0);
    
            //Creation d'une nouvelle Wingededge avec V0 comme StartVertex
            WingedEdge splitEdge = new WingedEdge(edges.Count(),
                                                  V0,
                                                  edge.endVertex, 
                                                  edge.rightFace, 
                                                  edge.leftFace, 
                                                  edge, 
                                                  edge, 
                                                  edge.endCWEdge, 
                                                  edge.endCCWEdge);
            edges.Add(splitEdge);

            //Mise à jour des clockwise et counter clockwise
            if(edge == edge.endCCWEdge.startCWEdge) {edge.endCCWEdge.startCWEdge = splitEdge;}
            if(edge == edge.endCWEdge.startCCWEdge) {edge.endCWEdge.startCCWEdge = splitEdge;}
            if(edge == edge.endCWEdge.endCCWEdge) {edge.endCWEdge.endCCWEdge = splitEdge;}
            if(edge == edge.endCCWEdge.endCWEdge) {edge.endCCWEdge.endCWEdge = splitEdge;}

            //Mise à jour de endVertex
            edge.endVertex = V0;
            edge.endVertex.edge = splitEdge;
            //Mise à jour de  end Clockwise et end Counter clockwise
            edge.endCCWEdge = splitEdge;
            edge.endCWEdge = splitEdge;
            //Mise à jour de l'edge de la vertice
            V0.edge = splitEdge;
            splitEdge.endVertex.edge = splitEdge;
        }

        public void SplitFace(Face face, Vector3 splittingPoint)
        {
            bool isRecycled = false;
            Face currentFace = face;
            //Création d'un nouveau Vertex V0 avec les coordonnées de splittingPoint
            Vertex V0 = new Vertex(vertices.Count(), splittingPoint);
            //Ajout dans la liste de vertices 
            vertices.Add(V0);

            //Récupération des edges et vertices de la face courrante
            List<WingedEdge> faceEdges= face.GetEdges();
            List<Vertex> faceVertex = face.GetVertex();

            //Réorganisation des listes de vertices et d'edges :
            /* Raison de la réorganisation, on prend comme exemple la face à recycler:
             * - Cas 1 (face.edge.leftFace == face) :
             * La première edge de la liste faceEdges correspondra à la rightEdge et la seconde à la bottomEdge de la face à recycler.
             * Ici il n'y a pas de problème, l'ordre de la liste dans ce cas 1 nous facilite le travail pour recycler et créer les nouvellles faces.
             * - Cas 2 (face.edge.rightFace == face) :
             * Le dernier élément de la liste faceEdges correspondra à la rightEdge et le premier élément à la bottomEdge.
             * Ici l'ordre n'est pas idéal...
             * Pour simplifier l'utilisation des listes de faceEdges et faceVertex, on décale le dernier élément à la premiere position de la liste pour obtenir une liste dans le même ordre que le cas 1.
             */
            if (face.edge.rightFace == face)//Réorganisation dans le cas de rightFace
            {
                faceVertex.Insert(0, faceVertex[faceVertex.Count() - 1]);
                faceVertex.RemoveAt(faceVertex.Count() - 1);

                faceEdges.Insert(0, faceEdges[faceEdges.Count() - 1]);
                faceEdges.RemoveAt(faceEdges.Count() - 1);
            }


            for (int i = 0; i < faceEdges.Count(); i+=2)
            {
                //Ajout des nouvelles faces dans le cas ou la face initiale est déjà recyclée
                if (isRecycled)
                {
                    currentFace = new Face(faces.Count());
                    faces.Add(currentFace);
                }
                WingedEdge rightEdge = faceEdges[i];
                WingedEdge bottomEdge = faceEdges[(i + 1) % faceEdges.Count()];
                WingedEdge topEdge;
                WingedEdge leftEdge;

                WingedEdge rightEdgePrevEdge = faceEdges[(i - 1 + faceEdges.Count()) % faceEdges.Count()];
                WingedEdge bottomEdgeNextEdge = faceEdges[(i + 2) % faceEdges.Count()];

                if (!isRecycled) //edges de face recyclé
                {
                    topEdge = new WingedEdge(edges.Count(), V0, faceVertex[i], currentFace, null, null, null, rightEdgePrevEdge, rightEdge);
                    edges.Add(topEdge);
                    leftEdge = new WingedEdge(edges.Count(), V0, faceVertex[i + 2], null, currentFace, null, null, bottomEdge, bottomEdgeNextEdge);
                    edges.Add(leftEdge);
                }
                else
                {
                    if (i == 6) //edges de la last face
                    {
                        topEdge = edges[edges.Count() - 1];
                        leftEdge = edges[edges.Count() - 4];

                        topEdge.rightFace = currentFace;
                        leftEdge.leftFace = currentFace;
                    }
                    else //edges de la 2e et 3e face
                    {
                        topEdge = edges[edges.Count() - 1];
                        topEdge.rightFace = currentFace;

                        leftEdge = new WingedEdge(edges.Count(), V0, faceVertex[i + 2], null, currentFace, null, null, bottomEdge, bottomEdgeNextEdge);
                        edges.Add(leftEdge);
                    }
                }

                //Lié topEdge et leftEdge
                topEdge.startCWEdge = leftEdge;
                leftEdge.startCCWEdge = topEdge;

                //Mise à jour de rightEdge
                if (rightEdge.startVertex == topEdge.endVertex) { 
                    rightEdge.startCWEdge = topEdge; 
                    rightEdge.rightFace = currentFace;
                    } 
                else { 
                    rightEdge.endCWEdge = topEdge; 
                    rightEdge.leftFace = currentFace;
                    }

                //Mise à jour de bottomEdge
                if (bottomEdge.startVertex == leftEdge.endVertex) { 
                    bottomEdge.startCCWEdge = leftEdge;
                    bottomEdge.leftFace = currentFace; 
                } 
                else { 
                    bottomEdge.endCCWEdge = leftEdge; 
                    bottomEdge.rightFace = currentFace;
                }

                //Compléter currentFace.edge and V0.edge
                if(currentFace.edge == null) currentFace.edge = topEdge;
                V0.edge = topEdge;

                isRecycled = true;
            }
        }

        public Mesh ConvertToFaceVertexMesh()
        {
            Mesh faceVertexMesh = new Mesh();
            // magic happens
            Vector3[] m_vertices = new Vector3[vertices.Count()];
            int[] m_quads = new int[faces.Count() * 4];


            //On doit rï¿½cupï¿½rer les edges d'une face et rï¿½cupï¿½rer les vertices dans le mï¿½me sens.
            //On cree donc deux mï¿½thodes distinctes dans la classe Face GetEdgesFace et GetVertexFace
            //FaceEdges

            //Vertices
            for (int i = 0; i < vertices.Count(); i++)
            {
                m_vertices[i] = vertices[i].position;
            }

            int index = 0;

            //Quads
            for (int j = 0; j < faces.Count(); j++)
            {
                List<Vertex> maListeVertexFace = faces[j].GetVertex();
                for (int w = 0; w < maListeVertexFace.Count(); w++)
                {
                    m_quads[index++] = maListeVertexFace[w].index;
                }
                    
            }

            faceVertexMesh.vertices = m_vertices;
            faceVertexMesh.SetIndices(m_quads, MeshTopology.Quads, 0);
            return faceVertexMesh;
        }
        public string ConvertToCSV(string separator = "\t")
        {

            List<string> strings = new List<string>();

            //Vertices
            for (int i=0; i<vertices.Count();i++)
            {
                List<WingedEdge> edges_vertice = edges.FindAll(edge => edge.startVertex == vertices[i] || edge.endVertex == vertices[i]);
                int[] edges_adj = new int[edges_vertice.Count()];
                for (int j = 0; j < edges_vertice.Count(); j++)
                {
                    edges_adj[j] = edges_vertice[j].index;
                }
                Vector3 pos = vertices[i].position;
                strings.Add(vertices[i].index + separator
                    + pos.x.ToString("N03") + " "
                    + pos.y.ToString("N03") + " "
                    + pos.z.ToString("N03") + separator
                    + string.Join(" ", edges_adj)
                    + separator + separator);
            }
            for (int i = vertices.Count(); i < edges.Count(); i++)
            {
                strings.Add(separator + separator + separator + separator);
            }
            //Edges
            for (int i = 0; i < edges.Count(); i++)
            {
                strings[i] += edges[i].index + separator
                    + edges[i].startVertex.index + separator
                    + edges[i].endVertex.index + separator
                    + $"{(edges[i].leftFace != null ? $"{edges[i].leftFace.index}" : "NULL")}" + separator
                    + $"{(edges[i].rightFace != null ? $"{edges[i].rightFace.index}" : "NULL")}" + separator
                    + $"{(edges[i].startCCWEdge != null ? $"{edges[i].startCCWEdge.index}" : "NULL")}" + separator
                    + $"{(edges[i].startCWEdge != null ? $"{edges[i].startCWEdge.index}" : "NULL")}" + separator
                    + $"{(edges[i].endCWEdge != null ? $"{edges[i].endCWEdge.index}" : "NULL")}" + separator
                    + $"{(edges[i].endCCWEdge != null ? $"{edges[i].endCCWEdge.index}" : "NULL")}" + separator
                    + separator;
            }
            //Faces
            for (int i = 0; i < faces.Count(); i++)
            {
                List<WingedEdge> edges_face = faces[i].GetEdges();
                int[] face_edges = new int[edges_face.Count()];
                for (int j = 0; j < edges_face.Count(); j++)
                {
                    face_edges[j] = edges_face[j].index;
                }
                strings[i] += faces[i].index + separator + string.Join(" ", face_edges) + separator + separator;
            }
            //Pr�sentation CSV
            string str = "Vertex" + separator + separator + separator + separator + "WingedEdges" + separator + separator + separator + separator + separator + separator + "Faces\n"
                + "Index" + separator + "Position" + separator + "Edge" + separator + separator +
                "Index" + separator + "Start Vertex" + separator + "End Vertex" + separator + "Left Face" + separator + "Right Face" + separator + "Start CCW Edge" + separator + "Start CW Edge" + separator + "End CW Edge" + separator + "End CCW Edge" + separator + separator +
                "Index" + separator + "Edge\n"
                + string.Join("\n", strings);
            //Debug.Log(str);
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
                for (int i = 0; i < vertices.Count(); i++)
                {
                    Vector3 worldPos = transform.TransformPoint(vertices[i].position);
                    Handles.Label(worldPos, "V" + vertices[i].index, style);
                    Gizmos.DrawSphere(worldPos,.15f);
                }
            }
            //faces
            if (drawFaces)
            {
                style.normal.textColor = Color.green;
                for (int i = 0; i < faces.Count(); i++)
                {
                    List<Vertex> vertices_une_face = faces[i].GetVertex();
                    //Debug.Log(vertices_une_face.Count());
                    Vector3 total = new Vector3(0,0,0);
                    for (int w = 0; w < vertices_une_face.Count(); w++)
                    {
                        Vector3 pt = transform.TransformPoint(vertices_une_face[w].position);
                        total += pt;
                    }
   
                    Handles.Label((total) / 4.0f, "F" + faces[i].index, style);
                }
            }
            //edges
            if (drawEdges)
            {
                style.normal.textColor = Color.blue;
                for (int i = 0; i < edges.Count(); i++)
                {
                    Vector3 start = transform.TransformPoint(edges[i].startVertex.position);
                    Vector3 end = transform.TransformPoint(edges[i].endVertex.position);
                    Vector3 pos = Vector3.Lerp(start, end, 0.5f);

                    Handles.Label(pos, "e" + edges[i].index, style);
                    Gizmos.DrawLine(start,end);
                }
            }
        }
    }
}