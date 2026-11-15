using UnityEngine;

// 可以被射线选中的物体的基类。RayPointer 只认这个类型：
// 射线进入/离开时调用 OnHoverEnter/OnHoverExit，指着它扣下扳机时调用 OnSelect。
public abstract class Pointable : MonoBehaviour
{
    public virtual void OnHoverEnter() { }
    public virtual void OnHoverExit() { }
    public abstract void OnSelect();
}
