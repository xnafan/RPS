# RPS
A simple Rock-Paper-Scissors simulation.
The icons move around and the loser in every collision is changed to the winner's type.

<img width="961" alt="image" src="https://user-images.githubusercontent.com/3811290/209578444-683c1997-9fa2-45fd-9c14-b607a69af453.png">

# Interesting elements
The [GamePartitioningHelper](https://github.com/xnafan/RPS/blob/master/RPS/GamePartitioningHelper.cs) class splits the game area into sub-segments.
At the beginning of every update the game objects are put into lists depending on which rectangles they overlap.
This makes it faster to find collision candidates.
