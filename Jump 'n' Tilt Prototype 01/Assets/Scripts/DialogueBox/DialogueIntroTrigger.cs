﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author:Michelle Limbach
public class DialogueIntroTrigger : MonoBehaviour
{
    //Method to trigger the first paragraph in the Intro scene
    void Start()
    {
        gameObject.GetComponent<DialogueTrigger>().TriggerDialogue();
    }
}
