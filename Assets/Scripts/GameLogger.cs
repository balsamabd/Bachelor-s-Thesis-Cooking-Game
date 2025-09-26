using UnityEngine;
using System.IO;

public static class GameLogger
{
    public static int correctOrders = 0;
    public static int wrongOrders = 0;
    public static int spawnedOrders = 0;   
    public static int trashedObjects = 0;  

    public static void LogResult(bool isCorrect)
    {
        if (isCorrect) correctOrders++;
        else wrongOrders++;
    }

    public static void LogOrderSpawned()
    {
        spawnedOrders++;
    }

    public static void LogObjectTrashed()
    {
        trashedObjects++;
    }

    public static void SaveResults()
    {
        string path = Application.persistentDataPath + "/results.txt";
        string data =
            $"Correct Orders: {correctOrders}\n" +
            $"Wrong  Orders: {wrongOrders}\n" +
            $"Orders Spawned: {spawnedOrders}\n" +
            $"Objects Trashed: {trashedObjects}";
        File.WriteAllText(path, data);
        Debug.Log($"Results saved to: {path}");
    }

    public static void Reset()
    {
        correctOrders = 0;
        wrongOrders = 0;
        spawnedOrders = 0;
        trashedObjects = 0;
    }
}
