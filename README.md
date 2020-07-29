# RayCast
A GUI for rendering a 3D raycasting perspective that corresponds to a 2D map.

## How It Works
The program is a Windows Forms App (.NET Framework). When running *Program.cs*, the user can interact with a GUI that shows two panels: a two-dimensional map that gives an overview and a pseudo-three-dimensional render of the corresponding perspective. 
The map consists of a space filled with boundaries (walls) and a moveable source (which represents the point of view).

The following interactive components are available:
* Change position and direction using the WASD-keys.
* Change viewing direction using the mouse-wheel (only when mouse hovers over the 3D panel).
* Adjust the field of vision by changing the angle using the first trackbar.
* Adjust the height of the boundaries using the second trackbar.
* Adjust the viewing distance using the third trackbar.
* RESET: reset all trackbars to default values.
* ADD WALLS: insert boundaries in the 2D map by mouse-dragging.
* REMOVE WALLS: delete boundaries in the 2D map by mouse-clicking on them.
* CLEAR: remove all boundaries except the outermost walls.
* RANDOM: replace all current boundaries by five randomly placed walls.

## Screenshot
![screenshot](/screenshot.png)

## Authors
* Stan Verlaan

## Acknowledgements
This project was inspired by The Coding Train's coding challenge #145 and #146. See the following Youtube videos:
* Coding Challenge #145: 2D Raycasting (https://www.youtube.com/watch?v=TOEi6T2mtHo)
* Coding Challenge #146: Rendering Raycasting (https://www.youtube.com/watch?v=vYgIKn7iDH8)

The following code-snippet from user Robin Andersson on StackOverflow helped with the mouse-boundary collision-detection:
* https://stackoverflow.com/questions/907390/how-can-i-tell-if-a-point-belongs-to-a-certain-line

Also see the following Wikipedia-pages:
* Ray casting (https://en.wikipedia.org/wiki/Ray_casting)
* Line–line intersection (https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection)
