using UnityEngine;

public class PlayerAttack
{
    private PlayerController pc;
    private InputReader inputReader;
    
    public PlayerAttack(PlayerController playerController)
    {
        pc = playerController;
        inputReader = pc.inputReader;
    }

    public void DoEnable()
    {
        inputReader.AttackEvent += OnAttack;
    }

    public void DoDisable()
    {
        inputReader.AttackEvent -= OnAttack;

    }

    private void OnAttack()
    {
        Debug.Log("Attack");
    }
}