# RPS
A simple Rock-Paper-Scissors simulation.
The icons move around and the loser in every collision is changed to the winner's type.

<img width="961" alt="image" src="https://user-images.githubusercontent.com/3811290/209578444-683c1997-9fa2-45fd-9c14-b607a69af453.png">

# Interesting elements

## Game partitioning helper class
The [GamePartitioningHelper](https://github.com/xnafan/RPS/blob/master/RPS/GamePartitioningHelper.cs) class splits the game area into sub-segments.
At the beginning of every update the game objects are put into lists depending on which rectangles they overlap.
This makes it faster to find collision candidates.

## Game object movement
The [GameObject](https://github.com/xnafan/RPS/blob/master/RPS/Model/GameObject.cs) stores a direction (in radian) for movement, every update that direction is moved a little bit randomly clockwise or counterclockwise. For every update the GameObject's new location is calculated, and if crosses the outside border of the game area a new direction is found, until a valid move can be performed.
