using Svelto.ECS;

namespace Boxtopia.GUIs.Generic
{
    public interface IButtonClick
    {
        DispatchOnSet<ButtonEvents> buttonEvent { get; set; }
        
        ButtonEvents action { set; }
    }

    public interface IUIState
    {
        bool interactive { set; }
    }
}