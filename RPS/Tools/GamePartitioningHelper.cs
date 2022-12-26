using Microsoft.Xna.Framework;
using RPS.Model;
using System.Collections.Generic;
using System.Linq;

namespace RPS.Tools;
public class GamePartitioningHelper<T> where T : IBounded
{

    public Rectangle GameBounds { get; set; }
    public List<Rectangle> GameBoundsPartitions { get; set; } = new List<Rectangle>();

    public Dictionary<Rectangle, List<T>> PartitionContents { get; set; } = new Dictionary<Rectangle, List<T>>
        ();

    public GamePartitioningHelper(Rectangle gameBounds, int partitionsPerSide = 3)
    {
        GameBounds = gameBounds;
        var widthPartitions = partitionsPerSide;
        var heightPartitions = partitionsPerSide;
        var widthOfPartition = GameBounds.Width / widthPartitions;
        var heightOfPartition = GameBounds.Height / heightPartitions;
        for (int x = 0; x < widthPartitions; x++)
        {
            for (int y = 0; y < heightPartitions; y++)
            {
                var partition = new Rectangle(widthOfPartition * x, heightOfPartition * y, widthOfPartition, heightOfPartition);
                GameBoundsPartitions.Add(partition);
                PartitionContents[partition] = new List<T>();
            }
        }
    }


    public void Update(IEnumerable<T> objects)
    {
        ClearPartitionContents();
        foreach (var item in objects)
        {
            foreach (var partition in GetIntersections(item))
            {
                PartitionContents[partition].Add(item);
            }
        }
    }

    public void Remove(T objectToRemove)
    {
        GetIntersections(objectToRemove).ToList().ForEach(intersection => PartitionContents[intersection].Remove(objectToRemove));
    }
    public void ClearPartitionContents()
    {
        GameBoundsPartitions.ForEach(partition => PartitionContents[partition].Clear());
    }

    public IEnumerable<Rectangle> GetIntersections(T item)
    {
        foreach (var partition in GameBoundsPartitions)
        {
            if (item.GetBounds().Intersects(partition))
            {
                yield return partition;
            }
        }
    }

    public IEnumerable<T> GetCollisionCandidates(T item)
    {
        foreach (var partition in GetIntersections(item))
        {
            foreach (var gameObject in PartitionContents[partition])
            {
                yield return gameObject;
            }
        }
    }


    public IEnumerable<T> GetCollisions(T item)
    {
        foreach (var obj in GetCollisionCandidates(item))
        {
            if (item.GetBounds().Intersects(obj.GetBounds()) && !item.Equals(obj))
            {
                yield return obj;
            }
        }
    }

}