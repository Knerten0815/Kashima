﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.PlayerLoop;
/*
*
* Class that overs Events to subscribe too. Can be accessed by other classes to trigger certain events that are subscribed too by all 
* needed parties. Also handles loading and saving game currently. Temporary, probably needs a better architecture and save options should 
* only be accessed by limited other classes and not everyone for example.
* 
*
*@Katja
*/
public class ManagementSystem : MonoBehaviour
{

    private static List<int> collectiblesGathered = new List<int>();
    private static int currentLevel = 0;
    private static int[] Highscore = { 0, 0, 0 };
    private static int unlockedLevels = 0;

    /*
    * Cheap and not perfect Singleton initialization. 
    *
    *
    *@Katja
    */
    private static ManagementSystem _instance;
    public static ManagementSystem Instance


    {
        get
        {
            if (_instance == null)
            {
                GameObject sSystem = new GameObject("ManagementSystem");
                sSystem.AddComponent<ManagementSystem>();
            }
            return _instance;
        }
    }


    /*
    *
    * Static function that when called adds the ID of the collectible to the currently gathered Collectibles
    *
    *@Katja
    */
    public static void AddCollectible(int n)
    {
        collectiblesGathered.Add(n);
    }

    /*
    * ScoreUp Event, can trigger the score going up but also things like Audio tracks for the pickUp
    *
    * @Katja
    */

    public delegate void scoreUp(int score);
    public static event scoreUp pickUpHit;
    /*
    *
    * Static function that when called triggers the event for scoreUp. 
    *
    * @Katja
    */
    public static void pickUp(int scoreValue)
    {
        if (pickUpHit != null)
        pickUpHit(scoreValue);
    }


    public delegate void levelLoad(int unlockedLevels, int currentLevel);
    
    public static event levelLoad levelLoadMethod;
    /*
    *
    *   CollectibleLoad Event that currently is just there to notify rare what collectibles have already been Loaded could be expanded to other things
    *   that are loaded at the start
    *
    * @Katja
    */
    public delegate void pickupLoad(int collectibleID);
    public static event pickupLoad collectibleOnLoad;

    /*
    *
    *   LoadGame is called before start and then rare pick ups are notified whether they were already gathered and have to be set inactive
    *   Does not work in htis prototype version currently:
    *   NullReferenceException: Object reference not set to an instance of an object
    *   ManagementSystem.Awake () (at Assets/Scripts/ManagementSystem.cs:100)
    *   No fucking clue why honestly. 
    * @Katja
    */

    public void Start()
    {

        LoadGame();
        // collectiblesGathered.Clear();
        //collectiblesGathered.Add(1);
        //collectiblesGathered.Add(2);
        //collectiblesGathered.Add(0);
        //unlockedLevels = 2;
        //currentLevel = 2;

        if (collectibleOnLoad != null)
        {
            foreach (int ID in collectiblesGathered)
            {
                collectibleOnLoad(ID);
            }
        }
    
        levelLoadMethod(unlockedLevels, currentLevel);
    

    }



    public delegate void healthCurrent(int health);
    public static event healthCurrent healthPassOn;

    public static void updatePlayerHealth(int health)
    {
        Debug.Log("TEST hPuP");
        if (healthPassOn != null)
            healthPassOn(health);
    }

    public static void updateTime(float time)
    {
        Debug.Log(time);
    }

    public delegate void pickupHealth();
    public static event pickupHealth healthPickUpHit;
    public static void hUp()
    {
        //Debug.Log("TEST hUp");
        healthPickUpHit();
    }

    public delegate void collected();
    public static event collected collectedScroll;

    public static void collectedUpdate()
    {
        Debug.Log("TEST cUp");

        collectedScroll();
    }


    public delegate void pickupTime();
    public static event pickupTime timePickUpHit;

    public static void tUp()
    {
        //Debug.Log("TEST tUp");
        timePickUpHit();
    }


    /*
    * CreateSaveGameObject creates an empty Save object and overrides it's collectiblesGathered attribute with the current
    * ManagementSystem version
    *
    *@Katja
    */
    private static Save CreateSaveGameObject()
    {
        Save save = new Save();
        collectiblesGathered = new List<int>();

        collectiblesGathered.Add(0);
        collectiblesGathered.Add(1);
        collectiblesGathered.Add(2);
        collectiblesGathered.Add(3);
        collectiblesGathered.Add(4);
        collectiblesGathered.Add(5);
        collectiblesGathered.Add(6);
        collectiblesGathered.Add(7);
        collectiblesGathered.Add(8);
        save.collectiblesGathered = collectiblesGathered;
        save.currentLevel = 1;
        save.Highscore = Highscore;
        save.unlockedLevels = 2;
        return save;

    }
   
    /*
     * 
     * Creates binary data from Save object and saves it in file titles /gamesave.save
     * 
     * @Katja
     */
    public static void SaveGame()
    {
        
        Save save = CreateSaveGameObject();
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, save);
        file.Close();
    }

   
    public static void ResetGameSave()
    {
        collectiblesGathered = new List<int>();
        currentLevel = 0;
        Highscore = new int[]{ 0, 0, 0 };
        unlockedLevels = 0;
        Save save = CreateSaveGameObject();
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, save);
        file.Close();
    }
    /*
     * 
     * Checks first if save File exists. If it exists it translates the binary format back to a Save object instance which
     * is then saved in the management systems attribute of collectiblesGathered
     * 
     * @Katja
    */
    public void LoadGame()
    {

        if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
        {

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();
           // List<int> collectiblesGatheredDEBUG = new List<int>();
           // collectiblesGatheredDEBUG.Add(2);
            collectiblesGathered = save.collectiblesGathered;
            Highscore = save.Highscore;
            unlockedLevels = save.unlockedLevels;
            currentLevel = save.currentLevel;
            Debug.Log("Does it load");
        }
        else { 
            SaveGame();
            }

    }
}
