﻿using UnityEngine;
using UnityEngine.UI;

public class NameListEntry : MonoBehaviour
{
    [SerializeField] private Text nameLabel;
    [SerializeField] private Text statusLabel;
    [SerializeField] private Image statusImage;

    [SerializeField] private Color statusWaitingColor;
    [SerializeField] private Color statusReadyColor;

    public string Name
    {
        get => nameLabel.text;
        set => nameLabel.text = value;
    }  
    
    private PlayerNetworkStatus _status;
    public PlayerNetworkStatus Status
    {
        get => _status;
        set {
            _status = value;
            statusLabel.text = _status.ToString();
            switch(_status)
            {
                case PlayerNetworkStatus.Waiting: statusImage.color = statusWaitingColor; break;
                case PlayerNetworkStatus.Ready: statusImage.color = statusReadyColor; break;
            }
        }
    }

    private void Start()
    {
        Status = PlayerNetworkStatus.Waiting;
    }
}

public enum PlayerNetworkStatus { Waiting, Ready };