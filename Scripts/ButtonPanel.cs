using UnityEngine;

// 按键面板 + 计数器。运行时用代码生成 columns × rows 个紧密排列的按键，并在旁边生成一块计数面板，
// 显示当前处于按下状态的按键数量。
//
// 两种排布：
// Flat：平面网格。本物体的位置就是网格中心，按键排在本物体的局部 XY 平面上，局部 +z 指向背离用户的一侧，
//       所以把本物体放在用户正前方、不旋转即可。作业里 4×3 的面板用这种（columns=4, rows=3）。
// Sphere：排在球面上。本物体的位置是球心，应放在用户头部附近（例如 (0, 1.5, 0)），
//       按键分布在以局部 +z 为中心、水平张角 horizontalSpan、竖直张角 verticalSpan 的一块球面上，
//       每个按键都朝向球心，也就是朝向用户。作业里 30×20 的曲面面板用这种。
//
// 球面上的紧密排列：第 i 行的仰角为 φ，第 j 列的方位角为 θ，相邻列相差 Δθ，相邻行相差 Δφ。
// 按键中心放在 R·(cosφ·sinθ, sinφ, cosφ·cosθ)。同一行相邻两个按键中心的距离（弦长）是 2R·cosφ·sin(Δθ/2)，
// 相邻两行是 2R·sin(Δφ/2)，按键的宽和高就取这两个值再乘一个略小于 1 的系数留出缝。
// 越靠近上下边缘 cosφ 越小，按键会相应变窄，这样每一行都正好排满、没有重叠。
public class ButtonPanel : MonoBehaviour
{
    public enum Layout { Flat, Sphere }

    [Header("排布")]
    public Layout layout = Layout.Sphere;
    public int columns = 30;
    public int rows = 20;

    [Header("Flat 参数（米）")]
    public float buttonWidth = 0.14f;
    public float buttonHeight = 0.09f;
    public float gap = 0.006f;

    [Header("Sphere 参数")]
    public float radius = 2.5f;
    [Range(10f, 180f)] public float horizontalSpan = 110f;
    [Range(10f, 120f)] public float verticalSpan = 60f;
    [Range(0.5f, 1f)] public float fill = 0.94f;

    [Header("按键")]
    public float buttonDepth = 0.04f;
    [Tooltip("按下后向后平移的距离，占按键厚度的比例")]
    [Range(0.1f, 1f)] public float pressRatio = 0.6f;
    public Color releasedColor = new Color(0.85f, 0.55f, 0.25f);
    public Color pressedColor = new Color(0.9f, 0.1f, 0.1f);
    [Range(0f, 1f)] public float hoverLighten = 0.35f;

    int pressedCount;
    TextMesh counterText;
    Material released, releasedHover, pressed, pressedHover;

    void Start()
    {
        CreateMaterials();
        if (layout == Layout.Flat) BuildFlat();
        else BuildSphere();
        UpdateCounter();
    }

    public void OnButtonToggled(bool nowPressed)
    {
        pressedCount += nowPressed ? 1 : -1;
        UpdateCounter();
    }

    public Material MaterialFor(bool isPressed, bool hovered)
    {
        if (isPressed) return hovered ? pressedHover : pressed;
        return hovered ? releasedHover : released;
    }

