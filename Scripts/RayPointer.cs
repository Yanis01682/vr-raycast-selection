using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// 光线投射指向。挂在 OVRCameraRig/TrackingSpace 下的 LeftHandAnchor 或 RightHandAnchor 上，
// 每帧从手柄位置沿手柄朝向（transform.forward）发一条射线，用 Physics.Raycast 求第一个命中的物体。
// 命中带 Pointable 组件的物体时让它高亮，此时扣下食指扳机就对它调用 OnSelect。
// 扳机通过 UnityEngine.XR 的 InputDevices 读取，不依赖 Oculus Integration 里的 OVRInput，
// 用 Oculus XR Plugin 或 OpenXR 都能读到。
// 没有连头显时（直接在编辑器里点 Play），退化为用鼠标从主相机发射线、左键点击，方便调试。
[RequireComponent(typeof(LineRenderer))]
public class RayPointer : MonoBehaviour
{
    public enum Hand { Left, Right }

    [Tooltip("读哪只手柄的扳机，挂在 LeftHandAnchor 上时会自动设为 Left")]
    public Hand hand = Hand.Right;
    public float maxDistance = 20f;
    public Color idleColor = new Color(1f, 0.2f, 0.2f);
    public Color hitColor = new Color(0.2f, 1f, 0.3f);
    public float lineWidth = 0.006f;

    LineRenderer line;
    Pointable current;
    bool triggerWasDown;
    readonly List<InputDevice> devices = new List<InputDevice>();

    void Reset()
    {
        if (name.Contains("Left")) hand = Hand.Left;
    }

    void Awake()
    {
        if (name.Contains("Left")) hand = Hand.Left;
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth * 0.5f;
        // Sprites/Default 支持顶点色，射线颜色靠它显示；万一打包时被剔除，就保留 LineRenderer 原有材质
        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader != null) line.material = new Material(lineShader);
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    void Update()
    {
        bool xr = TryGetHandDevice(out InputDevice device);
        Ray ray;
        bool pressed;

        if (xr)
        {
            ray = new Ray(transform.position, transform.forward);
            device.TryGetFeatureValue(CommonUsages.triggerButton, out bool down);
            pressed = down && !triggerWasDown;   // 只在按下的那一帧触发一次
            triggerWasDown = down;
        }
        else
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            ray = cam.ScreenPointToRay(Input.mousePosition);
            pressed = Input.GetMouseButtonDown(0);
        }

        Pointable target = null;
        Vector3 end = ray.origin + ray.direction * maxDistance;
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            end = hit.point;
            target = hit.collider.GetComponentInParent<Pointable>();
        }

        if (target != current)
        {
            if (current != null) current.OnHoverExit();
            if (target != null) target.OnHoverEnter();
            current = target;
        }
        if (pressed && current != null) current.OnSelect();

        // 鼠标调试时射线从相机中心出发，画出来只是一个点，没有意义，所以只在 XR 下画线
        line.enabled = xr;
        if (xr)
        {
            line.SetPosition(0, ray.origin);
            line.SetPosition(1, end);
            Color c = current != null ? hitColor : idleColor;
            line.startColor = c;
            line.endColor = c;
        }
    }

    bool TryGetHandDevice(out InputDevice device)
    {
        XRNode node = hand == Hand.Left ? XRNode.LeftHand : XRNode.RightHand;
        InputDevices.GetDevicesAtXRNode(node, devices);
        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i].isValid)
            {
                device = devices[i];
                return true;
            }
        }
        device = default;
        return false;
    }
}
