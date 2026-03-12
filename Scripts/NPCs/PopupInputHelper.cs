using Godot;
using System;

public partial class PopupInputHelper : Node
{
    // simple helper that forwards any key or click to the parent popup
    public override void _Input(InputEvent @event)
    {
        if (@event.IsPressed() && (@event is InputEventKey || @event is InputEventMouseButton))
        {
            // notify the overlay controller
            GetParent().Call("OnNextSlide");
            GetViewport().SetInputAsHandled();
        }
    }
}