    void BuildFlat()
    {
        float stepX = buttonWidth + gap;
        float stepY = buttonHeight + gap;
        float totalW = columns * stepX - gap;
        float totalH = rows * stepY - gap;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                float x = -totalW / 2 + buttonWidth / 2 + j * stepX;
                float y = totalH / 2 - buttonHeight / 2 - i * stepY;
                CreateButton(i, j, new Vector3(x, y, 0f), Quaternion.identity,
                             new Vector3(buttonWidth, buttonHeight, buttonDepth));
            }
        }

        // 计数面板放在网格左侧，与网格同一平面
        float boardSize = Mathf.Max(0.3f, totalH);
        CreateCounter(new Vector3(-totalW / 2 - 0.05f - boardSize / 2, 0f, 0f), Quaternion.identity, boardSize);
    }

    void BuildSphere()
    {
        float dTheta = horizontalSpan / columns * Mathf.Deg2Rad;
        float dPhi = verticalSpan / rows * Mathf.Deg2Rad;
        float buttonH = 2f * radius * Mathf.Sin(dPhi / 2f) * fill;

        for (int i = 0; i < rows; i++)
        {
            // 第 0 行在最上面
            float phi = (verticalSpan / 2f - (i + 0.5f) * verticalSpan / rows) * Mathf.Deg2Rad;
            float buttonW = 2f * radius * Mathf.Cos(phi) * Mathf.Sin(dTheta / 2f) * fill;

            for (int j = 0; j < columns; j++)
            {
                float theta = (-horizontalSpan / 2f + (j + 0.5f) * horizontalSpan / columns) * Mathf.Deg2Rad;
                Vector3 dir = SphereDir(theta, phi);
                // LookRotation(dir) 让按键的 +z 沿球心指向外，也就是背离用户，按下时往这个方向平移
                CreateButton(i, j, dir * radius, Quaternion.LookRotation(dir, Vector3.up),
                             new Vector3(buttonW, buttonH, buttonDepth));
            }
        }

        // 计数面板放在球面上、按键区左侧外面一点
        float boardTheta = (-horizontalSpan / 2f - 14f) * Mathf.Deg2Rad;
        Vector3 boardDir = SphereDir(boardTheta, 0f);
        CreateCounter(boardDir * radius, Quaternion.LookRotation(boardDir, Vector3.up), 0.8f);
    }

    static Vector3 SphereDir(float theta, float phi)
    {
        return new Vector3(Mathf.Cos(phi) * Mathf.Sin(theta), Mathf.Sin(phi), Mathf.Cos(phi) * Mathf.Cos(theta));
    }

    void CreateButton(int row, int col, Vector3 localPos, Quaternion localRot, Vector3 size)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);   // 自带 BoxCollider，射线能打到
        go.name = $"Button_{row}_{col}";
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale = size;
        go.AddComponent<PushButton>().Init(this, buttonDepth * pressRatio);
    }

    void CreateCounter(Vector3 localPos, Quaternion localRot, float size)
    {
        GameObject root = new GameObject("Counter");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = localPos;
        root.transform.localRotation = localRot;

        // 深色底板，稍微放在文字后面；去掉碰撞体，不挡射线
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Quad);
        board.name = "Board";
        Destroy(board.GetComponent<Collider>());
        board.transform.SetParent(root.transform, false);
        board.transform.localPosition = new Vector3(0f, 0f, 0.01f);
        board.transform.localScale = new Vector3(size, size * 0.75f, 1f);
        Material boardMat = new Material(released);
        boardMat.color = new Color(0.12f, 0.12f, 0.15f);
        board.GetComponent<Renderer>().sharedMaterial = boardMat;

        GameObject text = new GameObject("Text");
        text.transform.SetParent(root.transform, false);
        counterText = text.AddComponent<TextMesh>();
        Font font = BuiltinFont();
        counterText.font = font;
        text.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        counterText.anchor = TextAnchor.MiddleCenter;
        counterText.alignment = TextAlignment.Center;
        counterText.fontSize = 120;                 // 字号大、characterSize 小，文字更清晰
        counterText.characterSize = size * 0.012f;
        counterText.color = Color.white;
    }

    void UpdateCounter()
    {
        if (counterText != null) counterText.text = $"Pressed\n{pressedCount}";
    }

    void CreateMaterials()
    {
        // 从一个临时立方体上拿默认材质作模板，内置管线和 URP 下都是可用的着色器，打包到头显上也不会丢
        GameObject probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Material template = probe.GetComponent<Renderer>().sharedMaterial;
        released = MakeMaterial(template, releasedColor);
        releasedHover = MakeMaterial(template, Color.Lerp(releasedColor, Color.white, hoverLighten));
        pressed = MakeMaterial(template, pressedColor);
        pressedHover = MakeMaterial(template, Color.Lerp(pressedColor, Color.white, hoverLighten));
        Destroy(probe);
    }

    static Material MakeMaterial(Material template, Color c)
    {
        Material m = new Material(template);
        m.color = c;
        return m;
    }

    static Font BuiltinFont()
    {
#if UNITY_2022_2_OR_NEWER
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
    }
}
