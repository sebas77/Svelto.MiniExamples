using System;
using UnityEngine;

public class ListToPopupAttribute : PropertyAttribute
{
    public Type   classType;
    public string listName;
    
    public ListToPopupAttribute(Type classType, string listName)
    {
        this.classType = classType;
        this.listName  = listName;
    }
}