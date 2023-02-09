# Geometric_Modeling


Projet Geometric_Modeling : 

	Gabriel Leroux - Hugo Henriques - Rayen Soltani 
 
La scène:
 
        Nous avons une scène qui comporte un cube, une chips, un octogone et une patate (sphère modifiée).
        Lorsqu'on lance la scène, ces 4 objets sont subdivisés 3 fois avant de revenir à leur état initial grâce au script Subdivide.

Remarques : 

    Nous n'avons pas de remarque particulière sur le projet. Dans l'ensemble il nous a plu. 
    De la même manière que l'unité mathématiques pour la 3D, il était très intéressant de faire des mathématiques appliquées à la géométrie dans l'espace. En effet, 
    cela est bien plus complexe et difficile à appréhender que ce que nous pensions au début de l'unité. 
    Pour ce qui est du projet en lui même, il n'y a pas de limitations ni de bugs connus à l'heure actuelles malgré les multiples tests.


Limitations :
 
    Nous avons pu noter que nos alogrithmes avaient une certaine limite lors de la compilation. 
    Le programme n'arrive pas à compiler à partir de 5 subdivisions simultanées. 
    Nous avons aussi pu voir que sur un objet unique, nous n'avons pas réussi à faire 10 subdivisions d'affilé. Nous avons fait le test sur un cube et la scène 
    charge indéfiniment. 

	Nous avons essayé de lisser un maximum le code avec des commentaires afin de vous faciliter la lecture, même si celle-ci ne reste pas évidente compte tenu de la 
    densité et de la complexité du projet. 

Améliorations :

	En ce qui concerne les voies d'amélioration nous avons mené à terme ce projet, nous pensons que les voies d'améliorations porteraient sur l'optimisation du code. Notamment la méthode de création 
    des nouveaux points.


Répartition du travail : 

	- Formes du TD1 : Box et Chips (Rayen et Hugo), Polygone régulier et Pacman (Gabriel)
	- Script WingedEdge.cs (Gabriel)
	- Script HalfEdge.cs (Gabriel)
	- TD2 Méthodes nécessaires à l'algorithme de subdivision : 
		- Méthode générale de subdivision (Hugo)
		- Méthode de création des nouveaux points (Hugo et Rayen)
		- Split des edges (Rayen)
		- Split des faces (Hugo)
	- Script Subdivide.cs (Gabriel)
 
Lors des développements des méthodes de création des nouveaux points ainsi que des splits nous avons été guidé par Hugo Zhang qui nous a permis de mieux 
comprendre ces méthodes (grâce à ses super dessins surtout !).
