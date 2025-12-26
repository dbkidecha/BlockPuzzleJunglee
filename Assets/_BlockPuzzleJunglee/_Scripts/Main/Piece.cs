using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ArtboxGames;

public class Piece : MonoBehaviour {
    public int id;
    public Vector2 center;
    public List<Vector2> defaultPositions = new List<Vector2>();
    public List<Vector2> boardPositions = new List<Vector2>();
    public List<Vector2> tilePositions = new List<Vector2>();
    public List<Block> matches = new List<Block>();

    public List<Block> tiles = new List<Block>();
    public Vector2 bottomPosition;
    public Block bottomBackground;

    public enum Status {Dragging, OnBoard, OnBottom, OnTween };
    public Status status = Status.OnBottom;

    protected Vector3 beginPosition, beginTouchPosition;
    public bool isExtra = false;

    public GameObject shadows;

    private Vector3 upDelta;

    public Block tileCenter
    {
        get { return tiles[0]; }
    }

    private void Start()
    {
        foreach(Block tile in tiles)
        {
            tile.GetComponent<Image>().sprite = MonoUtils.instance.tileSprites[id];
        }
        UpdatePiece();
    }

    private Vector3 GetUpDelta()
    {
        float minY = float.MaxValue;
        foreach(var tile in tiles)
        {
            minY = Mathf.Min(minY, tile.transform.position.y);
        }

        return Vector3.up * (beginTouchPosition.y + 0.3f - minY);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (status != Status.Dragging) return;

        Vector3 moveDelta = Camera.main.ScreenToWorldPoint(Input.mousePosition) - beginTouchPosition;
        transform.position = beginPosition + upDelta + moveDelta;

        TileRegion.instance.CheckMatch(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (status != Status.OnBoard && status != Status.OnBottom || !GameState.canPlay) return;

        beginPosition = transform.position;
        beginTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        upDelta = GetUpDelta();

        iTween.ScaleTo(gameObject, Vector3.one, 0.06f);
        iTween.MoveTo(gameObject, beginPosition + upDelta, 0.06f);

        transform.SetParent(MonoUtils.instance.dragRegion);
        if (status == Status.OnBoard)
        {
            TileRegion.instance.ClearCovers(this);
        }

        status = Status.Dragging;

        matches.Clear();
        boardPositions.Clear();
        UpdatePiece();
        Sound.instance.Play(Sound.Others.Select);

        MainController.tutorialSteps?.Invoke(1);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (status != Status.Dragging) return;

        if (matches.Count == tiles.Count)
        {
            Sound.instance.Play(Sound.Others.OnBoard);
            iTween.MoveTo(gameObject, iTween.Hash("position", matches[0].transform.position, "speed", 10f, "oncomplete", "CompleteMoveToBoard"));

            MainController.tutorialSteps?.Invoke(2);
        }
        else
        {
            iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * Const.SCALED_TILE, "speed", 4f));
            iTween.MoveTo(gameObject, iTween.Hash("position", bottomBackground.transform.position, "speed", 15f, "oncomplete", "CompleteMoveToBottom_EndDrag"));

            MainController.tutorialSteps?.Invoke(0);
        }

        status = Status.OnTween;
        ResetMatchColor();
    }

    public void MoveToBottom()
    {
        if (status == Status.OnBoard)
        {
            transform.SetParent(MonoUtils.instance.dragRegion);
            TileRegion.instance.ClearCovers(this);
            iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * Const.SCALED_TILE, "speed", 4f));
            iTween.MoveTo(gameObject, iTween.Hash("position", bottomBackground.transform.position, "speed", 20f, "oncomplete", "CompleteMoveToBottom"));
            status = Status.OnTween;
        }
    }

    public void FadeIn()
    {
        foreach (Block tile in tiles) tile.GetComponent<Image>().canvasRenderer.SetAlpha(1);
    }

    public void ResetMatchColor()
    {
        foreach(Transform highlight in MonoUtils.instance.highlightsTransform)
        {
            if (highlight.gameObject.activeSelf)
                MainController.instance.GetComponent<Pooler>().Push(highlight.gameObject);
        }
    }

    public void HighlightMatchColor()
    {
        foreach(Block tile in matches)
        {
            GameObject highlight = MainController.instance.GetComponent<Pooler>().GetPooledObject();
            highlight.transform.SetParent(MonoUtils.instance.highlightsTransform);
            highlight.transform.localScale = Vector3.one;
            highlight.transform.position = tile.transform.position;
        }
    }

    private void CompleteMoveToBoard()
    {
        status = Status.OnBoard;
        transform.SetParent(MonoUtils.instance.piecesTransform);

        foreach(var tile in matches)
        {
            tile.hasCover = true;
            boardPositions.Add(tile.position);
        }

        UpdateTileBoardPosition();

        TileRegion.instance.CheckGameComplete();
        UpdatePiece();
        Utils.IncreaseNumMoves(GameState.chosenWorld, GameState.chosenLevel);
    }

    public void UpdateTileBoardPosition()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].boardPosition = boardPositions[i];
        }
    }

    private void CompleteMoveToBottom()
    {
        status = Status.OnBottom;
        transform.SetParent(MonoUtils.instance.piecesBottomTransform);
        UpdatePiece();
        FadeIn();
    }

    private void CompleteMoveToBottom_EndDrag()
    {
        CompleteMoveToBottom();
        Sound.instance.Play(Sound.Others.OnBottom);
    }

    public void UpdatePiece()
    {
        shadows.SetActive(status == Status.OnBottom);
    }
}
