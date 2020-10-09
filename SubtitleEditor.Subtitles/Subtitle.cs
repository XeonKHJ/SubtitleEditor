using System;
using System.Collections.Generic;

namespace SubtitleEditor.Subtitles
{
    public class Subtitle
    {
        private List<Dialogue> _dialogues { set; get; } = new List<Dialogue>();

        public IReadOnlyList<Dialogue> Dialogues
        {
            get
            {
                return _dialogues;
            }
        }

        public void AddDialogue(Dialogue dialogue)
        {
            _dialogues.Add(dialogue);
            DialogueAdded?.Invoke(this, dialogue);
        }

        public void DeleteDialogue(Dialogue dialogue)
        {
            _dialogues.Remove(dialogue);
            DialogueDeleted?.Invoke(this, dialogue);
        }

        public void DeleteDialogueByIndex(int index)
        {
            var dialogue = _dialogues[index];
            _dialogues.RemoveAt(index);
            DialogueDeleted?.Invoke(this, dialogue);
        }


        public event EventHandler<Dialogue> DialogueAdded;
        public event EventHandler<Dialogue> DialogueDeleted;
    }
}
