namespace NetEase.Behaviors;

using System.Windows;

/// <summary>
/// WPF中的绑定代理类（BindingProxy），继承自Freezable
/// 核心作用：解决WPF中"数据上下文（DataContext）断裂"场景下的绑定问题
/// 例如：ContextMenu、DataTemplate、Style等元素，其DataContext默认不继承上级容器，可通过此类传递数据
/// </summary>
public class BindingProxy : Freezable
{
    /// <summary>
    /// 重写Freezable的抽象方法（必须实现，因Freezable是抽象类）
    /// Freezable用于创建可"冻结"（不可修改）的对象，此处仅利用其"可作为资源且能保留DataContext"的特性，无需实际冻结逻辑
    /// </summary>
    /// <returns>返回当前BindingProxy类的新实例，满足Freezable的对象创建规范</returns>
    protected override Freezable CreateInstanceCore()
    {
        // 返回新的BindingProxy实例，Freezable内部机制会调用此方法创建对象副本（如需要时）
        return new BindingProxy();
    }

    /// <summary>
    /// 定义依赖属性（DataProperty）：用于存储需要传递的DataContext或任意数据对象
    /// 依赖属性支持WPF的绑定系统、属性变更通知等核心能力，是实现数据传递的关键
    /// </summary>
    // DependencyProperty.Register参数说明：
    // 1. "Data"：属性名称（对外暴露的属性名）
    // 2. typeof(object)：属性存储的数据类型（支持任意对象，灵活适配不同场景）
    // 3. typeof(BindingProxy)：依赖属性所属的所有者类型（当前BindingProxy类）
    // 4. new UIPropertyMetadata(null)：属性元数据，设置默认值为null（无初始数据）
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

    /// <summary>
    /// 包装依赖属性的CLR属性（方便代码中直接访问DataProperty）
    /// 对外提供普通属性的访问方式，内部通过GetValue/SetValue操作依赖属性
    /// </summary>
    public object Data
    {
        get { return GetValue(DataProperty); } // 从依赖属性获取当前值
        set { SetValue(DataProperty, value); } // 给依赖属性设置新值
    }
}