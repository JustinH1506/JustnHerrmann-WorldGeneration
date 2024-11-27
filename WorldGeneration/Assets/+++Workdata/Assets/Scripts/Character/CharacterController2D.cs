using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float stepDuration = 0.4f;
    [SerializeField] private Transform charVisual;
    [SerializeField] private Ease stepEase;
    [SerializeField] private Ease visualEase;
    
    private Vector2Int currentPos;
    private Vector2Int moveDirection;

    private bool isAllowedToMove = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (moveDirection != Vector2.zero && isAllowedToMove)
        {
            MoveCharacter();
        }
    }

    void OnMove(InputValue inputValue)
    {
        moveDirection = new Vector2Int(Mathf.RoundToInt(inputValue.Get<Vector2>().x), Mathf.RoundToInt(inputValue.Get<Vector2>().y));

        if (moveDirection.x > 0 )
        {
            charVisual.GetComponent<SpriteRenderer>().flipX = false;
        }
        else if(moveDirection.x < 0)
        {
            charVisual.GetComponent<SpriteRenderer>().flipX = true;

        }
    }

    void MoveCharacter()
    {
        Vector2Int newPos = currentPos + moveDirection;
        DungeonTile dungeonTile = TileCheck(newPos);
        if (!dungeonTile) return;
        
        if (dungeonTile.walkable)
        {
            isAllowedToMove = false;
            transform.DOMove(newPos.ToVector3(), stepDuration).SetEase(stepEase).OnComplete(() =>
            {
                currentPos = newPos;
                isAllowedToMove = true;
            });
            charVisual.DOLocalMoveY(0.3f, stepDuration / 2f).SetEase(Ease.InBack).SetLoops(2, LoopType.Yoyo);
        }
    }
    
    DungeonTile TileCheck(Vector2Int checkPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPosition.ToVector3() + Vector3.back * 0.2f, Vector3.forward);

        if (hit)
        {
            return Physics2D.Raycast(checkPosition.ToVector3() + Vector3.back * 0.2f, Vector3.forward).transform.GetComponent<DungeonTile>();
        }
        else
        {
            return null;
        }
    }
}
