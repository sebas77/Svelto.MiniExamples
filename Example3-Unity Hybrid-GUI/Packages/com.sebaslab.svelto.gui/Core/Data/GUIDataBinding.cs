using System;

namespace Svelto.ECS.GUI
{
    delegate void DataBindingUpdate<T>(T value);

    struct GUIDataBinding
    {
        internal Delegate           callback;
        internal string             key;
        internal WidgetDataSource   dataSource;
    }
}