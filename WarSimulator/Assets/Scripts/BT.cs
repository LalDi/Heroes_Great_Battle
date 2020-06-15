using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Node
{
    public abstract bool Invoke();
}

public class Composite : Node
{
    public override bool Invoke()
    {
        throw new NotImplementedException();
    }

    public void AddChild(Node node)
    {
        childs.Add(node);
    }

    public List<Node> GetChilds()
    {
        return childs;
    }
    private List<Node> childs = new List<Node>();
}

public class Selector : Composite
{
    public override bool Invoke()
    {
        foreach (var node in GetChilds())
        {
            if (node.Invoke())
            {
                return true;
            }
        }
        return false;
    }
}

public class Sequence : Composite
{
    public override bool Invoke()
    {
        foreach (var node in GetChilds())
        {
            if (!node.Invoke())
            {
                return false;
            }
        }
        return true;
    }
}

public class Action : Node
{
    public Func<bool> actionFunc;

    public override bool Invoke()
    {
        if (actionFunc())
            return true;
        return false;
    }
}

public abstract class BT : MonoBehaviour
{
    public abstract void Init();
    public abstract IEnumerator BehaviourTree();
}   