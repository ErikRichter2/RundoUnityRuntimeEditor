namespace Rundo.RuntimeEditor.Behaviours
{
    public interface IDragDropHandler
    {
        bool CanHandleDragDrop(DragDropBehaviour dropBehaviour);
        void HandleDragDrop(DragDropBehaviour dropBehaviour);
    }
}

