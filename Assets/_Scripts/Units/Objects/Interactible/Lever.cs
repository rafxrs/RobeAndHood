using System.Collections;
using System.Collections.Generic;
using _Scripts.Units.Objects;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public Action action;
    public bool leverState=false;
    public GameObject[] doActionOn;

    float _timeBetweenSwitch = 1f;
    float _nextSwitch = -1f;
    Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void OntriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Weapon")
        {
            // ScriptableWeapon w = other.GetComponent<ScriptableWeapon>();
            SwitchLeverState();
        }
    }

    public void SwitchLeverState()
    {
        if (Time.time > _nextSwitch)
        {
            leverState = !leverState;
            _animator.SetBool("State", leverState);
            DoAction();
            _nextSwitch = Time.time + _timeBetweenSwitch;
        }
        else 
        {
            return;
        }
        
        
    }

    [System.Serializable]
    public enum Action
    {
        Door,
        Platform,
        MovingPlatform,
    }

    void DoAction()
    {
        switch (action)
        {
            case Action.Door:
                foreach (GameObject gobj in doActionOn)
                {
                    gobj.SetActive(false);
                }
                break;
            case Action.Platform:
                foreach (GameObject gobj in doActionOn)
                {
                    gobj.SetActive(leverState);
                }
                break;
            case Action.MovingPlatform :
                foreach (GameObject gobj in doActionOn)
                {
                    gobj.GetComponent<MovingPlatform>().move = leverState;
                }
                break;
        }
        
    }
    

}
