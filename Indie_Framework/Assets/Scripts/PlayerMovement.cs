using UnityEngine;

public class PlayerMovement 
{   
    private float speed = 5f;

    private InputReader inputReader;
    private Vector2 moveInput;
    private CharacterController controller;
    private Material material;
    private PlayerController pc;

    public PlayerMovement(PlayerController playerController)
    {
        pc = playerController;
        inputReader = pc.inputReader;
        controller = pc.controller;
    }
    
    public void DoEnable()
    {
        inputReader.MoveEvent += OnMove;
        inputReader.EnablePlayerInput();
    }
    
    public void DoDisable()
    {
        inputReader.MoveEvent -= OnMove;
    }
    
    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }
    
    public void DoUpdate()
    {
        Vector3 move = pc.transform.right * moveInput.x + pc.transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);
    }
}