using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : EventTrigger
{
    private bool selected;
    private Vector2 offset;
    private GamePiece gamePiece;

    private void Awake()
    {
        gamePiece = GetComponent<GamePiece>();    
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && selected)
        {
            gamePiece.RotatePiece();
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        selected = true;
        offset = eventData.position - new Vector2(transform.position.x, transform.position.y);
        gamePiece.BeginDrag();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        selected = false;
        gamePiece.TryPlacePiece();
    }

    public override void OnDrag(PointerEventData eventData)
    {
        transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - offset;
    }
}
