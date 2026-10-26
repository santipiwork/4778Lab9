using UnityEngine;
using UnityEngine.InputSystem;

public class CarnivalGame : MonoBehaviour
{
    [SerializeField] private Texture2D shipImage;
    [SerializeField] private Texture2D bulletImage;
    [SerializeField] private Texture2D enemyImage;
    private Transform player;
    private BulletPool pool;
    private ScoreManager score;
    private Target[] targets;
    private float nextShot;
    private GUIStyle label;

    private Sprite MakeSprite(Texture2D image)
    {
        return Sprite.Create(image, new Rect(0, 0, image.width, image.height),
            new Vector2(0.5f, 0.5f), 32f);
    }

    private void Start()
    {
        Camera camera = Camera.main;
        camera.transform.SetPositionAndRotation(new Vector3(0, 0, -10), Quaternion.identity);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().renderPostProcessing = false;

        player = new GameObject("Player Ship").transform;
        player.SetParent(transform);
        player.position = new Vector3(0, -4f, 0);
        player.gameObject.AddComponent<SpriteRenderer>().sprite = MakeSprite(shipImage);
        pool = new BulletPool(MakeSprite(bulletImage), 10, transform);
        score = gameObject.AddComponent<ScoreManager>();

        Sprite enemy = MakeSprite(enemyImage);
        targets = new[]
        {
            new TargetBuilder(enemy).WithSpeed(1f).WithSize(1.5f).WithPoints(10)
                .Build("Large - 10 points", new Vector3(-3, 0, 0), transform),
            new TargetBuilder(enemy).WithSpeed(2f).WithSize(1f).WithPoints(20)
                .Build("Medium - 20 points", new Vector3(0, 1.5f, 0), transform),
            new TargetBuilder(enemy).WithSpeed(3f).WithSize(0.7f).WithPoints(30)
                .Build("Small - 30 points", new Vector3(3, 3f, 0), transform)
        };
        foreach (Target target in targets) score.Observe(target);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        float move = 0;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move--;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move++;
        float edge = Mathf.Max(0, Camera.main.orthographicSize * Camera.main.aspect - 0.4f);
        Vector3 position = player.position + Vector3.right * (move * 6f * Time.deltaTime);
        position.x = Mathf.Clamp(position.x, -edge, edge);
        player.position = position;
        if (keyboard.spaceKey.isPressed && Time.time >= nextShot)
        {
            pool.Fire(player.position + Vector3.up * 0.6f);
            nextShot = Time.time + 0.18f;
        }
        if (keyboard.rKey.wasPressedThisFrame)
        {
            pool.Reset();
            score.ResetScore();
            foreach (Target target in targets) target.ResetTarget();
            player.position = new Vector3(0, -4f, 0);
            nextShot = 0;
        }
    }

    private void OnGUI()
    {
        if (score == null) return;
        if (label == null) label = new GUIStyle(GUI.skin.label) { fontSize = 20 };
        label.normal.textColor = Color.white;
        GUI.Label(new Rect(15, 10, 450, 30), "Score: " + score.Score, label);
        GUI.Label(new Rect(15, 40, 450, 30), "Ammo: " + pool.Ammo + " / " + pool.Capacity, label);
        if (pool.Ammo == 0 && pool.ActiveCount == 0)
            GUI.Label(new Rect(15, 70, 450, 30), "Out of ammo! Press R to restart.", label);
        GUI.Label(new Rect(15, Screen.height - 60, Screen.width, 30), "A/D or arrows: move    Space: shoot    R: reset", label);
        GUI.Label(new Rect(15, Screen.height - 32, Screen.width, 30), "Large: 10 points    Medium: 20    Small: 30", label);
    }
}
