using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs: EventArgs
    {
        public ClearCounter selectedCounter;
    }

    [SerializeField] private float  moveSpeed = 7f;
    [SerializeField] private LayerMask countersLayerMask;

    private bool isWalking;
    private Vector3 lastInteractDir;
    private ClearCounter selectedCounter;

    public static Player Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("There is more than one player instance");
        }

        Instance = this;
    }


    private void Start()
    {
        GameInput.Instance.OnInteractAction += Instance_OnInteractAction;
    }

    private void Instance_OnInteractAction(object sender, System.EventArgs e)
    {
        if(selectedCounter != null)
        {
            selectedCounter.Interact();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            // Attempt x movement 
            Vector3 moveDirX = new Vector3(moveDir.x, 0f, 0f).normalized;
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                // Attempt z movement 
                Vector3 moveDirZ = new Vector3(0f, 0f, moveDir.z).normalized;
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDir * moveDistance;
        }

        isWalking = moveDir != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float interactDistance = 2f;

        if(moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        RaycastHit raycastHit;
        if (Physics.Raycast(transform.position, lastInteractDir, out raycastHit, interactDistance, countersLayerMask)) {
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
            {
                // Has ClearCounter
                if(clearCounter != selectedCounter)
                {
                    SetSelectedCounter(clearCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void SetSelectedCounter(ClearCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    
}
