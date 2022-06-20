using System.Collections.Generic;
using System.Linq;
using Rundo.Core.Events;
using UnityEngine;

namespace Rundo.Core.Commands
{
    public class CommandProcessor : ICommandProcessor
    {
        public bool IsUndoRedoInProcess { get; private set; }

        private readonly List<List<(ICommand Undo, ICommand Redo)>> _undoList = new List<List<(ICommand Undo, ICommand Redo)>>();
        private readonly List<List<(ICommand Undo, ICommand Redo)>> _redoList = new List<List<(ICommand Undo, ICommand Redo)>>();

        private int _lastFrame;
        public IEventSystem EventDispatcher { get; protected set; } = new EventSystem();
        
        public void AddUndoRedoData(ICommand redoData, ICommand undoData)
        {
            if (IsUndoRedoInProcess)
                return;

            undoData.CommandProcessor = redoData.CommandProcessor;
            undoData.EventDispatcher = redoData.EventDispatcher;

            // group together all commands generated in one frame
            if (_undoList.Count == 0 || _lastFrame != Time.frameCount)
            {
                _lastFrame = Time.frameCount;
                _undoList.Add(new List<(ICommand Undo, ICommand Redo)>());
            }

            _undoList.Last().Add((undoData, redoData));
            _redoList.Clear();
        }

        public void Undo()
        {
            if (_undoList.Count > 0)
            {
                IsUndoRedoInProcess = true;

                var temp = new List<(ICommand Undo, ICommand Redo)>(_undoList.Last());
                temp.Reverse();
                foreach (var it in temp)
                    it.Undo.Process();
                
                _redoList.Insert(0, _undoList.Last());
                _undoList.Remove(_undoList.Last());
                
                IsUndoRedoInProcess = false;
            }
        }

        public void Redo()
        {
            if (_redoList.Count > 0)
            {
                IsUndoRedoInProcess = true;

                foreach (var it in _redoList.First())
                    it.Redo.Process();

                _undoList.Add(_redoList.First());
                _redoList.Remove(_redoList.First());
                
                IsUndoRedoInProcess = false;
            }
        }

        public virtual bool CanProcess()
        {
            return true;
        }

        public void ClearUndoRedo()
        {
            _undoList.Clear();
            _redoList.Clear();
        }

        public void Destroy()
        {
            ClearUndoRedo();
            EventDispatcher.UnregisterAll();
        }

        public void Process(ICommand command)
        {
            command.CommandProcessor = this;
            command.Process();
        }
    }
}

