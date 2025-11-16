using System;
using UnityEngine;

public class PlayerController: MonoBehaviour
{
    public InputReader inputReader;
    [HideInInspector] public CharacterController controller;

    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = new PlayerMovement(this);
        playerAttack = new PlayerAttack(this);
    }

    private void OnEnable()
    {
        playerMovement.DoEnable();
        playerAttack.DoEnable();
    }

    private void OnDisable()
    {
        playerMovement.DoDisable();
        playerAttack.DoDisable();
    }

    private void Update()
    {
        playerMovement.DoUpdate();
    }
}