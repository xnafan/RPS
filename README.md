# RPS
A Rock-Paper-Scissors simulation.
The icons move around and the loser in every collision is changed to the winner's type. You can toggle between random or hunt/flee behavior.

<img width="960" alt="image" src="https://user-images.githubusercontent.com/3811290/209653712-7dbaa279-54d0-43fc-8781-cfc5c717564e.png">


# Interesting elements

## Game partitioning helper class
The [GamePartitioningHelper](https://github.com/xnafan/RPS/blob/master/RPS/GamePartitioningHelper.cs) class splits the game area into sub-segments.
At the beginning of every update the game objects are put into lists depending on which rectangles they overlap.
This makes it faster to find collision candidates.

## Game object movement
The [GameObject](https://github.com/xnafan/RPS/blob/master/RPS/Model/GameObject.cs) stores a direction (in radian) for movement, every update that direction is moved a little bit randomly clockwise or counterclockwise. For every update the GameObject's new location is calculated, and if crosses the outside border of the game area a new direction is found, until a valid move can be performed.

# Keyboard overview

F5 : Restart game  
F11 : Toggle fullscreen  
'+' : Add 100 game objects and restart  
'-' : Remove 100 game objects and restart  
'I' : toggle intelligent behavior (hunt/flee)  
