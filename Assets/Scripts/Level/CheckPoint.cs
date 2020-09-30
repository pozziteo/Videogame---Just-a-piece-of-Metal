﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == PlayerController.Player.gameObject)
        {
            PlayerController.Player.SetCurrentCheckpoint(gameObject);
        }
    }
}
