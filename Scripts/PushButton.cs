using UnityEngine;

// 面板上的一个按键，只有松开和按下两个状态，默认松开。
// 每被选中一次切换一次状态：按下时换成按下的颜色，并沿自身 +z（背离用户的方向）平移 pressDepth，
// 看上去是凹进去了；再按一次换回原色并回到原位。
// 射线指着它时颜色变亮作为悬停提示，这样选中前就能看出射线指着哪个键。
// 三种颜色用的是 ButtonPanel 共享的三个材质，换的是 sharedMaterial，600 个按键也不会各自复制一份材质。
public class PushButton : Pointable
{
    public bool IsPressed { get; private set; }

    ButtonPanel panel;
    Renderer rend;
    Vector3 restLocalPos;
    float pressDepth;
    bool hovered;

    public void Init(ButtonPanel owner, float depth)
    {
        panel = owner;
        rend = GetComponent<Renderer>();
        restLocalPos = transform.localPosition;
        pressDepth = depth;
        Refresh();
    }

    public override void OnHoverEnter() { hovered = true; Refresh(); }
    public override void OnHoverExit() { hovered = false; Refresh(); }

    public override void OnSelect()
    {
        IsPressed = !IsPressed;
        // localPosition 在父物体（面板）坐标系下，所以要把按键自己的 +z 换算到父坐标系里再平移
        Vector3 back = transform.localRotation * Vector3.forward;
        transform.localPosition = IsPressed ? restLocalPos + back * pressDepth : restLocalPos;
        Refresh();
        panel.OnButtonToggled(IsPressed);
    }

    void Refresh()
    {
        rend.sharedMaterial = panel.MaterialFor(IsPressed, hovered);
    }
}
