using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // Attach SpriteRenderer
    public Sprite[] frames; // Drag sprite sheet frames here
    public float frameRate = 10f; // Frames per second

    private float timer;
    private int currentFrame;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrame = (currentFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[currentFrame];
        }
    }
}
