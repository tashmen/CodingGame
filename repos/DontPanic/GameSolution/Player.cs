using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Algorithms.Graph;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    public enum ObjectType
    {
        Block = 0,
        Clone,
        Elevator,
        Exit,
        DecisionPoint
    }
    public class GameObject
    {
        public int Id;
        public int Floor;
        public int Location;
        public string Direction;
        public ObjectType Type;
        public GameObject(int id, int floor, int location, ObjectType type, string direction = "NONE")
        {
            Id = id;
            Floor = floor;
            Location = location;
            Direction = direction;
            Type = type;
        }
        public void Print()
        {
            Console.Error.WriteLine("Id: " + Id + " F: " + Floor + " L: " + Location + " T: " + Type.ToString());
        }
    }
    static void Main(string[] args)
    {
        Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();
        int objectId = 0;
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int nbFloors = int.Parse(inputs[0]); // number of floors
        int width = int.Parse(inputs[1]); // width of the area
        int nbRounds = int.Parse(inputs[2]); // maximum number of rounds
        int exitFloor = int.Parse(inputs[3]); // floor on which the exit is found
        int exitPos = int.Parse(inputs[4]); // position of the exit on its floor
        int nbTotalClones = int.Parse(inputs[5]); // number of generated clones
        int nbAdditionalElevators = int.Parse(inputs[6]); // ignore (always zero)
        int nbElevators = int.Parse(inputs[7]); // number of elevators
        Dictionary<int, List<int>> elevatorFloorToPosition = new Dictionary<int, List<int>>();
        for (int i = 0; i < nbElevators; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
            int elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor
            objects[++objectId] = new GameObject(objectId, elevatorFloor, elevatorPos, ObjectType.Elevator);
            objects[++objectId] = new GameObject(objectId, elevatorFloor + 1, elevatorPos, ObjectType.DecisionPoint);
            if (elevatorFloor > 1)
            {
                objects[++objectId] = new GameObject(objectId, elevatorFloor - 1, elevatorPos, ObjectType.DecisionPoint);
            }
            AddElevator(elevatorFloorToPosition, elevatorFloor, elevatorPos);
        }
        objects[++objectId] = new GameObject(objectId, exitFloor, exitPos, ObjectType.Exit);
        int exitObjectId = objectId;

        objects[++objectId] = new GameObject(objectId, exitFloor - 1, exitPos, ObjectType.DecisionPoint);


        foreach (GameObject obj in objects.Values.Where(go => go.Type == ObjectType.DecisionPoint))
        {
            if (objects.Values.Where(go => go.Type == ObjectType.Elevator && go.Floor == obj.Floor && go.Location == obj.Location).Any())
            {
                objects.Remove(obj.Id);
            }
        }
        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
            int clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
            string direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT
            GraphLinks links = new GraphLinks(false);
            if (cloneFloor != -1)
            {
                GameObject clone = new GameObject(++objectId, cloneFloor, clonePos, ObjectType.Clone, direction);
                AddLinks(links, objects, clone, direction);
                links.CalculateShortestPathsFromStartNode(clone.Id, 9999999);
                int nextObjectId;
                try
                {
                    nextObjectId = links.GetShortestPath(clone.Id, exitObjectId);
                    GameObject nextObject = objects[nextObjectId];
                    if (nextObject.Location > clone.Location && direction == "LEFT")
                    {
                        Console.WriteLine("BLOCK");
                    }
                    else if (nextObject.Location < clone.Location && direction == "RIGHT")
                    {
                        Console.WriteLine("BLOCK");
                    }
                    else if (nextObject.Location == clone.Location && nextObject.Type == ObjectType.DecisionPoint)
                    {
                        Console.WriteLine("ELEVATOR");
                        nextObject.Type = ObjectType.Elevator;
                    }
                    else
                    {
                        Console.WriteLine("WAIT");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WAIT");
                }
            }
            else
            {
                Console.WriteLine("WAIT");
            }
        }
    }
    public static void AddLinks(GraphLinks links, Dictionary<int, GameObject> objects, GameObject currentPoint, string direction)
    {
        currentPoint.Print();
        foreach (GameObject objOnSameFloor in objects.Values.Where(go => go.Floor == currentPoint.Floor))
        {
            int dist = Math.Abs(objOnSameFloor.Location - currentPoint.Location);
            if (objOnSameFloor.Location < currentPoint.Location && direction == "RIGHT")
            {
                dist += 3;
                direction = "LEFT";
            }
            else if (objOnSameFloor.Location > currentPoint.Location && direction == "LEFT")
            {
                dist += 3;
                direction = "RIGHT";
            }
            
            links.AddLink(currentPoint.Id, objOnSameFloor.Id, dist);
            GameObject theObj = objOnSameFloor;
            while (theObj.Type == ObjectType.Elevator)
            {
                GameObject nextFloor = objects.Values.Where(go => go.Floor == theObj.Floor + 1 && go.Location == theObj.Location).First();
                links.AddLink(theObj.Id, nextFloor.Id, 1);
                theObj = nextFloor;
            }
            if(theObj.Id != objOnSameFloor.Id)
            {
                AddLinks(links, objects, theObj, direction);
            }
            else//Wasn't an elevator, but it could be a decision point where an elevator is placed
            {
                if(objOnSameFloor.Type == ObjectType.DecisionPoint)
                {
                    GameObject nextFloor = objects.Values.Where(go => go.Floor == objOnSameFloor.Floor + 1 && go.Location == objOnSameFloor.Location).First();
                    links.AddLink(objOnSameFloor.Id, nextFloor.Id, 1);
                    AddLinks(links, objects, nextFloor, direction);
                }
            }
            
        }
    }
    public static void AddElevator(Dictionary<int, List<int>> elevatorFloorToPosition, int elevatorFloor, int elevatorPos)
    {
        if (elevatorFloorToPosition.ContainsKey(elevatorFloor))
        {
            elevatorFloorToPosition[elevatorFloor].Add(elevatorPos);
        }
        else
        {
            elevatorFloorToPosition[elevatorFloor] = new List<int>() { elevatorPos };
        }
    }
}